using System;
using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Single source of truth for "which runtime platform should platform-gated UI behave as."
    /// In a build it is simply <see cref="Application.platform" />. In the editor — both edit mode and
    /// Play mode, including Play Mode tests — it is a <em>simulated</em> platform that authors and tests
    /// pick, so the UI can be previewed and asserted as iOS or Android without leaving the editor
    /// (<see cref="Application.platform" /> would otherwise report <c>OSXEditor</c>/<c>WindowsEditor</c>).
    /// </summary>
    /// <remarks>
    /// <see cref="PlatformSpecific" /> and <see cref="PlatformSwitch" /> read <see cref="Current" /> instead
    /// of <see cref="Application.platform" /> directly, and re-evaluate whenever <see cref="Changed" /> fires,
    /// which is how flipping the simulated platform live-updates the scene.
    /// </remarks>
    public static class PlatformResolver
    {
        /// <summary>
        /// Raised when the simulated editor platform changes so live views can re-apply their visibility.
        /// Never fires in a build. Because it is a static event, editor Domain Reload being disabled means
        /// stale subscribers survive between Play sessions — clear it from the enter-play-mode reset hook.
        /// </summary>
        public static event Action Changed;

#if UNITY_EDITOR
        const string PrefKey = "E7.PlatformResolver.SimulatedPlatform";
        const RuntimePlatform DefaultSimulated = RuntimePlatform.IPhonePlayer;

        /// <summary>
        /// The simulated platform, persisted per-machine in EditorPrefs. Setting it raises <see cref="Changed" />.
        /// </summary>
        public static RuntimePlatform Current
        {
            get => (RuntimePlatform)UnityEditor.EditorPrefs.GetInt(PrefKey, (int)DefaultSimulated);
            set
            {
                if (Current == value) return;
                UnityEditor.EditorPrefs.SetInt(PrefKey, (int)value);
                Changed?.Invoke();
            }
        }
#else
        /// <summary>The real runtime platform in a build.</summary>
        public static RuntimePlatform Current => Application.platform;
#endif

        /// <summary>
        /// True when <see cref="Current" /> is one of <paramref name="platforms" />. A null or empty list
        /// matches nothing.
        /// </summary>
        public static bool IsActiveOn(RuntimePlatform[] platforms)
        {
            if (platforms == null) return false;
            RuntimePlatform now = Current;
            foreach (RuntimePlatform p in platforms)
            {
                if (p == now) return true;
            }
            return false;
        }

        /// <summary>
        /// Drops all <see cref="Changed" /> subscribers. Call from the enter-play-mode reset when editor
        /// Domain Reload is disabled, so listeners from a previous Play session do not leak into the next.
        /// </summary>
        public static void ClearEventSubscribers() => Changed = null;
    }
}
