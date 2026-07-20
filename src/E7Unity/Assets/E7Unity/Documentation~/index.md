# E7Unity

A small set of Unity components that Exceed7 Experiments keeps reaching for across projects.

> [!NOTE]
> Everything here is MIT licensed and used in shipping games, but this package makes no promise of stability. It grows
> and shrinks as its author's needs change.

E7Unity is deliberately not a framework. Each piece is a self-contained component that solves one recurring annoyance
— a button that can tell a press from a click, a Timeline track that can fade a whole UI tree, a scene that decides
for itself when it has really started. Take the ones you want; nothing here depends on anything else here.

## Two assemblies

The package is split so that a project which does not use Timeline never pays for it:

| Assembly | Namespace | What lives there |
| --- | --- | --- |
| `E7.E7Unity` | `E7.E7Unity` | Plain `MonoBehaviour` components — buttons, scene start-up, layout helpers. |
| `E7.E7Unity` (Timeline part) | `E7.E7Unity.Timeline` | Timeline tracks, clips and markers, plus their editors. |

Both currently compile into the one runtime assembly `E7.E7Unity` with its editor half in `E7.E7Unity.Editor`, but the
namespaces are kept separate so the Timeline portion can be lifted into its own assembly without changing any calling
code.

## Components

Ordinary components you drop onto a game object.

- [ButtonEx](components/button-ex.md) — a touch-oriented button with separate down, up and click events.
- [SceneEntryPoint](components/scene-entry-point.md) — one place where a scene decides it has started.
- [SceneTransition](components/scene-transition.md) — load a scene from a UnityEvent or a Timeline marker.
- [PlatformSpecific](components/platform-specific.md) — keep a game object only on certain platforms.
- [LerpOnRatio](components/lerp-on-ratio.md) — a `0..1` float driven by the screen's aspect ratio.
- [VersionNumber](components/version-number.md) — show the build version in a TextMeshPro label.

## Timeline

Tracks, clips and markers for `UnityEngine.Timeline`.

- [CanvasGroup alpha](timeline/canvas-group-alpha.md) — fade an entire uGUI tree from clip weight.
- [CanvasGroup raycasts](timeline/canvas-group-raycasts.md) — lock the player out of a UI while it animates.
- [Animator trigger](timeline/animator-trigger.md) — hand an `Animator` back to its own controller at a chosen moment.

## Learn more

- [Getting Started / Installing](getting-started/installing.md)
