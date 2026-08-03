using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace E7.E7Unity
{
    /// <summary>
    /// A group of <see cref="ToggleAnimatorUi"/> in which only one can be on at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Toggles register themselves as they are enabled and drop out as they are disabled, so a group only ever knows
    /// about the toggles currently alive under it.
    /// </para>
    /// <para>
    /// Turning one on turns the rest off through their authored "going out" animation, since that is a consequence of
    /// something the player just did. Corrections the group makes on its own — enforcing that something is on when
    /// the scene loads, or after a toggle is destroyed — arrive at the resting pose without animating, so a screen
    /// does not open on toggles animating themselves into their starting state.
    /// </para>
    /// </remarks>
    [DisallowMultipleComponent]
    public class ToggleGroupAnimatorUi : UIBehaviour
    {
        [Tooltip("Is it allowed that no toggle is switched on?")]
        [SerializeField] private bool allowSwitchOff;

        private readonly List<ToggleAnimatorUi> toggles = new List<ToggleAnimatorUi>();

        /// <summary>
        /// Whether it is allowed that no toggle is switched on.
        /// </summary>
        /// <remarks>
        /// When enabled, pressing the toggle that is currently switched on switches it off, leaving nothing on. When
        /// disabled, pressing it leaves it on. Note that even when disabled the group does not enforce the constraint
        /// right away if nothing is on as the scene loads — it only prevents the player from switching the last one
        /// off.
        /// </remarks>
        public bool AllowSwitchOff
        {
            get => allowSwitchOff;
            set => allowSwitchOff = value;
        }

        /// <summary>
        /// Because all the toggles have registered themselves in OnEnable, Start should check to make sure at least
        /// one toggle is on in groups that do not allow switching off.
        /// </summary>
        protected override void Start()
        {
            EnsureValidState();
            base.Start();
        }

        protected override void OnEnable()
        {
            EnsureValidState();
            base.OnEnable();
        }

        /// <summary>
        /// Notify the group that the given toggle is on, turning every other member off.
        /// </summary>
        /// <param name="toggle">The toggle that got triggered on.</param>
        /// <param name="sendCallback">If other toggles should send onValueChanged.</param>
        /// <param name="animate">If other toggles should play their way out rather than arrive at the resting pose.</param>
        public void NotifyToggleOn(ToggleAnimatorUi toggle, bool sendCallback = true, bool animate = true)
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i] == toggle)
                    continue;

                toggles[i].SetFromGroup(false, sendCallback, animate);
            }
        }

        /// <summary>
        /// Unregister a toggle from the group.
        /// </summary>
        /// <param name="toggle">The toggle to remove.</param>
        public void UnregisterToggle(ToggleAnimatorUi toggle)
        {
            if (toggles.Contains(toggle))
                toggles.Remove(toggle);
        }

        /// <summary>
        /// Register a toggle with the group so it is watched for changes and notified if another toggle in the group
        /// changes.
        /// </summary>
        /// <param name="toggle">The toggle to register with the group.</param>
        public void RegisterToggle(ToggleAnimatorUi toggle)
        {
            if (!toggles.Contains(toggle))
                toggles.Add(toggle);
        }

        /// <summary>
        /// Ensure that the group still holds a valid state. This is relevant when the group starts, or when a toggle
        /// has been deleted from it.
        /// </summary>
        public void EnsureValidState()
        {
            if (!allowSwitchOff && !AnyTogglesOn() && toggles.Count != 0)
            {
                toggles[0].SetFromGroup(true, true, false);
                NotifyToggleOn(toggles[0], true, false);
            }

            ToggleAnimatorUi firstActive = GetFirstActiveToggle();
            if (firstActive == null)
                return;

            for (int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i] == firstActive)
                    continue;

                toggles[i].SetFromGroup(false, true, false);
            }
        }

        /// <summary>
        /// Are any of the toggles on?
        /// </summary>
        public bool AnyTogglesOn()
        {
            return GetFirstActiveToggle() != null;
        }

        /// <summary>
        /// The toggles in this group that are on.
        /// </summary>
        /// <remarks>
        /// This only checks the on or off state, not the GameObject's active state.
        /// </remarks>
        public IEnumerable<ToggleAnimatorUi> ActiveToggles()
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i] != null && toggles[i].IsOn)
                    yield return toggles[i];
            }
        }

        /// <summary>
        /// The first toggle that is on, or <c>null</c> if none are.
        /// </summary>
        /// <remarks>
        /// This only checks the on or off state, not the GameObject's active state.
        /// </remarks>
        public ToggleAnimatorUi GetFirstActiveToggle()
        {
            for (int i = 0; i < toggles.Count; i++)
            {
                if (toggles[i] != null && toggles[i].IsOn)
                    return toggles[i];
            }
            return null;
        }

        /// <summary>
        /// Switch all toggles off, regardless of whether <see cref="AllowSwitchOff"/> is enabled.
        /// </summary>
        public void SetAllTogglesOff(bool sendCallback = true, bool animate = true)
        {
            bool oldAllowSwitchOff = allowSwitchOff;
            allowSwitchOff = true;

            for (int i = 0; i < toggles.Count; i++)
            {
                toggles[i].SetFromGroup(false, sendCallback, animate);
            }

            allowSwitchOff = oldAllowSwitchOff;
        }
    }
}
