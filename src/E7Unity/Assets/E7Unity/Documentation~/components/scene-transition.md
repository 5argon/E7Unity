# SceneTransition

Load a scene from something that cannot call code directly.

## The component

`SceneTransition` is deliberately trivial. It exists so that a `UnityEvent` — a button's `onClick`, an animation event
— can load a scene without you writing a one-line script for every scene you want to reach.

`LoadScene(string sceneName)` performs a `LoadSceneMode.Single` load, replacing the current scene. Pick it in the
inspector's UnityEvent dropdown and type the scene name into the string argument.

The scene must be in the build's scene list, as with any `SceneManager.LoadScene` call.

## From a Timeline

The same component doubles as a Timeline notification receiver, which lets an outro sequence end by moving to the next
scene — with the moment chosen on the timeline itself rather than by a script watching the director's clock.

Two halves:

- **`SceneTransitionMarker`** — a marker you place on the timeline. It carries the `sceneName` to load. It only makes
  the request; on its own it does nothing.
- **`SceneTransition`** — the component that listens and performs the load.

Timeline delivers notifications to the object bound to the track holding the marker, so the `SceneTransition`
component must be on that bound object. A marker on an unbound track is silently ignored.

Any notification that is not a `SceneTransitionMarker` is ignored, so the component is safe to leave on an object that
receives other markers too.

> [!NOTE]
> `SceneTransitionMarker` lives in the `E7.E7Unity` namespace rather than `E7.E7Unity.Timeline`, because it belongs to
> this component rather than to the Timeline extensions.
