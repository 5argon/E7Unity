# SceneEntryPoint

One place where a scene decides it has actually started.

## The problem

Left alone, every component in a scene begins in its own `Start`, in an order nobody chose. That is fine until you
want something to happen *for every scene* right as it opens — pause before the opening animation, disable input for a
moment, report the screen to analytics. There is no single place to hang that behaviour, so it ends up copy-pasted
into each scene's main script.

`SceneEntryPoint` is that single place. Components stop starting themselves, and instead wait to be started by it.

## Starting a scene

Put the component on a game object and it will begin the scene for you, running three things in order:

1. The `ISceneEntryPoint` on its own game object, if there is one.
2. The `entryPoint` UnityEvent.
3. `entryDirector`, if assigned.

The interface is the tidiest route for a scene's main script:

```csharp
using E7.E7Unity;

public class TitleLogic : MonoBehaviour, ISceneEntryPoint
{
    public void EntryPoint()
    {
        // The scene is settled. Begin.
    }
}
```

Put that script on the same game object as the `SceneEntryPoint` and it is found automatically — no wiring.

`Awake` still runs at its normal time in every case. Only the entry point is deferred, so `Awake` remains the right
place to fetch references and set up state.

## The entry director

`entryDirector` is not merely played — it is played **and** evaluated on that same frame. Without the immediate
evaluation, a `PlayableDirector` would leave the scene showing its un-posed state for one frame before the first
frame of the timeline is applied, which reads as a flash of the wrong thing.

## The editor settling delay

In the editor only, and only for the first scene of a play session, the entry point waits eight frames before firing.

Entering play mode costs a noticeable hitch that a build does not have. Without the wait, that hitch lands on top of
the scene's opening animation, and every animation looks like it stutters. Waiting a few frames separates the two, so
what you are watching is the animation and not the editor.

Assign a `CanvasGroup` to `lagCombatUninteractable` and it stops receiving raycasts for the duration of that wait,
which prevents an input during those frames that would be impossible in a real build.

Builds never wait.
