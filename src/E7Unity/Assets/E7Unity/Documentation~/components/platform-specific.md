# PlatformSpecific

Keep a game object only on the platforms you list.

Add the component, fill the `platforms` array with the `RuntimePlatform` values the object is meant for, and it will
deactivate itself everywhere else.

The check happens once in `Awake`, comparing `Application.platform` against the list. An empty list therefore means
"nowhere", and the object is always deactivated.

## Why it is useful while authoring

Because the decision is deferred to runtime, the object can be left **active** in the scene or prefab while you work
on it. You see it in the editor, arrange it, and it quietly disappears on the platforms it is not for.

That is the difference from stripping with `#if UNITY_IOS` and friends: preprocessor branches change what the editor
itself shows you, whereas this leaves authoring alone and only affects the running game.

## Caveats

- It only ever *deactivates*. An object that starts inactive in the scene will be activated by this component if the
  platform matches, so use it on objects you are happy to have switched on.
- The decision is made in `Awake` and never revisited.
- `Application.platform` in the editor reports the editor platform (`OSXEditor`, `WindowsEditor`, `LinuxEditor`), not
  the build target you are aiming at. Include the editor platform in the list if you want to see the object while
  playing in the editor.
