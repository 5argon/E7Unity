/// <summary>
/// Odin Hierarchy
/// Sirawat Pitaksarit / 5argon
/// Exceed7 Experiments 
/// Contact : http://5argon.info, http://exceed7.com, 5argon@exceed7.com
/// </summary>

using UnityEditor;
using UnityEngine;

using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

[InitializeOnLoad]
public static class OdinHierarchyDrawer
{
    private static Dictionary<string, PropertyInfo> iconProperties = new Dictionary<string, PropertyInfo>();
    public static OdinHierarchySettings ohsStatic;

    static OdinHierarchyDrawer()
    {
        LoadSettings();
        iconProperties.Clear();
        foreach (var item in typeof(EditorIcons).GetProperties(Flags.StaticPublic).OrderBy(x => x.Name))
        {
            var returnType = item.GetReturnType();
            if (typeof(EditorIcon).IsAssignableFrom(returnType))
            {
                iconProperties.Add(item.Name, item);
            }
        }
        EditorApplication.hierarchyWindowItemOnGUI -= Draw;
        EditorApplication.hierarchyWindowItemOnGUI += Draw;
    }

    public static void LoadSettings()
    {
        string[] find = AssetDatabase.FindAssets("t:OdinHierarchySettings");
        if (find.Length > 0)
        {
            ohsStatic = AssetDatabase.LoadAssetAtPath<OdinHierarchySettings>(AssetDatabase.GUIDToAssetPath(find[0]));
        }
    }

    private static void Draw(int id, Rect rect)
    {
        if (ohsStatic == null || ohsStatic.enabled == false)
        {
            return;
        }

        GameObject gameObject = EditorUtility.InstanceIDToObject(id) as GameObject;

        if (gameObject == null)
        {
            return;
        }

        Rect extended = new Rect(rect);
        extended.xMin = extended.xMin - 2;

        OdinHierarchySettings.Item item = ohsStatic.MatchSettingsWithGameObject(gameObject);

        if (item != null)
        {
            Rect rectToUse = extended;

            GUIStyle style;
            switch (item.style)
            {
                case OdinHierarchySettings.Style.Off: style = SirenixGUIStyles.None; break;
                case OdinHierarchySettings.Style.A1: style = SirenixGUIStyles.Button; break;
                case OdinHierarchySettings.Style.A2: style = EditorStyles.miniButton; break;
                case OdinHierarchySettings.Style.A3: style = EditorStyles.miniButtonMid; break;
                case OdinHierarchySettings.Style.A4: style = EditorStyles.helpBox; break;
                case OdinHierarchySettings.Style.B1: style = EditorStyles.whiteLabel; break;
                case OdinHierarchySettings.Style.B2: style = EditorStyles.whiteMiniLabel; break;
                default: style = SirenixGUIStyles.None; break;
            }
            style = new GUIStyle(style);
            style.alignment = item.textAnchor;
            GUIHelper.PushColor(item.color);
            string textToDraw = string.IsNullOrEmpty(item.overrideText) ? gameObject.name : item.overrideText;
            EditorGUI.LabelField(rectToUse, item.drawText ? textToDraw : "", style);
            GUIHelper.PopColor();

            if (item.decoration)
            {
                if (item.decorationType == OdinHierarchySettings.Decoration.Highlight)
                {
                    SirenixEditorGUI.DrawSolidRect(rectToUse, item.decorationColor);
                }
                if (item.decorationType == OdinHierarchySettings.Decoration.Underline)
                {
                    SirenixEditorGUI.DrawBorders(rectToUse, 0, 0, 0, 1, item.decorationColor);
                }
                if (item.decorationType == OdinHierarchySettings.Decoration.Left)
                {
                    Rect extended2 = new Rect(extended);
                    extended.xMin = extended.xMin - 2;
                    SirenixEditorGUI.DrawBorders(extended, 4, 0, 0, 0, item.decorationColor);
                }
                if (item.decorationType == OdinHierarchySettings.Decoration.Right)
                {
                    SirenixEditorGUI.DrawBorders(rectToUse, 0, 4, 0, 0, item.decorationColor);
                }
            }

            if (item.icon && string.IsNullOrEmpty(item.iconName) == false)
            {
                EditorIcon editorIcon = (EditorIcon)iconProperties[item.iconName].GetGetMethod().Invoke(null, null);

                float iconSize = ohsStatic.baseIconSize + item.iconSizeAdded;
                var rightPadding = ohsStatic.baseIconRightPadding + item.iconRightPaddingAdded;

                EditorGUIUtility.SetIconSize(new Vector2(iconSize, iconSize));
                var iconRect = new Rect(rect.xMax - (iconSize + rightPadding), rect.yMin + ((rect.height - iconSize) / 2.0f), iconSize + rightPadding, iconSize);
                //SirenixEditorGUI.DrawSolidRect(iconDrawRect, Color.yellow); //For debugging
                var iconContent = new GUIContent(editorIcon.Active);
                EditorGUI.LabelField(iconRect, iconContent, SirenixGUIStyles.None);
                EditorGUIUtility.SetIconSize(Vector2.zero);
            }
        }
    }
}