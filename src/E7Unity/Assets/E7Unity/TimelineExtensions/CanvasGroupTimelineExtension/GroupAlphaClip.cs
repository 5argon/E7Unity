using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// A clip on a <see cref="GroupAlphaTrack"/>. Its weight becomes the bound <see cref="CanvasGroup"/>'s alpha.
    /// </summary>
    /// <remarks>
    /// Supports blending, so overlapping two clips cross-fades, and extrapolation, so the alpha can be held before
    /// and after the clip rather than snapping back.
    /// </remarks>
    public class GroupAlphaClip : PlayableAsset, ITimelineClipAsset
    {
        /// <summary>
        /// Per-clip settings. Its fields are drawn directly in the clip inspector rather than behind a foldout.
        /// </summary>
        public GroupAlphaClipBehaviour template;

        /// <summary>
        /// Declares support for blending and extrapolation.
        /// </summary>
        public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;

        /// <inheritdoc/>
        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<GroupAlphaClipBehaviour>.Create(graph, template);
        }
    }
}
