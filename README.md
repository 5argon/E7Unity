# E7Unity

A small set of Unity components that Exceed7 Experiments keeps reaching for across projects.

Not a framework — each piece is self-contained and solves one recurring annoyance. A button that can tell a press from
a click. Timeline tracks that fade or lock a whole uGUI tree. A scene that decides for itself when it has really
started. Take the ones you want; nothing here depends on anything else here.

This repository is a **Unity project** (so you can open it and work on the package in place). The package itself lives
at [`src/E7Unity/Assets/E7Unity`](src/E7Unity/Assets/E7Unity), and its documentation — guides plus the generated C#
API reference — is built from
[`src/E7Unity/Assets/E7Unity/Documentation~`](src/E7Unity/Assets/E7Unity/Documentation~).

## Install (Unity Package Manager, Git URL)

Unity can install a package that lives in a repo subfolder via the `?path=` query. Add this to your project's
`Packages/manifest.json` dependencies:

```json
"com.e7.e7unity": "https://github.com/5argon/E7Unity.git?path=src/E7Unity/Assets/E7Unity"
```

## Contents

**Components** — `ButtonAnimatorUi`, `ToggleAnimatorUi`, `ToggleGroupAnimatorUi`, `NonDrawingGraphic`, `SceneEntryPoint`, `SceneTransition`, `PlatformSpecific`, `LerpOnRatio`, `VersionNumber`

**Timeline** — CanvasGroup alpha track, CanvasGroup raycast track, Animator trigger marker and receiver

## Requirements

Unity 2019.1 or newer. The only package dependency is `com.unity.timeline`; uGUI and TextMeshPro ship with Unity.

## Building the documentation

The site is built with [DocFX](https://dotnet.github.io/docfx/) from `.docfx_project`:

```bash
./build-docs.sh           # full build (API metadata + site), then serve
./build-docs.sh --fast    # skip the API compile — for Markdown/CSS edits
```

Open `src/E7Unity` in Unity once first, so it generates the `.sln` the API-metadata step compiles against.

## License

MIT. See the caveat below before depending on it.

> This package grows and shrinks with its author's needs — things get deleted when they stop being useful. It is used
> in shipping games, but it makes no promise of stability to anyone else. No responsibility is taken for bugs, build
> failures, or lost revenue.

## Author

Exceed7 Experiments
