using System;
using UnityEngine;
using UnityEngine.Playables;

namespace E7.Timeline
{
    [Serializable]
    public class UnblocksRaycastsTrackMixerBehaviour : PlayableBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private PostPlaybackState postPlaybackState;
#pragma warning restore 0649

        public enum PostPlaybackState
        {
            /// <summary>
            /// Blocks raycasts = you **could** click on things.
            /// </summary>
            BlocksRaycasts,

            /// <summary>
            /// Unblocks raycasts = you **could NOT** click on things, the ray went through.
            /// </summary>
            UnblocksRaycasts,

            Revert
        };

        private CanvasGroup affectedCg;
        private bool blocksRaycastsOriginal;
        public override void OnPlayableDestroy(Playable playable)
        {
            if (affectedCg != null)
            {
                switch (postPlaybackState)
                {
                    case PostPlaybackState.BlocksRaycasts:
                        affectedCg.blocksRaycasts = true;
                        break;
                    case PostPlaybackState.UnblocksRaycasts:
                        affectedCg.blocksRaycasts = false;
                        break;
                    case PostPlaybackState.Revert:
                        affectedCg.blocksRaycasts = blocksRaycastsOriginal;
                        break;
                }
            }
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            //Debug.Log($"Process frame {Time.frameCount}");
            if (playerData is CanvasGroup cg)
            {
                if (affectedCg == null)
                {
                    affectedCg = cg;
                    blocksRaycastsOriginal = cg.blocksRaycasts;
                }

                //Gymnastic back to the graph then drill down to the "timeline playable" (one step above track playables)
                // var graph = playable.GetGraph().GetRootPlayable(0);
                // var graphDuration = graph.GetDuration();
                // var graphTime = graph.GetTime();

                //Otherwise if found any clip, it is uninteractable.
                int inputCount = playable.GetInputCount();
                if (inputCount == 0)
                {
                    //Debug.Log($"Not blocking");
                    cg.blocksRaycasts = false;
                }
                else
                {
                    for (int i = 0; i < inputCount; i++)
                    {
                        var weight = playable.GetInputWeight(i);
                        if (weight > 0)
                        {
                            //Debug.Log($"Not blocking with clip");
                            cg.blocksRaycasts = false;
                            return;
                        }
                    }
                    //Debug.Log($"blocking without clip");
                    cg.blocksRaycasts = true;
                }
            }
        }
    }

}