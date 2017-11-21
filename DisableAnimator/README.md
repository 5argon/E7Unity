# DisableAnimator

Together with "LegacyAnimator" to solve the Animator performance issue. This is for when you really can't go legacy and want to use the graph.

The concept is simple, that is if your Animator is in the state of waiting for trigger, **doing nothing** or **stuck at a certain state**, you should disable the Animator component. When you want to go on, enable it back and `SetTrigger`.

## How to use
- This is a `StateMachineBehaviour`. Put it in a state that you would like to allow disabling to take place. Also you need to set the tag as "Disable".
- When arrives at that state, it will check if **all layers** is at the state with the tag "Disable" or not. This is to prevent disabling while other layers are still working. So it is reccommended that you keep looping at the state with this script. Besure to put "Disable" tag in other layers where you think it is safe to disable. (No need to attach this script, just the tag.)

## Caution

- When enable back, it will still be at the same state.
- If you want to reset the state, disabling-enabling won't do. You can use `Rebind()` but it will initialize the Animator causing an overhead. I reccommend making a `Reset` trigger which results in Any State -> your first state on all layers.
