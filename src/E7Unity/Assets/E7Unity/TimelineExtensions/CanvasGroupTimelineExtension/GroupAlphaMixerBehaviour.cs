using UnityEngine;
using UnityEngine.Playables;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Blends every clip on a <see cref="GroupAlphaTrack"/> into the bound <see cref="CanvasGroup"/>'s alpha.
    /// </summary>
    /// <remarks>
    /// Each clip contributes its weight multiplied by its own
    /// <see cref="GroupAlphaClipBehaviour.alphaScale"/>; the contributions are summed and clamped to <c>0..1</c>, so
    /// overlapping clips cross-fade rather than fight.
    /// </remarks>
    public class GroupAlphaMixerBehaviour : PlayableBehaviour
    {
        /// <inheritdoc/>
        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            if (playerData is CanvasGroup cg)
            {
                float finalAlpha = 0;
                int inputCount = playable.GetInputCount();
                for (int i = 0; i < inputCount; i++)
                {
                    var sp = (ScriptPlayable<GroupAlphaClipBehaviour>)playable.GetInput(i);
                    var weight = playable.GetInputWeight(i);
                    finalAlpha += weight * (sp.GetBehaviour().alphaScale);
                }
                cg.alpha = Mathf.Clamp01(finalAlpha);
            }
        }
    }
}
