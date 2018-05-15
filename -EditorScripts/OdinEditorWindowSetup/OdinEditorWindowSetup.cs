using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

using Sirenix.OdinInspector.Editor;
using Sirenix.OdinInspector;
using Sirenix.Utilities.Editor;
using Sirenix.Utilities;

/// <summary>
/// Gives a good template for adding title and icons.
/// After subclassing, your class can just call base.OpenWindow() on the code with [MenuItem]
/// </summary>
public abstract class OdinEditorWindowSetup<T> : OdinEditorWindow where T : OdinEditorWindowSetup<T>
{
    protected abstract EditorIcon Icon { get; }
    protected abstract string Title { get; }
    private static T window;

    protected static void OpenWindow()
    {
        window = GetWindow<T>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
    }

    protected override void OnEnable()
    {
        window = GetWindow<T>("", false);
        RegisterRefreshTitle();
        base.OnEnable();
    }

    private static void RefreshTitle()
    {
        window.titleContent = new GUIContent(window.Title, window.Icon?.Active);
    }

    private static void RegisterRefreshTitle()
    {
        RefreshTitle();
        UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= RefreshTitle;
        UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += RefreshTitle;
    }

    private static void RefreshTitle(Scene from, Scene to) => RefreshTitle();
}

/// <summary>
/// Gives a good template for adding title and icons.
/// After subclassing, your class can just call base.OpenWindow() on the code with [MenuItem]
/// </summary>
public abstract class OdinMenuEditorWindowSetup<T> : OdinMenuEditorWindow where T : OdinMenuEditorWindowSetup<T>
{
    protected abstract EditorIcon Icon { get; }
    protected abstract string Title { get; }
    private static T window;

    protected static void OpenWindow()
    {
        window = GetWindow<T>();
        window.position = GUIHelper.GetEditorWindowRect().AlignCenter(700, 700);
    }

    protected override void OnEnable()
    {
        window = GetWindow<T>("", false);
        RegisterRefreshTitle();
        base.OnEnable();
    }

    private static void RefreshTitle()
    {
        window.titleContent = new GUIContent(window.Title, window.Icon?.Active);
    }

    private static void RegisterRefreshTitle()
    {
        RefreshTitle();
        UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode -= RefreshTitle;
        UnityEditor.SceneManagement.EditorSceneManager.activeSceneChangedInEditMode += RefreshTitle;
    }

    private static void RefreshTitle(Scene from, Scene to) => RefreshTitle();
}