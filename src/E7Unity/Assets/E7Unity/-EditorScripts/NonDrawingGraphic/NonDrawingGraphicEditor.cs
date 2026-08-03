using System.Collections.Generic;
using UnityEditor;

namespace E7.E7Unity
{
    /// <summary>
    /// Inspector for <see cref="NonDrawingGraphic"/>, hiding the <see cref="UnityEngine.UI.Graphic"/> settings that
    /// only describe how something is drawn.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Material and colour reach the component but never leave it, since it produces no geometry for either to apply
    /// to. The cull-state event goes too, matching Unity's own graphic inspectors, which do not show it either.
    /// </para>
    /// <para>
    /// <c>Maskable</c> stays, and is easy to mistake for another drawing-only setting. It decides whether a raycast
    /// respects the masks above it — <c>MaskableGraphic.Raycast</c> passes <c>!maskable</c> as its ignore-masks
    /// argument — so on a component that exists purely to be hit, it is the difference between a press area clipped
    /// to a scroll view and one that keeps taking presses beyond the edge.
    /// </para>
    /// </remarks>
    [CustomEditor(typeof(NonDrawingGraphic), true)]
    [CanEditMultipleObjects]
    public class NonDrawingGraphicEditor : PropertyHidingEditor
    {
        private static readonly string[] hiddenProperties =
        {
            "m_Material",
            "m_Color",
            "m_OnCullStateChanged",
        };

        protected override IReadOnlyList<string> HiddenProperties => hiddenProperties;
    }
}
