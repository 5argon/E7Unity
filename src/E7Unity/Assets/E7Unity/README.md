# E7Unity

A small set of Unity components that Exceed7 Experiments keeps reaching for across projects.

Not a framework — each piece is self-contained and solves one recurring annoyance. A button that can tell a press from
a click. Timeline tracks that fade or lock a whole uGUI tree. A scene that decides for itself when it has really
started. Take the ones you want; nothing here depends on anything else here.

## Contents

**Components** — `ButtonAnimatorUi`, `ToggleAnimatorUi`, `ToggleGroupAnimatorUi`, `NonDrawingGraphic`, `SceneEntryPoint`, `SceneTransition`, `PlatformSpecific`, `LerpOnRatio`, `VersionNumber`

**Timeline** — CanvasGroup alpha track, CanvasGroup raycast track, Animator trigger marker and receiver

## Requirements

Unity 6000.3 or newer. The only package dependency is `com.unity.timeline`; uGUI and TextMeshPro ship with Unity.

## Installation

Add it through the Package Manager from this repository's git URL, from disk, or keep it embedded under your
project's `Packages/` folder.

The runtime assembly is `E7.E7Unity` (editor half: `E7.E7Unity.Editor`). It is `autoReferenced`, so `Assembly-CSharp`
sees it with no setup; assembly definitions of your own need it added to their references.

## Documentation

Offline documentation is included under `Documentation~`. Open it with any Markdown reader — start at
[`Documentation~/index.md`](Documentation~/index.md), or jump to
[Getting Started / Installing](Documentation~/getting-started/installing.md).

(`Documentation~` ends with `~` so Unity's Asset Database leaves it out of your project.)

## Related packages

Some scripts that once lived here have their own repositories:

- [OdinHierarchy](https://github.com/5argon/OdinHierarchy)
- [protobuf-unity](https://github.com/5argon/protobuf-unity)
- [E7ECS](https://github.com/5argon/E7ECS)

## License

MIT. See the caveat below before depending on it.

> This package grows and shrinks with its author's needs — things get deleted when they stop being useful. It is used
> in shipping games, but it makes no promise of stability to anyone else. No responsibility is taken for bugs, build
> failures, or lost revenue.

## Author

Exceed7 Experiments
