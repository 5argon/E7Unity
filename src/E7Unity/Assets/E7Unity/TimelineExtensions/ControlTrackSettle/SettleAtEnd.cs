using UnityEngine;
using UnityEngine.Playables;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Marks a <see cref="PlayableDirector"/> driven by a control track as one whose final pose has to survive the
    /// end of playback, so <see cref="DirectorSettle"/> pushes it to its own duration once the driving Timeline
    /// stops.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A control track writes its target's time only from inside the clip's own frame processing, and the value it
    /// writes is the playhead clamped to the target's duration. Nothing recomputes that value once the playhead
    /// leaves the clip, so the target keeps whatever partial time its last evaluation happened to produce. On a
    /// frame that stalls long enough to step over the stretch of clip past the target's content, the target is
    /// left mid-animation for good — a curtain caught half open, a camera stopped short of its mark.
    /// </para>
    /// <para>
    /// The component carries no settings — its presence is the whole message, and it declares intent rather than
    /// doing work. Attach it to the game object holding the sub-director whose last frame matters.
    /// </para>
    /// <para>
    /// Leave it off directors whose ending is a handover rather than a pose. A sub-director that spends its last
    /// frame returning a skeleton, an animator or a particle system to component-driven playback wants to be let
    /// go, not pinned; evaluating it again reasserts the Timeline's own state over whatever took over.
    /// </para>
    /// <para>
    /// Where control tracks nest, mark every level whose pose matters. <see cref="DirectorSettle"/> descends only
    /// through the targets it settles, so an unmarked director in the middle of a chain also shields everything
    /// below it.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(PlayableDirector))]
    public class SettleAtEnd : MonoBehaviour
    {
    }
}
