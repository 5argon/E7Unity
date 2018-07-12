// Blurbs :
/*

SimpleSFXSystem - Kick start your game's sound effects for FREE.

Aren't you tired of finding only plugins that offers gargantuan amount of configutations PER SOUND (and gargantuan price tag), while you just want
to hear simple 2D sound as is?

SimpleSFXSystem (SSS) aims to offer the simplest framework to play your AudioClip in game as possible. FOR FREE. SSS's strength is the unrivaled workflow, probably has the record breaking time from throwing a bunch of .wav files into your project to hearing them in your game. Probably 1-2 minutes and all your sounds will be playable from any code.

Every game needs some sounds. Every time you start a project let's kick start your game with SimpleSFXSystem!

The plugins offers exactly these 4 essential core functions for MOST games :
1. A panel to manage all sound's volume. Essential in getting that perfect mix with the game and make the change reflects throughout your game.
2. A button to preview sound in that panel. While coding you can't remember all the sounds right?
3. Ability to route them to AudioMixer, so you can implement SFX slider in options screen and many more possibility such as effects.
4. A simple way to play SPECIFIC SFX from ANY code that is not relying on string so compiler can catch your error.
Such as : SoundEffectPlayer.Instance.PlayUIClick(); or SoundEffectPlayer.Instance.PlaySwordSlash(0.65f);

Thats it! I believe these are functions that most games need.

If your game needs 3D sound (positional sound) with area based reverb, etc. then SimpleSFXSystem is not for you.

However nowadays many wonderful games (especially on mobile) is without 3D sounds at all. Endless runners, match-3 puzzles, etc.
all just need good 2D sound system since there is no character position hence you (the player) are the only listener. SSS is for that.

I have been thinking, positional sound has becoming more and more "special case" for those FPS games. Unity have to offer them for flexibility though.

Advantages :

- Automatic code generation : I did mention you can play sounds like SoundEffectPlayer.Instance.PlaySwordSlash(0.65f) that offers tight integration with your
compiler. But do you need to code all of them up yourself to match with your SFX name? NO! If you store your SFXs in the same folder (you should),
with just a few clicks SSS can generate the code to play those sounds for you! Every time you adds more sounds, just click Generate Code and enjoy.
You can instantly play that SFX directly from anywhere. Function name is based on file name. (HealPotion -> PlayHealPotion)

- Nice and simplest control panel : After generating code, all those sounds magically became a nice interface!
Two extremely valuable controls include volume slider and preview button. Nothing more.
This allows each sound to neatly consume exactly 1 line in the editor which is very important. I have seen big plugins
that has too many settings per sound and when your project uses more that just looks bad and tiring to set things up.

- Super modular : You might asked where is my multiple randomized walk sound? Random volume on each plays? Pitch variations settings?
Code them up! You are a programmer! (See examples in the "Ideas" page.) This plugins tries to be smallest, but offers as much flexibility as possible.
Big and expensive plugins locks many niche functions inside them, but SSS leaves the creativity up to you.

- With full source code : Enough said!

- It's free : But not cheap in quality. This withstands my own projects with over 300,000 of downloads.

Why this is free? Well, I want SSS to be kind of new standard for those who is starting a Unity project out there.

I still remembered that day I started using Unity 3.0. It was very exciting, and I remembered just having
some sound effects dramatically improve the game. I want everyone to be able to feel that in their game. And the best way is free price tag.

Free is big, it removes many barriers. Imagine passionate kids starting out their Unity journey, and they can just get SSS from Asset Store
with few clicks.

Unity is growing fast, and there will be more crators in the future. I made that for them.

FAQ

Q. I want to play randomized audio for each plays, like step sound or jump sound.
A. You have to code a subclass of SoundEffect class for that. I have provided example of it in the Bonus folder.

Q. What about pitch variation on each plays? How to adjust pitch range?
A. You can code them all up as a subclass of SoundEffect class. I also provided examples of them.

Q. Ok, how about looping, continuous sound effects? I have made seamlessly loopable sound but I noticed your SSS is all PlayOneShot style...
A. For looping sound, unfortunately that is out of SSS's scope. I can add that and many more features, but it will defeat the purpose of simplicity. You should place a new AudioSource with "Loop" checked and command that as usual.

Q. Can I do positional sound?
A. I am sorry but no. SimpleSFXSystem is meant to be "simple". To get beyond that, it's exactly what Unity team provided for you!

Q. I can't seemed to hear the effect of adjusted volume settings on the fly while in play mode.
A. The one you hear in play mode is from prefab on the scene. If you are adjusting volume on the prefab in your Resources folder that will not take effect since SSS can see that you already have one in your scene and avoids re-cloning. Please exit play mode and enter again. Sorry! (But you can press preview button to hear and fine tune it in game's context while in play mode.)

Q. Since you provided the source code why not explain the big picture a bit so I can get started in modifying?
A. I have commented several parts of the code but I will explain here anyway.

The magic of editor volume sliders is because of "SoundEffect" class. Whenever you use this class, editor will be customized.
SoundEffect it is just a storage for audio with fancy editor. To play it "normally" you have to grab the AudioClip out of SoundEffect class and play it in your AudioSource.

That means if you have "the class" with a lot of public SoundEffect, that class GameObject's editor pane is basically turned into a big control panel. If you made this GameObject with this class attached in to a prefab, you now have in-editor sound effect control panel. What's left is to throw this prefab into all of your scenes so its all connected. But with SSS, it is even better...

"The class" I mentioned can be just any class, but if you subclass from SSS's "SoundEffectSource" class, you gain the ability to access all your sounds
from anywhere in any scripts with singleton pattern plus the ability to play the sound immediately without placing this prefab into all scenes. Although the requirement is you have to place the prefab in Resources folder so SSS can spawn it for you in run-time.

SSS not only helps providing these 2 "SoundEffect" and "SoundEffectSource" class but helps you generate all those codes from your audio assets as well.
The generated code works together tightly with these 2 classes so the adjusted volume has immediate effect and you can also override the volume.

When you call "SoundEffectSource"'s Play method, it will clone the GameObject needed to play the sound for you from Resources folder. By keeping your subclassed SoundEffectSource in the Resources folder, it acts as an in-editor settings panel and Instantiate into a real thing on runtime. Notice that this GameObject also have one personal AudioSource attached to it. This is the source of all your audio from this subclassed SoundEffectSource, which also responsible for routing sound to AudioMixer. Remember, this your player game object should have 2 scripts attached to it : subclass of SoundEffectSource and AudioSource.

Q. Is this really free? You better not hiding nasty things like limited time trials or something..
A. It is really free. However I hope *a little bit* that when you know this plugins you might became interested in my other paid plugins that is Introloop. :D

*/

