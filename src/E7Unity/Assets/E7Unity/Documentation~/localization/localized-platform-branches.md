# LocalizedPlatformBranches

See and edit a string's per-platform variants from the game object that shows it.

Unity Localization can point one table entry at a different entry per platform, using `PlatformOverride` metadata. The
feature works well, but the branching is only visible in the Localization Tables window — from the game object, a
string that says something different on Android looks exactly like one that does not.

Add this component beside a `Localize String Event` and its inspector shows the root entry and every platform branch
together, each locale editable in place.

## What it shows

For every `Localize String Event` on the same game object:

- the **root** entry — the one the component points at, which holds the metadata
- one section per platform that branches, with the entry it resolves to
- each locale's value under each, editable directly
- whether the root value is ever shown at runtime, which it is not once every platform has a branch

Branches are read from the `PlatformOverride` metadata, never inferred from key names, so an entry deliberately
pointed somewhere unconventional still displays the truth.

Beside the key is Localization's own open-in-tables icon. It opens the Localization Tables window searching for the
key with its suffix removed, so the root and every platform branch land in the view together — which is what the
shared naming buys you. Localization's built-in shortcut searches the entry's id instead, showing one row.

## Creating branches

A string with no branching yet offers **Add Platform Branching**. That creates the metadata, creates a branch entry
for each configured platform in every locale, and seeds each one with the root's current text so you start from the
shared wording and edit only what differs.

A single missing platform can be filled in the same way with its **Create** button.

Entries are named from [the naming convention](#naming-convention). Nothing is renamed behind your back — the
inspector reports a key that disagrees with the convention and offers to rename it, but never insists.

## Naming convention

Metadata is invisible outside the Tables window, so the keys themselves are worth naming systematically: a root
`Title/PlayGeneric` branching to `Title/PlayiOS` and `Title/PlayAndroid` announces what it does everywhere a key
appears — a component inspector, a prefab's serialized data, a code reference.

The defaults are `Generic`, `iOS` and `Android`, and they apply with nothing configured. To change them, add
**E7Unity ▸ Platform Branch Naming** to the Metadata list on your Localization Settings asset, where you can set the
root suffix and the list of platforms with their suffixes.

The convention only generates names and reports mismatches. It never decides where a branch goes — that stays with
the metadata.

## Notes

- Only the `Entry` override type is editable here, since that is the one whose branches live in the same table
  collection. An override redirecting to another collection is reported and left to the Tables window.
- The component stores nothing. It is a place for the inspector to draw and a marker that the text here is not the
  same everywhere; deleting it removes the inspector, not the branching.
- Unlike the rest of this section, the component itself compiles whether or not Unity Localization is installed —
  only its inspector is gated. A `MonoBehaviour` is referenced from prefabs by script GUID, so one that vanished
  with the package would leave a missing script in every prefab using it.
- Edits go through `Undo`, and the tables they touch are marked dirty for the next save.
