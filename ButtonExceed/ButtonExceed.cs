using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public class ButtonExceed : Button 
{
    [Space]
    public LegacyAnimator buttonAnimator;

    [UnityEngine.Serialization.FormerlySerializedAs("downAction")]
    public UnityEvent onDown;

    public Graphic[] additionalTintTargetGraphics = new Graphic[0];
    public ColorBlockExceed colorBlockExceed = ColorBlockExceed.defaultColorBlock;
    public AnimationTriggersExceed animationTriggersExceed = new AnimationTriggersExceed();

    [Tooltip("Button still looks like normal even when disabled.")]
    public bool noChangeDisable;

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
                tintColor = colorBlockExceed.normalColor;
                transitionSprite = null;
                triggerName = animationTriggersExceed.normalTrigger;
                break;
            case SelectionState.Pressed:
                tintColor = colorBlockExceed.pressedColor;
                transitionSprite = spriteState.pressedSprite;
                triggerName = animationTriggersExceed.pressedTrigger;
                break;
            case SelectionState.Disabled:
                if (!noChangeDisable)
                {
                    tintColor = colorBlockExceed.disabledColor;
                    transitionSprite = spriteState.disabledSprite;
                    triggerName = animationTriggersExceed.disabledTrigger;
                }
                else
                {
                    tintColor = colorBlockExceed.normalColor;
                    transitionSprite = null;
                    triggerName = animationTriggersExceed.normalTrigger;
                }
                break;
            default: //Highlighted is invisible
                tintColor = colorBlockExceed.normalColor;
                transitionSprite = null;
                triggerName = animationTriggersExceed.normalTrigger;
                break;
        }

        if (gameObject.activeInHierarchy)
        {
            switch (transition)
            {
                case Transition.ColorTint:
                    StartColorTween(tintColor * colors.colorMultiplier, instant);
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

    void StartColorTween(Color targetColor, bool instant)
    {
        if (additionalTintTargetGraphics == null && additionalTintTargetGraphics.Length == 0 && targetGraphic == null)
            return;

        targetGraphic.CrossFadeColor(targetColor, instant ? 0f : colorBlockExceed.fadeDuration, true, true);
        foreach (Graphic g in additionalTintTargetGraphics)
        {
            g?.CrossFadeColor(targetColor, instant ? 0f : colorBlockExceed.fadeDuration, true, true);
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
            if (animationTriggersExceed.normalTrigger != "")
            {
                buttonAnimator.SampleFirstFrame(animationTriggersExceed.normalTrigger);
            }
        }
        base.OnEnable();
    }

}

/// <summary>
/// Sorry desktop devs but I have removed "highlighted" for now..
/// </summary>
[System.Serializable]
public struct ColorBlockExceed : System.IEquatable<ColorBlockExceed>
{
    [SerializeField]
    private Color m_NormalColor;

    [SerializeField]
    private Color m_PressedColor;

    [SerializeField]
    private Color m_DisabledColor;

    [Range(1, 5)]
    [SerializeField]
    private float m_ColorMultiplier;

    [SerializeField]
    private float m_FadeDuration;

    public Color normalColor => m_NormalColor;
    public Color pressedColor => m_PressedColor;
    public Color disabledColor => m_DisabledColor;
    public float colorMultiplier => m_ColorMultiplier;
    public float fadeDuration => m_FadeDuration;

    public ColorBlockExceed(Color normal, Color pressed, Color disabled)
    {
        m_NormalColor = normal;
        m_PressedColor = pressed;
        m_DisabledColor = disabled;
        m_ColorMultiplier = 1.0f;
        m_FadeDuration = 0;
    }

    public static ColorBlockExceed defaultColorBlock => new ColorBlockExceed(
        new Color32(255, 255, 255, 255),
        new Color32(200, 200, 200, 255),
        new Color32(200, 200, 200, 128)
    );

    public override bool Equals(object obj)
    {
        if (!(obj is ColorBlock))
            return false;

        return Equals((ColorBlock)obj);
    }

    public bool Equals(ColorBlockExceed other)
    {
        return normalColor == other.normalColor &&
            pressedColor == other.pressedColor &&
            disabledColor == other.disabledColor &&
            colorMultiplier == other.colorMultiplier &&
            fadeDuration == other.fadeDuration;
    }

    public static bool operator ==(ColorBlockExceed point1, ColorBlockExceed point2)
    {
        return point1.Equals(point2);
    }

    public static bool operator !=(ColorBlockExceed point1, ColorBlockExceed point2)
    {
        return !point1.Equals(point2);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }
}

[System.Serializable]
public class AnimationTriggersExceed
{
    private const string kDefaultNormalAnimName = "Normal";
    // private const string kDefaultSelectedAnimName = "Highlighted";
    private const string kDefaultPressedAnimName = "Pressed";
    private const string kDefaultUpAnimName = "Up";
    private const string kDefaultDisabledAnimName = "Disabled";

    [SerializeField]
    private string m_NormalTrigger = kDefaultNormalAnimName;

    // [SerializeField]
    // private string m_HighlightedTrigger = kDefaultSelectedAnimName;

    [SerializeField]
    private string m_PressedTrigger = kDefaultPressedAnimName;

    [SerializeField]
    private string m_UpTrigger = kDefaultPressedAnimName;

    [SerializeField]
    private string m_DisabledTrigger = kDefaultDisabledAnimName;

    public string normalTrigger => m_NormalTrigger;
    //public string highlightedTrigger => m_HighlightedTrigger;
    public string pressedTrigger => m_PressedTrigger;
    public string upTrigger => m_UpTrigger;
    public string disabledTrigger => m_DisabledTrigger;
}
