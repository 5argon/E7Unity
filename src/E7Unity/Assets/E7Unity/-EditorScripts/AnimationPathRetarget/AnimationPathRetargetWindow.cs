using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

namespace E7.E7Unity.AnimationPathRetarget
{
    /// <summary>
    /// Retargets the hierarchy paths of the clips an Animator or Animation plays, after the objects they animate
    /// were renamed, reparented, or wrapped in a new layer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Paths are grouped rather than listed per clip, because one rename usually breaks the same path in every
    /// clip of a controller — a single drop then fixes all of them. Dropping a game object writes the path
    /// <see cref="UnityEditor.AnimationUtility.CalculateTransformPath" /> computes for it, so no path is ever
    /// typed by hand.
    /// </para>
    /// <para>
    /// Nothing is written until Apply, and the write goes through
    /// <see cref="AnimationPathRetargeting.Apply" /> so the clips' runtime binding hashes stay in step with the
    /// path strings.
    /// </para>
    /// </remarks>
    public sealed class AnimationPathRetargetWindow : EditorWindow
    {
        const string StyleSheetPath =
            "Packages/com.e7.e7unity/-EditorScripts/AnimationPathRetarget/AnimationPathRetargetWindow.uss";

        [SerializeField] Object target;
        [SerializeField] bool followSelection = true;
        [SerializeField] bool showHealthy;

        AnimationPathRetargeting.Target resolved;
        List<AnimationPathRetargeting.PathEntry> entries = new List<AnimationPathRetargeting.PathEntry>();

        /// <summary>Writing curves raises <c>onCurveWasModified</c> per binding; rescanning under it is wasted work.</summary>
        bool applying;

        ObjectField targetField;
        VisualElement notice;
        VisualElement list;
        VisualElement footer;
        Label summary;
        Button applyButton;

        [MenuItem("Window/Animation/Path Retargeter")]
        public static AnimationPathRetargetWindow Open()
        {
            var window = GetWindow<AnimationPathRetargetWindow>();
            window.titleContent = new GUIContent("Path Retargeter");
            window.minSize = new Vector2(420, 320);
            window.Show();
            return window;
        }

        [MenuItem("CONTEXT/Animator/Retarget Animation Paths")]
        static void FromAnimator(MenuCommand command)
        {
            AnimationPathRetargetWindow window = Open();
            window.SetTarget(((Component)command.context).gameObject);
        }

        [MenuItem("CONTEXT/Animation/Retarget Animation Paths")]
        static void FromAnimation(MenuCommand command)
        {
            AnimationPathRetargetWindow window = Open();
            window.SetTarget(((Component)command.context).gameObject);
        }

        /// <summary>Points the window at something and rescans, as the context menu entries do.</summary>
        public void SetTarget(Object newTarget)
        {
            target = newTarget;
            followSelection = false;
            if (targetField != null) targetField.SetValueWithoutNotify(target);
            Rescan();
        }

        void OnEnable()
        {
            Selection.selectionChanged += OnSelectionChanged;
            AnimationUtility.onCurveWasModified += OnCurveWasModified;
            EditorApplication.hierarchyChanged += Rescan;
        }

        void OnDisable()
        {
            Selection.selectionChanged -= OnSelectionChanged;
            AnimationUtility.onCurveWasModified -= OnCurveWasModified;
            EditorApplication.hierarchyChanged -= Rescan;
        }

        void OnSelectionChanged()
        {
            if (!followSelection || Selection.activeObject == null) return;
            if (Selection.activeObject == target) return;
            target = Selection.activeObject;
            targetField?.SetValueWithoutNotify(target);
            Rescan();
        }

        void OnCurveWasModified(AnimationClip clip, EditorCurveBinding binding,
                                AnimationUtility.CurveModifiedType type) => Rescan();

        void CreateGUI()
        {
            VisualElement root = rootVisualElement;

            var sheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(StyleSheetPath);
            if (sheet != null) root.styleSheets.Add(sheet);
            root.AddToClassList("retarget-root");

            root.Add(BuildToolbar());

            notice = new VisualElement { name = "notice" };
            root.Add(notice);

            list = new ScrollView(ScrollViewMode.Vertical) { name = "list" };
            list.style.flexGrow = 1;
            root.Add(list);

            footer = new VisualElement { name = "footer" };
            footer.AddToClassList("footer");
            summary = new Label { name = "summary" };
            summary.AddToClassList("summary");
            applyButton = new Button(ApplyChanges) { text = "Apply" };
            applyButton.AddToClassList("apply");
            footer.Add(summary);
            footer.Add(applyButton);
            root.Add(footer);

            Rescan();
        }

