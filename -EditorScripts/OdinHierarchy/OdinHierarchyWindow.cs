/// <summary>
/// Odin Hierarchy
/// Sirawat Pitaksarit / 5argon
/// Exceed7 Experiments 
/// Contact : http://5argon.info, http://exceed7.com, 5argon@exceed7.com
/// </summary>

using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Reflection;

using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

public class OdinHierarchyWindow : OdinEditorWindow
{
    private static OdinHierarchySettings ohsStatic;
    private static OdinHierarchyWindow window;

    [MenuItem("Window/Odin Hierarchy")]
    private static void Open()
    {
        window = GetWindow<OdinHierarchyWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
        window.titleContent = new GUIContent("Hierarchy Setup");
    }

    protected override void OnEnable()
    {
        base.OnEnable();
        string[] find = AssetDatabase.FindAssets("t:OdinHierarchySettings");
        if (find.Length > 0)
        {
            ohsStatic = AssetDatabase.LoadAssetAtPath<OdinHierarchySettings>(AssetDatabase.GUIDToAssetPath(find[0]));
            EditorUtility.SetDirty(this);
        }
    }

    public void Refresh() => OnEnable();

    protected override object GetTarget()
    {
        if (ohsStatic != null)
        {
            return ohsStatic;
        }
        else
        {
            //It works but is this how we supposed to use GetTarget to draw custom messages??
            return new OdinHierarchySettings.Error();
        }
    }

    protected override void OnBeginDrawEditors()
    {
        EditorApplication.RepaintHierarchyWindow();
    }

    [InitializeOnLoad]
    public class DrawHierarchyIcon
    {
        private static Dictionary<string, PropertyInfo> iconProperties = new Dictionary<string, PropertyInfo>();
        static DrawHierarchyIcon()
        {
            iconProperties.Clear();
            foreach (var item in typeof(EditorIcons).GetProperties(Flags.StaticPublic).OrderBy(x => x.Name))
            {
                var returnType = item.GetReturnType();
                if (typeof(EditorIcon).IsAssignableFrom(returnType))
                {
                    iconProperties.Add(item.Name, item);
                }
            }
            EditorApplication.hierarchyWindowItemOnGUI -= DrawIconOnWindowItem;
            EditorApplication.hierarchyWindowItemOnGUI += DrawIconOnWindowItem;
        }

        private static void DrawIconOnWindowItem(int instanceID, Rect rect)
        {
            if (ohsStatic == null || ohsStatic.enabled == false)
            {
                return;
            }

            GameObject gameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;

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
}