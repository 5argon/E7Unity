# AnimatorTriggerMarker & AnimatorTriggerReceiver

In timeline, we could animate via `Animator` in coorperation with clips in the timeline. This "hold `Animator`'s hand" and overrides the controller that `Animator` could use. It is now possible to use `Animator` to control things without the controller asset. Timeline is borrowing only "controlling power" of `Animator` and not the "running engine".

However, usually when we are finished with timeline, we would want to let that `Animator` do its own thing (take over), but not before the timeline finishes!

This new marker notify a `string`, which represent the trigger. The track where this marker resides should be the animation track, so that the object that contains the `Animator` could receive the notification. (However theoretically you could put it anywhere since markers are not locked to any particular track, but they may be useless.)

The receiver is called `AnimatorTriggerReceiver` component you should attach near the `Animator` on the same game object. The receiver will then use the string to `.SetTrigger` to the `Animator`. Simple!