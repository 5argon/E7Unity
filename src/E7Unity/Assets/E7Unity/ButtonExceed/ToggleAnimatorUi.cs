using System;
using UnityEngine;
using UnityEngine.Events;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace E7.E7Unity
{
    /// <summary>
    /// A touch-oriented toggle built on the same three-moment interaction model as
    /// <see cref="ButtonAnimatorUi"/>, holding an on/off value on top of it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Turning on and turning off are two separately authored animations rather than one property blended in both
    /// directions. Beyond the five triggers of <see cref="SelectableAnimatorUi"/>, a toggle drives <c>ToggleOn</c> and
    /// <c>ToggleOff</c> — the check coming in and going out — each running through a one-shot state that falls
    /// through to the resting <c>On</c> or <c>Off</c> pose once it finishes.
    /// </para>
    /// <para>
    /// <c>SnapOn</c> and <c>SnapOff</c> reach the same two resting poses without playing anything in between. Those
    /// are what a toggle uses when it is enabled or restored from saved settings, so a dialog opening on an
    /// already-checked toggle shows the check rather than animating it in.
    /// </para>
    /// <para>
    /// An <c>IsOn</c> bool carries the current value for conditions of your own. The generated controller declares it
    /// without using it, leaving it free for cases such as a pressed look that differs while checked.
    /// </para>
    /// <para>
    /// Only one toggle in a <see cref="ToggleGroupAnimatorUi"/> can be on at a time. Being in a group changes what
    /// turning off means: unless the group allows switching off, pressing the toggle that is already on leaves it on.
    /// </para>
    /// <para>
    /// To get started, right-click the component header and choose <b>Create Animator</b>. It builds a controller
    /// wired with the interaction triggers, a click layer, a focus layer, a disabled layer, an idle layer and a
    /// toggle state layer, as a reasonable starting point.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(Animator))]
    public class ToggleAnimatorUi : SelectableAnimatorUi
    {
        /// <summary>
        /// UnityEvent callback carrying the value the toggle settled on.
        /// </summary>
        [Serializable]
        public class ToggleEvent : UnityEvent<bool>
        {}

        /// <summary>
        /// Invoked whenever the value settles, with the value it settled on. Note that a toggle held on by its group
        /// reports <c>true</c> even though it was pressed to turn off.
        /// </summary>
        public ToggleEvent onValueChanged = new ToggleEvent();

        [Tooltip("Is the toggle currently on or off?")]
        [SerializeField] private bool isOn;

        [Tooltip("Group this toggle belongs to, in which only one toggle can be on at a time.")]
        [SerializeField] private ToggleGroupAnimatorUi group;

        /// <summary>
        /// Whether the toggle is on, and the two authored paths between the checked and unchecked poses.
        /// </summary>
        internal static readonly AnimatorFlag onFlag =
            new AnimatorFlag("IsOn", "ToggleOn", "ToggleOff", "SnapOn", "SnapOff");

        private const string toggleStateLayer = "Toggle State Layer";

        /// <summary>
        /// Whether the toggle is currently on. Setting it animates the change and invokes
        /// <see cref="onValueChanged"/>, exactly as a press would.
        /// </summary>
        public bool IsOn
        {
            get => isOn;
            set => Set(value, true, true);
        }

        /// <summary>
        /// Group this toggle belongs to, in which only one toggle can be on at a time. Assigning moves the toggle
        /// between groups, leaving both in a valid state.
        /// </summary>
        public ToggleGroupAnimatorUi Group
        {
            get => group;
            set
            {
                SetGroup(value, true, true);
                ApplyToggleState(false);
            }
        }

        /// <summary>
        /// Change the value and animate it, without invoking <see cref="onValueChanged"/>.
        /// </summary>
        public void SetIsOnWithoutNotify(bool value)
        {
            Set(value, false, true);
        }

        /// <summary>
        /// Change the value without animating it or invoking <see cref="onValueChanged"/>, arriving straight at the
        /// resting pose. Suited to filling a screen in from saved settings.
        /// </summary>
        public void SetIsOnInstant(bool value)
        {
            Set(value, false, false);
        }

        internal void SetFromGroup(bool value, bool sendCallback, bool animate)
        {
            Set(value, sendCallback, animate);
        }

        private void Set(bool value, bool sendCallback, bool animate)
        {
            if (isOn == value)
                return;

            bool previous = isOn;

            // if we are in a group and set to true, do group logic
            isOn = value;
            if (group != null && group.isActiveAndEnabled && IsActive())
            {
                if (isOn || (!group.AnyTogglesOn() && !group.AllowSwitchOff))
                {
                    isOn = true;
                    group.NotifyToggleOn(this, sendCallback, animate);
                }
            }

            // A group can hold this toggle on after a press meant to turn it off, which lands back on the pose that
            // is already showing and has nothing to animate.
            if (isOn != previous)
            {
                ApplyToggleState(animate);
            }

            // Always send event when toggle is clicked, even if value didn't change
            // due to already active toggle in a toggle group being clicked.
            if (sendCallback)
            {
                onValueChanged.Invoke(isOn);
            }
        }

        protected override void OnClicked()
        {
            Set(!isOn, true, true);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            SetGroup(group, false, false);
        }

        protected override void WriteRestingPose()
        {
            base.WriteRestingPose();
            ApplyToggleState(false);
        }

        protected override void OnDisable()
        {
            SetGroup(null, false, false);
            base.OnDisable();
        }

        protected override void OnDestroy()
        {
            if (group != null)
                group.EnsureValidState();
            base.OnDestroy();
        }

        private void SetGroup(ToggleGroupAnimatorUi newGroup, bool setMemberValue, bool animate)
        {
            // Sometimes IsActive returns false in OnDisable so don't check for it.
            // Rather remove the toggle too often than too little.
            if (group != null)
                group.UnregisterToggle(this);

            // At runtime the group variable should be set but not when calling this method from OnEnable or OnDisable.
            // That's why we use the setMemberValue parameter.
            if (setMemberValue)
                group = newGroup;

            // Only register to the new group if this toggle is active.
            if (newGroup != null && IsActive())
                newGroup.RegisterToggle(this);

            // If we are in a new group, and this toggle is on, notify group.
            // Note: Don't refer to group here as it's not guaranteed to have been set.
            if (newGroup != null && isOn && IsActive())
                newGroup.NotifyToggleOn(this, true, animate);
        }

        /// <summary>
        /// Drive the toggle state layer to match <see cref="isOn"/>, either through the animation authored for
        /// arriving at that value or straight to the resting pose.
        /// </summary>
        private void ApplyToggleState(bool animate)
        {
            ApplyFlag(onFlag, isOn, animate);
        }

    #if UNITY_EDITOR
        [ContextMenu("Create Animator")]
        void CreateAnimator()
        {
            string path = SelectableAnimatorUiBuilder.AskSavePath(gameObject);
            if (string.IsNullOrEmpty(path))
                return;

            AnimatorController controller = SelectableAnimatorUiBuilder.Create(path, toggleStateLayer);
            animator.runtimeAnimatorController = controller;

            SelectableAnimatorUiBuilder.BuildFlagLayer(
                controller, SelectableAnimatorUiBuilder.firstExtraLayer, onFlag,
                "Off", "On", "TurningOn", "TurningOff");

            AssetDatabase.ImportAsset(path);
        }
    #endif
    }
}
