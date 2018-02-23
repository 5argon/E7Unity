# InvokerPlayable

This is a "kit" to create a timeline asset that can invoke parameterless methods in your game's class when the playhead arrives at the clip's beginning or running backward and hit the clip's end.

It is an abstract class that requires a dictionary of pairing of `Enum` and a parameterless delegate, and a property that tells which enum is currently selected. Then you could make a public variable of that `Enum` so Unity shows them as a drop down and link that up with the required abstract property. You just built a selectable method invoker that the compiler can check! Yeah!

## Subclassing example

```csharp
[System.Serializable]
public class ResultEntryPointDirector : InvokerPlayableAsset<ResultEntryPoint>
{
    private enum SelectedMethod
    {
        LogYo,
        RedPerfectTransform,
        Unlocking,
        DisplayBonusGain,
        Prompt
    }

    [SerializeField] SelectedMethod methodToInvoke;

    protected override Enum selectedMethod => methodToInvoke;

    protected override Dictionary<Enum, Action> MethodPairings(ResultEntryPoint resultEntryPoint) => new Dictionary<Enum, Action>()
    {
        [SelectedMethod.LogYo] = () => Debug.Log("yo")
        [SelectedMethod.RedPerfectTransform] = resultEntryPoint.RedPerfectTransformActivate
    };
}
```

After this, connect the `ExposedReference<T>` in that appears in your clip's inspector with the thing in your scene, that will automatically becomes `MethodPairings`'s argument.
Name the `Enum` as anything that you like it to be named at the drop down.

(You see the pain point is you have to type the enum 2 times, but it is better than magic strings I think...)

What you get. You can see when the playhead crosses the clip it logs as specified in the dictionary : 

![subclassing result](https://thumbs.gfycat.com/BeneficialDigitalFieldmouse-size_restricted.gif)

(Full size : https://gfycat.com/BeneficialDigitalFieldmouse)

(Actually you can't do this outside of Play Mode, since I have `Application.isPlaying` before the `.Invoke()` to prevent it wrecking havoc on your scene while previewing.)

You can use Unity's general purpose "Playable Track" to host the clip, or maybe to reduce clutter you could also create your own specialized one. It takes just 3 lines using the provided Track class for subclassing but please make a new file with the same name as class or Unity might complain.

```csharp
using UnityEngine.Timeline;
[TrackClipType(typeof(ResultEntryPointDirector))]
public class ResultEntryPointTrack : InvokerPlayableTrack<ResultEntryPointDirector>{}
```