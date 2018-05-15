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
    private static OdinHierarchyWindow window;

    [MenuItem("Window/Odin Hierarchy")]
    private static void Open()
    {
        window = GetWindow<OdinHierarchyWindow>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
        window.titleContent = new GUIContent("Odin Hierarchy");
    }

    public void Refresh() => OnEnable();

    protected override object GetTarget()
    {
        if (OdinHierarchyDrawer.ohsStatic != null)
        {
            return OdinHierarchyDrawer.ohsStatic;
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
}