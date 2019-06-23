using UnityEngine;
using UnityEngine.Playables;

namespace E7.Timeline
{
    public class GroupAlphaMixerBehaviour : PlayableBehaviour
    {
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
