using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.Timeline
{
    /// <summary>
    /// If having more than 1 clip, on the duration of <see cref="UnblocksRaycastsClip">, <see cref="CanvasGroup"> is uninteractable.
    /// If the track contains no clips at all, the entire track is uninteractable as a special case as that's common thing I want to do.
    /// 
    /// Useful for making UI animations, where usually you don't want your player to mess with the UI while the intro/outro sequence is still running.
    /// The clip should usually span the entire length, but you could shrink them a bit to allow player to interact with the UI tree earlier.
    /// </summary>
    [TrackBindingType(typeof(CanvasGroup))]
    [TrackColor(0.4f, 0, 0)]
    [TrackClipType(typeof(UnblocksRaycastsClip))]
    public class UnblocksRaycastsTrack : TrackAsset
    {
        public UnblocksRaycastsTrackMixerBehaviour template;

        /// <summary>
        /// Make the track evaluate even if empty.
        /// </summary>
        public override bool isEmpty => false;

        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            //Debug.Log($"Creating track mixer {Time.frameCount}");
            return ScriptPlayable<UnblocksRaycastsTrackMixerBehaviour>.Create(graph, template, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (director != null)
            {
                var binding = director.GetGenericBinding(this);
                if (binding is CanvasGroup cg)
                {
                    driver.AddFromName<CanvasGroup>(cg.gameObject, "m_Interactable");
                    driver.AddFromName<CanvasGroup>(cg.gameObject, "m_BlocksRaycasts");
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
            if(clip.duration <= float.Epsilon)
            {
                clip.duration = 1;
            }
        }
    }

}