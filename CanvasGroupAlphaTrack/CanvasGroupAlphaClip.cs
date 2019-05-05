using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

public class CanvasGroupAlphaClip : PlayableAsset, ITimelineClipAsset
{
    public bool interactable;
    public ClipCaps clipCaps => ClipCaps.Extrapolation | ClipCaps.Blending;

    public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
    {
         //return Playable.Create(graph);
        return ScriptPlayable<CanvasGroupAlphaBehaviour>.Create(graph);
    }
}
