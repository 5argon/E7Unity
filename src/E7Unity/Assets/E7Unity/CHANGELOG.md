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
- **`ButtonExceed`**, superseded by `ButtonEx`.
- **`GraphicConstructionKit`** sprite set and the `SubmoduleMetaBackup` script, neither of which belonged in a code
  package.
- **Per-folder `README.md` files**, folded into `Documentation~`.
- **Assembly definition references** to Addressables, UniTask, ResourceManager and Mathematics, plus the inert
  Odin/Firebase `precompiledReferences` and the `HAS_AAS` version define — nothing left in the package used them.
