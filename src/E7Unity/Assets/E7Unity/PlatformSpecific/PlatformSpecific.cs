using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Marks its game object as belonging on a specific set of runtime platforms, gating its visibility
    /// through <see cref="PlatformResolver" /> so the decision can be simulated in the editor.
    /// </summary>
    /// <remarks>
    /// On its own this component can only hide an object it was authored active on — a disabled game object
    /// receives no <c>Awake</c> and so can never re-enable itself, and it deliberately does nothing in edit
    /// mode so authoring stays unaffected. Put a <see cref="PlatformSwitch" /> on a common ancestor to drive
    /// tagged objects in <em>both</em> directions and to preview them live in the editor, which lets the
    /// mutually-exclusive variants be authored inactive instead of overlapping. The serialized
    /// <see cref="platforms" /> field is the data a <see cref="PlatformSwitch" /> reads.
    /// </remarks>
    public class PlatformSpecific : MonoBehaviour
    {
        /// <summary>
        /// Platforms on which the game object stays active. On any platform outside this list it is deactivated.
        /// </summary>
        public RuntimePlatform[] platforms;

        void Awake() => Apply();

        // React to a simulated-platform flip during Play (including Play Mode tests). This can only hide;
        // re-activation is owned by a PlatformSwitch ancestor.
        void OnEnable() => PlatformResolver.Changed += Apply;

        void OnDisable() => PlatformResolver.Changed -= Apply;

        void Apply()
        {
            if (!PlatformResolver.IsActiveOn(platforms))
            {
                gameObject.SetActive(false);
            }
        }
    }
}
