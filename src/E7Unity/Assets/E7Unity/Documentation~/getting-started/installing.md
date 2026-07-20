# Installing

## Add the package

E7Unity is a UPM package. Add it through the Package Manager from a git URL, from disk, or keep it embedded under your
project's `Packages/` folder.

Its only declared dependency is `com.unity.timeline`. It also uses uGUI (`com.unity.ugui`) and TextMeshPro, both of
which ship with Unity.

## Referencing it

The runtime assembly is `E7.E7Unity` and the editor half is `E7.E7Unity.Editor`. The runtime assembly is
`autoReferenced`, so scripts in your project's predefined assemblies (`Assembly-CSharp`) see it with no setup.

If your code lives in its own assembly definition — which it should — add `E7.E7Unity` to that assembly definition's
**Assembly Definition References**.

## Namespaces

Types are split across two namespaces, so a `using` makes clear which half you are touching:

```csharp
using E7.E7Unity;          // ButtonEx, SceneEntryPoint, SceneTransition, PlatformSpecific, ...
using E7.E7Unity.Timeline; // GroupAlphaTrack, UnblocksRaycastsTrack, AnimatorTriggerMarker, ...
```

Components dropped onto a game object through the inspector need no `using` at all — that is only for code.

## Track icons

Timeline tracks look for their icon by name in a `Gizmos` folder. To get the track icons in the Timeline window, copy
`GroupAlphaTrack.png` and `UnblocksRaycastsTrack.png` from the package's
`TimelineExtensions/CanvasGroupTimelineExtension/` folder into your project's `Assets/Gizmos/`.

Everything works without them; the tracks are simply unadorned.
