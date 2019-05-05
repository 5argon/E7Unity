using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity
{
    [TrackBindingType(typeof(CanvasGroup))]
    [TrackColor(0.4f, 0, 0)]
    [TrackClipType(typeof(CanvasGroupAlphaClip))]
    public class CanvasGroupAlphaTrack : TrackAsset
    {
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<CanvasGroupAlphaMixerBehaviour>.Create(graph, inputCount);
        }

        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (director != null)
            {
                var binding = director.GetGenericBinding(this);
                if (binding is CanvasGroup cg)
                {
                    driver.AddFromComponent(cg.gameObject, cg);
                }
            }
        }
    }
}