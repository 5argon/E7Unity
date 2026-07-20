# VersionNumber

Show the build's version in a TextMeshPro label.

Add the component, assign a `TextMeshProUGUI` to `versionNumber`, and on `Awake` it writes:

```
Version {Application.version}
```

`Application.version` comes from **Project Settings → Player → Version**, so the label follows the real build number
with nothing to keep in sync by hand. Handy on a title screen, a settings page, or a debug corner where you need to
know at a glance which build a tester is looking at.

Leaving `versionNumber` unassigned is harmless — the component does nothing.

The text is written once at `Awake` and never updated, which is all that is needed since the version cannot change
while the game runs.
