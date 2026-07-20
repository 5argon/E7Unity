using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Marks a stretch of time during which the bound <see cref="CanvasGroup"/> lets raycasts pass straight through,
    /// so nothing under it can be clicked.
    /// </summary>
    /// <remarks>
    /// The clip carries no settings — its presence and extent are the whole message. See
    /// <see cref="UnblocksRaycastsTrack"/> for how a track full of these behaves.
    /// </remarks>
    public class UnblocksRaycastsClip : PlayableAsset, ITimelineClipAsset
    {
        /// <summary>
        /// No blending or extrapolation: the clip is either in effect or it is not.
        /// </summary>
        public ClipCaps clipCaps => ClipCaps.None;

        /// <inheritdoc/>
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            //Weight won't appear if return Playable.Null here lol
            return Playable.Create(graph);
        }
    }
}
