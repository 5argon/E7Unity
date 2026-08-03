# ButtonAnimatorUi

A touch-oriented button that separates pressing down, lifting up, and clicking into three distinct events.

It builds on [Animator UI](animator-ui.md), which covers the animation model the whole family shares. This page is
what the button adds on top.

## Why three events

A stock `Button` gives you one moment: the click. On a touch screen that is not enough. Pressing should be felt
straight away — a click sound, a dent in the graphic — while the button's actual action should wait for a genuine
click, meaning the finger lifted while still inside the button.

Those are different moments, and lifting *outside* the button is a third one: the player started a press and thought
better of it. That deserves the graphic to spring back, but definitely not the action.

`ButtonAnimatorUi` exposes exactly those three:

| Event | Fires when |
| --- | --- |
| `onDown` | The button is pressed. |
| `onUp` | The press is released, **whether or not** the pointer is still inside the button. |
| `onClick` | The press is released **inside** the button. This is where the action goes. |

A genuine click therefore raises `onUp` *and* `onClick`. Backing out raises only `onUp`.

All three are `UnityEvent`s, so they can be wired in the inspector like any button. `onDown` and `onUp` come from
`SelectableAnimatorUi` and are shared with every widget in the family; `onClick` is the button's own.

## Creating the controller

Right-click the component header and choose **Create Animator**. You will be asked where to save, and the generated
controller comes with:

- A state and clip per trigger, on the base layer, wired with any-state transitions.
- A **Click Effect Layer** so a click flourish can play over the base state.
- A **Focus Layer** holding the keyboard cursor steady under whatever the base layer is doing.
- An **Idle Layer** with a looping idle clip.

The clips are empty — it is a starting skeleton, not a finished look.

## Mouse and keyboard together

The keyboard cursor is `Focused`, not `Selected`, because a mouse press takes the event system's selection too and a
cursor drawn on `Selected` would appear under every click. A light hover reads `Highlighted` together with
`PointerMode`; the bolder cursor lives on the Focus Layer. Only one is ever live, so the player never sees two
cursors at once.

[Animator UI](animator-ui.md) explains the whole arrangement, including `UiInputMode`.

## Known gap

A press that begins while the button is not interactable, and is released after it becomes interactable, still counts
as a click.