        VisualElement BuildToolbar()
        {
            var toolbar = new Toolbar();

            targetField = new ObjectField
            {
                objectType = typeof(Object),
                allowSceneObjects = true,
                tooltip = "An Animator, an Animation, an Animator Controller, or a single clip."
            };
            targetField.AddToClassList("target-field");
            targetField.style.flexGrow = 1;
            targetField.SetValueWithoutNotify(target);
            targetField.RegisterValueChangedCallback(evt =>
            {
                target = evt.newValue;
                followSelection = false;
                toolbar.Q<ToolbarToggle>("follow")?.SetValueWithoutNotify(false);
                Rescan();
            });
            toolbar.Add(targetField);

            var follow = new ToolbarToggle { name = "follow", text = "Follow Selection", value = followSelection };
            follow.RegisterValueChangedCallback(evt =>
            {
                followSelection = evt.newValue;
                if (followSelection) OnSelectionChanged();
            });
            toolbar.Add(follow);

            var healthy = new ToolbarToggle { name = "healthy", text = "Show Resolved", value = showHealthy };
            healthy.RegisterValueChangedCallback(evt =>
            {
                showHealthy = evt.newValue;
                RebuildList();
            });
            toolbar.Add(healthy);

            var accept = new ToolbarButton(AcceptConfidentSuggestions)
            {
                text = "Auto-Fix",
                tooltip = "Propose the top suggestion for every broken path that has exactly one complete match."
            };
            toolbar.Add(accept);

            toolbar.Add(new ToolbarButton(Rescan) { text = "Rescan" });
            return toolbar;
        }

        // ---- scanning --------------------------------------------------------------------------------------

        void Rescan()
        {
            if (list == null || applying) return;

            resolved = AnimationPathRetargeting.Resolve(target);
            entries = AnimationPathRetargeting.Scan(resolved.Clips, resolved.Root);
            RebuildList();
        }

        void RebuildList()
        {
            if (list == null) return;

            list.Clear();
            notice.Clear();

            if (!string.IsNullOrEmpty(resolved?.Note))
                notice.Add(new HelpBox(resolved.Note,
                    resolved.IsValid ? HelpBoxMessageType.Info : HelpBoxMessageType.Warning));

            if (resolved != null && resolved.IsValid)
            {
                string[] problems = resolved.Clips
                    .Select(c => new { c, problem = AnimationPathRetargeting.EditabilityProblem(c) })
                    .Where(x => x.problem != null)
                    .Select(x => $"{x.c.name}: {x.problem}")
                    .ToArray();
                if (problems.Length > 0)
                    notice.Add(new HelpBox("Some clips cannot be rewritten.\n" + string.Join("\n", problems),
                        HelpBoxMessageType.Warning));
            }

            IEnumerable<AnimationPathRetargeting.PathEntry> shown =
                showHealthy ? entries : entries.Where(e => e.IsBroken || e.HasChange);

            var any = false;
            foreach (AnimationPathRetargeting.PathEntry entry in shown)
            {
                list.Add(BuildCard(entry));
                any = true;
            }

            if (!any && resolved != null && resolved.IsValid)
                list.Add(new HelpBox(
                    entries.Count == 0
                        ? "These clips bind nothing yet."
                        : "Every path resolves. Turn on Show Resolved to retarget one anyway.",
                    HelpBoxMessageType.Info));

            RefreshFooter();
        }

        // ---- one path --------------------------------------------------------------------------------------

        VisualElement BuildCard(AnimationPathRetargeting.PathEntry entry)
        {
            var card = new VisualElement();
            card.AddToClassList("card");
            if (entry.IsBroken) card.AddToClassList("card--broken");
            if (entry.HasChange) card.AddToClassList("card--changed");

            var header = new VisualElement();
            header.AddToClassList("card__header");

            var status = new Label(StatusOf(entry));
            status.AddToClassList("pill");
            status.AddToClassList(entry.TransformExists == null ? "pill--unknown"
                                  : entry.IsBroken ? "pill--broken" : "pill--ok");
            header.Add(status);

            var pathLabel = new Label(string.IsNullOrEmpty(entry.Path) ? "(the Animator's own object)" : entry.Path);
            pathLabel.AddToClassList("path");
            pathLabel.tooltip = "The path as the clips store it, relative to the Animator's game object.";
            header.Add(pathLabel);
            card.Add(header);

            int clipCount = entry.Clips.Count();
            var counts = new Label($"{entry.Bindings.Count} binding{(entry.Bindings.Count == 1 ? "" : "s")} " +
                                   $"in {clipCount} clip{(clipCount == 1 ? "" : "s")}");
            counts.AddToClassList("counts");
            card.Add(counts);

            card.Add(BuildDropRow(entry, card));

            if (entry.Suggestions.Count > 0) card.Add(BuildSuggestions(entry, card));

            card.Add(BuildDetails(entry));

            RegisterDrop(card, entry, card);
            return card;
        }

