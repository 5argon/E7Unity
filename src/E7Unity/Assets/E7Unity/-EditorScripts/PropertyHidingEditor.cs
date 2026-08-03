using System.Collections.Generic;
using UnityEditor;

namespace E7.E7Unity
{
    /// <summary>
    /// An inspector that draws everything a component serializes except the properties it names, for components that
    /// inherit settings from a Unity base class and do not honour all of them.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A field that does nothing is worse than no field at all: it reads as a supported option, and the only way to
    /// find out otherwise is to try it and watch nothing happen.
    /// </para>
    /// <para>
    /// Iterating what the object serializes, rather than listing what to draw, means fields a subclass adds show up
    /// without this inspector having to be told about them.
    /// </para>
    /// </remarks>
    public abstract class PropertyHidingEditor : Editor
    {
        /// <summary>
        /// Serialized property paths to leave out, such as <c>m_Color</c>.
        /// </summary>
        protected abstract IReadOnlyList<string> HiddenProperties { get; }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty property = serializedObject.GetIterator();
            for (bool enterChildren = true; property.NextVisible(enterChildren); enterChildren = false)
            {
                if (IsHidden(property.propertyPath))
                    continue;

                using (new EditorGUI.DisabledScope(property.propertyPath == "m_Script"))
                {
                    EditorGUILayout.PropertyField(property, true);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private bool IsHidden(string propertyPath)
        {
            IReadOnlyList<string> hidden = HiddenProperties;
            for (int i = 0; i < hidden.Count; i++)
            {
                if (hidden[i] == propertyPath)
                    return true;
            }

            return false;
        }
    }
}
