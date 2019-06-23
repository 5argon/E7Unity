using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ButtonExceed : Button 
{
    [UnityEngine.Serialization.FormerlySerializedAs("downAction")]
    public UnityEvent onDown;
    public Graphic[] additionalTintTargetGraphics = new Graphic[0];
    public AnimationTriggersExceed animationTriggersExceed = new AnimationTriggersExceed();
    public LegacyAnimator buttonAnimator;
    public bool noChangeDisable;

    private List<string> LimitToTriggers => buttonAnimator?.LimitToTriggers();

    public override void OnPointerDown(PointerEventData eventData)
    {
        onDown.Invoke();
        base.OnPointerDown(eventData);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        Color tintColor;
        Sprite transitionSprite;
        string triggerName;

        switch (state)
        {
            case SelectionState.Normal:
                tintColor = colors.normalColor;
                transitionSprite = null;
                triggerName = animationTriggers.normalTrigger;
                break;
            case SelectionState.Pressed:
                tintColor = colors.pressedColor;
                transitionSprite = spriteState.pressedSprite;
                triggerName = animationTriggers.pressedTrigger;
                break;
            case SelectionState.Disabled:
                if (!noChangeDisable)
                {
                    tintColor = colors.disabledColor;
                    transitionSprite = spriteState.disabledSprite;
                    triggerName = animationTriggers.disabledTrigger;
                }
                else
                {
                    tintColor = colors.normalColor;
                    transitionSprite = null;
                    triggerName = animationTriggers.normalTrigger;
                }
                break;
            default: //Highlighted has no effect.
                tintColor = colors.normalColor;
                transitionSprite = null;
                triggerName = animationTriggers.normalTrigger;
                break;
        }

        if (gameObject.activeInHierarchy)
        {
            switch (transition)
            {
                case Transition.ColorTint:
                    StartColorTween(tintColor * base.colors.colorMultiplier, instant);
                    break;
                case Transition.SpriteSwap:
                    DoSpriteSwap(transitionSprite);
                    break;
                case Transition.Animation:
                    TriggerAnimation(triggerName);
                    break;
            }
        }
    }

    bool ColorTintOrSpriteSwap => transition == Transition.ColorTint || transition == Transition.SpriteSwap;

    void StartColorTween(Color targetColor, bool instant)
    {
        if (additionalTintTargetGraphics == null && additionalTintTargetGraphics.Length == 0 && targetGraphic == null)
            return;

        targetGraphic?.CrossFadeColor(targetColor, instant ? 0f : /*colors.fadeDuration*/0, true, true);
        foreach (Graphic g in additionalTintTargetGraphics)
        {
            g?.CrossFadeColor(targetColor, instant ? 0f : /*colors.fadeDuration*/0, true, true);
        }
    }

    void DoSpriteSwap(Sprite newSprite)
    {
        if (image == null)
            return;
        image.overrideSprite = newSprite;
    }

    /// <summary>
    /// Uses legacy animation.
    /// </summary>
    void TriggerAnimation(string triggername)
    {
        if (transition != Transition.Animation || buttonAnimator == null || !buttonAnimator.isActiveAndEnabled || string.IsNullOrEmpty(triggername) || Application.isPlaying == false)
            return;

        buttonAnimator.SetTrigger(triggername);
    }

    /// <summary>
    /// In case the animation got stuck before disabling this should bring it back to normal.
    /// </summary>
    protected override void OnEnable()
    {
        if (buttonAnimator != null && Application.isPlaying)
        {
            if (animationTriggers.normalTrigger != "")
            {
                buttonAnimator.SampleFirstFrame(animationTriggers.normalTrigger);
            }
        }
        base.OnEnable();
    }
}
