# E7Unity - A collection of Unity things that Exceed7 Experiments is using.

I have this as a Git submodule on both of my real game projects. Maybe someone else needs it, so I decided to share it for free. Maybe you can submodule it too, or just copy what you want to use. The license for everything is MIT.

I am not taking any responsibility for any problem caused by this, if there are bugs, they don't compile (should not happen since it compiles in my games...), or cause revenue loss to your game!

But I think they are useful :D

Read about each one below, or maybe there are comments in the code.

## CachedData

The DIY cache-able data structure! The class actually cannot automatically cache anything, you must do it yourself by setting its Data accessor, lol.

Currently responsible for huge performance boost in Gameplay Scene of Mel Cadence. If lane does not change, any other things read lane's data from CachedData. If it changes, each CachedData gets the SetDirty(), and my other script will update the Data before anyone can get it again. (If someone did get it while it's dirty, an exception will be thrown.)

## GraphicConstructionKit

With these seemingly useless bits of graphics you can make something wonderful. Actually, an entire game's effects in Duel Otters was animated from these bits. Pack them into an atlas!

## IntegrationTest

Cool classes for doing that. Contains some cheating methods which can save you pain in querying something to test from the scene.

## Mercy

If you are doing dependency injection in Unity you will notice that you could not "new" the MonoBehaviour, thus you cannot inject things on constructor. You can't use inspector drag-and-drop either if they are not MonoBehaviour. The last safest choice that isn't setting to a public variable is to assign them to {set; private get;} accessor during some entry point script. But would you forget to do that because compiler, Unity, and no one can check it for you if you leave any accessor null? Let Mercy help you! She will come to save you if you did forget! Using a very lame script parsing technology. (You can't have any conditional in your entry point script, or she will explode)

## Others

UIDisabler is pretty cool. You can paste it to any Game Object and you can collect UIs in it. Then with one function call they can be all disbled! I use this on every scene. In situation like what will happen if you press "Back"? Of course the scene will transition to previous scene. In that transition time, if player mash the screen something bad might happen so I can just disable all the buttons in the scene with this.

VerticalAdjustOnHorizontal is cool for UI. On iPad the screen is taller, you often wants to move things down. It's pretty tricky to do if you have the reference on width. With this one you can have width reference but things can move vertically! But only once on Awake..

## PlayerData

Use this to make a AES-encrypted binary save file when you don't want to make it plainly visible like in PlayerPrefs. It is the save file system in both Duel Otters and Mel Cadence. Knowing this, please don't try to hack the game's save file. __/|\__

DynaList is a tricky class that can do dictionary-like task, but it is serializable because it uses List<T>. I use this when I want to include dictionaries in the save file.

## SceneUtils 

CrossSceneConnector, this is insane. I am really proud of this girl. I can't work multi-scene without her.

## SimfileSFXSystem (Will be separated to a free Asset Store product later)

This is big! It is responsible to all SFXs you can hear in Duel Otters and Mel Cadence. Basically, I want to play sounds by script WITHOUT any magic strings. It's painful to write script for each SFXs, so this script can turn your entire SFX folder into a playable static methods in just a few seconds. Revolutionary! But you have to learn its system a bit.



