using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Drives every <see cref="PlatformSpecific" /> beneath it — active or inactive — showing exactly those
    /// whose platform list matches <see cref="PlatformResolver.Current" /> and hiding the rest.
    /// </summary>
    /// <remarks>
    /// Because this lives on an always-active ancestor, it can re-enable a disabled child, which a
    /// self-managing <see cref="PlatformSpecific" /> cannot do from its own <c>Awake</c>. That removes the
    /// need to author mutually-exclusive variants active-and-overlapping just so their <c>Awake</c> runs —
    /// the off-platform variants can start inactive. With <see cref="ExecuteAlways" /> it also re-applies in
    /// edit mode whenever the simulated platform is flipped (via the Dev Options window), so the scene
    /// previews the iOS or Android layout without entering Play.
    /// </remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class PlatformSwitch : MonoBehaviour
    {
        void OnEnable()
        {
            PlatformResolver.Changed += Apply;
            Apply();
        }

        void OnDisable() => PlatformResolver.Changed -= Apply;

#if UNITY_EDITOR
        void OnValidate()
        {
            if (!isActiveAndEnabled) return;
            // SetActive is illegal directly inside OnValidate; defer a tick.
            UnityEditor.EditorApplication.delayCall += DeferredApply;
        }

        void DeferredApply()
        {
            UnityEditor.EditorApplication.delayCall -= DeferredApply;
            if (this != null && isActiveAndEnabled)
            {
                Apply();
            }
        }
#endif

        void Apply()
        {
            foreach (PlatformSpecific tag in GetComponentsInChildren<PlatformSpecific>(true))
            {
                bool shouldBeActive = PlatformResolver.IsActiveOn(tag.platforms);
                if (tag.gameObject.activeSelf != shouldBeActive)
                {
                    tag.gameObject.SetActive(shouldBeActive);
                }
            }
        }
    }
}
