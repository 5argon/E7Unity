using UnityEditor;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Draws an <see cref="UnblocksRaycastsTrack"/>'s settings inline instead of behind the <c>template</c> foldout.
    /// </summary>
    [CustomEditor(typeof(UnblocksRaycastsTrack))]
    public class UnblocksRaycastsTrackDrawer : DrawThingsInTemplate { }
}
