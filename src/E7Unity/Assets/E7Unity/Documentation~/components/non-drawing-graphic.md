# NonDrawingGraphic

A `Graphic` that receives raycasts across its rectangle without drawing anything.

## Why

uGUI hit-tests graphics, not rectangles. A press area therefore needs a `Graphic` on it for the event system to find
— but not necessarily a visible one. The area a finger can hit is often larger than anything drawn in it, and a
button whose only visible parts are a small icon and a label still wants its whole rect to be pressable.

The two usual ways of arranging that both cost something:

| Stand-in | Cost |
| --- | --- |
| An empty legacy `Text` | Drags a font dependency along, and keeps a legacy uGUI `Text` alive in a project that otherwise moved to TextMeshPro. |
| An `Image` with alpha `0` | Puts a fully transparent quad through the batch every frame, drawn for nothing. |

`NonDrawingGraphic` produces no geometry at all while still filling its rect for hit-testing.

## Using it

Add it to the game object that should be pressable, in place of whichever stand-in is there.

Its inspector shows only the settings that do something:

| Setting | Effect |
| --- | --- |
| `Raycast Target` | Whether the component is hit at all. On by default, which is the whole point of it. |
| `Raycast Padding` | Grows or shrinks the hit area relative to the rect. |
| `Maskable` | Whether the hit area is clipped by the masks above it. |

`Material` and `Color` are hidden, because they reach the component and never leave it — there is no geometry for
either to apply to.

`Maskable` is the one worth a thought, and the easiest to mistake for another drawing-only setting:

```csharp
// UnityEngine.UI.MaskableGraphic
public override bool Raycast(Vector2 sp, Camera eventCamera) => Raycast(sp, eventCamera, !maskable);
```

It is handed straight through as the ignore-masks argument. On a component that exists purely to be hit, that is the
difference between a press area clipped to its scroll view and one that keeps taking presses past the edge. Leave it
on unless you want the latter.

> [!NOTE]
> Replacing a stand-in that a `Selectable` points at as its `Target Graphic` is safe — `NonDrawingGraphic` is a
> `Graphic`, so the reference still resolves. Widgets in the [Animator UI](animator-ui.md) family ignore that field
> anyway, since they force `transition` to `None`.

## How it works

`OnPopulateMesh` clears the vertex helper instead of filling it, and the two dirty-marking calls are inert:

```csharp
public override void SetMaterialDirty() {}
public override void SetVerticesDirty() {}
```

Overriding those keeps the component from queueing itself for a canvas rebuild it has nothing to contribute to, so it
stays out of the rebuild batch entirely rather than being visited and found empty.
