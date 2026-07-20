# CanvasGroup alpha

Fade an entire uGUI tree from Timeline clip weight.

**Track:** `GroupAlphaTrack` · **Clip:** `GroupAlphaClip` · **Binding:** `CanvasGroup`

## Why a CanvasGroup

The obvious way to make a UI tree appear and disappear on a timeline is an Activation Track, which switches the game
objects themselves. That works, but every flip re-runs layout and wakes each component underneath — expensive, and
often visible as a hitch on a large panel.

Fading the tree's `CanvasGroup` alpha instead leaves the hierarchy entirely alone. Nothing is enabled or disabled;
the tree simply stops being drawn.

## Using it

Add a **E7.E7Unity / GroupAlphaTrack** to the timeline and bind it to the `CanvasGroup` at the root of the tree you
want to fade. Add a clip and the alpha follows that clip's **weight**.

Because it is driven by weight, the ordinary Timeline gestures for shaping weight are exactly the gestures that shape
the fade:

- Hold <kbd>Cmd</kbd> / <kbd>Ctrl</kbd> and drag a clip's edge to slope it into a fade in or out.
- Overlap two clips to cross-fade between them.

Contributions from overlapping clips are summed, then clamped to `0..1`.

## Capping a clip's alpha

Each clip has an `alphaScale` (`0..1`, default `1`), the alpha it contributes at full weight. Use it when you want a
clip to settle at partially transparent while still using the full weight range for its fade — a dimmed backdrop that
fades to 40% rather than to opaque, for instance.

The blended result is the sum of every clip's weight multiplied by its own scale.

The clip inspector draws these settings inline rather than behind Timeline's usual `template` foldout.

## Clip capabilities

The clip declares **Blending** and **Extrapolation**. Extrapolation lets the alpha be held before and after the clip
instead of snapping back to zero, which is usually what you want at the end of an intro sequence.

## Editor preview

The track registers the `CanvasGroup` alpha with Timeline's preview system, so scrubbing in the editor does not leave
the alpha permanently modified once preview ends.
