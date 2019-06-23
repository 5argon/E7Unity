# CanvasGroupTimelineExtension

The binding is of type `CanvasGroup`. It is useful for your big tree of uGUI.

## GroupAlphaTrack

Use this track to control `CanvasGroup`'s alpha based on weight of the clip. Useful when you want to show-hide the entire thing, but using Activation Track cause too much layout change and awakes.

Cmd + click and drag the edge of the clip to make a weight slope which affects alpha.

## UnblocksRaycastsTrack

On the duration of the click, `CanvasGroup` let the raycast pass through.

Useful for making UI animations, where usually you don't want your player to mess with the UI while the intro/outro sequence is still running.
The clip should usually span the entire length, but you could shrink them a bit to allow player to interact with the UI tree earlier.

Put the `UninteractableTrack.png` in your `Assets/Gizmos` to get a track icon.