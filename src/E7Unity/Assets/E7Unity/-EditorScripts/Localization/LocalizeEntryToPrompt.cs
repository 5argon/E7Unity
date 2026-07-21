#if E7UNITY_LOCALIZATION
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.Localization;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.Localization.Tables;

namespace E7.E7Unity.Localization
{
    /// <summary>
    /// Adds three clipboard commands to the context menu (the ⋮ menu) of every
    /// <c>LocalizeStringEvent</c> component, turning localized text into a ready-to-paste prompt
    /// for an external large language model.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <b>Copy Translation Filling Prompt</b> asks the model to fill in the languages that are
    /// still missing for the clicked entry, using the languages that already have a value as
    /// trusted references. It refuses to run when the entry has no value in any language, because
    /// the prompt is built around leaving the existing translations untouched — with nothing to
    /// reference, there is nothing to build on. It also invites the model to flag outright
    /// grammatical mistakes in the existing values, but only ones it is completely certain about.
    /// </para>
    /// <para>
    /// <b>Copy Translation Review Prompt</b> asks the model to review the clicked entry's existing
    /// translations — against each other and against the surrounding UI — and give per-language
    /// feedback with candidate rewordings.
    /// </para>
    /// <para>
    /// <b>Copy Translation Review Prompt (Entire Hierarchy)</b> ignores which component was
    /// clicked and instead sweeps every localized text in the whole active scene — or just the
    /// prefab being edited when in Prefab Mode — into one prompt. The values are laid out as a
    /// GameObject-by-language matrix so the model can judge the whole screen's wording together,
    /// and its instructions steer toward a high-level, screen-wide review rather than the
    /// word-by-word alternatives the single-entry review asks for.
    /// </para>
    /// <para>
    /// The whole tool is compiled only when the Unity Localization package
    /// (<c>com.unity.localization</c>) is present, gated by the <c>E7UNITY_LOCALIZATION</c>
    /// define. Projects that pull in E7Unity without Localization simply never see these menu
    /// items.
    /// </para>
    /// </remarks>
    public static class LocalizeEntryToPrompt
    {
        const string FillingMenu = "CONTEXT/LocalizeStringEvent/Copy Translation Filling Prompt";
        const string ReviewMenu = "CONTEXT/LocalizeStringEvent/Copy Translation Review Prompt";
        const string ReviewAllMenu = "CONTEXT/LocalizeStringEvent/Copy Translation Review Prompt (Entire Hierarchy)";

        [MenuItem(FillingMenu, true)]
        static bool ValidateFilling(MenuCommand command) => HasEntry(command);

        [MenuItem(ReviewMenu, true)]
        static bool ValidateReview(MenuCommand command) => HasEntry(command);

        [MenuItem(ReviewAllMenu, true)]
        static bool ValidateReviewAll(MenuCommand command) => command.context is LocalizeStringEvent;

        /// <summary>The single-entry commands need a component that actually points at a table entry.</summary>
        static bool HasEntry(MenuCommand command)
            => command.context is LocalizeStringEvent lse
               && lse.StringReference != null
               && !lse.StringReference.IsEmpty;

        [MenuItem(FillingMenu)]
        static void CopyFillingPrompt(MenuCommand command)
        {
            if (!(command.context is LocalizeStringEvent lse)) return;
            var values = ResolveEntry(lse);
            if (values == null) return;

            if (values.Present.Count == 0)
            {
                Debug.LogWarning(
                    $"[LocalizeEntryToPrompt] \"{values.EntryKey}\" has no translation in any language yet. " +
                    "The filling prompt relies on at least one existing translation as a trusted reference — " +
                    "fill one language in first, or use \"Copy Translation Review Prompt\" instead.", lse);
                return;
            }

            if (values.Missing.Count == 0)
            {
                Debug.LogWarning(
                    $"[LocalizeEntryToPrompt] \"{values.EntryKey}\" already has a value in every configured " +
                    "language — nothing to fill. Use \"Copy Translation Review Prompt\" to review them instead.", lse);
                return;
            }

            EditorGUIUtility.systemCopyBuffer = BuildFillingPrompt(values, BuildHierarchy(GetRoots(lse.transform), new[] { lse.transform }));
            Debug.Log(
                $"[LocalizeEntryToPrompt] Copied a filling prompt for \"{values.EntryKey}\" " +
                $"({values.Present.Count} reference language(s), {values.Missing.Count} to fill) to the clipboard.", lse);
        }

