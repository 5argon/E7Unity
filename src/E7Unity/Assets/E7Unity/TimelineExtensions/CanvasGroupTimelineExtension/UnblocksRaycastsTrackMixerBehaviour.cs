using System;
using UnityEngine;
using UnityEngine.Playables;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Decides, each frame, whether the bound <see cref="CanvasGroup"/> blocks raycasts, and where to leave it once
    /// playback ends.
    /// </summary>
    /// <remarks>
    /// The rule is inverted from what the names suggest: the group stops blocking raycasts — meaning the player
    /// <b>cannot</b> click — whenever a clip is in effect, or whenever the track has no clips at all. With clips
    /// present but none currently weighted, the group blocks raycasts and the UI is clickable again.
    /// </remarks>
    [Serializable]
    public class UnblocksRaycastsTrackMixerBehaviour : PlayableBehaviour
    {
#pragma warning disable 0649
        [SerializeField] private PostPlaybackState postPlaybackState;
#pragma warning restore 0649

        /// <summary>
        /// Where the bound <see cref="CanvasGroup"/> is left once the graph is torn down.
        /// </summary>
        public enum PostPlaybackState
        {
            /// <summary>
            /// Leave the group blocking raycasts, so the player can click things.
            /// </summary>
            BlocksRaycasts,

            /// <summary>
            /// Leave the group letting raycasts through, so the player cannot click things.
            /// </summary>
            UnblocksRaycasts,

            /// <summary>
            /// Restore whatever the group's value was when this track first touched it.
            /// </summary>
            Revert
        };

        private CanvasGroup affectedCg;
        private bool blocksRaycastsOriginal;

        /// <summary>
        /// Applies <see cref="PostPlaybackState"/> to the bound group as the graph is destroyed.
        /// </summary>
        /// <param name="playable">The playable being destroyed.</param>
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

        /// <inheritdoc/>
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
