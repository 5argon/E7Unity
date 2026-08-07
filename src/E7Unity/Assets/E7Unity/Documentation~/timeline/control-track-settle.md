# Control track settle

Keep a sub-director driven by a control track from being stranded mid-animation by a stalled frame.

**Component:** `DirectorSettle` · **Marker component:** `SettleAtEnd`

## The stranding problem

Timeline samples the world at one instant per frame and keeps no memory of the previous one. Everything is a function
of where the playhead is right now, which is what makes scrubbing in the editor behave exactly like playback, and
what keeps the cost of a frame bounded no matter how far the playhead jumped to reach it.

Most tracks are fine with that. An animation clip set to hold past its end stretches its interval indefinitely, so it
stays under the playhead wherever the playhead lands. An activation track recomputes itself from scratch every frame,
so it is always right for the current instant.

A control track driving another `PlayableDirector` is the exception. It writes its target's time only from inside its
own clip's frame processing, and the value it writes is the playhead clamped to the target's duration. Once the
playhead is past the clip, nothing recomputes it — the target simply keeps whatever partial time the last evaluation
happened to leave there.

So the target only reaches its final frame if some evaluation landed on the stretch of clip *beyond* the target's own
content, which is where the clamp takes effect. Size a control clip to exactly match the timeline it drives and that
stretch does not exist at all. Give it only a frame or two and a single long frame can step over it. Either way the
target holds a pose from the middle of its sequence, for good: a curtain caught half open, a camera stopped short of
its mark.

The failure needs a slow frame at one particular moment, so it survives testing easily and shows up on a player's
device instead.

## Using it

1. Put `DirectorSettle` on the director that plays the Timeline containing the control tracks.
2. Put `SettleAtEnd` on each sub-director whose last frame has to stick.

Nothing else is wired. When playback stops, `DirectorSettle` walks the Timeline's control tracks, resolves each
clip's target through the director's own bindings, and moves every target carrying `SettleAtEnd` to its own duration.

## Nested control tracks

Control tracks nest — a timeline driven by a control clip can hold control clips of its own. `DirectorSettle`
descends the whole chain, and it has to: evaluating a target at its own duration does **not** settle what that target
drives, because at that instant a control clip ending on the duration is already outside the half-open interval
Timeline queries with, so it never runs and never writes its own target's time. Each level is settled explicitly.

The descent follows only targets that were actually settled. An unmarked director partway down a chain therefore
shields everything below it as well, so mark every level whose pose matters.

This is also why clip-geometry margins get awkward at depth. Widening a control clip inside a nested timeline grows
that timeline's duration when it is set to `Based On Clips`, which silently truncates every clip driving it from
above — and the required margin compounds, since each level needs its own on top of the level below. Settling has no
such arithmetic, because it does not depend on where samples land at all.

## Why the marker component

`SettleAtEnd` declares intent and does no work of its own. Settling every target would be wrong, because not every
sub-director ends on a pose.

A sub-director whose last frame *releases* something — handing a skeleton, an animator or a particle system back to
component-driven playback — wants to be let go rather than pinned. Evaluating it again rebuilds its graph and
reasserts the Timeline's own state over whatever just took over, undoing exactly the handover it was there to
perform. Those targets are left alone by simply not carrying the component.

Because the decision is a component on the target rather than a rule inside `DirectorSettle`, adding a new kind of
track never silently changes which targets get settled.

## Why it listens for `stopped`

`PlayableDirector.stopped` is raised by the change of state rather than read off the playhead, so no amount of
stalling can miss it — unlike a marker placed near the end of the Timeline, which is skipped whenever the duration
falls even fractionally below it.

It also fires after the playhead has left every clip. That ordering matters: settling a target while its control clip
is still under the playhead is undone on the very next frame, when the clip notices its target is out of sync and
drags it back.

## Held directors

A director set to wrap mode **Hold** never stops, so it never raises `stopped` and `DirectorSettle` does nothing for
it. It does not need to — holding pins the playhead at the Timeline's last instant and re-asserts it every frame,
which pins any control clip still under that instant along with it.

Holding is a different trade rather than a better one. Because the graph is never destroyed, no track ever runs its
teardown: an activation track never applies its post-playback state, a control clip never stops the director it
drives, and any track that releases something it borrowed never releases it. A Timeline whose ending depends on any
of those wants `stopped` and this component instead.

## Choosing a clip length

Independently of this component, a control clip should extend past the content it drives, so the clamp has somewhere
to take effect. How far depends on where the clip sits.

A clip in the *middle* of a Timeline has the playhead pass all the way through that stretch, so it only needs to be
wider than a single frame's advance can cover. `Time.maximumDeltaTime` — Maximum Allowed Timestep in the Time
settings, `0.3333` by default — caps how far the playhead moves in one evaluation, so a stretch longer than that
cannot be stepped over.

A clip at the *end* of a Timeline cannot rely on that, because the playhead stops at the duration and never traverses
the rest. Those are the ones this component exists for.