        [MenuItem(ReviewMenu)]
        static void CopyReviewPrompt(MenuCommand command)
        {
            if (!(command.context is LocalizeStringEvent lse)) return;
            var values = ResolveEntry(lse);
            if (values == null) return;

            if (values.Present.Count == 0)
            {
                Debug.LogWarning(
                    $"[LocalizeEntryToPrompt] \"{values.EntryKey}\" has no translations to review yet.", lse);
                return;
            }

            EditorGUIUtility.systemCopyBuffer = BuildReviewPrompt(values, BuildHierarchy(GetRoots(lse.transform), new[] { lse.transform }));
            Debug.Log(
                $"[LocalizeEntryToPrompt] Copied a review prompt for \"{values.EntryKey}\" " +
                $"({values.Present.Count} language(s)) to the clipboard.", lse);
        }

        [MenuItem(ReviewAllMenu)]
        static void CopyReviewAllPrompt(MenuCommand command)
        {
            if (!(command.context is LocalizeStringEvent clicked)) return;

            var roots = GetScopeRoots(clicked, out var modeLabel);

            var components = new List<LocalizeStringEvent>();
            foreach (var root in roots)
                components.AddRange(root.GetComponentsInChildren<LocalizeStringEvent>(true));
            var withEntry = components
                .Where(c => c.StringReference != null && !c.StringReference.IsEmpty)
                .ToList();

            if (withEntry.Count == 0)
            {
                Debug.LogWarning(
                    $"[LocalizeEntryToPrompt] Found no localized text with a table entry in {modeLabel}.", clicked);
                return;
            }

            // Mark every component's GameObject in the tree, but give each distinct entry one row.
            var targets = new HashSet<Transform>(withEntry.Select(c => c.transform));
            var rows = new List<ScreenEntry>();
            var seen = new HashSet<string>();
            foreach (var component in withEntry)
            {
                var values = ResolveEntry(component);
                if (values == null) continue;
                if (seen.Add(values.CollectionName + " " + values.EntryKey))
                    rows.Add(new ScreenEntry(component.gameObject.name, values));
            }

            if (rows.Count == 0)
            {
                Debug.LogWarning(
                    $"[LocalizeEntryToPrompt] Could not resolve any table entry in {modeLabel}.", clicked);
                return;
            }

            EditorGUIUtility.systemCopyBuffer = BuildReviewAllPrompt(rows, BuildHierarchy(roots, targets), modeLabel);
            Debug.Log(
                $"[LocalizeEntryToPrompt] Reviewed {modeLabel} — copied an entire-hierarchy review prompt " +
                $"({rows.Count} distinct entr{(rows.Count == 1 ? "y" : "ies")} across {withEntry.Count} " +
                $"component{(withEntry.Count == 1 ? "" : "s")}) to the clipboard.", clicked);
        }

        /// <summary>
        /// Resolves the collection behind the component and splits every project locale into the
        /// ones that already have a value and the ones that do not. Returns <c>null</c> (after
        /// logging) when the collection cannot be found.
        /// </summary>
        static EntryValues ResolveEntry(LocalizeStringEvent lse)
        {
            var reference = lse.StringReference;
            var collection = LocalizationEditorSettings.GetStringTableCollection(reference.TableReference);
            if (collection == null)
            {
                Debug.LogWarning(
                    $"[LocalizeEntryToPrompt] Could not find a String Table Collection for " +
                    $"\"{reference.TableReference}\". Is the table part of the project's Localization settings?", lse);
                return null;
            }

            var shared = collection.SharedData;
            var sharedEntry = shared != null ? shared.GetEntryFromReference(reference.TableEntryReference) : null;
            var entryKey = sharedEntry != null ? sharedEntry.Key : reference.TableEntryReference.ToString();

            var present = new List<LocaleValue>();
            var missing = new List<Locale>();
            foreach (var locale in LocalizationEditorSettings.GetLocales()
                         .Where(l => l != null)
                         .OrderBy(l => l.LocaleName))
            {
                var table = collection.GetTable(locale.Identifier) as StringTable;
                var entry = table != null ? table.GetEntryFromReference(reference.TableEntryReference) : null;
                var value = entry != null ? entry.Value : null;
                if (string.IsNullOrEmpty(value))
                    missing.Add(locale);
                else
                    present.Add(new LocaleValue(locale.LocaleName, locale.Identifier.Code, value));
            }

            return new EntryValues(collection.TableCollectionName, entryKey, present, missing);
        }

