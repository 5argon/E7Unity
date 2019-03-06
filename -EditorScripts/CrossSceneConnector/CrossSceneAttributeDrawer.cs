using System;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector.Editor.Drawers;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;
using System.Reflection;
using UnityEditor;
#endif
#endif

#if ODIN_INSPECTOR && UNITY_EDITOR
static class CrossSceneDrawer
{
    public static void DrawPropertyGroupLayout<T>(OdinGroupDrawer<T> ogd, InspectorProperty property, T attribute, GUIContent label, bool result) where T : CrossSceneSideAttribute 
    {
        if (SirenixEditorGUI.BeginFadeGroup(UniqueDrawerKey.Create(property, ogd), result))
        {
            var labelGetter = property.Context.Get<StringMemberHelper>(ogd, "LabelContext", (StringMemberHelper)null);

            if (labelGetter.Value == null)
            {
                labelGetter.Value = new StringMemberHelper(property, attribute.GroupName);
            }

            SirenixEditorGUI.BeginBox(attribute.ShowLabel ? labelGetter.Value.GetString(property) : null, attribute.CenterLabel);

            for (int i = 0; i < property.Children.Count; i++)
            {
                property.Draw(property.Children[i].Label);
            }

            SirenixEditorGUI.EndBox();
        }
        SirenixEditorGUI.EndFadeGroup();
    }
}

public class MainSideDrawer : OdinGroupDrawer<MainSideAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var property = this.Property;
        var attribute = this.Attribute;
        
        bool result = DeepReflection.CreateWeakInstanceValueGetter<bool>(property.ParentType, "mainSide")(property.ParentValues[0]);

        CrossSceneDrawer.DrawPropertyGroupLayout<MainSideAttribute>(this, property, attribute, label, result);
    }
}

public class TargetSideDrawer : OdinGroupDrawer<TargetSideAttribute>
{
    protected override void DrawPropertyLayout(GUIContent label)
    {
        var property = this.Property;
        var attribute = this.Attribute;

        bool result = DeepReflection.CreateWeakInstanceValueGetter<bool>(property.ParentType, "mainSide")(property.ParentValues[0]);

        result = !result;

        CrossSceneDrawer.DrawPropertyGroupLayout<TargetSideAttribute>(this, property, attribute, label, result);
    }
}

#endif