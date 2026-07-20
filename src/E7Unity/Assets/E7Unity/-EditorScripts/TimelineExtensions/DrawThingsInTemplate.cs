using UnityEditor;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Base editor for a playable asset or track whose settings live in a serialized field named <c>template</c>,
    /// drawing that field's contents inline instead of behind a foldout.
    /// </summary>
    /// <remarks>
    /// Timeline's convention of parking per-clip and per-track settings inside a <c>template</c> object means the
    /// default inspector buries every real setting one fold deep. Subclass this and point a
    /// <see cref="CustomEditor"/> at the asset to get those settings at the top level, where they belong.
    /// </remarks>
    public abstract class DrawThingsInTemplate : Editor
    {
        /// <summary>
        /// Draws each child property of the <c>template</c> field directly, skipping the wrapper itself.
        /// </summary>
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
