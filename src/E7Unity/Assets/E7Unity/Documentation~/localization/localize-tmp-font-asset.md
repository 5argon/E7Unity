# LocalizeTmpFontAsset

Swap a TextMeshPro component's font asset per locale.

Put it on the same game object as a `TMP_Text`, then point it at an asset table entry that holds a `TMP_FontAsset` for
each language. The font then follows the active locale like any other localized asset.

## Why an asset table

Because the fonts live in a table, adding a language becomes a data-only change: create the locale, fill in that
entry's new column, and every label using the component follows. No code, no per-object rewiring.

The usual arrangement is one entry for body text and one for headings — `Font/Body` and `Font/Heading`, say — each
assigned to the base text prefabs. Every instance in the project inherits the behaviour from those prefabs.

Two locales that share a font do not need two copies of it; put the font in a shared group with a label per locale.

## Editing behaviour

The component is `[ExecuteAlways]`, so the correct font appears in the scene while you author rather than only in
play mode.

It registers the properties it writes with Localization's property driver before assigning them, so swapping the font
neither dirties the scene nor shows up as a prefab override. Two properties are registered rather than one, because
assigning `TMP_Text.font` internally runs `LoadFontAsset`, which also writes `m_sharedMaterial`.

It is `[DisallowMultipleComponent]`, and does nothing if the game object has no `TMP_Text`.

> [!TIP]
> Tick **Wait For Completion** on each font reference in the table. Without it the label renders a frame or two with
> the previous font before the asset finishes loading, which is very visible on a screen that is already showing text.

## Requirements

Compiled only when the Unity Localization package is installed, guarded by the `E7UNITY_LOCALIZATION` define. A project
without that package does not get the component at all.
