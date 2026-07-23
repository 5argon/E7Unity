#if E7UNITY_LOCALIZATION
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.Localization.UI;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Metadata;
using UnityEngine.Localization.Settings;
using UnityEngine.Localization.Tables;

namespace E7.E7Unity.Localization
{
    /// <summary>
    /// Draws the per-platform branches of every <c>LocalizeStringEvent</c> sharing a game object with a
    /// <see cref="LocalizedPlatformBranches" />, and lets them be written, created and checked in place.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The branches are read from the <c>PlatformOverride</c> metadata on the shared table entry, never from the
    /// key names. <see cref="PlatformBranchNaming" /> is consulted only to name entries this inspector creates and
    /// to report keys that disagree with the convention, so an entry pointed somewhere deliberately unconventional
    /// still displays truthfully.
    /// </para>
    /// <para>
    /// Only the <c>Entry</c> override type is editable here, since that is the one whose branches live in the same
    /// table collection. An override redirecting to another collection is reported and left to the Localization
    /// Tables window.
    /// </para>
    /// </remarks>
    [CustomEditor(typeof(LocalizedPlatformBranches))]
    class LocalizedPlatformBranchesEditor : UnityEditor.Editor
    {
        readonly Dictionary<string, bool> _folded = new Dictionary<string, bool>();

        // Localization keeps its EditorIcons class internal, so the icon it puts beside a Localized String's table
        // dropdown is loaded by asset path instead; FindTexture picks the light/dark and @2x variant.
        const string TableWindowIconPath =
            "Packages/com.unity.localization/Editor/Icons/Localization Tables Window/LocalizationTablesWindow.png";

        static Texture2D s_TableWindowIcon;
        static bool s_TableWindowIconLoaded;
        static GUIStyle s_LocaleFoldout;

        static Texture2D TableWindowIcon()
        {
            if (!s_TableWindowIconLoaded)
            {
                s_TableWindowIconLoaded = true;
                s_TableWindowIcon = EditorGUIUtility.FindTexture(TableWindowIconPath);
            }
            return s_TableWindowIcon;
        }

        // Localization's own shortcut opens the window with the search box holding the entry's id, which shows that
        // one row. Searching the stem instead — the key with any suffix removed — brings the root and every platform
        // branch into one view, which is the whole point of naming them as a family.
        static void OpenInTables(TableReference table, TableEntryReference entry, string search)
        {
            LocalizationTablesWindow.ShowWindow(table, entry);

            if (!ApplySearch(search))
            {
                EditorApplication.delayCall += () => ApplySearch(search);
            }
        }

        // The search field is internal, so it is reached by reflection once the window has selected the collection.
        // Assigning its value raises the change event Localization listens to, which filters the table itself.
        static bool ApplySearch(string search)
        {
            if (!EditorWindow.HasOpenInstances<LocalizationTablesWindow>()) return false;

            var window = EditorWindow.GetWindow<LocalizationTablesWindow>(false, null, false);
            FieldInfo field = typeof(LocalizationTablesWindow).GetField(
                "m_ToolbarSearchField", BindingFlags.Instance | BindingFlags.NonPublic);

            if (window == null || !(field?.GetValue(window) is ToolbarSearchField searchField)) return false;

            searchField.value = search;
            return true;
        }

        // The locale row mirrors the one Unity draws for a Localized String: a bold foldout in the label column
        // with the value previewed, read-only and clipped, beside it. The editor for that value appears underneath
        // only once the row is opened, so a branch with several locales stays one line each until it is worked on.
        static bool DrawLocaleHeader(Locale locale, string value, bool expanded)
        {
            if (s_LocaleFoldout == null)
            {
                s_LocaleFoldout = new GUIStyle(EditorStyles.foldout)
                {
                    fontStyle = FontStyle.Bold,
                    clipping = TextClipping.Clip,
                };
            }

            Rect indented = EditorGUI.IndentedRect(EditorGUILayout.GetControlRect());
            float labelWidth = Mathf.Max(60f, EditorGUIUtility.labelWidth - EditorGUI.indentLevel * 15f);
            var foldoutRect = new Rect(indented.x, indented.y, labelWidth, indented.height);
            var previewRect = new Rect(indented.x + labelWidth, indented.y,
                Mathf.Max(0f, indented.xMax - indented.x - labelWidth), indented.height);

            // The rects already carry the indent, so drawing must not apply it a second time.
            int indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            bool result = EditorGUI.Foldout(foldoutRect, expanded, locale.Identifier.ToString(), true, s_LocaleFoldout);
            EditorGUI.LabelField(previewRect, Preview(value));
            EditorGUI.indentLevel = indent;

            return result;
        }

