using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity
{
    /// <summary>
    /// On the duration of <see cref="UninteractableClip">, <see cref="CanvasGroup"> is uninteractable.
    /// Except the last and the first frame of the timeline, even with the clip, the <see cref="CanvasGroup"> is still interactable.
    /// Useful for making UI animations, where usually you don't want your player to mess with the UI while the intro/outro sequence is still running.
    /// The clip should usually span the entire length, but you could shrink them a bit to allow player to interact with the UI tree earlier.
    /// </summary>
    [TrackBindingType(typeof(CanvasGroup))]
    [TrackColor(0.4f, 0, 0)]
    [TrackClipType(typeof(UninteractableClip))]
    public class UninteractableTrack : TrackAsset
    {
        public override bool isEmpty => false;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<UninteractableTrackMixerBehaviour>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (director != null)
            {
                var binding = director.GetGenericBinding(this);
                if (binding is CanvasGroup cg)
                {
                    driver.AddFromName<CanvasGroup>(cg.gameObject, "m_Interactable");
                }
            }
        }

        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.start = 0;
            //Clip starts with some preset duration, we want it to stretch the full length but that preset duration ruins the timeline length.
            clip.duration = float.Epsilon;
            //Now we can ask for the correct timeline length before the clip came
            clip.duration = clip.parentTrack.parent.duration;
        }
    }

}