// Introduction Video Ideas
/*

* Orchestral music *
This otter seems lively, but it would be even better with sounds.
I have several sound effects. Let's see in real time how long it takes to implement these new sounds.

* Stop watch begin *

....

Done.

* Stop watch stops. *

Jump sound too loud? Do I have to go to audio editor program?

No!

* Stop watch begin *

....

Done.

* Stop watch stops. *

What about my option screen's SFX slider?

Let's route it all to the mixer!

* Stop watch begin *

....

Done.

* Stop watch stops. *

SimpleSFXSystem (SSS)
Kick start your game's sound effects..

...

...for FREE

*/


// TODO:
// - Also auto generates a method that takes string to play the sound!
// - Preview audio with respect to volume settings.
// - Pitch variation support
// - Gen code by right clicking on folders or type folder name in popup. Folder structure should produce headers.
// - Auto connect audio file on clipboard paste.
// - Dynamic Script file. Automatically gen and paste. (might be dangerous!)
// - Support for spawning audio source for looping sound.
// - Make a demo. 2 otters shooting magic to each others. Left and right otters has different AudioMixer. UI buttons sound also has mixer.

// Base class for the "player" you will call from any script.
// Notice that it is an abstract class. The subclassed class will house all of your sound effects, which will in turn became
// a full fledged control panel for adjusting volumes, etc. Each SoundEffectSource can have one mixer routed to. If you need to route to different
// mixer, create another player. Good ideas might be : UISoundPlayer, CharacterSoundPlayer, EnemySoundPlayer, etc.

