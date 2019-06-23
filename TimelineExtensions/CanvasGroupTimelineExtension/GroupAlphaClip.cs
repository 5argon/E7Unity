using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.Timeline
{
    public class GroupAlphaClip : PlayableAsset, ITimelineClipAsset
    {
        public GroupAlphaClipBehaviour template;
        public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            return ScriptPlayable<GroupAlphaClipBehaviour>.Create(graph, template);
        }
    }
}
