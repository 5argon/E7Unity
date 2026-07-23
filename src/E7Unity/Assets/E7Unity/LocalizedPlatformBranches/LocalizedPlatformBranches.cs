using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Gives a game object whose localized text branches per platform an inspector for that branching, next to the
    /// <c>LocalizeStringEvent</c> it belongs to.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The component carries no data of its own. Which platforms a string branches on, and which entry each branch
    /// points at, is held by Unity Localization as <c>PlatformOverride</c> metadata on the shared table entry, and
    /// that metadata stays the only source of truth. This is a place for the inspector to draw, and a marker saying
    /// "the text here is not the same on every platform" — a fact that is otherwise invisible from the game object.
    /// </para>
    /// <para>
    /// Add it to any game object that already has one or more <c>LocalizeStringEvent</c>. Removing it removes only
    /// the inspector; the branching itself is untouched.
    /// </para>
    /// <para>
    /// It is deliberately not compiled out when Unity Localization is missing. A <c>MonoBehaviour</c> is serialized
    /// into prefabs and scenes by script GUID, so a component that disappeared with the package would leave a
    /// missing script behind in every prefab using it. The component compiles anywhere; only its inspector is gated.
    /// </para>
    /// </remarks>
    [AddComponentMenu("Localization/Localized Platform Branches")]
    [DisallowMultipleComponent]
    public class LocalizedPlatformBranches : MonoBehaviour
    {
    }
}
