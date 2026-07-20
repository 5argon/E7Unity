# Animator trigger

Hand an `Animator` back to its own controller at a moment you choose.

**Marker:** `AnimatorTriggerMarker` · **Receiver:** `AnimatorTriggerReceiver`

## The handover problem

While a Timeline animates an `Animator`, it takes that animator over completely and overrides its controller.
Timeline borrows the animator's ability to *pose* things, but not its state machine — which is what makes it possible
to animate through an `Animator` with no controller asset at all.

When the sequence is done you usually want the animator to go back to running itself: a character returning to idle, a
button resuming its own press animation. The natural instinct is to do that when the timeline ends, but "when the
timeline ends" is rarely the right frame — the handover often belongs slightly earlier, so the animator's own
animation blends in while the sequence is still finishing.

`AnimatorTriggerMarker` lets you place that handover exactly.

## Using it

1. Add an **E7.E7Unity / AnimatorTriggerMarker** to the animation track bound to the object you want to trigger, at
   the frame the handover should happen.
2. Type the name of the `Animator` trigger parameter into the marker's `trigger` field.
3. Put an `AnimatorTriggerReceiver` on that same bound game object.

When playback passes the marker, the receiver calls `SetTrigger` with that name.

## Binding matters

Timeline delivers notifications to the object **bound to the track holding the marker**. Markers are not actually
locked to any particular track, so the editor will happily let you drop one on an unbound track — but then nothing is
listening and nothing happens.

## The receiver

`AnimatorTriggerReceiver` requires an `Animator` on its game object and fills `triggerTarget` with it automatically,
so in the common case there is nothing to wire. Point `triggerTarget` elsewhere if the animator you want to drive is
not the one beside the receiver.

Notifications that are not `AnimatorTriggerMarker` are ignored, so the receiver is safe on an object that receives
other markers too.

## Picking the trigger name

The marker's inspector does not leave `trigger` as a free-form string to mistype. It resolves the animator bound to
the marker's own track — through the director currently previewing the timeline — and offers that animator's declared
trigger parameters as buttons.

That resolution needs the Timeline window open with a director selected and the track bound. Outside those
conditions the inspector falls back to a plain text field, which still works.
