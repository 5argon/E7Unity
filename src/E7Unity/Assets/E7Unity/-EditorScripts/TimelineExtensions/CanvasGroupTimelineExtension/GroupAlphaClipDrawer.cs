using UnityEditor;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Draws a <see cref="GroupAlphaClip"/>'s settings inline instead of behind the <c>template</c> foldout.
    /// </summary>
    [CustomEditor(typeof(GroupAlphaClip))]
    public class GroupAlphaClipDrawer : DrawThingsInTemplate { }
}