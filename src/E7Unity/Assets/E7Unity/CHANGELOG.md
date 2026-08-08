# Changelog

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

A cleanup pass that reduced the package to the parts actually in use, and gave it real documentation.

### Added

- **Documentation** in the UPM `Documentation~` layout, with a page per component and per Timeline feature. It doubles
  as the source for the DocFX site.
- **XML documentation comments** across every public type, method, property and field, so the generated API reference
  is worth reading.
- **`CHANGELOG.md`**.
- **`SelectableAnimatorUi`**, the animator-driven interaction model extracted out of `ButtonAnimatorUi` so more than
  one widget can share it. Adds a focus layer for keyboard and gamepad play, plus `Highlighted`, `Pressed`,
  `Selected` and `PointerMode` bools to write animator conditions against.
- **`ToggleAnimatorUi`** and **`ToggleGroupAnimatorUi`**, a toggle on that same model with separately authored
  animations for turning on and turning off, and a group in which only one member can be on at a time. Unity's own
  `ToggleGroup` takes only a `Toggle`, so the group is its own type rather than a reuse of that one.
- **`AnimatorFlag`**, the parameter set behind a state that persists rather than passes — a bool holding what is
  true, two triggers for the authored ways in and out, and two more for arriving without playing anything.
- **`UiInputMode`**, tracking whether the player is pointing or navigating so that hover and the keyboard cursor are
  never both on screen at once.
- **`NonDrawingGraphic`**, a `Graphic` that takes raycasts across its rect without producing any geometry, for press
  areas that need to be hit rather than seen.
- **`SelectableRegistryReset`**, emptying uGUI's static registry of selectables as the runtime starts. `Selectable`
  never clears it, so with editor Domain Reload off a single exception thrown out of any `DoStateTransition` raises
  the registry's count without the matching entry ever being removed, and every selectable enabled after that writes
  past the end of the array for the rest of the editor session. Applies to every selectable in the project, not only
  this package's.
- **`DirectorSettle`** and **`SettleAtEnd`**, closing the one gap a control track leaves in Timeline's sampled
  model. A control track writes its target director's time only from inside its own clip and nothing recomputes it
  afterwards, so a frame long enough to step over the end of the clip strands the target mid-animation.
  `DirectorSettle` on the driving director pushes each target to its final frame once playback stops, and
  `SettleAtEnd` on a target declares that it wants that — leaving alone the sub-directors whose ending is a
  handover back to component-driven playback rather than a pose.

### Changed

- **Namespaces.** Every type now lives under `E7.E7Unity`, and the Timeline extensions under `E7.E7Unity.Timeline`.
  Previously most types sat in the global namespace and the Timeline ones in an unrelated `E7.Timeline`. Add a `using`
  for the namespace you need; components wired through the inspector are unaffected, since Unity resolves those by
  file GUID.
- **`package.json`** declares `com.unity.timeline` instead of the unused `com.unity.mathematics`.

### Removed

- **Unused scripts.** Roughly fifty features were dropped, either superseded by packages Unity now ships (UniTask,
  `ValueTuple`, nested prefabs), broken against modern Unity (`LegacyAnimator`, `MouseTouch` on the old Input
  Manager, `TextFallback` on legacy uGUI `Text`), or dead behind `ODIN_INSPECTOR` / `HAS_AAS` defines that no longer
  resolve.
- **`ButtonExceed`**, superseded by `ButtonAnimatorUi`.
- **`GraphicConstructionKit`** sprite set and the `SubmoduleMetaBackup` script, neither of which belonged in a code
  package.
- **Per-folder `README.md` files**, folded into `Documentation~`.
- **Assembly definition references** to Addressables, UniTask, ResourceManager and Mathematics, plus the inert
  Odin/Firebase `precompiledReferences` and the `HAS_AAS` version define — nothing left in the package used them.
