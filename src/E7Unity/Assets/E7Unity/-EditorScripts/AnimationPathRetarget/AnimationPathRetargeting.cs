using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace E7.E7Unity.AnimationPathRetarget
{
    /// <summary>
    /// Collects the hierarchy paths an <see cref="AnimationClip" /> set binds to, reports which of them still
    /// resolve against a live hierarchy, and rewrites them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A clip stores every binding's path as a string relative to the game object carrying the
    /// <see cref="Animator" /> or <see cref="Animation" />, and a CRC32 of that same string inside
    /// <c>m_ClipBindingConstant</c> that Mecanim binds by at runtime. Going through
    /// <see cref="AnimationUtility" /> keeps the two in step; rewriting the YAML strings alone leaves a clip that
    /// looks correct in the Animation window and animates nothing in a player.
    /// </para>
    /// <para>
    /// Everything here is read-only until <see cref="Apply" /> is called, so a window can scan, score and preview
    /// without touching an asset.
    /// </para>
    /// </remarks>
    public static class AnimationPathRetargeting
    {
        /// <summary>One binding of one clip, kept so a rewrite can address it individually.</summary>
        public readonly struct BindingRef
        {
            public BindingRef(AnimationClip clip, EditorCurveBinding binding, bool isObjectReference)
            {
                Clip = clip;
                Binding = binding;
                IsObjectReference = isObjectReference;
            }

            public AnimationClip Clip { get; }
            public EditorCurveBinding Binding { get; }
            public bool IsObjectReference { get; }

            /// <summary>Reads as <c>Image.m_Enabled</c>; the type half is what decides whether a path resolves.</summary>
            public string PropertyLabel =>
                $"{(Binding.type != null ? Binding.type.Name : "?")}.{Binding.propertyName}";
        }

        /// <summary>A candidate replacement path, ranked against the path being replaced.</summary>
        public readonly struct Suggestion
        {
            public Suggestion(string path, int score, int missingProperties)
            {
                Path = path;
                Score = score;
                MissingProperties = missingProperties;
            }

            public string Path { get; }
            public int Score { get; }

            /// <summary>How many of the group's bindings would still fail to resolve at <see cref="Path" />.</summary>
            public int MissingProperties { get; }

            public bool IsComplete => MissingProperties == 0;
        }

        /// <summary>Every binding across the scanned clips that shares one path, plus its proposed replacement.</summary>
        public sealed class PathEntry
        {
            public string Path { get; internal set; }
            public List<BindingRef> Bindings { get; } = new List<BindingRef>();

            /// <summary>Null when no hierarchy was supplied, so the window can say "unknown" rather than "broken".</summary>
            public bool? TransformExists { get; internal set; }

            /// <summary>Bindings whose object could not be found — what the Animation window labels "(Missing!)".</summary>
            public int UnresolvedCount { get; internal set; }

            public IReadOnlyList<Suggestion> Suggestions { get; internal set; } = Array.Empty<Suggestion>();

            /// <summary>Set by the window. Null or equal to <see cref="Path" /> means this entry is left alone.</summary>
            public string Proposed { get; set; }

            public IEnumerable<AnimationClip> Clips => Bindings.Select(b => b.Clip).Distinct();

            public bool IsBroken => TransformExists == false || UnresolvedCount > 0;

            /// <summary>An empty proposal is the root itself, which is a legitimate destination; null is "untouched".</summary>
            public bool HasChange => Proposed != null && Proposed != Path;
        }

        /// <summary>What a target object turned out to be, so the window can explain itself.</summary>
        public sealed class Target
        {
            public Object Source { get; internal set; }

            /// <summary>The game object paths are relative to — an Animator's or Animation's own. Null when unknown.</summary>
            public GameObject Root { get; internal set; }

            public List<AnimationClip> Clips { get; } = new List<AnimationClip>();
            public string Note { get; internal set; }
            public bool IsValid => Clips.Count > 0;
        }

        // ---- discovery -------------------------------------------------------------------------------------

        /// <summary>
        /// Works out which clips to scan and which game object their paths are relative to, from whatever was
        /// dropped in: a game object or component carrying an <see cref="Animator" /> or <see cref="Animation" />,
        /// an <see cref="AnimatorController" />, or a lone <see cref="AnimationClip" />.
        /// </summary>
        public static Target Resolve(Object source)
        {
            var target = new Target { Source = source };
            if (source == null)
            {
                target.Note = "Drop an Animator, an Animation, an Animator Controller or a clip here.";
                return target;
            }

            if (source is Component component) source = component.gameObject;

            if (source is GameObject go)
            {
                ResolveFromGameObject(go, target);
                return target;
            }

            if (source is AnimationClip loneClip)
            {
                target.Clips.Add(loneClip);
                target.Note = "A clip on its own carries no hierarchy — assign the Animator that plays it to " +
                              "check and drag-and-drop paths.";
                return target;
            }

            if (source is RuntimeAnimatorController controller)
            {
                target.Clips.AddRange(ClipsOf(controller));
                target.Note = "A controller on its own carries no hierarchy — assign the Animator that plays it " +
                              "to check and drag-and-drop paths.";
                return target;
            }

            target.Note = $"{source.GetType().Name} is not something that plays animation.";
            return target;
        }

        static void ResolveFromGameObject(GameObject go, Target target)
        {
            var animator = go.GetComponent<Animator>();
            var legacy = go.GetComponent<Animation>();

            if (animator == null && legacy == null)
            {
                animator = go.GetComponentInChildren<Animator>(true);
                legacy = animator == null ? go.GetComponentInChildren<Animation>(true) : null;
                if (animator != null || legacy != null)
                {
                    GameObject found = animator != null ? animator.gameObject : legacy.gameObject;
                    target.Note = $"Paths are relative to \"{found.name}\", the nearest player below \"{go.name}\".";
                }
            }

            if (animator != null)
            {
                target.Root = animator.gameObject;
                if (animator.runtimeAnimatorController == null)
                {
                    target.Note = $"\"{animator.gameObject.name}\" has an Animator with no controller assigned.";
                    return;
                }

                target.Clips.AddRange(ClipsOf(animator.runtimeAnimatorController));
                return;
            }

            if (legacy != null)
            {
                target.Root = legacy.gameObject;
                target.Clips.AddRange(AnimationUtility.GetAnimationClips(legacy.gameObject).Where(c => c != null));
                return;
            }

            target.Note = $"\"{go.name}\" has no Animator or Animation, in itself or below it.";
        }

        /// <summary>
        /// Every distinct clip a controller can play. Sub-assets of the controller asset are included even when no
        /// state references them, so a clip parked in the asset is still reachable.
        /// </summary>
        public static IEnumerable<AnimationClip> ClipsOf(RuntimeAnimatorController controller)
        {
            var seen = new HashSet<AnimationClip>();

            foreach (AnimationClip clip in controller.animationClips)
                if (clip != null) seen.Add(clip);

            if (controller is AnimatorOverrideController overrides)
            {
                var pairs = new List<KeyValuePair<AnimationClip, AnimationClip>>();
                overrides.GetOverrides(pairs);
                foreach (var pair in pairs)
                {
                    if (pair.Key != null) seen.Add(pair.Key);
                    if (pair.Value != null) seen.Add(pair.Value);
                }
            }

            string assetPath = AssetDatabase.GetAssetPath(controller);
            if (!string.IsNullOrEmpty(assetPath))
                foreach (AnimationClip clip in AssetDatabase.LoadAllAssetsAtPath(assetPath).OfType<AnimationClip>())
                    seen.Add(clip);

            return seen.OrderBy(c => c.name, StringComparer.Ordinal);
        }

        // ---- scanning --------------------------------------------------------------------------------------

        /// <summary>
        /// Groups every binding of every clip by path, and resolves each one against <paramref name="root" />.
        /// Pass a null root to list the paths without judging them.
        /// </summary>
        public static List<PathEntry> Scan(IEnumerable<AnimationClip> clips, GameObject root)
        {
            var byPath = new Dictionary<string, PathEntry>(StringComparer.Ordinal);

            foreach (AnimationClip clip in clips)
            {
                if (clip == null) continue;
                foreach (EditorCurveBinding binding in AnimationUtility.GetCurveBindings(clip))
                    Record(byPath, clip, binding, false);
                foreach (EditorCurveBinding binding in AnimationUtility.GetObjectReferenceCurveBindings(clip))
                    Record(byPath, clip, binding, true);
            }

            List<PathEntry> entries = byPath.Values.ToList();

            foreach (PathEntry entry in entries)
            {
                if (root == null)
                {
                    entry.TransformExists = null;
                    continue;
                }

                entry.TransformExists = FindChild(root.transform, entry.Path) != null;
                entry.UnresolvedCount = entry.Bindings.Count(b => !Resolves(root, b.Binding));
                if (entry.IsBroken) entry.Suggestions = Suggest(root, entry);
            }

            return entries
                .OrderByDescending(e => e.IsBroken)
                .ThenBy(e => e.Path, StringComparer.Ordinal)
                .ToList();
        }

        static void Record(IDictionary<string, PathEntry> byPath, AnimationClip clip, EditorCurveBinding binding,
                           bool isObjectReference)
        {
            if (!byPath.TryGetValue(binding.path, out PathEntry entry))
            {
                entry = new PathEntry { Path = binding.path };
                byPath.Add(binding.path, entry);
            }

            entry.Bindings.Add(new BindingRef(clip, binding, isObjectReference));
        }

        /// <summary>Whether a binding finds its object — the same test behind the Animation window's "(Missing!)".</summary>
        public static bool Resolves(GameObject root, EditorCurveBinding binding) =>
            root != null && AnimationUtility.GetAnimatedObject(root, binding) != null;

        /// <summary>An empty path means the root itself, which <see cref="Transform.Find" /> will not return.</summary>
        public static Transform FindChild(Transform root, string path) =>
            string.IsNullOrEmpty(path) ? root : root.Find(path);

        /// <summary>The path <paramref name="target" /> would be written as, or null if it sits outside the root.</summary>
        public static string PathOf(Transform target, Transform root)
        {
            if (target == null || root == null) return null;
            if (target == root) return string.Empty;
            return target.IsChildOf(root) ? AnimationUtility.CalculateTransformPath(target, root) : null;
        }

        // ---- suggesting ------------------------------------------------------------------------------------

        /// <summary>
        /// Ranks the transforms under <paramref name="root" /> as replacements for a broken path, best first.
        /// </summary>
        /// <remarks>
        /// Carrying every property the group animates outweighs any name similarity, because a same-named object
        /// that lost its component is exactly the trap this tool exists to avoid. Name and path-tail matching only
        /// break ties between candidates that would all work.
        /// </remarks>
        public static IReadOnlyList<Suggestion> Suggest(GameObject root, PathEntry entry, int limit = 5)
        {
            if (root == null) return Array.Empty<Suggestion>();

            string[] oldSegments = SegmentsOf(entry.Path);
            string oldLeaf = oldSegments.Length > 0 ? oldSegments[oldSegments.Length - 1] : string.Empty;

            EditorCurveBinding[] distinct = entry.Bindings
                .Select(b => b.Binding)
                .GroupBy(b => (b.type, b.propertyName))
                .Select(g => g.First())
                .ToArray();

            var results = new List<Suggestion>();

            foreach (Transform candidate in root.GetComponentsInChildren<Transform>(true))
            {
                string path = PathOf(candidate, root.transform);
                if (path == null || path == entry.Path) continue;

                int missing = distinct.Count(b =>
                {
                    EditorCurveBinding probe = b;
                    probe.path = path;
                    return !Resolves(root, probe);
                });

                int score = (distinct.Length - missing) * 200 - missing * 120;

                string[] segments = SegmentsOf(path);
                string leaf = segments.Length > 0 ? segments[segments.Length - 1] : string.Empty;
                if (string.Equals(leaf, oldLeaf, StringComparison.Ordinal)) score += 120;
                else if (string.Equals(leaf, oldLeaf, StringComparison.OrdinalIgnoreCase)) score += 70;

                score += CommonTail(segments, oldSegments) * 30;
                score -= Math.Abs(segments.Length - oldSegments.Length) * 5;

                if (score > 0) results.Add(new Suggestion(path, score, missing));
            }

            return results
                .OrderByDescending(s => s.Score)
                .ThenBy(s => s.Path.Length)
                .Take(limit)
                .ToList();
        }

        /// <summary>
        /// The one suggestion safe to accept unreviewed, or null when the choice is genuinely ambiguous.
        /// </summary>
        /// <remarks>
        /// Carrying every animated property is common — sibling objects of the same kind all qualify — so it is
        /// not on its own a decision. An exact leaf-name match on top of that is, because it means the object kept
        /// its name and only moved, which is the rename-and-reparent case this tool is for.
        /// </remarks>
        public static Suggestion? ConfidentPick(PathEntry entry)
        {
            List<Suggestion> complete = entry.Suggestions.Where(s => s.IsComplete).ToList();
            if (complete.Count == 0) return null;
            if (complete.Count == 1) return complete[0];

            string leaf = LeafOf(entry.Path);
            if (string.IsNullOrEmpty(leaf)) return null;

            List<Suggestion> sameName = complete
                .Where(s => string.Equals(LeafOf(s.Path), leaf, StringComparison.Ordinal))
                .ToList();
            return sameName.Count == 1 ? sameName[0] : (Suggestion?)null;
        }

        static string LeafOf(string path)
        {
            string[] segments = SegmentsOf(path);
            return segments.Length > 0 ? segments[segments.Length - 1] : string.Empty;
        }

        static string[] SegmentsOf(string path) =>
            string.IsNullOrEmpty(path) ? Array.Empty<string>() : path.Split('/');

        static int CommonTail(string[] a, string[] b)
        {
            int n = 0;
            while (n < a.Length && n < b.Length &&
                   string.Equals(a[a.Length - 1 - n], b[b.Length - 1 - n], StringComparison.Ordinal)) n++;
            return n;
        }

        // ---- writing ---------------------------------------------------------------------------------------

        /// <summary>Why a clip cannot be rewritten, or null when it can.</summary>
        public static string EditabilityProblem(AnimationClip clip)
        {
            if (clip == null) return "The clip is missing.";
            if ((clip.hideFlags & HideFlags.NotEditable) != 0) return "The clip is marked not editable.";

            string path = AssetDatabase.GetAssetPath(clip);
            if (string.IsNullOrEmpty(path)) return "The clip is not saved as an asset.";
            if (AssetImporter.GetAtPath(path) is ModelImporter)
                return "The clip is imported from a model file — retarget it in the model's Animation tab instead.";

            var packageInfo = UnityEditor.PackageManager.PackageInfo.FindForAssetPath(path);
            if (packageInfo != null && packageInfo.source != UnityEditor.PackageManager.PackageSource.Embedded &&
                packageInfo.source != UnityEditor.PackageManager.PackageSource.Local)
                return $"The clip belongs to the immutable package \"{packageInfo.displayName}\".";

            return null;
        }

        /// <summary>
        /// A binding this rewrite would land on top of, because the destination path already animates the same
        /// property in the same clip.
        /// </summary>
        public static IEnumerable<string> Collisions(IEnumerable<PathEntry> entries)
        {
            var occupied = new HashSet<(AnimationClip, string, Type, string)>();
            foreach (PathEntry entry in entries)
            foreach (BindingRef reference in entry.Bindings)
                occupied.Add((reference.Clip, reference.Binding.path, reference.Binding.type,
                              reference.Binding.propertyName));

            foreach (PathEntry entry in entries.Where(e => e.HasChange))
            foreach (BindingRef reference in entry.Bindings)
            {
                var destination = (reference.Clip, entry.Proposed, reference.Binding.type,
                                   reference.Binding.propertyName);
                if (occupied.Contains(destination))
                    yield return $"{reference.Clip.name}: {entry.Proposed} already animates " +
                                 $"{reference.PropertyLabel}.";
            }
        }

        /// <summary>
        /// Rewrites every entry carrying a change and returns how many bindings moved. Old bindings are removed
        /// before the new ones are written so a rewrite onto an occupied path replaces rather than duplicates.
        /// </summary>
        public static int Apply(IReadOnlyList<PathEntry> entries)
        {
            List<PathEntry> changes = entries.Where(e => e.HasChange).ToList();
            if (changes.Count == 0) return 0;

            AnimationClip[] clips = changes.SelectMany(e => e.Clips).Distinct().ToArray();
            Undo.RegisterCompleteObjectUndo(clips, "Retarget Animation Paths");

            int moved = 0;

            foreach (AnimationClip clip in clips)
            {
                var floatBindings = new List<EditorCurveBinding>();
                var floatCurves = new List<AnimationCurve>();

                foreach (PathEntry entry in changes)
                foreach (BindingRef reference in entry.Bindings.Where(b => b.Clip == clip))
                {
                    EditorCurveBinding moving = reference.Binding;

                    if (reference.IsObjectReference)
                    {
                        ObjectReferenceKeyframe[] keys = AnimationUtility.GetObjectReferenceCurve(clip, moving);
                        AnimationUtility.SetObjectReferenceCurve(clip, moving, null);
                        moving.path = entry.Proposed;
                        AnimationUtility.SetObjectReferenceCurve(clip, moving, keys);
                    }
                    else
                    {
                        floatCurves.Add(AnimationUtility.GetEditorCurve(clip, moving));
                        AnimationUtility.SetEditorCurve(clip, moving, null);
                        moving.path = entry.Proposed;
                        floatBindings.Add(moving);
                    }

                    moved++;
                }

                if (floatBindings.Count > 0)
                    AnimationUtility.SetEditorCurves(clip, floatBindings.ToArray(), floatCurves.ToArray());

                EditorUtility.SetDirty(clip);
            }

            AssetDatabase.SaveAssets();
            return moved;
        }
    }
}
