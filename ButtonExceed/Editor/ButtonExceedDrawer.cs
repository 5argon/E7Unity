#define BUTTON_EXCEED_MOBILE

using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities;
using Sirenix.Utilities.Editor;

[CustomEditor(typeof(ButtonExceed))]
public class ButtonExceedDrawer : OdinEditor
{
    private PropertyTree customTree;
    protected override void DrawTree()
    {
        if (customTree == null)
        {
            customTree = PropertyTree.Create(serializedObject, new ButtonExceedAttributeProcessorLocator(), null);
        }
        InspectorUtilities.BeginDrawPropertyTree(customTree, true);
        InspectorUtilities.DrawPropertiesInTree(customTree);
        InspectorUtilities.EndDrawPropertyTree(customTree);
    }

    [OdinDontRegister]
    public class ButtonExceedAttributeProcessor : OdinAttributeProcessor
    {
        public override void ProcessChildMemberAttributes(InspectorProperty parentProperty, MemberInfo member, List<Attribute> attributes)
        {
            //Debug.Log($"Processing {member.Name}");
            switch (member.Name)
            {
                case "m_Interactable":
                    attributes.Add(new PropertyOrderAttribute(-29));
                    break;
                case "noChangeDisable":
                    attributes.Add(new PropertyTooltipAttribute("Button will still looks like Normal state even when interactable is false."));
                    attributes.Add(new PropertyOrderAttribute(-28));
                    break;
                case "m_OnClick":
                    attributes.Add(new TitleAttribute("Button Events"));
                    attributes.Add(new PropertyOrderAttribute(-19));
                    break;
                case "onDown":
                    attributes.Add(new PropertyOrderAttribute(-18));
                    break;
                case "m_Transition":
                    attributes.Add(new HideLabelAttribute());
                    attributes.Add(new EnumToggleButtonsAttribute());
                    attributes.Add(new BoxGroupAttribute("Transition"));
                    break;
                case "m_Colors":
                    attributes.Add(new ShowIfAttribute("m_Transition", Selectable.Transition.ColorTint));
                    attributes.Add(new HideLabelAttribute());
                    break;
                case "m_SpriteState":
                    attributes.Add(new ShowIfAttribute("m_Transition", Selectable.Transition.SpriteSwap));
                    attributes.Add(new HideLabelAttribute());
                    break;
                case "m_AnimationTriggers":
                case "animationTriggersExceed":
                    attributes.Add(new ShowIfAttribute("m_Transition", Selectable.Transition.Animation));
                    attributes.Add(new HideLabelAttribute());
                    break;
                case "buttonAnimator":
                    attributes.Add(new ShowIfAttribute("m_Transition", Selectable.Transition.Animation));
                    attributes.Add(new InlineEditorAttribute());
                    break;
                case "m_TargetGraphic":
                    attributes.Add(new ShowIfAttribute("ColorTintOrSpriteSwap", Selectable.Transition.ColorTint));
                    break;
                case "additionalTintTargetGraphics":
                    attributes.Add(new ShowIfAttribute("m_Transition", Selectable.Transition.ColorTint));
                    break;
                case "m_ColorMultiplier":
                    attributes.Add(new RangeAttribute(1,5));
                    break;
                case "m_NormalTrigger":
                case "m_PressedTrigger":
                case "m_DisabledTrigger":
                case "m_UpTrigger":
                    break;
                case "m_FadeDuration":
                    attributes.Add(new HideInInspector());
                    break;
#if BUTTON_EXCEED_MOBILE
                case "m_Navigation":
                case "m_HighlightedSprite":
                case "m_HighlightedTrigger":
                case "m_HighlightedColor":
                    attributes.Add(new HideInInspector());
                    break;
#else
                case "m_HighlightedTrigger":
                    break;
#endif
            }
        }
    }

    public class ButtonExceedAttributeProcessorLocator : MatchableOdinAttributeProcessorLocator
    {
        public override IEnumerable<Type> GetResolverTypes()
        {
            return new Type[] { typeof(ButtonExceedAttributeProcessor) };
        }
    }
}
