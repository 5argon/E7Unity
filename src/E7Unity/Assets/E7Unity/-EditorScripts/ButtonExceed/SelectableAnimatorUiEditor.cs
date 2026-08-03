using System.Collections.Generic;
using UnityEditor;

namespace E7.E7Unity
{
    /// <summary>
    /// Inspector for <see cref="SelectableAnimatorUi"/> and everything derived from it, hiding the parts of
    /// <see cref="UnityEngine.UI.Selectable"/> that it does not honour.
    /// </summary>
    /// <remarks>
    /// The transition mode is forced to <c>None</c>, so offering the dropdown and the colour, sprite and animation
    /// blocks behind it only invites picking something that has no effect. The target graphic goes with them, since
    /// it exists to be tinted or swapped by those same transitions. What remains is <c>Interactable</c>, navigation —
    /// which is what keyboard and gamepad focus travels through — and whatever the widget itself declares.
    /// </remarks>
    [CustomEditor(typeof(SelectableAnimatorUi), true)]
    [CanEditMultipleObjects]
    public class SelectableAnimatorUiEditor : PropertyHidingEditor
    {
        private static readonly string[] hiddenProperties =
        {
            "m_Transition",
            "m_Colors",
            "m_SpriteState",
            "m_AnimationTriggers",
            "m_TargetGraphic",
        };

        protected override IReadOnlyList<string> HiddenProperties => hiddenProperties;
    }
}
