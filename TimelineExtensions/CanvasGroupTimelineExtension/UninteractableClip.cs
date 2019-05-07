using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity
{
    /// <summary>
    /// On the duration of this clip, <see cref="CanvasGroup"> is uninteractable.
    /// Except the last and the first frame of the timeline, even with this clip, it is still interactable.
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