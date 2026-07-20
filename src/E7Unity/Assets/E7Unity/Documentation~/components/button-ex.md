# ButtonEx

A touch-oriented button that separates pressing down, lifting up, and clicking into three distinct events.

## Why three events

A stock `Button` gives you one moment: the click. On a touch screen that is not enough. Pressing should be felt
straight away — a click sound, a dent in the graphic — while the button's actual action should wait for a genuine
click, meaning the finger lifted while still inside the button.

Those are different moments, and lifting *outside* the button is a third one: the player started a press and thought
better of it. That deserves the graphic to spring back, but definitely not the action.

`ButtonEx` exposes exactly those three:

| Event | Fires when |
| --- | --- |
| `onDown` | The button is pressed. |
| `onUp` | The press is released, **whether or not** the pointer is still inside the button. |
| `onClick` | The press is released **inside** the button. This is where the action goes. |

A genuine click therefore raises `onUp` *and* `onClick`. Backing out raises only `onUp`.

All three are `UnityEvent`s, so they can be wired in the inspector like any button.

## Animation

`ButtonEx` derives from `Selectable`, but it does not use the built-in transition system —
`transition` is forced to `None`. Instead it drives an `Animator` through five explicit triggers:

`Normal`, `Down`, `Up`, `Click`, `Disabled`

The point of the split is that a click and a mere release can look different, which the `Normal`/`Pressed` pair of a
stock button cannot express. Only one trigger is ever pending: setting any of them resets the other four, so a
rapid press-release cannot leave two animations fighting.

The animator is required on the same game object. Triggers are only set when the animator's playable graph is valid,
so a button with no controller assigned simply animates nothing rather than throwing.

### Creating the controller

Right-click the component header and choose **Create Animator**. You will be asked where to save, and the generated
controller comes with:

- A state and clip per trigger, on the base layer, wired with any-state transitions.
- A separate **Click Effect Layer** so a click flourish can play over the base state.
- A separate **Idle Layer** with a looping idle clip.

The clips are empty — it is a starting skeleton, not a finished look.

> [!NOTE]
> The "selected" state is not supported, and neither is keyboard or gamepad navigation. This is a button for fingers.

## Known gap

A press that begins while the button is not interactable, and is released after it becomes interactable, still counts
as a click.
