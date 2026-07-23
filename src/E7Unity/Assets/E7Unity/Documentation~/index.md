# E7Unity

A small set of Unity components that Exceed7 Experiments keeps reaching for across projects.

E7Unity is deliberately not a framework. Each piece solves one recurring annoyance and has its own page below. Take the
ones you want; nothing here depends on anything else here.

> [!NOTE]
> Everything here is MIT licensed and used in shipping games, but this package makes no promise of stability. It grows
> and shrinks as its author's needs change.

## Components

Ordinary components you drop onto a game object.

| Page | What it does |
| --- | --- |
| [ButtonEx](components/button-ex.md) | A touch-oriented button with separate down, up and click events. |
| [SceneEntryPoint](components/scene-entry-point.md) | One place where a scene decides it has started. |
| [SceneTransition](components/scene-transition.md) | Load a scene from a UnityEvent or a Timeline marker. |
| [LerpOnRatio](components/lerp-on-ratio.md) | A `0..1` float driven by the screen's aspect ratio. |
| [VersionNumber](components/version-number.md) | Show the build version in a TextMeshPro label. |
| [Platform gating](components/platform-gating.md) | Show a game object on some platforms only, and preview that in the editor. |

## Localization

Additions to [Unity Localization](https://docs.unity3d.com/Packages/com.unity.localization@latest). They compile only
when that package is installed, so a project without it simply does not get them.

| Page | What it does |
| --- | --- |
| [LocalizeTmpFontAsset](localization/localize-tmp-font-asset.md) | Swap a TextMeshPro font per locale from an asset table. |
| [LocalizeEntryToPrompt](localization/localize-entry-to-prompt.md) | Copy a translation prompt for an entry, ready to paste into an LLM. |
| [LocalizedPlatformBranches](localization/localized-platform-branches.md) | See and edit a string's per-platform variants from the game object. |

## Timeline

Tracks, clips and markers for `UnityEngine.Timeline`.

| Page | What it does |
| --- | --- |
| [CanvasGroup alpha](timeline/canvas-group-alpha.md) | Fade an entire uGUI tree from clip weight. |
| [CanvasGroup raycasts](timeline/canvas-group-raycasts.md) | Lock the player out of a UI while it animates. |
| [Animator trigger](timeline/animator-trigger.md) | Hand an `Animator` back to its own controller at a chosen moment. |

## Reference

| Page | What it does |
| --- | --- |
| [Installing](getting-started/installing.md) | Adding the package to a project. |
| [Changelog](../CHANGELOG.md) | What changed, and when. |
| [API](api/) | Generated reference for every public type. |

## About the package

The code is split so that a project which does not use Timeline never pays for it:

| Assembly | Namespace | What lives there |
| --- | --- | --- |
| `E7.E7Unity` | `E7.E7Unity` | Plain `MonoBehaviour` components — buttons, scene start-up, layout helpers. |
| `E7.E7Unity` (Timeline part) | `E7.E7Unity.Timeline` | Timeline tracks, clips and markers, plus their editors. |

Both currently compile into the one runtime assembly `E7.E7Unity` with its editor half in `E7.E7Unity.Editor`, but the
namespaces are kept separate so the Timeline portion can be lifted into its own assembly without changing any calling
code.