        static string Preview(string value)
            => string.IsNullOrEmpty(value) ? string.Empty : value.Replace('\r', ' ').Replace('\n', ' ');

        public override void OnInspectorGUI()
        {
            var behaviour = (LocalizedPlatformBranches)target;
            LocalizeStringEvent[] components = behaviour.GetComponents<LocalizeStringEvent>();

            if (components.Length == 0)
            {
                EditorGUILayout.HelpBox(
                    "Add this beside a Localize String Event. It shows how that string branches per platform.",
                    MessageType.Info);
                return;
            }

            PlatformBranchNaming naming = ResolveNaming();
            for (int i = 0; i < components.Length; i++)
            {
                if (i > 0) EditorGUILayout.Space();
                DrawComponent(components[i], naming);
            }
        }

        static PlatformBranchNaming ResolveNaming()
        {
            LocalizationSettings settings = LocalizationEditorSettings.ActiveLocalizationSettings;
            PlatformBranchNaming configured = settings != null
                ? settings.GetMetadata().GetMetadata<PlatformBranchNaming>()
                : null;
            return configured ?? new PlatformBranchNaming();
        }

        void DrawComponent(LocalizeStringEvent component, PlatformBranchNaming naming)
        {
            LocalizedString reference = component.StringReference;
            if (reference == null || reference.IsEmpty)
            {
                EditorGUILayout.HelpBox("The Localize String Event has no entry selected.", MessageType.Info);
                return;
            }

            StringTableCollection collection = LocalizationEditorSettings.GetStringTableCollection(reference.TableReference);
            SharedTableData shared = collection != null ? collection.SharedData : null;
            SharedTableData.SharedTableEntry root = shared != null
                ? shared.GetEntryFromReference(reference.TableEntryReference)
                : null;

            if (root == null)
            {
                EditorGUILayout.HelpBox("Could not resolve the entry this component points at.", MessageType.Warning);
                return;
            }

            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField(root.Key, EditorStyles.boldLabel);

                Texture2D icon = TableWindowIcon();
                var open = new GUIContent(icon, "Open the Localization Tables window filtered to this entry and " +
                    "every platform branch beside it.");
                if (icon == null) open = new GUIContent("Tables", open.tooltip);

                if (GUILayout.Button(open, EditorStyles.iconButton, GUILayout.Width(22), GUILayout.Height(18)))
                {
                    OpenInTables(reference.TableReference, root.Id, naming.Stem(root.Key));
                }
            }

            var platformOverride = root.Metadata.GetMetadata<PlatformOverride>();
            if (platformOverride == null)
            {
                DrawWithoutBranching(collection, shared, root, naming);
                return;
            }

            if (!naming.IsRootKey(root.Key))
            {
                EditorGUILayout.HelpBox(
                    $"\"{root.Key}\" branches per platform but is not named as a branch root. " +
                    $"A root is expected to end in \"{naming.RootSuffix}\".", MessageType.Warning);
                if (GUILayout.Button($"Rename to \"{naming.Stem(root.Key) + naming.RootSuffix}\""))
                {
                    RenameRoot(component, shared, root, naming.Stem(root.Key) + naming.RootSuffix);
                    return;
                }
            }

            IEnumerable<Locale> locales = LocalizationEditorSettings.GetLocales();
            int branched = 0;

            DrawBranch(collection, locales, "Root", root.Key);

            foreach (PlatformBranchNaming.PlatformSuffix platform in naming.Platforms)
            {
                EntryOverrideType type = platformOverride.GetOverride(
                    out TableReference _, out TableEntryReference entryReference, platform.Platform);

                if (type == EntryOverrideType.None)
                {
                    DrawMissingBranch(collection, shared, root, platformOverride, naming, platform);
                    continue;
                }

                branched++;

                if (type != EntryOverrideType.Entry)
                {
                    EditorGUILayout.HelpBox(
                        $"{platform.Platform} redirects to another table collection. Edit it in the Localization " +
                        "Tables window.", MessageType.Info);
                    continue;
                }

                SharedTableData.SharedTableEntry branchEntry = shared.GetEntryFromReference(entryReference);
                if (branchEntry == null)
                {
                    EditorGUILayout.HelpBox($"{platform.Platform} points at an entry that no longer exists.",
                        MessageType.Error);
                    continue;
                }

                string expected = naming.BranchKey(root.Key, platform.Platform);
                bool offConvention = expected != null && branchEntry.Key != expected;

                if (DrawBranch(collection, locales, platform.Platform.ToString(), branchEntry.Key,
                        offConvention ? $"expected \"{expected}\"" : null,
                        offConvention ? $"Rename to \"{expected}\"" : null))
                {
                    RenameBranch(shared, platformOverride, platform.Platform, branchEntry, expected);
                    return;
                }
            }

