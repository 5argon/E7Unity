# Platform gating

Show a game object on some platforms and not others, and see that choice in the editor without building.

Three pieces work together. `PlatformSpecific` tags an object with the platforms it belongs to, `PlatformSwitch` sits
on a common ancestor and shows or hides everything tagged beneath it, and `PlatformResolver` is the single place that
answers "which platform are we pretending to be right now."

## PlatformSpecific

Add it to a game object and fill the `platforms` array with the `RuntimePlatform` values the object is meant for. An
empty array means "nowhere".

On its own the component can only *hide*: it checks in `Awake` and deactivates itself when the platform does not
match. It cannot bring itself back, because a deactivated game object never receives `Awake` again. It also does
nothing at all in edit mode, so authoring is unaffected.

That makes it usable by itself for the simple case — author the object active, let it disappear where it does not
belong — but it means mutually exclusive variants have to overlap in the scene, both active, each waiting to remove
itself. Adding a `PlatformSwitch` above them removes that restriction.

## PlatformSwitch

Put one on a common ancestor, such as the canvas root. It finds every `PlatformSpecific` beneath it, including
inactive ones, and sets each active or inactive to match the resolved platform.

Two things follow from it living on an always-active parent:

- It drives objects **both ways**, so the variants that do not apply can be authored inactive instead of overlapping.
- It is `[ExecuteAlways]`, so the scene re-arranges itself the moment the platform changes — no play mode needed.

It is `[DisallowMultipleComponent]`, and one per screen or canvas is normally enough.

## PlatformResolver

`PlatformResolver.Current` is what both components read instead of `Application.platform`.

In a build it is simply `Application.platform`. In the editor — edit mode, play mode and play-mode tests alike — it is
a simulated answer, because `Application.platform` would otherwise report `OSXEditor` or `WindowsEditor` and no mobile
layout would ever be visible. Two settings decide it, both stored in `EditorPrefs`:

| Setting | Meaning |
| --- | --- |
| `FollowDeviceSimulator` | While on (the default), a Device Simulator presenting a device chooses the platform. |
| `Simulated` | The explicit platform used whenever the simulator is not presenting one. |

`DeviceSimulatorPlatform` reports what the simulator is presenting, or `null` when it is presenting nothing — the
shim reports the desktop editor platform while the Game view is in plain Game mode, and that answer is treated as
"nothing to follow" rather than passed on. This matters: were the editor platform allowed through, every tagged object
would fail to match and the whole screen would empty itself. `Simulated` is always a real shipping platform, so it is
a safe fallback by construction.

Subscribe to the `Changed` event to re-apply anything else that depends on the platform. Note that it is a static
event, so with editor Domain Reload disabled its subscribers survive between play sessions — call
`ClearEventSubscribers` from an enter-play-mode reset hook.

## Following the Device Simulator

With `FollowDeviceSimulator` on, choosing a device in the **Game view → Simulator** decides the platform, which keeps
this system agreeing with everything else that reads `UnityEngine.Device`: safe area, resolution and Unity
Localization's platform entry overrides all follow the same pick.

The package adds a **Platform Resolver** panel to the Simulator window holding the same toggle and a readout of what
the platform currently resolves to. The panel is also how the live update works: it listens to the simulator's device
change event and raises `PlatformResolver.Changed`, so an `[ExecuteAlways]` `PlatformSwitch` re-arranges the scene
immediately. Opening and closing the simulator does the same, since both change the answer as much as swapping
devices does.

> [!NOTE]
> The device names in the simulator's built-in generic list do not state their platform. Both "Tablet" entries are
> iPads, for instance. The Simulator window's control panel shows the real device model if you need to check.

### Localized text refreshes with it

When Unity Localization is installed, a device change also re-resolves every localized component in the loaded scenes
and in an open prefab stage.

This is needed because Localization reads a `PlatformOverride` *inside* the table entry lookup rather than announcing
it through an event, so components that already resolved keep showing the entry they picked under the previous
platform. Re-running the lookup is cheap — tables stay cached, only the entry is resolved again — and the values it
writes go through Localization's property drivers, so nothing counts as a scene or prefab modification.

## In play-mode tests

Turn `FollowDeviceSimulator` off and set `Simulated` explicitly, then restore both afterwards. Two reasons to be
explicit rather than to rely on the fallback:

- A simulator window left open on one machine would otherwise change what the test asserts.
- `EditorPrefs` persist, so a test that ends early leaves its choice behind in the editor.

The explicit setting is also the *only* lever available. There is no public API for choosing a Device Simulator
device, so a test cannot drive the simulator itself.

## Comparison with `#if UNITY_IOS`

Preprocessor branches change what the editor itself compiles and shows you. This system leaves authoring alone and
decides at runtime, which is what makes it possible to look at both platforms' layouts in one editor session.
