# Animation path retargeter

Repair the clips of an Animator or Animation after the objects they animate were renamed, reparented, or wrapped in
a new layer.

**Window:** `Window ▸ Animation ▸ Path Retargeter` · **Also on:** the context menu of any `Animator` or `Animation`
component

## What breaks

An `AnimationClip` does not hold a reference to the object it animates. It holds a **path**: a `/`-joined string of
game object names, relative to the game object carrying the `Animator` or `Animation` that plays it. Rename any
object along that path, move the object somewhere else, or insert a layout wrapper between the player and the leaf,
and every binding through it stops finding its target. The Animation window then draws the row in yellow with
`(Missing!)` after the property name, and the clip animates nothing.

Unity repairs none of this. There is no remap-on-rename anywhere in the editor. The only built-in remedy is to
click the property row's label until it turns into a text field and retype the whole path, one curve at a time, in
a field that appears in no documentation.

The reason a search-and-replace through the `.anim` YAML is not the shortcut it appears to be: a clip stores every
path **twice**. The string in `m_FloatCurves` and `m_EditorCurves` is what the Animation window reads, but
`m_ClipBindingConstant.genericBindings[].path` holds a **CRC32 of that same string**, and that hash is what Mecanim
binds by in a player. Rewrite the strings alone and you get a clip that looks repaired in the editor and is dead at
runtime. Going through `AnimationUtility`, as this window does, keeps the two in step.

## Using it

1. Open the prefab whose animation is broken, or select its instance in a scene.
2. Open `Window ▸ Animation ▸ Path Retargeter`. With **Follow Selection** on it picks up whatever is selected;
   otherwise drop the Animator, the Animation, an Animator Controller, or a single clip into the field.
3. Broken paths are listed first. Each row is one path, with every binding that shares it across every clip the
   player can reach — one rename usually breaks the same path in all of them, so one repair fixes all of them.
4. Give the row its new target, by any of:
   - **dragging a game object onto the row**, from the Hierarchy;
   - picking one through the row's object field;
   - clicking one of the **suggestions**;
   - clicking **Auto-Fix**, which accepts every suggestion that is unambiguous.
5. Review the arrows, then **Apply**.

Nothing is written until Apply, and the whole rewrite is one undo step.

Paths are never typed. Whatever object arrives on a row is converted with
`AnimationUtility.CalculateTransformPath` against the player's own game object, so the string is correct by
construction. An object outside the player's hierarchy is refused rather than written as a path that cannot resolve.

## Reading a row

The pill on the left says what the scan found:

| Pill | Meaning |
| --- | --- |
| `RESOLVES` | Every binding on this path finds its object. Hidden unless **Show Resolved** is on. |
| `MISSING OBJECT` | No transform at this path at all — the usual result of a rename or a reparent. |
| `MISSING PROPERTY ×n` | The transform exists, but `n` bindings want a component it no longer carries. |
| `NO HIERARCHY` | A controller or clip was supplied on its own, so there is nothing to check against. Assign the Animator that plays it. |

`NO HIERARCHY` is the one state where the window cannot help much: a controller asset has no hierarchy of its own,
and the same controller may well be played by several prefabs whose hierarchies disagree. Retargeting is only
meaningful against one of them, which is why the window is driven by a player rather than by a controller.

## How suggestions are ranked

A candidate is scored first on whether it carries **every property the row's bindings animate** — checked by
rebinding each one against the candidate's path and seeing whether it resolves, the same test behind the Animation
window's `(Missing!)`. That outweighs any name similarity, because a same-named object that lost its component is
exactly the trap this tool exists to avoid. Name and path-tail matching only break ties between candidates that
would all work.

**Auto-Fix** is deliberately more cautious than the ranking. It accepts a suggestion only when the choice is not a
judgement call: either exactly one candidate in the whole hierarchy carries every animated property, or — when
several do, which is common among siblings of the same kind — exactly one of those also still has the **same name**
as the last segment of the broken path. That second rule is what makes the ordinary rename-and-reparent a single
click, while a genuine ambiguity is left for you to resolve from the suggestion chips.

## What it will not do

- **Clips imported from a model file** cannot be rewritten this way; retarget them in the model's Animation tab.
  The window lists them and refuses to apply rather than failing silently.
- **Clips inside an immutable package** are likewise refused.
- **Nothing detects that a controller is shared.** If two prefabs with different hierarchies play the same
  controller, repairing the paths for one breaks them for the other. That is a property of sharing a controller
  across dissimilar hierarchies, not of this window, but it is worth knowing before applying.

## Avoiding the problem

Every path is relative to the player's own game object, so the shallower that Animator sits, the more of the
hierarchy is able to break it. Put the Animator as close as possible to what it actually animates, and add layout
wrappers *above* it rather than between it and the animated leaves. An Animator sitting directly over its targets
has one-segment paths that survive almost any refactor above it.

That is not always available — `Selectable`-driven animation, for one, requires the Animator on the same game
object as the selectable — which is why this window exists.
