using System;
using System.Reflection;
using UnityEditor;
using UnityEditor.ShortcutManagement;
using UnityEngine;

namespace EditorImprovements
{
    public class EditorUtilities
    {
        [Shortcut("Tools/Clear Console", KeyCode.C, ShortcutModifiers.Alt)]
        static void ClearConsole()
        {
            // This simply does "LogEntries.Clear()" the long way:
            var logEntries = Type.GetType("UnityEditor.LogEntries,UnityEditor.dll");
            var clearMethod = logEntries.GetMethod("Clear", BindingFlags.Static | BindingFlags.Public);
            clearMethod.Invoke(null, null);
        }

        [Shortcut("Tools/Toggle Inspector Lock", KeyCode.E, ShortcutModifiers.Alt)]
        static void SelectLockableInspector()
        {
            EditorWindow inspectorToBeLocked = EditorWindow.mouseOverWindow; // "EditorWindow.focusedWindow" can be used instead

            if (inspectorToBeLocked != null && inspectorToBeLocked.GetType().Name == "InspectorWindow")
            {
                Type type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.InspectorWindow");
                PropertyInfo propertyInfo = type.GetProperty("isLocked");
                bool value = (bool)propertyInfo.GetValue(inspectorToBeLocked, null);
                propertyInfo.SetValue(inspectorToBeLocked, !value, null);

                inspectorToBeLocked.Repaint();
            }
        }

        [Shortcut("Tools/Toggle Inspector Mode", KeyCode.D, ShortcutModifiers.Alt)]
        static void ToggleInspectorDebug()
        {
            EditorWindow targetInspector = EditorWindow.mouseOverWindow; // "EditorWindow.focusedWindow" can be used instead

            if (targetInspector != null && targetInspector.GetType().Name == "InspectorWindow")
            {
                //Get the type of the inspector window to find out the variable/method from
                Type type = Assembly.GetAssembly(typeof(UnityEditor.Editor)).GetType("UnityEditor.InspectorWindow");
                //get the field we want to read, for the type (not our instance)
                FieldInfo field = type.GetField("m_InspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);

                //read the value for our target inspector
                InspectorMode mode = (InspectorMode)field.GetValue(targetInspector);
                //toggle the value
                mode = (mode == InspectorMode.Normal ? InspectorMode.Debug : InspectorMode.Normal);
                //Debug.Log("New Inspector Mode: " + mode.ToString());

                //Find the method to change the mode for the type
                MethodInfo method = type.GetMethod("SetMode", BindingFlags.NonPublic | BindingFlags.Instance);
                //Call the function on our targetInspector, with the new mode as an object[]
                method.Invoke(targetInspector, new object[] { mode });

                //refresh inspector
                targetInspector.Repaint();
            }
        }
    }
}
