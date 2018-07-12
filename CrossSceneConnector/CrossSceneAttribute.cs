using System;
using UnityEngine;
using System.Collections.Generic;
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

/// <summary>
/// Decorate just to remind yourself.
/// </summary>
[AttributeUsage(AttributeTargets.Field|AttributeTargets.Property)]
public class CrossSceneAttribute : Attribute
{
    public CrossSceneAttribute()
    {
    }
}

#if ODIN_INSPECTOR
public class MainSideAttribute : CrossSceneSideAttribute
{
    public MainSideAttribute() : base("Main Side") { }
    public MainSideAttribute(string sceneName) : base("Main Side", sceneName) { }
}

public class TargetSideAttribute : CrossSceneSideAttribute
{
    public TargetSideAttribute() : base("$SceneToConnect") { }
    public TargetSideAttribute(string sceneName) : base("$SceneToConnect", sceneName) { }
}

public class CrossSceneSideAttribute : BoxGroupAttribute
{
    public CrossSceneSideAttribute(string name, string sceneName = "") : base(sceneName == "" ? name : sceneName)
    {
    }
}

#endif