        static string BuildFillingPrompt(EntryValues values, string hierarchy)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are helping localize a Unity game. One UI text entry needs translations filled in for the languages that are still missing.");
            sb.AppendLine();
            AppendBranchHierarchy(sb, hierarchy, "translated");
            AppendEntry(sb, values);

            sb.AppendLine("### Existing translations (trusted references — do NOT change these)");
            sb.AppendLine();
            foreach (var p in values.Present)
                sb.AppendLine($"- {p.Name} ({p.Code}): \"{p.Value}\"");
            sb.AppendLine();

            sb.AppendLine("### Languages to fill");
            sb.AppendLine();
            foreach (var locale in values.Missing)
                sb.AppendLine($"- {locale.LocaleName} ({locale.Identifier.Code})");
            sb.AppendLine();

            sb.AppendLine("## What I need from you");
            sb.AppendLine();
            sb.AppendLine("Treat the existing translations above as trusted, high-accuracy references and leave them unchanged. Translate only the missing languages, keeping the same meaning, tone, register, and length as the references, and fit the UI role you infer from the hierarchy.");
            sb.AppendLine();
            sb.AppendLine("You do not have the full game's context, so where a word could reasonably be translated more than one way, present several candidates per language:");
            sb.AppendLine();
            sb.AppendLine("- List the candidates from most to least likely to be correct (descending certainty).");
            sb.AppendLine("- Give a short reason for each candidate — what it assumes about context, register, or length.");
            sb.AppendLine("- Note any UI constraints you inferred (for example, it is a button label, so keep it short).");
            sb.AppendLine();
            sb.AppendLine("Do not restyle or retranslate the existing translations — they are references, not something to rewrite. The one exception: if you are 100% certain an existing translation contains an outright grammatical mistake (a typo, a wrong conjugation, a broken particle, a clear agreement error), note it separately as a minor correction. Anything less than certain, leave it alone — no substantive or stylistic changes.");
            sb.AppendLine();
            sb.AppendLine("If the existing translations disagree with each other in tone or length, point that out.");
            return sb.ToString().TrimEnd() + "\n";
        }

        static string BuildReviewPrompt(EntryValues values, string hierarchy)
        {
            var sb = new StringBuilder();
            sb.AppendLine("You are helping localize a Unity game. One UI text entry already has translations in several languages. Your job is to REVIEW the existing translations — not to add new languages.");
            sb.AppendLine();
            AppendBranchHierarchy(sb, hierarchy, "reviewed");
            AppendEntry(sb, values);

            sb.AppendLine("### Existing translations to review");
            sb.AppendLine();
            foreach (var p in values.Present)
                sb.AppendLine($"- {p.Name} ({p.Code}): \"{p.Value}\"");
            sb.AppendLine();

            if (values.Missing.Count > 0)
            {
                sb.AppendLine("### Not yet translated (for your awareness — do not translate these here)");
                sb.AppendLine();
                foreach (var locale in values.Missing)
                    sb.AppendLine($"- {locale.LocaleName} ({locale.Identifier.Code})");
                sb.AppendLine();
            }

            sb.AppendLine("## What I need from you");
            sb.AppendLine();
            sb.AppendLine("Review each existing translation:");
            sb.AppendLine();
            sb.AppendLine("- Compare the translations against each other for consistency of meaning, tone, register, and length.");
            sb.AppendLine("- Check each one against the UI context you infer from the hierarchy (which screen, which section, whether it is a button label, whether it must stay short).");
            sb.AppendLine("- For every language, give specific feedback: is it accurate, natural, and appropriately concise, and does it match the shared meaning and the other languages?");
            sb.AppendLine("- Flag mistranslations, inconsistencies, wrong register, or length problems, and suggest a better alternative where you find one (and say why).");
            sb.AppendLine("- If a translation is good, say so briefly.");
            sb.AppendLine();
            sb.AppendLine("You do not have the full game's context, so state any assumption your feedback depends on, and where more than one reading is plausible, mention the options. Order the languages from the ones you are most confident have an issue to the ones you are least concerned about.");
            return sb.ToString().TrimEnd() + "\n";
        }

