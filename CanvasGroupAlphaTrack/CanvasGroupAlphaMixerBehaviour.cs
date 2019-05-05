using UnityEngine;
using UnityEngine.Playables;

public class CanvasGroupAlphaMixerBehaviour : PlayableBehaviour
{
    public override void ProcessFrame(Playable playable, FrameData info, object playerData)
    {
        if (playerData is CanvasGroup cg)
        {
            float finalAlpha = 0;
            int inputCount = playable.GetInputCount();
            for (int i = 0; i < inputCount; i++)
            {
                var clip = (ScriptPlayable<CanvasGroupAlphaBehaviour>) playable.GetInput(i);
                var weight = playable.GetInputWeight(i);
                finalAlpha += weight;
                //cg.interactable = clip.GetBehaviour().interactable;
            }
            cg.alpha = Mathf.Clamp01(finalAlpha);
        }
    }
}
