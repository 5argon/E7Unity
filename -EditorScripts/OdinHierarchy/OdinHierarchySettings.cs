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

using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;


[CreateAssetMenu]
public class OdinHierarchySettings : ScriptableObject
{
    public enum Style 
    {
        Off,
        A1,
        A2,
        A3,
        A4,
        B1,
        B2,
    }

    public enum Decoration 
    {
        Underline,
        Left,
        Right,
        Highlight
    }

    public class Error
    {
        [Title("Odin Hierarchy Settings")]
        [InfoBox("Please create an 'OdinHierarchySettings' file somewhere in the Project panel's Create menu!\nOdin Hierarchy uses the first one it found in the project.")]
        [Button("Yes I did that.", ButtonSizes.Medium)]
        public void Refresh()
        {
#if UNITY_EDITOR
            OdinHierarchyDrawer.LoadSettings();
#endif
        }
    }

    [Title("Odin Hierarchy Settings")]
    [CustomContextMenu("Secret", "ToggleSecret")]
    public bool enabled = true;

    [ShowIf("secret")]
    public float baseIconSize = defaultIconSize;
    [ShowIf("secret")]
    public float baseIconRightPadding = defaultIconRightPadding;

    private const float defaultIconSize = 14;
    private const float defaultIconRightPadding = 2;

    private static bool secret;
    public void ToggleSecret() => secret = !secret;

    public List<Item> items;

    [System.Serializable]
    public class Item
    {
        [TitleGroup("Matching")]
        public string containsName;
        [TitleGroup("Matching")]
        public string scene;
        [TitleGroup("Matching")]
        public string component;

        [TitleGroup("Appearances")]
        [HideLabel, HorizontalGroup("Appearances/appearanceGroup", Width = 0.2f)]
        public Color color = Color.yellow;
        [EnumToggleButtons, HideLabel, HorizontalGroup("Appearances/appearanceGroup")]
        public OdinHierarchySettings.Style style = OdinHierarchySettings.Style.A4;

        public bool icon;
        [ShowIf("icon"), ValueDropdown("Icons"), HideLabel, HorizontalGroup("IconGroup"), Indent(1)]
        public string iconName;

        private static List<string> Icons()
        {
            List<string> icons = new List<string>();
            foreach (var item in typeof(EditorIcons).GetProperties(Flags.StaticPublic).OrderBy(x => x.Name))
            {
                var returnType = item.GetReturnType();
                if (typeof(EditorIcon).IsAssignableFrom(returnType))
                {
                    icons.Add(item.Name);
                }
            }
            return icons;
        }
        [ShowIf("icon"), HorizontalGroup("IconGroup")]
        [Button(Name = "Reference...")]
        public void ViewIcons()
        {
            EditorIconsOverview.OpenOverivew();
        }

        [ShowIf("iconAndSecret"), Indent(1)]
        public float iconSizeAdded = 0;
        [ShowIf("iconAndSecret"), Indent(1)]
        public float iconRightPaddingAdded = 0;
        private bool iconAndSecret => icon && secret;

        public bool drawText;
        [ShowIf("drawText"), Indent(1)]
        public string overrideText;
        [ShowIf("drawText"), EnumPaging, Indent(1)]
        public TextAnchor textAnchor = TextAnchor.MiddleLeft;

        public bool decoration;
        [ShowIf("decoration"),EnumToggleButtons, Indent(1)]
        public OdinHierarchySettings.Decoration decorationType;
        [ShowIf("decoration"), Indent(1)]
        public Color decorationColor = Color.yellow;
    }

    public Item MatchSettingsWithGameObject(GameObject go)
    {
        return items?.FirstOrDefault(s => {
            bool nameMatch = string.IsNullOrEmpty(s.containsName) || (go.name?.Contains(s.containsName) ?? false);
            bool sceneMatch = string.IsNullOrEmpty(s.scene) || go.scene.name == s.scene;

            bool isLookingForBase = !string.IsNullOrEmpty(s.component) && s.component.Length > 1 && s.component[0] == '$';
            string className = isLookingForBase ? s.component.Substring(1) : s.component;

            bool componentMatch = string.IsNullOrEmpty(className) || 
            go.GetComponents<Component>().Select(c => isLookingForBase ? c?.GetType().BaseType.Name : c?.GetType().Name).Contains(className);
            bool allEmpty = string.IsNullOrEmpty(s.containsName) && string.IsNullOrEmpty(s.scene) && string.IsNullOrEmpty(s.component);

            return nameMatch && sceneMatch && componentMatch && !allEmpty;
        });
    }

}