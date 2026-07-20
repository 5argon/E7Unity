# CanvasGroup raycasts

Lock the player out of a UI while it is animating.

**Track:** `UnblocksRaycastsTrack` · **Clip:** `UnblocksRaycastsClip` · **Binding:** `CanvasGroup`

UI intros and outros take time, and during them a player can happily tap a button that is still sliding in — opening
a panel that the sequence is about to close, or firing an action twice. This track holds input off for exactly as
long as the sequence needs, driving the bound `CanvasGroup`'s `blocksRaycasts`.

> [!WARNING]
> The naming is inverted from what it sounds like. "Unblocks raycasts" means rays pass **through** the group — which
> is precisely when the player **cannot** click anything. Blocking raycasts is the clickable state.

## Two ways to use it

The track behaves differently depending on whether it has clips, and both arrangements are useful.

### With clips

The UI is unclickable for the extent of each clip, and clickable everywhere else on the track.

A new clip is created spanning the **entire timeline**, rather than at some arbitrary default length, since covering
the whole sequence is nearly always the intent. Shorten it from the end to hand control back to the player early —
useful when the last stretch of an intro is only decorative.

### With no clips at all

An empty track makes the UI unclickable for its **whole duration**.

This is common enough to be worth a special case, and it is why the track keeps evaluating while empty — normally
Timeline skips empty tracks entirely.

## After playback

`postPlaybackState` on the track decides where the `CanvasGroup` is left once the graph is torn down:

| Value | Result |
| --- | --- |
| `BlocksRaycasts` | Leave the UI clickable. |
| `UnblocksRaycasts` | Leave the UI unclickable. |
| `Revert` | Restore whatever the value was when the track first touched it. |

`Revert` is the safe default for a sequence that runs inside a longer-lived screen. `BlocksRaycasts` suits an intro
that ends by handing the screen to the player.

The track inspector draws these settings inline rather than behind Timeline's usual `template` foldout.

## Editor preview

The track registers the `CanvasGroup`'s interactable and raycast flags with Timeline's preview system, so scrubbing in
the editor does not leave them permanently modified once preview ends.