        static string BuildReviewAllPrompt(List<ScreenEntry> rows, string hierarchy, string modeLabel)
        {
            var locales = LocalizationEditorSettings.GetLocales()
                .Where(l => l != null)
                .OrderBy(l => l.LocaleName)
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine($"You are helping localize a Unity game. Below is an ENTIRE screen's worth of localized text — every localized entry in {modeLabel} — with all of its languages. Your job is a holistic, top-down review of the whole set together, not a deep dive on any single word.");
            sb.AppendLine();

            sb.AppendLine("## The screen (UI hierarchy)");
            sb.AppendLine();
            sb.AppendLine("This is the hierarchy of the whole screen. Every branch that leads to a localized text is expanded; branches with no localized text are collapsed and marked with `(…)`. Read the object names for the on-screen role of each text.");
            sb.AppendLine();
            AppendTree(sb, hierarchy);
            sb.AppendLine("Legend: `[[ ]]` marks every GameObject that owns a localized text.");
            sb.AppendLine();

            sb.AppendLine("## The translation matrix");
            sb.AppendLine();
            sb.AppendLine("One row per localized text (labelled by its GameObject), one column per language. Read it as a matrix: down each language column the wording should agree across rows on shared terms, tone, casing, and formality; across each row the languages should agree in meaning; and ideally every cell is filled. An empty cell is a language with no translation yet.");
            sb.AppendLine();
            AppendMatrix(sb, rows, locales);

            sb.AppendLine("## What I need from you");
            sb.AppendLine();
            sb.AppendLine("Review the whole screen from a top-down view — how well all of this text works together, not word-by-word perfection:");
            sb.AppendLine();
            sb.AppendLine("- Read down each language column: do the entries agree with each other on shared terms, product and feature names, button verbs, tone, capitalization, and formality?");
            sb.AppendLine("- Read across each row: do the languages agree in meaning and register, and which cells are missing?");
            sb.AppendLine("- Call out rows that clash with the rest — a different wording for the same concept, inconsistent punctuation or casing, mixed politeness levels.");
            sb.AppendLine("- Note anything obviously wrong or conspicuously missing at the screen level.");
            sb.AppendLine();
            sb.AppendLine("Keep it high level. Do NOT drill into multiple candidate translations per word or long per-word reasoning — that is what the single-entry review is for. Give screen-wide observations, name specific GameObjects only when they break consistency, and finish with a short prioritized list of the most important fixes. State any assumption you have to make where the context is unclear.");
            return sb.ToString().TrimEnd() + "\n";
        }

        static void AppendMatrix(StringBuilder sb, List<ScreenEntry> rows, List<Locale> locales)
        {
            sb.Append("| GameObject |");
            foreach (var locale in locales)
                sb.Append(' ').Append(EscapeCell($"{locale.LocaleName} ({locale.Identifier.Code})")).Append(" |");
            sb.Append('\n');

            sb.Append("| --- |");
            foreach (var _ in locales)
                sb.Append(" --- |");
            sb.Append('\n');

            foreach (var row in rows)
            {
                var byCode = new Dictionary<string, string>();
                foreach (var p in row.Values.Present)
                    byCode[p.Code] = p.Value;

                sb.Append("| ").Append(EscapeCell(row.GameObjectName)).Append(" |");
                foreach (var locale in locales)
                {
                    var cell = byCode.TryGetValue(locale.Identifier.Code, out var value)
                        ? $"\"{EscapeCell(value)}\""
                        : "";
                    sb.Append(' ').Append(cell).Append(" |");
                }
                sb.Append('\n');
            }
            sb.Append('\n');
        }

        static void AppendBranchHierarchy(StringBuilder sb, string hierarchy, string verb)
        {
            sb.AppendLine("## Where this text appears (UI hierarchy)");
            sb.AppendLine();
            sb.AppendLine("This is the GameObject hierarchy of the screen the text belongs to. Only the branch leading to the text is expanded; unrelated branches are collapsed and marked with `(…)`. The GameObject names are the main context you have, so read them for meaning.");
            sb.AppendLine();
            AppendTree(sb, hierarchy);
            sb.AppendLine($"Legend: `[[ ]]` marks the GameObject whose text is being {verb}.");
            sb.AppendLine();
        }

        static void AppendEntry(StringBuilder sb, EntryValues values)
        {
            sb.AppendLine("## The text entry");
            sb.AppendLine();
            sb.AppendLine($"- Table collection: \"{values.CollectionName}\"");
            sb.AppendLine($"- Entry key: \"{values.EntryKey}\"");
            sb.AppendLine();
        }

