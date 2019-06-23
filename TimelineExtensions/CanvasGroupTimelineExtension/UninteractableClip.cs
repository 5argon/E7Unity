using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.Timeline
{
    /// <summary>
    /// On the duration of this clip, <see cref="CanvasGroup"> is not blocking raycast.
    /// </summary>
    public class UninteractableClip : PlayableAsset, ITimelineClipAsset
    {
        public ClipCaps clipCaps => ClipCaps.None;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            //Weight won't appear if return Playable.Null here lol
            return Playable.Create(graph);
        }
    }

}