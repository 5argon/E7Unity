# ToggleAnimatorUi

A toggle on the same interaction model as [`ButtonAnimatorUi`](button-animator-ui.md), holding an on/off value on top
of it, and `ToggleGroupAnimatorUi` for the case where only one of several may be on.

Both build on [Animator UI](animator-ui.md), which covers the animation model they share with the button.

## Turning on and turning off are separate animations

A single property blended in both directions gives one animation played forwards and backwards. A check appearing and
a check leaving usually want to be different — a pop in, a fade out — so they get a path each.

`IsOn` is an `AnimatorFlag`, which spends five parameters on that:

| Parameter | Reaches |
| --- | --- |
| `ToggleOn` | The check coming in, through a `TurningOn` state that falls through to `On`. |
| `ToggleOff` | The check going out, through `TurningOff`, falling through to `Off`. |
| `SnapOn` / `SnapOff` | The `On` and `Off` poses directly, with nothing played. |
| `IsOn` (bool) | Declared but unused by the generated transitions, free for conditions of your own — a pressed look that differs while checked, for instance. |

Snapping is not an optimisation. A screen opening on a toggle that was already on should show it on, not animate it
in, and a group correcting itself at load should not look like the player did something.

## Reading and writing the value

| Member | Animates | Notifies |
| --- | --- | --- |
| `IsOn` (property) | yes | yes |
| `SetIsOnWithoutNotify(bool)` | yes | no |
| `SetIsOnInstant(bool)` | no | no |
| A press by the player | yes | yes |

`SetIsOnInstant` is the one for filling a screen in from saved settings — the toggle arrives at the right pose with
nothing played and no listener fired. In a group it still turns its siblings off, and they snap too.

`onValueChanged` is a `UnityEvent<bool>` carrying the value the toggle settled on, wirable in the inspector.

> [!NOTE]
> "Settled on" is doing real work in that sentence. A toggle held on by its group reports `true` even though it was
> pressed to turn off — see below.

## ToggleGroupAnimatorUi

Put one on a common ancestor and point each member's `Group` field at it. Only one member can be on at a time.

Toggles register themselves as they are enabled and drop out as they are disabled, so a group only knows about the
toggles currently alive beneath it. That also means a group is safe to build from pooled or spawned rows.

`AllowSwitchOff` decides what pressing the toggle that is already on means:

| `AllowSwitchOff` | Pressing the on toggle |
| --- | --- |
| off (default) | Leaves it on. `onValueChanged` still fires, with `true`. |
| on | Switches it off, leaving nothing on. |

The event firing with `true` on a press that meant "off" is deliberate, and matches Unity's own `Toggle`: a control
built on top of a group needs to hear about the press even when the value did not move. The toggle does *not* replay
its coming-in animation in that case, because the pose never changed.

Even with `AllowSwitchOff` off, a group does not force something on the moment a scene loads with everything off —
it prevents the player switching the last one off, and corrects itself in `Start` and `OnEnable`.

### What animates and what snaps

| Cause | Siblings |
| --- | --- |
| The player turns one on | Play their way out — it is a consequence of something just done. |
| `EnsureValidState` at load, or after a member is destroyed | Snap. Nothing was done; the group is tidying up. |
| `SetAllTogglesOff` | Play out by default, `animate: false` to snap. |

### Other members

`AnyTogglesOn`, `ActiveToggles`, `GetFirstActiveToggle` report the current state and check the value only, not
whether the game object is active. `RegisterToggle` and `UnregisterToggle` exist for the lifecycle the toggles drive
themselves, and rarely need calling by hand. `SetAllTogglesOff` clears the group regardless of `AllowSwitchOff`.

## Why not Unity's ToggleGroup

`UnityEngine.UI.ToggleGroup` is hardwired to the concrete `Toggle` — `List<Toggle> m_Toggles`,
`NotifyToggleOn(Toggle)`, `RegisterToggle(Toggle)`. A toggle that is not a `Toggle` can never join one, so the group
is a type of its own rather than a reuse of that one. The semantics are deliberately kept the same, so what you know
about the built-in group carries over.

## Creating the controller

Right-click the component header and choose **Create Animator**. The generated controller comes with everything
[`ButtonAnimatorUi`](button-animator-ui.md) gets, minus the idle layer, plus a **Toggle State Layer** holding `Off`,
`On`, `TurningOn` and `TurningOff` wired to the five parameters above.

The clips are empty — it is a starting skeleton, not a finished look.

## Not a Toggle

`ToggleAnimatorUi` does not derive from `UnityEngine.UI.Toggle`, so a `[SerializeField] Toggle` pointed at one is a
type mismatch Unity nulls out on the next save, with no error at authoring time. Nor does it have `Toggle`'s
`graphic` field and fade transition — the checkmark is animated by the controller like everything else.
