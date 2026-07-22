#if UNITY_2021_1_OR_NEWER
using UnityEditor.DeviceSimulation;
using UnityEngine.UIElements;

namespace E7.E7Unity
{
    /// <summary>
    /// Bridges the Device Simulator into <see cref="PlatformResolver" />, so that picking a device also decides
    /// what <see cref="PlatformSpecific" /> and <see cref="PlatformSwitch" /> show, and adds a panel to the
    /// Simulator window holding the switch that governs it.
    /// </summary>
    /// <remarks>
    /// The simulator raises an event for every device change, which is forwarded as
    /// <see cref="PlatformResolver.Changed" /> so an <c>ExecuteAlways</c> <see cref="PlatformSwitch" /> re-applies
    /// the scene immediately. Creating and destroying this plugin brackets the window's own lifetime, and both
    /// ends forward the same notification because entering and leaving Simulator mode changes the answer as much
    /// as swapping devices does.
    /// </remarks>
    class PlatformResolverSimulatorPlugin : DeviceSimulatorPlugin
    {
        Toggle _follow;
        Label _resolved;
        bool _subscribed;

        public override string title => "Platform Resolver";

        public override void OnCreate()
        {
            Subscribe();
            PlatformResolver.NotifyChanged();
        }

        public override VisualElement OnCreateUI()
        {
            Subscribe();

            VisualElement root = new VisualElement();

            _follow = new Toggle("Drive Platform-Gated UI")
            {
                tooltip = "While on, the device selected here decides which PlatformSpecific objects are shown. " +
                          "While off, the platform picked in the project's own dev options decides instead.",
            };
            _follow.SetValueWithoutNotify(PlatformResolver.FollowDeviceSimulator);
            _follow.RegisterValueChangedCallback(evt => PlatformResolver.FollowDeviceSimulator = evt.newValue);
            root.Add(_follow);

            _resolved = new Label();
            root.Add(_resolved);

            root.RegisterCallback<AttachToPanelEvent>(_ =>
            {
                PlatformResolver.Changed += Refresh;
                Refresh();
            });
            root.RegisterCallback<DetachFromPanelEvent>(_ => PlatformResolver.Changed -= Refresh);

            Refresh();
            return root;
        }

        public override void OnDestroy()
        {
            if (_subscribed)
            {
                deviceSimulator.deviceChanged -= OnDeviceChanged;
                _subscribed = false;
            }
            PlatformResolver.NotifyChanged();
        }

        void Subscribe()
        {
            if (_subscribed || deviceSimulator == null) return;
            deviceSimulator.deviceChanged += OnDeviceChanged;
            _subscribed = true;
        }

        // Refreshing here as well as through the event keeps the panel truthful even across an enter-play-mode
        // reset, which drops every PlatformResolver.Changed subscriber including this one.
        void OnDeviceChanged()
        {
            PlatformResolver.NotifyChanged();
            Refresh();
        }

        void Refresh()
        {
            _follow?.SetValueWithoutNotify(PlatformResolver.FollowDeviceSimulator);
            if (_resolved == null) return;
            _resolved.text = PlatformResolver.FollowDeviceSimulator
                ? $"Platform-gated UI resolves to {PlatformResolver.Current}"
                : $"Not driving — platform-gated UI resolves to {PlatformResolver.Simulated}";
        }
    }
}
#endif