            EditorGUILayout.HelpBox(branched == naming.Platforms.Count
                ? "Every listed platform branches, so the root value is never shown at runtime."
                : "The root value is shown on any platform without a branch.", MessageType.None);
        }

        void DrawWithoutBranching(StringTableCollection collection, SharedTableData shared,
            SharedTableData.SharedTableEntry root, PlatformBranchNaming naming)
        {
            EditorGUILayout.HelpBox("This string does not branch per platform yet.", MessageType.Info);

            if (!GUILayout.Button("Add Platform Branching")) return;

            Undo.RegisterCompleteObjectUndo(shared, "Add platform branching");

            var platformOverride = new PlatformOverride();
            foreach (PlatformBranchNaming.PlatformSuffix platform in naming.Platforms)
            {
                AddBranch(collection, shared, root, platformOverride, naming, platform);
            }

            root.Metadata.AddMetadata(platformOverride);
            EditorUtility.SetDirty(shared);
        }

        void DrawMissingBranch(StringTableCollection collection, SharedTableData shared,
            SharedTableData.SharedTableEntry root, PlatformOverride platformOverride,
            PlatformBranchNaming naming, PlatformBranchNaming.PlatformSuffix platform)
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                EditorGUILayout.LabelField($"{platform.Platform} — no branch", EditorStyles.miniLabel);
                if (GUILayout.Button("Create", GUILayout.Width(60)))
                {
                    Undo.RegisterCompleteObjectUndo(shared, "Create platform branch");
                    AddBranch(collection, shared, root, platformOverride, naming, platform);
                    EditorUtility.SetDirty(shared);
                }
            }
        }

        // Creates the branch entry in every locale, seeded from the root so editing starts from the shared wording,
        // then records it in the metadata that actually decides where the branch goes.
        static void AddBranch(StringTableCollection collection, SharedTableData shared,
            SharedTableData.SharedTableEntry root, PlatformOverride platformOverride,
            PlatformBranchNaming naming, PlatformBranchNaming.PlatformSuffix platform)
        {
            string branchKey = naming.BranchKey(root.Key, platform.Platform);
            if (string.IsNullOrEmpty(branchKey)) return;

            foreach (Locale locale in LocalizationEditorSettings.GetLocales())
            {
                if (!(collection.GetTable(locale.Identifier) is StringTable table)) continue;

                StringTableEntry existing = table.GetEntryFromReference(branchKey);
                if (existing != null) continue;

                StringTableEntry seed = table.GetEntryFromReference(root.Key);
                Undo.RegisterCompleteObjectUndo(table, "Create platform branch");
                table.AddEntry(branchKey, seed != null ? seed.Value : string.Empty);
                EditorUtility.SetDirty(table);
            }

            // Record the branch the same way the project records its other references. An id keeps the override
            // pointing at the right entry even if that entry is later renamed.
            SharedTableData.SharedTableEntry created = shared.GetEntryFromReference(branchKey);
            if (created != null && LocalizationEditorSettings.EntryReferenceMethod == EntryReferenceMethod.Id)
            {
                platformOverride.AddPlatformEntryOverride(platform.Platform, created.Id);
            }
            else
            {
                platformOverride.AddPlatformEntryOverride(platform.Platform, branchKey);
            }

            EditorUtility.SetDirty(shared);
        }

        /// <summary>Returns true when the offered fix was pressed, so the caller can act and stop drawing.</summary>
        bool DrawBranch(StringTableCollection collection, IEnumerable<Locale> locales, string title, string key,
            string note = null, string fixLabel = null)
        {
            string id = collection.TableCollectionName + "|" + key;
            bool open = !_folded.TryGetValue(id, out bool stored) || stored;
            open = EditorGUILayout.Foldout(open, $"{title}  ·  {key}", true);
            _folded[id] = open;

            if (!open) return false;

            bool fixPressed = false;

            EditorGUI.indentLevel++;
            if (note != null)
            {
                EditorGUILayout.HelpBox($"Named off-convention — {note}.", MessageType.Warning);
                if (fixLabel != null) fixPressed = GUILayout.Button(fixLabel);
            }

            foreach (Locale locale in locales)
            {
                if (!(collection.GetTable(locale.Identifier) is StringTable table)) continue;

                StringTableEntry entry = table.GetEntryFromReference(key);
                string current = entry != null ? entry.Value : string.Empty;

                string localeId = id + "|" + locale.Identifier.Code;
                bool localeOpen = _folded.TryGetValue(localeId, out bool localeStored) && localeStored;
                localeOpen = DrawLocaleHeader(locale, current, localeOpen);
                _folded[localeId] = localeOpen;

                if (!localeOpen) continue;

                EditorGUI.indentLevel++;
                EditorGUI.BeginChangeCheck();
                string edited = EditorGUILayout.DelayedTextField(current);
                bool committed = EditorGUI.EndChangeCheck();
                EditorGUI.indentLevel--;

                if (!committed) continue;

                Undo.RegisterCompleteObjectUndo(table, "Edit localized value");
                if (entry != null)
                {
                    entry.Value = edited;
                }
                else
                {
                    table.AddEntry(key, edited);
                }
                EditorUtility.SetDirty(table);
            }
            EditorGUI.indentLevel--;
            return fixPressed;
        }

        // The branch entry is pointed at by the override, so a rename has to re-record it. Re-recording also
        // upgrades a reference held by name to one held by id, which the next rename would not disturb.
        void RenameBranch(SharedTableData shared, PlatformOverride platformOverride, RuntimePlatform platform,
            SharedTableData.SharedTableEntry branchEntry, string newKey)
        {
            if (shared.Contains(newKey))
            {
                Debug.LogError($"[LocalizedPlatformBranches] \"{newKey}\" already exists in " +
                    $"{shared.TableCollectionName}. Rename or remove it first.");
                return;
            }

            Undo.RegisterCompleteObjectUndo(shared, "Rename platform branch");
            shared.RenameKey(branchEntry.Key, newKey);

            if (LocalizationEditorSettings.EntryReferenceMethod == EntryReferenceMethod.Id)
            {
                platformOverride.AddPlatformEntryOverride(platform, branchEntry.Id);
            }
            else
            {
                platformOverride.AddPlatformEntryOverride(platform, newKey);
            }

            EditorUtility.SetDirty(shared);
            Repaint();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }

        // Renaming a key keeps the entry's id, so references made by id are unaffected. A reference made by name
        // holds the old spelling and would be left pointing at nothing, so the one on the component being edited is
        // rewritten here. Unity's own rename paths do not do this, and cannot reach names held anywhere else.
        void RenameRoot(LocalizeStringEvent component, SharedTableData shared,
            SharedTableData.SharedTableEntry root, string newKey)
        {
            string oldKey = root.Key;

            // Renaming does not check for collisions the way adding a key does, and two entries sharing a name
            // makes every lookup by that name ambiguous.
            if (shared.Contains(newKey))
            {
                Debug.LogError($"[LocalizedPlatformBranches] \"{newKey}\" already exists in " +
                    $"{shared.TableCollectionName}. Rename or remove it first.");
                return;
            }

            Undo.RegisterCompleteObjectUndo(shared, "Rename branch root");
            shared.RenameKey(oldKey, newKey);
            EditorUtility.SetDirty(shared);

            var serialized = new SerializedObject(component);
            SerializedProperty byName = serialized.FindProperty("m_StringReference.m_TableEntryReference.m_Key");
            SerializedProperty byId = serialized.FindProperty("m_StringReference.m_TableEntryReference.m_KeyId");

            if (byName != null && byName.stringValue == oldKey)
            {
                byName.stringValue = newKey;

                // An id alongside the name takes precedence when resolving, and unlike the name it survives the
                // next rename.
                if (byId != null && byId.longValue == 0) byId.longValue = root.Id;

                serialized.ApplyModifiedProperties();

                Debug.LogWarning($"[LocalizedPlatformBranches] Renamed \"{oldKey}\" to \"{newKey}\". This component " +
                    "referenced it by name, so the reference was repaired — but any other prefab, scene or script " +
                    "referencing that key by name still needs updating. References made by id are unaffected.");
            }

            Repaint();
            UnityEditorInternal.InternalEditorUtility.RepaintAllViews();
        }
    }
}
#endif
