# PreprocessorPreprocessor 

## NANI

### Why working with preprocessors?

To acts as a super early unit test. Unity comes with several automatic preprocessors :

- UNITY_ENGINE : While in editor only.
- DEVELOPMENT_BUILD : When checking Development Build in the build setting while in a real build AND also automatically on Test Runner real device build.
- UNITY_IOS, UNITY_ANDROID, UNITY_STANDALONE : For real build these are turned on.

If you have some code in your game which intends to be used only by you, take the time to wrap it in UNITY_ENGINE. This code can be a whole method, or just one accessor which sounds cheat-y like PlayerHp with `set` accessor. (You might wrap the `set` part so that it works only with your editor tools)

If you press build and see errors that something does not exist, you have broken your own rule and then you will never slip the code in the real build that it should not be. In exchange your code looks ugly with preprocessors, some people hates it but I think it is a good sanity check.

### The problem

So for example, I have a class which calls a cheat dialog box just for my own use. But I also want to have it in my mobile cheat build and not in the real one. The tag ideally should be : 

```
#if UNITY_EDITOR || DEVELOPMENT_BUILD
```

Then I just check the box when building so that I can have cheats. Easy?

But there's a problem with DEVELOPMENT_BUILD tag, it auto turned on when using Test Runner in the real device. It also turned on when you just want to see a log printing in the game at the bottom-left corner, a feature of Development Build.

When doing an integration testing I want the game to be in a shippable state, not the state which contains various cheats. Pressing real device test automatically adds DEVELOPMENT_BUILD and include my cheat codes. This tells us that DEVELOPMENT_BUILD should not be reuse outside of its intended definition. (Testing & log visible)

I cannot use DEVELOPMENT_BUILD to include my cheats, I have to make my own DEV_BUILD and remember to add that to player settings before running the test so that I can have cheats without the log. Now the tags are

```
#if UNITY_EDITOR || DEV_BUILD
```

Next problem, the integration test code. This code must be outside of `Editor` folder, since they can be run in the real device. The code must be able to include together with the game. By definition, we don't want this code in the real game. How to exclude it from the real game?

1. Use the new Assembly Definition (.asm) exclude platform? It does not work since just from Android, Editor, iOS you can't tell when is the test, when is the real build. 
2. The only way is to check DEVELOPMENT_BUILD. So, all integration test code should be wrapped from head to toe with DEVELOPMENT_BUILD and UNITY_EDITOR. This way you can't make a mistake of calling their method in the real game. It will result in a compile error when you build.

```
#if UNITY_EDITOR || DEVELOPMENT_BUILD
```

Now, some tests might have to access cheat codes (not the contrary), you might have to add `&& DEV_BUILD`.

```
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && DEV_BUILD)
```

Things get more complicated as my game which is meant for mobile, now can be built into a standalone to function as a separated tools (it uses many code together with the game, but only necessary scenes are included). So I will mark any code which intended for this tool build as UNITY_STANDALONE because I don't want unnecessary things to be leaked if the person using the tools decided to share it and someone else hack the code. Also it makes the scope of my "standalone tool" tighter, as when I violated my own definition the compiler will complain.

And then what if I will have the real standalone game for sale in the future? Now I have to change the tag to something like TOOL_BUILD and don't forget to add that tag when switching the build to standalone.

You might have your own reason more than this. But at this point I wish I could `#if IN_EDITOR || INTEGRATION_TEST || TOOL_BUILD || CHEAT_BUILD` and have it expand to it's correct definition. Sadly C# can only define in the same file. You have to put your define in every file for this to work. It is not feasible.

The only project-wide defines is with Unity's player settings. But they are not automatic. You have to sync it to your current build settings or the build you are going to do by yourself.

Now it is either :
1. You do it in PlayerSettings
2. You have 