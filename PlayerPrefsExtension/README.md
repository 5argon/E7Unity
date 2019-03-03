# Player Prefs Extension

You can now use `enum` to remember something persistent in [`PlayerPrefs`](https://docs.unity3d.com/ScriptReference/PlayerPrefs.html) quick and easy.

## Strength and weakness of the original `PlayerPrefs`

- Any method calls on `PlayerPrefs` API do not access the disk. It has a separated memory loaded in the game.
- Save manually to disk to the correct place cross-platform with [`PlayerPrefs.Save()`](https://docs.unity3d.com/ScriptReference/PlayerPrefs.Save.html)
- Save automatically on application quit cross-platform to reduce the hassle.
- Built-in anti tampering because it always comes with default value resolution. Imagine someone edit something that should be an `int` into a `string`.
- Alternative faster storage to your save file. Saving all that details in the save file would be too unwieldly and costly if you do compression and encryption on your save.
- They are sitting there clear as day, as an easily hackable and editable `.plist` file. They are suitable for things like remembering last cursor position in a particular menu. `PlayerPrefs` is for **preferences**, not for saving sensitive data.

This extension aims to make `PlayerPrefs` easier to use. It is not to make `PlayerPrefs` any more glorious than its original design.

## Requirements

- C# 7.3

It utilize the latest, cutting-edge technology, [Enum constriant](https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/generics/constraints-on-type-parameters#enum-constraints)!! But I did nothing useful from the constrained type currently lol