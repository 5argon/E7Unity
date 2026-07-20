# LerpOnRatio

A single `0..1` float driven by the screen's aspect ratio.

## The idea

Phones and tablets are not merely different sizes, they are different *shapes*. A layout that looks right on a
squarish tablet is usually too sparse on a long phone, and the fix is rarely "one layout or the other" — it is a
smooth adjustment between them.

`LerpOnRatio` turns the current aspect ratio into one normalized number you can feed anywhere a `t` is wanted:

```csharp
using E7.E7Unity;

public class Banner : MonoBehaviour
{
    [SerializeField] private LerpOnRatio ratio;
    [SerializeField] private RectTransform target;

    void Update()
    {
        // Slide the banner further down the squarer the screen gets.
        target.anchoredPosition = Vector2.Lerp(tallPosition, widePosition, ratio.Value);
    }
}
```

The component defines an implicit conversion to `float`, so `ratio` can often stand in for `ratio.Value` directly.

## The curve

The value comes from an `AnimationCurve` whose **time axis is the aspect ratio**, taken as the long side divided by
the short side. Reading it that way means the component behaves identically in portrait and landscape.

The default curve created by **Reset** covers the range worth caring about on mobile:

| Ratio | Value | Typical device |
| --- | --- | --- |
| `4:3` | `0` | iPad, squarish tablets |
| `16:9` | ~`0.67` | Older phones |
| `2:1` | `1` | Modern tall phones |

Edit the curve to change the response — it is an ordinary curve, so you can flatten a region, invert it, or push the
change to one end.

## Behaviour

`Value` re-reads `Screen` on every access, so it follows device rotation and window resizing with no bookkeeping and
no event to subscribe to. That also means it is a small amount of work per read — sample it once into a local if you
need it many times in a frame.

The curve is only clamped by its own keys, so a curve that leaves `0..1` will return values outside that range.
