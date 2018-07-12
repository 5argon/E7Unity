# RuntimeScriptableObject

A `ScriptableObject` that if you put it in `Resources` folder, you can use a static property `.Get` to load it at runtime to a static variable space! But you have to name it exactly like your subclass name. It uses `typeof(T).Name` to get the name of your class at runtime.

The `static` variable is cached automatically on the first load. Subsequent `.Get` does not call `Resources.Load` again.

You could combo with "Odin Inspector" to make a "game settings" with good UI in editor, then you could request its value easily at runtime. Note that things in `Resources` folder is not difficult to pry and hack by players.