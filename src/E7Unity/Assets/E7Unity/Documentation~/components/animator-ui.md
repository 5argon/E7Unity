# Animator UI

A family of interactive widgets that drive their look through an `Animator` instead of `Selectable`'s built-in
transitions, so that what a control does and what it looks like stop being the same decision.

`SelectableAnimatorUi` is the shared base. [`ButtonAnimatorUi`](button-animator-ui.md) and
[`ToggleAnimatorUi`](toggle-animator-ui.md) build on it, and anything else you write can too.

## Why not the built-in transitions

A stock `Selectable` offers colour tint, sprite swap, or an animator driven by five fixed states. All three route
through one `SelectionState`, which is a priority cascade:

```csharp
// UnityEngine.UI.Selectable.currentSelectionState
if (!IsInteractable()) return SelectionState.Disabled;
if (isPointerDown)     return SelectionState.Pressed;
if (hasSelection)      return SelectionState.Selected;
if (isPointerInside)   return SelectionState.Highlighted;
return SelectionState.Normal;
```

Press a selected widget and the state reports `Pressed` — the selection is gone from the value, and there is no way
to read around it because the three fields behind that cascade are private. A control that should stay visibly
focused while the player presses it cannot be expressed.

The same flattening hides a distinction that matters on touch screens. `Normal`/`Pressed` cannot say whether a press
ended in a click or in the player backing out, because by the time either happens the state is `Normal` again.

So `SelectableAnimatorUi` forces `transition` to `None` and drives animator parameters itself, split by what each
piece of state actually is: moments that pass, and states that persist.

## Moments that pass

Pressing down, lifting up, and clicking are three separate moments rather than one.

The separation matters on touch screens. A press should be felt straight away — a click sound, a dent in the graphic
— while the action itself should wait for a genuine click, meaning the finger lifted while still inside the widget.
Lifting *outside* is the player changing their mind, and earns a cosmetic response without the action.

Five triggers carry them:

| Trigger | Fires when |
| --- | --- |
| `Normal` | The widget returns to rest. |
| `Down` | The widget is pressed. |
| `Up` | The press is released, inside or outside. |
| `Click` | The press is released **inside**. |
| `Disabled` | The widget stops being interactable. |

Only one is ever pending. Setting any of them resets the other four, so a rapid press-release cannot leave two
animations fighting each other.

## States that persist

A trigger describes an instant, which is exactly wrong for being checked or being focused. Those have to survive
every momentary animation playing over them, and stay readable as conditions elsewhere in the controller.

`AnimatorFlag` is the shape they share — a bool holding what is true, and four triggers reaching the two resting
poses:

| Part | Purpose |
| --- | --- |
| bool | What is true right now, for conditions anywhere in the controller. |
| turn-on / turn-off triggers | The authored ways in and out, each a one-shot state falling through to the pose it was heading for. |
| snap-on / snap-off triggers | The same two poses, with nothing played in between. |

The snap pair is what enabling, restoring saved values, and corrections a toggle group makes on its own all use. It
is why a dialog that opens on an already-checked toggle shows the check rather than animating it in.

Two flags exist. `Focused` comes from the base and is described below; `IsOn` belongs to
[`ToggleAnimatorUi`](toggle-animator-ui.md).

## Bools to write conditions against

| Bool | True while |
| --- | --- |
| `Highlighted` | The pointer rests inside the widget. |
| `Pressed` | The widget is held down. |
| `Selected` | The widget holds the event system's selection, however it got it. |
| `PointerMode` | The player is steering with a pointing device rather than navigating. |

These are deliberately plain bools rather than triggers. A trigger is consumed by the transition that takes it, so a
second layer conditioning on the same trigger becomes order-dependent between layers. Bools can be read by every
layer at once, which is what lets a focus ring react to a press without competing with the interaction layer for the
`Down` trigger.

## Focus is not selection

The event system's selection is a poor stand-in for a keyboard cursor, because a mouse press takes it too:

```csharp
// UnityEngine.UI.Selectable.OnPointerDown
if (IsInteractable() && navigation.mode != Navigation.Mode.None && EventSystem.current != null)
    EventSystem.current.SetSelectedGameObject(gameObject, eventData);
```

