using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;
using UnityEngine.Timeline;
using Object = UnityEngine.Object;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Rewrites every <see cref="PlayableDirector"/>'s <c>m_SceneBindings</c> so the array is a pure function of its
    /// timeline: one row per track that declares a binding type, in track order, and nothing else.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Prefab overrides address arrays positionally (<c>m_SceneBindings.Array.data[5].key</c>) and carry no identity,
    /// so they cannot survive the source array changing length or order. Anything that compacts the array at the
    /// source silently re-pairs an instance's keys with other rows' values, strands overrides past the new end, and
    /// drops bindings — with no error. <c>UnityEditor.DirectorEditor</c> compacts it on every inspect, because a
    /// track whose output declares no target type must not own a row; on data written before Unity 2018.2 (which had
    /// a row for every track, ControlTrack included) that is exactly what happens, and it is also where
    /// <c>ObjectDisposedException: SerializedProperty m_SceneBindings.Array.data[n] has disappeared!</c> comes from.
    /// </para>
    /// <para>
    /// Deriving the whole array from the timeline settles it: a base prefab and every instance of it then agree on
    /// which key sits at which index by construction, so an instance only ever needs a <c>.value</c> override and
    /// positional drift has nothing left to drift against.
    /// </para>
    /// <para>
    /// The run captures, applies and verifies in one pass, holding everything in memory. It refuses to write anything
    /// if any director has two rows claiming the same track, because which one <c>GetGenericBinding</c> answers with
    /// is not something to bake in on a guess — revert that instance's Bindings override, re-bind by hand, run again.
    /// </para>
    /// </remarks>
    public static class DirectorBindingCanonicalizer
    {
        const string Menu = "Assets/Canonicalize Playable Director Bindings";

        class Snapshot
        {
            public string Location;
            public string Name;
            public string AssetPath;
            /// <summary>Rows exactly as serialized, keyed by GlobalObjectId. "" means an empty reference.</summary>
            public readonly List<KeyValuePair<string, string>> Rows = new List<KeyValuePair<string, string>>();
            /// <summary>The array this director should end up with, values carried over from <see cref="Rows"/>.</summary>
            public readonly List<KeyValuePair<string, string>> Canonical = new List<KeyValuePair<string, string>>();
            public readonly List<string> DuplicateKeys = new List<string>();
        }

        [MenuItem(Menu)]
        static void Run()
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
            {
                Debug.LogError("Exit Play Mode first.");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;

            var setup = EditorSceneManager.GetSceneManagerSetup();
            var before = new Dictionary<string, Snapshot>();
            var after = new Dictionary<string, Snapshot>();
            var written = 0;

            try
            {
                Walk("Reading director bindings", false, (d, loc) =>
                {
                    var snap = Capture(d, loc);
                    before[Id(d)] = snap;
                    return false;
                });

                var ambiguous = before.Values.Where(s => s.DuplicateKeys.Count > 0).ToList();
                if (ambiguous.Count > 0)
                {
                    Debug.LogError(
                        "Nothing was written. These directors have two rows claiming the same track — revert the " +
                        "Bindings override on each, re-bind by hand, then run again:\n" +
                        string.Join("\n", ambiguous.Select(s =>
                            $"    {s.Location} :: {s.Name}  ({string.Join(", ", s.DuplicateKeys)})")));
                    return;
                }

                Walk("Rewriting director bindings", true, (d, loc) =>
                    before.TryGetValue(Id(d), out var plan) && Apply(d, plan));

                Walk("Verifying director bindings", false, (d, loc) =>
                {
                    after[Id(d)] = Capture(d, loc);
                    return false;
                });

                written = before.Count;
            }
            catch (OperationCanceledException)
            {
                Debug.LogWarning("Cancelled — anything already saved stays saved. Re-run, or revert in version control.");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                if (setup.Length > 0) EditorSceneManager.RestoreSceneManagerSetup(setup);
            }

            if (written > 0) Report(before, after);
        }

        // ------------------------------------------------------------ walking

        /// <summary>
        /// Visits every director in the project, prefab assets first in dependency order so a base is rewritten
        /// before anything inheriting from it, then scenes. The callback returns whether it changed the object.
        /// </summary>
        /// <remarks>
        /// Prefab assets are edited where they sit. <c>PrefabUtility.LoadPrefabContents</c> would instantiate them,
        /// running <c>Awake</c>/<c>OnEnable</c> on every <c>[ExecuteAlways]</c> component inside — uGUI's
        /// <c>Selectable</c> is one — and saving afterwards would commit whatever those did.
        /// </remarks>
        static void Walk(string title, bool writable, Func<PlayableDirector, string, bool> visit)
        {
            var prefabs = OrderByDependency(AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Distinct()
                .Where(p =>
                {
                    var a = AssetDatabase.LoadAssetAtPath<GameObject>(p);
                    return a != null && a.GetComponentsInChildren<PlayableDirector>(true).Length > 0;
                })
                .ToList());

            for (var i = 0; i < prefabs.Count; i++)
            {
                Tick(title, prefabs[i], i, prefabs.Count);

                var root = AssetDatabase.LoadAssetAtPath<GameObject>(prefabs[i]);
                if (root == null) continue;

                var dirty = false;
                foreach (var d in root.GetComponentsInChildren<PlayableDirector>(true))
                    dirty |= visit(d, prefabs[i]);

                if (writable && dirty) AssetDatabase.SaveAssetIfDirty(root);
            }

            var scenes = AssetDatabase.FindAssets("t:SceneAsset", new[] { "Assets" })
                .Select(AssetDatabase.GUIDToAssetPath).Distinct().ToList();

            for (var i = 0; i < scenes.Count; i++)
            {
                Tick(title, scenes[i], i, scenes.Count);

                Scene scene;
                try { scene = EditorSceneManager.OpenScene(scenes[i], OpenSceneMode.Single); }
                catch (Exception e) { Debug.LogWarning($"Skipped {scenes[i]}: {e.Message}"); continue; }

                var dirty = false;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var d in root.GetComponentsInChildren<PlayableDirector>(true))
                        dirty |= visit(d, scenes[i]);

                if (!writable || !dirty) continue;
                EditorSceneManager.MarkSceneDirty(scene);
                EditorSceneManager.SaveScene(scene);
            }
        }

        static void Tick(string title, string path, int i, int n)
        {
            if (EditorUtility.DisplayCancelableProgressBar(title, path, n == 0 ? 1f : i / (float)n))
                throw new OperationCanceledException();
        }

        static List<string> OrderByDependency(List<string> prefabs)
        {
            var pool = new HashSet<string>(prefabs);
            var visited = new HashSet<string>();
            var ordered = new List<string>(prefabs.Count);

            void Visit(string path)
            {
                if (!visited.Add(path)) return;
                foreach (var dep in AssetDatabase.GetDependencies(path, recursive: false))
                    if (dep != path && pool.Contains(dep))
                        Visit(dep);
                ordered.Add(path);
            }

            foreach (var p in prefabs) Visit(p);
            return ordered;
        }

        // ------------------------------------------------------------ capture

        static Snapshot Capture(PlayableDirector director, string location)
        {
            var asset = director.playableAsset;
            var snap = new Snapshot
            {
                Location = location,
                Name = director.name,
                AssetPath = asset != null ? AssetDatabase.GetAssetPath(asset) : "",
            };

            var serialized = new SerializedObject(director);
            var bindings = serialized.FindProperty("m_SceneBindings");
            if (bindings != null)
            {
                var seen = new HashSet<Object>();
                for (var i = 0; i < bindings.arraySize; i++)
                {
                    var element = bindings.GetArrayElementAtIndex(i);
                    var key = element.FindPropertyRelative("key").objectReferenceValue;
                    var value = element.FindPropertyRelative("value").objectReferenceValue;

                    if (key != null && !seen.Add(key)) snap.DuplicateKeys.Add(key.name);
                    snap.Rows.Add(new KeyValuePair<string, string>(Id(key), Id(value)));
                }
            }

            var carried = snap.Rows.Where(r => r.Key.Length > 0)
                .GroupBy(r => r.Key).ToDictionary(g => g.Key, g => g.First().Value);

            foreach (var track in CanonicalKeys(asset))
            {
                var keyId = Id(track);
                carried.TryGetValue(keyId, out var valueId);
                snap.Canonical.Add(new KeyValuePair<string, string>(keyId, valueId ?? ""));
            }

            return snap;
        }

        /// <summary>
        /// The tracks that must own a row, in the order the timeline lists them.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Built from <see cref="TimelineAsset.GetOutputTracks"/> and keyed by the track asset itself, never by
        /// <c>PlayableBinding.sourceObject</c>. Most tracks report themselves as the source object, but reading it
        /// blindly is not safe — see the marker track below.
        /// </para>
        /// <para>
        /// A timeline's own marker track is skipped. It reports <c>outputTargetType</c> of <c>GameObject</c> but a
        /// null <c>sourceObject</c>, so <c>DirectorEditor</c> ends up calling
        /// <c>SetGenericBinding(null, null)</c> — a no-op — and Unity therefore never writes a row for it at all.
        /// Nothing reads one either: <c>TimelinePlayable.CreateTrackOutput</c> special-cases
        /// <c>timelineAsset.markerTrack == track</c> and routes notifications to every
        /// <c>INotificationReceiver</c> on the director's own GameObject, ignoring the binding. A row here is
        /// invisible to the runtime and unrenderable in the Inspector, which draws it blank, iconless and disabled
        /// because <c>FindBinding</c> matches on that same null source object.
        /// </para>
        /// <para>
        /// This skips only the timeline's <em>own</em> marker track. A <c>SignalTrack</c> used as an ordinary track
        /// falls through to <c>base.outputs</c>, reports itself as the source, and does use its binding.
        /// </para>
        /// </remarks>
        static IEnumerable<Object> CanonicalKeys(PlayableAsset asset)
        {
            if (asset == null) yield break;

            if (asset is TimelineAsset timeline)
            {
                foreach (var track in timeline.GetOutputTracks())
                {
                    if (track == null) continue;
                    if (track == timeline.markerTrack) continue;
                    if (track.outputs.FirstOrDefault().outputTargetType == null) continue;
                    yield return track;
                }
                yield break;
            }

            foreach (var binding in asset.outputs)
                if (binding.outputTargetType != null && binding.sourceObject != null)
                    yield return binding.sourceObject;
        }

        // -------------------------------------------------------------- apply

        static bool Apply(PlayableDirector director, Snapshot plan)
        {
            var serialized = new SerializedObject(director);
            var bindings = serialized.FindProperty("m_SceneBindings");
            if (bindings == null) return false;

            // Stale positional overrides have to go before the canonical layout lands, or they keep addressing rows
            // that have moved. Reverting alone leaves no dirty flag, so remember that it happened.
            var hadOverride = HasBindingOverride(director);
            if (hadOverride)
            {
                try { PrefabUtility.RevertPropertyOverride(bindings, InteractionMode.AutomatedAction); }
                catch (Exception e) { Debug.LogWarning($"Could not revert bindings on '{director.name}': {e.Message}", director); }
                serialized = new SerializedObject(director);
                bindings = serialized.FindProperty("m_SceneBindings");
            }

            var changed = hadOverride;

            if (bindings.arraySize != plan.Canonical.Count)
            {
                bindings.arraySize = plan.Canonical.Count;
                changed = true;
            }

            for (var i = 0; i < plan.Canonical.Count; i++)
            {
                var element = bindings.GetArrayElementAtIndex(i);
                var key = element.FindPropertyRelative("key");
                var value = element.FindPropertyRelative("value");

                var wantKey = Resolve(plan.Canonical[i].Key);
                var wantValue = Resolve(plan.Canonical[i].Value);

                // Assigned only on difference, so an instance already matching its base records no override.
                if (key.objectReferenceValue != wantKey) { key.objectReferenceValue = wantKey; changed = true; }
                if (value.objectReferenceValue != wantValue) { value.objectReferenceValue = wantValue; changed = true; }
            }

            if (!changed) return false;

            serialized.ApplyModifiedPropertiesWithoutUndo();
            EditorUtility.SetDirty(director);
            if (PrefabUtility.IsPartOfPrefabInstance(director))
                PrefabUtility.RecordPrefabInstancePropertyModifications(director);
            return true;
        }

        static bool HasBindingOverride(PlayableDirector director)
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(director)) return false;

            var root = PrefabUtility.GetNearestPrefabInstanceRoot(director);
            if (root == null) return false;

            var modifications = PrefabUtility.GetPropertyModifications(root);
            if (modifications == null) return false;

            var source = PrefabUtility.GetCorrespondingObjectFromSource(director);
            return modifications.Any(m =>
                m.propertyPath != null &&
                m.propertyPath.StartsWith("m_SceneBindings", StringComparison.Ordinal) &&
                (source == null || m.target == source));
        }

        // ------------------------------------------------------------- verify

        /// <summary>
        /// Checks the rewrite against the rows as they were serialized, deciding what was allowed to disappear from
        /// the <c>[TrackBindingType]</c> attribute rather than from <c>outputs</c>.
        /// </summary>
        /// <remarks>
        /// The attribute is deliberately a different mechanism from the one <see cref="CanonicalKeys"/> uses. A check
        /// that re-runs the code it is checking shares its blind spots and proves nothing: reading both sides through
        /// <c>outputs</c> is what let a whole project's marker-track bindings vanish while the run reported success.
        /// </remarks>
        static void Report(Dictionary<string, Snapshot> before, Dictionary<string, Snapshot> after)
        {
            var lost = new List<string>();
            var dropped = new Dictionary<string, int>();
            int preserved = 0, rowsBefore = 0, rowsAfter = 0;

            foreach (var pair in before)
            {
                var was = pair.Value;
                if (!after.TryGetValue(pair.Key, out var now))
                {
                    lost.Add($"{was.Location} :: {was.Name} — director disappeared between passes");
                    continue;
                }

                rowsBefore += was.Rows.Count;
                rowsAfter += now.Rows.Count;

                var nowMap = now.Rows.Where(r => r.Key.Length > 0)
                    .GroupBy(r => r.Key).ToDictionary(g => g.Key, g => g.First().Value);

                foreach (var row in was.Rows)
                {
                    if (row.Key.Length == 0) { Bump(dropped, "row with an empty key"); continue; }

                    if (nowMap.TryGetValue(row.Key, out var value))
                    {
                        if (value == row.Value) preserved++;
                        else lost.Add($"{was.Location} :: {was.Name} — value changed for {Describe(row.Key)}: " +
                                      $"{Describe(row.Value)} -> {Describe(value)}");
                        continue;
                    }

                    var reason = WhyDroppable(row.Key, was.AssetPath);
                    if (reason == null)
                        lost.Add($"{was.Location} :: {was.Name} — DROPPED a bindable row: " +
                                 $"{Describe(row.Key)} was bound to {Describe(row.Value)}");
                    else
                        Bump(dropped, reason);
                }
            }

            var sb = new StringBuilder();
            sb.AppendLine($"Canonicalized {before.Count} directors: {rowsBefore} rows -> {rowsAfter}.");
            sb.AppendLine($"  bindings preserved exactly: {preserved}");
            foreach (var kv in dropped.OrderByDescending(k => k.Value))
                sb.AppendLine($"  dropped, {kv.Key}: {kv.Value}");

            if (lost.Count == 0)
            {
                sb.AppendLine();
                sb.AppendLine("VERIFIED — every row keyed to a live bindable track kept its exact value.");
                Debug.Log(sb.ToString());
                return;
            }

            sb.AppendLine();
            sb.AppendLine($"REGRESSIONS: {lost.Count}. Revert in version control and do not commit.");
            foreach (var l in lost.Take(200)) sb.AppendLine("  " + l);
            if (lost.Count > 200) sb.AppendLine($"  … and {lost.Count - 200} more");
            Debug.LogError(sb.ToString());
        }

        /// <summary>Why a row was allowed to vanish, or null when losing it is a regression.</summary>
        static string WhyDroppable(string keyId, string ownAssetPath)
        {
            var key = Resolve(keyId);
            if (key == null) return "key pointed at a deleted track";

            var path = AssetDatabase.GetAssetPath(key);
            if (ownAssetPath.Length == 0 || path != ownAssetPath) return "row belonged to another PlayableAsset";

            if (!(key is TrackAsset track)) return "key was not a track";

            // Resolved through the asset rather than TrackAsset.timelineAsset so this stays a plain data question.
            if (AssetDatabase.LoadMainAssetAtPath(ownAssetPath) is TimelineAsset timeline && track == timeline.markerTrack)
                return "timeline's own marker track — Unity never writes this row and nothing reads it";

            return Attribute.GetCustomAttribute(track.GetType(), typeof(TrackBindingTypeAttribute)) == null
                ? "track declares no binding type"
                : null;
        }

        static void Bump(Dictionary<string, int> counts, string reason) =>
            counts[reason] = counts.TryGetValue(reason, out var n) ? n + 1 : 1;

        // -------------------------------------------------------------- ident

        static string Id(Object o) => o == null ? "" : GlobalObjectId.GetGlobalObjectIdSlow(o).ToString();

        static Object Resolve(string id) =>
            !string.IsNullOrEmpty(id) && GlobalObjectId.TryParse(id, out var parsed)
                ? GlobalObjectId.GlobalObjectIdentifierToObjectSlow(parsed)
                : null;

        static string Describe(string id)
        {
            if (string.IsNullOrEmpty(id)) return "<none>";
            var o = Resolve(id);
            return o != null ? $"'{o.name}' ({o.GetType().Name})" : id;
        }
    }
}