        static void AppendTree(StringBuilder sb, string hierarchy)
        {
            sb.AppendLine("```");
            sb.AppendLine(hierarchy);
            sb.AppendLine("```");
            sb.AppendLine();
        }

        /// <summary>Escapes a value so it survives inside a single Markdown table cell.</summary>
        static string EscapeCell(string s)
            => (s ?? "").Replace("|", "\\|").Replace("\r", " ").Replace("\n", " ");

        /// <summary>
        /// The top-level objects to start a single-entry tree from: the prefab root while editing
        /// a prefab, every root object of the scene for a scene instance, or the transform's own
        /// root when the component lives on a prefab asset that is not open in a stage.
        /// </summary>
        static IEnumerable<Transform> GetRoots(Transform target)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.IsPartOfPrefabContents(target.gameObject))
                return new[] { stage.prefabContentsRoot.transform };

            var scene = target.gameObject.scene;
            if (scene.IsValid())
                return scene.GetRootGameObjects().Select(g => g.transform);

            return new[] { target.root };
        }

        /// <summary>
        /// The roots that the entire-hierarchy review sweeps: the single prefab root when in
        /// Prefab Mode, otherwise every root of the scene the clicked component lives in.
        /// <paramref name="modeLabel"/> reports which was chosen, for the log and the prompt.
        /// </summary>
        static Transform[] GetScopeRoots(Component clicked, out string modeLabel)
        {
            var stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null && stage.IsPartOfPrefabContents(clicked.gameObject))
            {
                modeLabel = $"Prefab \"{stage.prefabContentsRoot.name}\"";
                return new[] { stage.prefabContentsRoot.transform };
            }

            var scene = clicked.gameObject.scene;
            if (scene.IsValid())
            {
                modeLabel = $"Scene \"{scene.name}\"";
                return scene.GetRootGameObjects().Select(g => g.transform).ToArray();
            }

            var root = clicked.transform.root;
            modeLabel = $"Prefab asset \"{root.name}\"";
            return new[] { root };
        }

        /// <summary>
        /// Renders the hierarchy under <paramref name="roots"/>, expanding only the branches that
        /// reach a member of <paramref name="targets"/> and collapsing every other branch to a
        /// single <c>(…)</c> line. Each target GameObject is wrapped in <c>[[ ]]</c>.
        /// </summary>
        static string BuildHierarchy(IEnumerable<Transform> roots, ICollection<Transform> targets)
        {
            var path = new HashSet<Transform>();
            foreach (var target in targets)
                for (var t = target; t != null; t = t.parent)
                    path.Add(t);

            var sb = new StringBuilder();
            foreach (var root in roots)
                AppendNode(sb, root, 0, targets, path);
            return sb.ToString().TrimEnd();
        }

        static void AppendNode(StringBuilder sb, Transform node, int depth, ICollection<Transform> targets, HashSet<Transform> path)
        {
            var onPath = path.Contains(node);
            var name = targets.Contains(node) ? $"[[{node.name}]]" : node.name;
            if (!onPath && node.childCount > 0)
                name += "  (…)";

            sb.Append(' ', depth * 2).Append("- ").Append(name).Append('\n');

            if (onPath)
                for (var i = 0; i < node.childCount; i++)
                    AppendNode(sb, node.GetChild(i), depth + 1, targets, path);
        }

        /// <summary>One locale that has a non-empty value for an entry.</summary>
        readonly struct LocaleValue
        {
            public readonly string Name;
            public readonly string Code;
            public readonly string Value;

            public LocaleValue(string name, string code, string value)
            {
                Name = name;
                Code = code;
                Value = value;
            }
        }

        /// <summary>The per-locale state of one entry: which languages have a value and which do not.</summary>
        sealed class EntryValues
        {
            public readonly string CollectionName;
            public readonly string EntryKey;
            public readonly List<LocaleValue> Present;
            public readonly List<Locale> Missing;

            public EntryValues(string collectionName, string entryKey, List<LocaleValue> present, List<Locale> missing)
            {
                CollectionName = collectionName;
                EntryKey = entryKey;
                Present = present;
                Missing = missing;
            }
        }

        /// <summary>One row of the entire-hierarchy matrix: a GameObject and the entry it localizes.</summary>
        sealed class ScreenEntry
        {
            public readonly string GameObjectName;
            public readonly EntryValues Values;

            public ScreenEntry(string gameObjectName, EntryValues values)
            {
                GameObjectName = gameObjectName;
                Values = values;
            }
        }
    }
}
#endif