// When subclassing, sending itself via T allows this class to know the subclass class. (Like class MySFXPlayer : SoundEffectSource<MySFXPlayer> )
// and allowing the singleton pattern for any sources.

using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using System.Reflection;
using System.IO;
using System.Text;
#endif
using UnityEngine.Audio;
using System.Collections;
using System;

[RequireComponent(typeof(AudioSource))]
public abstract class SoundEffectSource<T> : MonoBehaviour where T : MonoBehaviour, new() {

    public AudioMixerGroup routeToMixerGroup;
    private AudioSource source;

#if UNITY_EDITOR


    [ContextMenu("Generate Code to Clipboard")]
    private void GenerateCode()
    {
        string[] actions = GetAllSoundNames("Actions");
        string[] uis = GetAllSoundNames("UIs");
        StringBuilder sb = new StringBuilder();

        sb.Append("\t[Header(\"UI Sounds\")]\r\n");

        foreach (string s in uis)
        {
            AppendDeclaration(sb, s);
        }

        sb.Append("\t[Header(\"Action Sounds\")]\r\n");
        foreach (string s in actions)
        {
            AppendDeclaration(sb, s);
        }

        foreach (string s in uis)
        {
            AppendPlayFunction(sb, s);
        }
        foreach (string s in actions)
        {
            AppendPlayFunction(sb, s);
        }

        EditorGUIUtility.systemCopyBuffer = sb.ToString();
        Debug.Log(uis.Length + " UI sounds and " + actions.Length + " action sounds code copied to clipboard.");

    }

    private void AppendDeclaration(StringBuilder sb, string s)
    {
        sb.Append("\tpublic SoundEffect ");
        sb.Append(Char.ToLower(s[0]));
        sb.Append(s.Substring(1));
        sb.Append(";\r\n");
    }

    private void AppendPlayFunction(StringBuilder sb, string s)
    {
        sb.Append("\r\n");
        sb.Append("\tpublic void Play");
        sb.Append(Char.ToUpper(s[0]));
        sb.Append(s.Substring(1));
        sb.Append("(float forceVolume = 555)\r\n");
        sb.Append("\t{\r\n");
        sb.Append("\t\tPlaySoundEffect(");
        sb.Append(Char.ToLower(s[0]));
        sb.Append(s.Substring(1));
        sb.Append(",forceVolume);\r\n");
        sb.Append("\t}\r\n");
    }
    public static string[] GetAllSoundNames(string subfolder)
    {
        string[] s = Directory.GetFiles(Application.dataPath + @"/-Common/Audio/" + subfolder, "*.???");
        for (int i = 0; i < s.Length; i++)
        {
            s[i] = Path.GetFileNameWithoutExtension(s[i]);
        }
        return s;
    }

#endif

    private static T instance;
    public static T Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType(typeof(T)) as T;
                if (instance == null)
                {
                    GameObject obj = (GameObject)Resources.Load(SimpleSFXSystemSettings.sourcePrefabFolder + typeof(T).Name);
                    instance = (GameObject.Instantiate(obj) as GameObject).GetComponent<T>();
                    instance.name = typeof(T).Name;
                    DontDestroyOnLoad(instance);
                }
                SoundEffectSource<T> cast = instance as SoundEffectSource<T>;
                cast.source = instance.gameObject.GetComponent<AudioSource>();
                cast.source.outputAudioMixerGroup = cast.routeToMixerGroup;
            }
            return instance;
        }
    }

    public void OnEnable()
    {
        if(instance == null)
        {
            //Enable with instance == null means we are not using static..
            source = GetComponent<AudioSource>();
        }
    }

    protected void PlaySoundEffect(SoundEffect sfx)
    {
        source.PlayOneShot(sfx.Get.audioClip, sfx.volume);
    }

    protected void PlaySoundEffect(SoundEffect sfx, float forceVolume)
    {
        source.PlayOneShot(sfx.Get.audioClip, forceVolume);
    }


}
