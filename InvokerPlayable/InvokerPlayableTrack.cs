using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine;

[TrackColor(255/255f, 69/255f, 131/255f)]
public abstract class InvokerPlayableTrack<T> : TrackAsset where T : class, INamedInvoker
{
    public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
    {
        foreach (TimelineClip clip in m_Clips)
        {
            T clipAsset = clip.asset as T;
            clip.displayName = clipAsset.SelectedMethodToString();
        }

        ScriptPlayable<InvokerPlayableBehaviour> playable = ScriptPlayable<InvokerPlayableBehaviour>.Create(graph, inputCount);
        return playable;
    }

}