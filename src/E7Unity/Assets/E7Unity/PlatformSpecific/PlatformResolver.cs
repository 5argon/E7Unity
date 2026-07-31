using System;
using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Single source of truth for "which runtime platform should platform-gated UI behave as."
    /// In a build it is simply <see cref="Application.platform" />. In the editor — both edit mode and
    /// Play mode, including Play Mode tests — it is a <em>simulated</em> platform, so the UI can be previewed
    /// and asserted as iOS or Android without leaving the editor (<see cref="Application.platform" /> would
    /// otherwise report <c>OSXEditor</c>/<c>WindowsEditor</c>).
    /// </summary>
    /// <remarks>
    /// <para>
    /// Two knobs decide the editor answer. <see cref="FollowDeviceSimulator" /> hands the choice to the Device
    /// Simulator whenever it presents a device, which keeps this system agreeing with everything else that
    /// reads <c>UnityEngine.Device</c> — safe area, resolution and Unity Localization's platform entry
    /// overrides all follow that same pick. <see cref="Simulated" /> is the explicit answer used whenever the
    /// simulator is not driving, which covers a plain Game view, batch mode, and tests.
    /// </para>
    /// <para>
    /// <see cref="PlatformSpecific" /> and <see cref="PlatformSwitch" /> read <see cref="Current" /> instead of
    /// <see cref="Application.platform" /> directly, and re-evaluate whenever <see cref="Changed" /> fires,
    /// which is how changing the platform live-updates the scene.
    /// </para>
    /// </remarks>
    public static class PlatformResolver
    {
        /// <summary>
        /// Raised when the resolved editor platform changes so live views can re-apply their visibility.
        /// Never fires in a build. Because it is a static event, editor Domain Reload being disabled means
        /// stale subscribers survive between Play sessions — clear it from the enter-play-mode reset hook.
        /// </summary>
        public static event Action Changed;

#if UNITY_EDITOR
        const string SimulatedPrefKey = "E7.PlatformResolver.SimulatedPlatform";
        const string FollowPrefKey = "E7.PlatformResolver.FollowDeviceSimulator";
        const RuntimePlatform DefaultSimulated = RuntimePlatform.IPhonePlayer;

        /// <summary>
        /// The platform to behave as while the Device Simulator is not driving, persisted per-machine in
        /// EditorPrefs. It is always a shipping platform, which also makes it the fallback that keeps
        /// platform-gated objects from resolving to a desktop editor platform and all hiding themselves.
        /// Setting it raises <see cref="Changed" />.
        /// </summary>
        public static RuntimePlatform Simulated
        {
            get => (RuntimePlatform)UnityEditor.EditorPrefs.GetInt(SimulatedPrefKey, (int)DefaultSimulated);
            set
            {
                if (Simulated == value) return;
                UnityEditor.EditorPrefs.SetInt(SimulatedPrefKey, (int)value);
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// Whether a Device Simulator presenting a device picks the platform in preference to
        /// <see cref="Simulated" />. Tests turn this off so that a simulator window left open on an author's
        /// machine cannot change what they assert. Setting it raises <see cref="Changed" />.
        /// </summary>
        public static bool FollowDeviceSimulator
        {
            get => UnityEditor.EditorPrefs.GetBool(FollowPrefKey, true);
            set
            {
                if (FollowDeviceSimulator == value) return;
                UnityEditor.EditorPrefs.SetBool(FollowPrefKey, value);
                Changed?.Invoke();
            }
        }

        /// <summary>
        /// The platform the Device Simulator presents, or <see langword="null" /> when it presents none — the
        /// shim reports the desktop editor platform while the Game view is not in Simulator mode, and that
        /// answer is reported as "nothing to follow" rather than passed on.
        /// </summary>
        public static RuntimePlatform? DeviceSimulatorPlatform
        {
            get
            {
                RuntimePlatform platform;
                try
                {
                    platform = UnityEngine.Device.Application.platform;
                }
                catch (NullReferenceException)
                {
                    // The Simulator window installs its shim before handing it a device, and asking the shim
                    // in that window throws from inside it. Having no device yet is the same answer as
                    // presenting none.
                    return null;
                }
                return IsEditorPlatform(platform) ? (RuntimePlatform?)null : platform;
            }
        }

        /// <summary>
        /// The platform to behave as: the Device Simulator's while it drives, otherwise <see cref="Simulated" />.
        /// </summary>
        public static RuntimePlatform Current
        {
            get
            {
                if (FollowDeviceSimulator)
                {
                    RuntimePlatform? fromSimulator = DeviceSimulatorPlatform;
                    if (fromSimulator.HasValue) return fromSimulator.Value;
                }
                return Simulated;
            }
        }

        /// <summary>
        /// Raises <see cref="Changed" /> on behalf of a source outside this class, such as the Device Simulator
        /// reporting that its device changed.
        /// </summary>
        public static void NotifyChanged() => Changed?.Invoke();

        static bool IsEditorPlatform(RuntimePlatform platform)
            => platform == RuntimePlatform.OSXEditor
               || platform == RuntimePlatform.WindowsEditor
               || platform == RuntimePlatform.LinuxEditor;
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
