using UnityEngine;
using UnityEngine.Playables;

namespace E7.E7Unity
{
    public class UninteractableTrackMixerBehaviour : PlayableBehaviour
    {
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is CanvasGroup cg)
            {
                //Gymnastic back to the graph then drill down to the "timeline playable" (one step above track playables)
                var graph = playable.GetGraph().GetRootPlayable(0);
                var graphDuration = graph.GetDuration();
                var graphTime = graph.GetTime();

                //Special case : if the timeline is at the end frame (holding or not) or at the beginning, it is interactable.
                if (graphTime == 0 || graphTime == graphDuration)
                {
                    cg.interactable = true;
                    return;
                }

                //Otherwise if found any clip, it is uninteractable.
                int inputCount = playable.GetInputCount();
                for (int i = 0; i < inputCount; i++)
                {
                    var weight = playable.GetInputWeight(i);
                    if (weight > 0)
                    {
                        cg.interactable = false;
                        return;
                    }
                }
                cg.interactable = true;
            }
        }
    }

}