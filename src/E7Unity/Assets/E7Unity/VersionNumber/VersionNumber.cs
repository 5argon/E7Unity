using UnityEngine;
using TMPro;

namespace E7.E7Unity
{
    /// <summary>
    /// Writes the player's <see cref="Application.version"/> into a <see cref="TextMeshProUGUI"/> on <c>Awake</c>,
    /// so a title or settings screen can display the build version without a script of its own.
    /// </summary>
    /// <remarks>
    /// The text is formatted as <c>"Version {version}"</c>. Nothing happens when <see cref="versionNumber"/> is
    /// left unassigned.
    /// </remarks>
    public class VersionNumber : MonoBehaviour
    {
        /// <summary>
        /// Label that receives the version string. When <c>null</c>, this component does nothing.
        /// </summary>
        public TextMeshProUGUI versionNumber;

        void Awake()
        {
            if (versionNumber != null)
            {
                versionNumber.text = "Version " + Application.version;
            }
        }
    }
}
