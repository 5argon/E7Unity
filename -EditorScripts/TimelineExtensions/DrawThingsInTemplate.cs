using UnityEditor;

namespace E7.Timeline
{
    /// <summary>
    /// A helper editor so you could have the behaviour template named "template" in your playable asset
    /// and the drawer would skip drawing the outer template foldout.
    /// </summary>
    public abstract class DrawThingsInTemplate : Editor
    {
        public override void OnInspectorGUI()
        {
            var template = serializedObject.FindProperty("template");
            EditorGUI.BeginChangeCheck();
            serializedObject.Update();
            var hasNext = template.NextVisible(enterChildren: true);
            while (hasNext)
            {
                EditorGUILayout.PropertyField(template);
                hasNext = template.NextVisible(enterChildren: false);
            }
            serializedObject.ApplyModifiedProperties();
            EditorGUI.EndChangeCheck();
        }
    }
}