        static string StatusOf(AnimationPathRetargeting.PathEntry entry)
        {
            if (entry.TransformExists == null) return "NO HIERARCHY";
            if (entry.TransformExists == false) return "MISSING OBJECT";
            if (entry.UnresolvedCount > 0) return $"MISSING PROPERTY ×{entry.UnresolvedCount}";
            return "RESOLVES";
        }

        VisualElement BuildDropRow(AnimationPathRetargeting.PathEntry entry, VisualElement card)
        {
            var row = new VisualElement();
            row.AddToClassList("drop-row");

            var caption = new Label("Retarget to");
            caption.AddToClassList("caption");
            row.Add(caption);

            var picker = new ObjectField
            {
                objectType = typeof(GameObject),
                allowSceneObjects = true,
                tooltip = "Drag a game object from the Hierarchy, or anywhere onto this card."
            };
            picker.style.flexGrow = 1;
            picker.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue is GameObject go) Propose(entry, go);
                picker.SetValueWithoutNotify(null);
            });
            row.Add(picker);

            var proposal = new Label();
            proposal.AddToClassList("proposal");
            UpdateProposalLabel(proposal, entry);
            row.Add(proposal);

            var clear = new Button(() =>
            {
                entry.Proposed = null;
                card.RemoveFromClassList("card--changed");
                UpdateProposalLabel(proposal, entry);
                RefreshFooter();
            }) { text = "×", tooltip = "Drop this proposal" };
            clear.AddToClassList("clear");
            row.Add(clear);

            return row;
        }

        void UpdateProposalLabel(Label label, AnimationPathRetargeting.PathEntry entry)
        {
            if (!entry.HasChange)
            {
                label.text = string.Empty;
                label.style.display = DisplayStyle.None;
                return;
            }

            label.style.display = DisplayStyle.Flex;
            label.text = "→ " + (string.IsNullOrEmpty(entry.Proposed) ? "(the Animator's own object)" : entry.Proposed);
        }

        VisualElement BuildSuggestions(AnimationPathRetargeting.PathEntry entry, VisualElement card)
        {
            var row = new VisualElement();
            row.AddToClassList("suggestions");

            var caption = new Label("Suggestions");
            caption.AddToClassList("caption");
            row.Add(caption);

            foreach (AnimationPathRetargeting.Suggestion suggestion in entry.Suggestions)
            {
                AnimationPathRetargeting.Suggestion captured = suggestion;
                var chip = new Button(() => Propose(entry, captured.Path))
                {
                    text = string.IsNullOrEmpty(captured.Path) ? "(root)" : captured.Path,
                    tooltip = captured.IsComplete
                        ? "Every property this path animates exists here."
                        : $"{captured.MissingProperties} of the animated properties are missing here."
                };
                chip.AddToClassList("chip");
                chip.AddToClassList(captured.IsComplete ? "chip--complete" : "chip--partial");
                row.Add(chip);
            }

            return row;
        }

        VisualElement BuildDetails(AnimationPathRetargeting.PathEntry entry)
        {
            var foldout = new Foldout { text = "Bindings", value = false };
            foldout.AddToClassList("details");

            foreach (IGrouping<AnimationClip, AnimationPathRetargeting.BindingRef> group in
                     entry.Bindings.GroupBy(b => b.Clip))
            {
                var clipRow = new VisualElement();
                clipRow.AddToClassList("clip-row");

                AnimationClip clip = group.Key;
                var ping = new Button(() => EditorGUIUtility.PingObject(clip)) { text = clip.name };
                ping.AddToClassList("clip-name");
                clipRow.Add(ping);

                var properties = new Label(string.Join(", ",
                    group.Select(b => b.PropertyLabel).Distinct().OrderBy(s => s)));
                properties.AddToClassList("properties");
                clipRow.Add(properties);

                foldout.Add(clipRow);
            }

            return foldout;
        }

        // ---- drag and drop ---------------------------------------------------------------------------------

        void RegisterDrop(VisualElement zone, AnimationPathRetargeting.PathEntry entry, VisualElement card)
        {
            zone.RegisterCallback<DragEnterEvent>(enter =>
            {
                if (DraggedTransform(out GameObject _)) card.AddToClassList("card--hover");
            });
            zone.RegisterCallback<DragLeaveEvent>(_ => card.RemoveFromClassList("card--hover"));
            zone.RegisterCallback<DragExitedEvent>(_ => card.RemoveFromClassList("card--hover"));

            zone.RegisterCallback<DragUpdatedEvent>(evt =>
            {
                DragAndDrop.visualMode = DraggedTransform(out _)
                    ? DragAndDropVisualMode.Link
                    : DragAndDropVisualMode.Rejected;
                evt.StopPropagation();
            });

            zone.RegisterCallback<DragPerformEvent>(evt =>
            {
                card.RemoveFromClassList("card--hover");
                if (!DraggedTransform(out GameObject dropped)) return;
                DragAndDrop.AcceptDrag();
                Propose(entry, dropped);
                evt.StopPropagation();
            });
        }

        /// <summary>A drag is accepted only when it is one game object living under the scanned root.</summary>
        bool DraggedTransform(out GameObject dropped)
        {
            dropped = null;
            if (resolved?.Root == null) return false;
            if (DragAndDrop.objectReferences.Length != 1) return false;
            if (!(DragAndDrop.objectReferences[0] is GameObject go)) return false;
            if (AnimationPathRetargeting.PathOf(go.transform, resolved.Root.transform) == null) return false;

            dropped = go;
            return true;
        }

        // ---- proposals -------------------------------------------------------------------------------------

        void Propose(AnimationPathRetargeting.PathEntry entry, GameObject dropped)
        {
            string path = AnimationPathRetargeting.PathOf(dropped.transform, resolved?.Root?.transform);
            if (path == null)
            {
                ShowNotification(new GUIContent($"\"{dropped.name}\" is not under \"{resolved?.Root?.name}\"."));
                return;
            }

            Propose(entry, path);
        }

        /// <summary>
        /// The rebuild is deferred because a proposal arrives from a drop or a chip click on an element the
        /// rebuild destroys, and tearing that element down inside its own event dispatch throws.
        /// </summary>
        void Propose(AnimationPathRetargeting.PathEntry entry, string path)
        {
            entry.Proposed = path;
            rootVisualElement.schedule.Execute(RebuildList);
        }

        void AcceptConfidentSuggestions()
        {
            var accepted = 0;
            foreach (AnimationPathRetargeting.PathEntry entry in entries.Where(e => e.IsBroken && !e.HasChange))
            {
                AnimationPathRetargeting.Suggestion? pick = AnimationPathRetargeting.ConfidentPick(entry);
                if (pick == null) continue;
                entry.Proposed = pick.Value.Path;
                accepted++;
            }

            RebuildList();
            ShowNotification(new GUIContent(accepted == 0
                ? "No path had an unambiguous match — pick a suggestion or drop an object."
                : $"Proposed {accepted} retarget{(accepted == 1 ? "" : "s")}. Review, then Apply."));
        }

        // ---- applying --------------------------------------------------------------------------------------

        void RefreshFooter()
        {
            if (footer == null) return;

            List<AnimationPathRetargeting.PathEntry> changes = entries.Where(e => e.HasChange).ToList();
            int bindings = changes.Sum(e => e.Bindings.Count);
            int clips = changes.SelectMany(e => e.Clips).Distinct().Count();

            string[] collisions = AnimationPathRetargeting.Collisions(entries).ToArray();
            bool blocked = resolved != null &&
                           resolved.Clips.Any(c => AnimationPathRetargeting.EditabilityProblem(c) != null);

            summary.text = changes.Count == 0
                ? $"{entries.Count(e => e.IsBroken)} broken of {entries.Count} paths"
                : $"{changes.Count} path{(changes.Count == 1 ? "" : "s")} → " +
                  $"{bindings} binding{(bindings == 1 ? "" : "s")} in {clips} clip{(clips == 1 ? "" : "s")}" +
                  (collisions.Length > 0 ? $"  ·  {collisions.Length} would overwrite an existing binding" : "");

            summary.tooltip = collisions.Length > 0 ? string.Join("\n", collisions) : string.Empty;
            applyButton.SetEnabled(changes.Count > 0 && !blocked);
            applyButton.text = changes.Count > 0 ? $"Apply {changes.Count}" : "Apply";
        }

        void ApplyChanges()
        {
            string[] collisions = AnimationPathRetargeting.Collisions(entries).ToArray();
            if (collisions.Length > 0 &&
                !EditorUtility.DisplayDialog("Retarget Animation Paths",
                    "Some rewrites land on a binding that already exists and will replace it:\n\n" +
                    string.Join("\n", collisions.Take(10)) +
                    (collisions.Length > 10 ? $"\n… and {collisions.Length - 10} more." : ""),
                    "Replace", "Cancel"))
                return;

            int moved;
            applying = true;
            try { moved = AnimationPathRetargeting.Apply(entries); }
            finally { applying = false; }

            Rescan();
            ShowNotification(new GUIContent($"Retargeted {moved} binding{(moved == 1 ? "" : "s")}."));
        }
    }
}