Draw a cursor on `Selected` and one appears under every click, staying there until something else is selected.

`Focused` is narrower: selected **while the player is navigating**. Clicking with the mouse leaves a widget selected
but unfocused, so no cursor is drawn; pressing an arrow key afterwards flips the mode and the cursor appears on that
same widget, already in the right place.

That is the reason focus follows the current input mode rather than the kind of event that brought the selection. A
widget selected by a click and then reached by a key press would otherwise stay unfocused until something reselected
it.

## UiInputMode

`UiInputMode` holds which kind of input is steering, so hover and the keyboard cursor are never both on screen at
once — two cursors, and the player cannot tell which one Enter will hit.

| Mode | Meaning |
| --- | --- |
| `Pointer` | A pointing device is steering, so hover belongs on whatever sits under it. |
| `Directional` | Keyboard or gamepad navigation is steering, so the focused widget carries the cursor. |

Widgets report it from events they already receive, so it follows the player with nothing wired up: landing the
pointer on a widget or pressing one counts as pointing, and navigating counts as directional. `Changed` fires only
when the answer actually changes, and every enabled widget re-applies its focus and `PointerMode` from it.

Authoring against it: a light hover reads `Highlighted` together with `PointerMode`; a bolder keyboard cursor lives
on the focus layer, which is already gated on the mode.

> [!NOTE]
> Two switches happen where no widget is listening — a mouse crossing empty space, and the first arrow key pressed
> while nothing is selected. Call `UiInputMode.ReportPointer()` or `ReportDirectional()` from your own input handling
> to cover them. Both are public for exactly this.

`Current` and `Changed` are static, so with editor Domain Reload disabled they would otherwise outlive a play
session. The class resets both from a `SubsystemRegistration` hook of its own — nothing to add to your reset code.

## Layers

The generated controller separates concerns by layer, so a persistent look sits steady over a momentary one instead
of interrupting it.

| Layer | Holds |
| --- | --- |
| 0 — base | The `Normal`/`Down`/`Up`/`Disabled` interaction states. |
| 1 — Click Effect Layer | A one-shot `Click` flourish, over whatever the base layer shows. |
| 2 — Focus Layer | The keyboard cursor, steady across presses and clicks. |
| 3+ | Whatever the widget adds — an idle layer for a button, an on/off layer for a toggle. |

## Parameters you leave out

A parameter the running controller does not declare is skipped rather than set. A controller written before a
parameter existed keeps working untouched, and one that only cares about pressing can declare only `Down` and `Up`
without a console full of "parameter does not exist".

The check reads the controller's declared parameters once per enable and matches on name hash, so it costs nothing
per press.

Parameters are also only set while the animator's playable graph is valid, meaning a widget with no controller
assigned animates nothing rather than throwing.

## The inspector

`SelectableAnimatorUiEditor` covers every widget in the family and hides what they do not honour: the transition
dropdown, the colour, sprite and animation blocks behind it, and the target graphic, which exists to be tinted or
swapped by those same transitions. `Interactable`, navigation, and the widget's own fields remain.

Navigation is left visible on purpose — it is what keyboard and gamepad focus moves through.

## Writing your own widget

Derive from `SelectableAnimatorUi` and implement `OnClicked`, which runs after the `Click` trigger has fired:

```csharp
public class MyWidget : SelectableAnimatorUi
{
    protected override void OnClicked()
    {
        // the widget's action
    }
}
```

If it holds state of its own, give it an `AnimatorFlag`, apply it with `ApplyFlag`, and override `ApplyRestingPose`
so enabling arrives at the right pose without playing the way there:

```csharp
private static readonly AnimatorFlag armedFlag =
    new AnimatorFlag("Armed", "Arm", "Disarm", "SnapArmed", "SnapDisarmed");

protected override bool ApplyRestingPose()
{
    if (!base.ApplyRestingPose())
        return false;

    ApplyFlag(armedFlag, armed, false);
    return true;
}
```

Returning the base's answer matters. The `Animator` is ordered after the widget on the game object, so on the frame
the object is enabled its playable graph may not exist yet; a `false` tells the base to try again next frame.

`SelectableAnimatorUiBuilder.BuildFlagLayer` builds the four-state layer for a flag, if you write a **Create
Animator** menu item of your own.
