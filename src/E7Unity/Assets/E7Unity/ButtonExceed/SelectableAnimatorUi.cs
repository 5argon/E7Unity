using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace E7.E7Unity
{
    /// <summary>
    /// Base for touch-oriented widgets that separate pressing down, lifting up, and clicking into three distinct
    /// events instead of collapsing them into one, and that drive their look through an <see cref="Animator"/>
    /// rather than through <see cref="Selectable"/>'s built-in transitions.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The separation matters on touch screens. A press should usually be felt immediately — a click sound, a dent in
    /// the graphic — while the action itself should only happen on a genuine click, that is, when the finger lifts
    /// while still inside the widget. Lifting outside is the player changing their mind, and deserves a cosmetic
    /// response without the action. <see cref="onDown"/>, <see cref="onUp"/> and <see cref="OnClicked"/> map onto
    /// exactly those three moments.
    /// </para>
    /// <para>
    /// Animation follows the same split. Rather than the <c>Normal</c>/<c>Pressed</c> pair of a stock selectable, it
    /// drives five explicit <see cref="Animator"/> triggers — <c>Normal</c>, <c>Down</c>, <c>Up</c>, <c>Click</c> and
    /// <c>Disabled</c> — so a click and a mere release can look different. Only one is ever pending: setting any of
    /// them resets the rest.
    /// </para>
    /// <para>
    /// Being focused is not one of those moments, so it is not one of those triggers. A widget stays focused while it
    /// is pressed, released and clicked, which a single <see cref="Selectable.SelectionState"/> cannot say — that
    /// enum reports whichever of pressed, selected or highlighted ranks highest, and the rest is lost. Focus is
    /// therefore an <see cref="AnimatorFlag"/> on a layer of its own, overlaying whatever the interaction layer is
    /// doing underneath.
    /// </para>
    /// <para>
    /// Focus is narrower than selection. A mouse press selects a widget too, so the event system's selection cannot
    /// stand in for a keyboard cursor without one appearing under every click. <c>Focused</c> means selected while
    /// <see cref="UiInputMode"/> says the player is navigating rather than pointing, which leaves the cursor on the
    /// right widget when they go back to the keyboard without drawing it while they are on the mouse. The plain
    /// <c>Selected</c>, <c>Highlighted</c>, <c>Pressed</c> and <c>PointerMode</c> bools sit alongside it for
    /// conditions of your own — hover that defers to the keyboard reads <c>Highlighted</c> and <c>PointerMode</c>
    /// together.
    /// </para>
    /// <para>
    /// The built-in transition system is not supported — <see cref="Selectable.transition"/> is forced to
    /// <see cref="Selectable.Transition.None"/> because animation is driven by the parameters above.
    /// </para>
    /// <para>
    /// Parameters the controller does not declare are skipped rather than set, so a controller authored before a
    /// parameter existed keeps working and a controller that only cares about some states need only declare those.
    /// </para>
    /// </remarks>
    /// <seealso cref="ButtonAnimatorUi"/>
    /// <seealso cref="ToggleAnimatorUi"/>
    [RequireComponent(typeof(Animator))]
    public abstract class SelectableAnimatorUi : Selectable, IPointerClickHandler
    {
        /// <summary>
        /// Invoked whenever the press is released, whether inside or outside the widget's rectangle. Suited to
        /// cosmetic responses, since a release outside means the player backed out.
        /// </summary>
        [Tooltip("On Up will invoke regardless if you up inside or outside the button's rectangle.")]
        public UnityEvent onUp;

        /// <summary>
        /// Invoked the moment the widget is pressed. Suited to immediate feedback such as a press sound.
        /// </summary>
        public UnityEvent onDown;

        private SelectionState currentState = SelectionState.Normal;

        private readonly HashSet<int> declaredParameters = new HashSet<int>();
        private bool parametersCached;

        private bool selected;
        private bool focusApplied;
        private bool focusKnown;

        internal const string triggerNormal = "Normal";
        internal const string triggerDown = "Down";
        internal const string triggerUp = "Up";
        internal const string triggerClick = "Click";
        internal const string triggerDisabled = "Disabled";

        internal const string boolHighlighted = "Highlighted";
        internal const string boolPressed = "Pressed";
        internal const string boolSelected = "Selected";
        internal const string boolPointerMode = "PointerMode";

        /// <summary>
        /// Whether the widget carries the keyboard cursor, kept true across pressing, releasing and clicking it.
        /// </summary>
        internal static readonly AnimatorFlag focusedFlag =
            new AnimatorFlag("Focused", "Focus", "Unfocus", "SnapFocused", "SnapUnfocused");

        private static readonly int hashNormal = Animator.StringToHash(triggerNormal);
        private static readonly int hashDown = Animator.StringToHash(triggerDown);
        private static readonly int hashUp = Animator.StringToHash(triggerUp);
        private static readonly int hashClick = Animator.StringToHash(triggerClick);
        private static readonly int hashDisabled = Animator.StringToHash(triggerDisabled);
        private static readonly int hashHighlighted = Animator.StringToHash(boolHighlighted);
        private static readonly int hashPressed = Animator.StringToHash(boolPressed);
        private static readonly int hashSelected = Animator.StringToHash(boolSelected);
        private static readonly int hashPointerMode = Animator.StringToHash(boolPointerMode);

    #if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            transition = Transition.None;
        }

        protected override void Reset()
        {
            base.Reset();
            transition = Transition.None;
        }
    #endif

        protected override void OnEnable()
        {
            base.OnEnable();

            parametersCached = false;
            focusKnown = false;
            UiInputMode.Changed += OnInputModeChanged;

            if (!ApplyRestingPose() && Application.isPlaying)
            {
                StartCoroutine(ApplyRestingPoseWhenAnimatorReady());
            }
        }

        protected override void OnDisable()
        {
            UiInputMode.Changed -= OnInputModeChanged;

            // Invalidate after the base has cleared state through it, so that pass still runs off the cache.
            base.OnDisable();
            parametersCached = false;
        }

        /// <summary>
        /// Return the animator to a clean pose and put every state that persists back into it, without playing the
        /// way there.
        /// </summary>
        /// <remarks>
        /// <para>
        /// A screen can close on top of a press, leaving the animator parked partway through a clip with everything
        /// that clip wrote still applied. Enabling the object again does not undo that: the state machine returns to
        /// its default state, but a state writes nothing back unless its own clip animates the same properties, and
        /// a resting clip rarely animates what a press clip does. Rebinding and writing defaults is what puts those
        /// properties back.
        /// </para>
        /// <para>
        /// Evaluating once at the end lands the result on the frame the object appears rather than the frame after,
        /// which is the difference between a clean open and a flash of however the last press left it.
        /// </para>
        /// </remarks>
        /// <returns>Whether the animator was ready to be told.</returns>
        private bool ApplyRestingPose()
        {
            if (!AnimatorUsable)
                return false;

            animator.Rebind();
            animator.WriteDefaultValues();

            // Rebinding returned every parameter to its default, so nothing previously applied still holds.
            focusKnown = false;

            WriteRestingPose();

            animator.Update(0f);
            return true;
        }

        /// <summary>
        /// Put every state that persists into its pose. Override to add whatever else a widget holds, calling the
        /// base first.
        /// </summary>
        protected virtual void WriteRestingPose()
        {
            EventSystem events = EventSystem.current;
            selected = events != null && events.currentSelectedGameObject == gameObject;

            SetBool(hashSelected, selected);
            SetBool(hashPointerMode, UiInputMode.IsPointer);
            ApplyFocus(false);
        }

        /// <summary>
        /// The <see cref="Animator"/> is ordered after this component on the GameObject, so on the frame the object
        /// is enabled its playable graph can still be missing and there is nothing to set parameters on yet.
        /// </summary>
        private IEnumerator ApplyRestingPoseWhenAnimatorReady()
        {
            yield return null;
            ApplyRestingPose();
        }

        /// <summary>
        /// Fires the <c>Down</c> animation trigger and <see cref="onDown"/>. Does nothing while the widget is
        /// inactive or not interactable.
        /// </summary>
        /// <param name="eventData">Pointer data supplied by the event system.</param>
        public override void OnPointerDown(PointerEventData eventData)
        {
            UiInputMode.ReportPointer();
            base.OnPointerDown(eventData);
            if (!IsActive() || !IsInteractable())
                return;
            SetBool(hashPressed, true);
            ClearAndTrigger(hashDown);
            onDown.Invoke();
        }

        /// <summary>
        /// Fires the <c>Up</c> animation trigger and <see cref="onUp"/> on release, regardless of whether the pointer
        /// is still inside the widget. Does nothing while the widget is inactive or not interactable.
        /// </summary>
        /// <param name="eventData">Pointer data supplied by the event system.</param>
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            SetBool(hashPressed, false);
            if (!IsActive() || !IsInteractable())
                return;
            ClearAndTrigger(hashUp);
            onUp.Invoke();
        }

        //TODO: It is possible to down while uninteractable, then up when interactable and trigger this click..
        /// <summary>
        /// Fires the <c>Click</c> animation trigger and hands over to <see cref="OnClicked"/> when the pointer is
        /// released inside the widget. Does nothing while the widget is inactive or not interactable.
        /// </summary>
        /// <param name="eventData">Pointer data supplied by the event system.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            ClearAndTrigger(hashClick);
            OnClicked();
        }

        /// <summary>
        /// What a genuine click means for this widget, invoked after the <c>Click</c> trigger has been fired.
        /// </summary>
        protected abstract void OnClicked();

        /// <summary>
        /// Raises the <c>Highlighted</c> bool while the pointer rests inside the widget, which on desktop is the
        /// hover look.
        /// </summary>
        public override void OnPointerEnter(PointerEventData eventData)
        {
            UiInputMode.ReportPointer();
            base.OnPointerEnter(eventData);
            SetBool(hashHighlighted, true);
        }

        public override void OnPointerExit(PointerEventData eventData)
        {
            base.OnPointerExit(eventData);
            SetBool(hashHighlighted, false);
        }

        /// <summary>
        /// Navigating is how the player says they are on the keyboard. Whatever the move lands on reports the switch
        /// itself as it takes the selection, so the mode is only forced here afterwards, for a move that had nowhere
        /// to go and left the selection where it was.
        /// </summary>
        /// <remarks>
        /// Reporting first would hand the cursor to the widget being navigated away from, which then loses it again
        /// in the same frame — an arrival and a departure animation for a cursor that was never there.
        /// </remarks>
        public override void OnMove(AxisEventData eventData)
        {
            base.OnMove(eventData);
            UiInputMode.ReportDirectional();
        }

        /// <summary>
        /// Takes the event system's selection, and reads the kind of event that brought it to tell a mouse press
        /// apart from a navigation move.
        /// </summary>
        public override void OnSelect(BaseEventData eventData)
        {
            base.OnSelect(eventData);

            selected = true;
            SetBool(hashSelected, true);

            UiInputMode.Report(eventData is PointerEventData
                ? UiInputMode.Mode.Pointer
                : UiInputMode.Mode.Directional);

            ApplyFocus(true);
        }

        public override void OnDeselect(BaseEventData eventData)
        {
            base.OnDeselect(eventData);

            selected = false;
            SetBool(hashSelected, false);
            ApplyFocus(true);
        }

        protected override void InstantClearState()
        {
            base.InstantClearState();

            ClearAndTrigger(hashNormal);
            SetBool(hashHighlighted, false);
            SetBool(hashPressed, false);

            selected = false;
            SetBool(hashSelected, false);
            focusKnown = false;
            ApplyFocus(false);
        }

        /// <summary>
        /// Clear out any unused trigger and trigger the next one.
        /// </summary>
        protected void ClearAndTrigger(int trigger)
        {
            if (!AnimatorUsable)
                return;

            ResetTrigger(hashNormal);
            ResetTrigger(hashDown);
            ResetTrigger(hashUp);
            ResetTrigger(hashClick);
            ResetTrigger(hashDisabled);

            SetTrigger(trigger);
        }

        /// <summary>
        /// Drive a flag to <paramref name="value"/>, either along the path authored for arriving at it or straight to
        /// the resting pose.
        /// </summary>
        protected void ApplyFlag(in AnimatorFlag flag, bool value, bool animate)
        {
            if (!AnimatorUsable)
                return;

            SetBool(flag.valueHash, value);

            ResetTrigger(flag.turnOnHash);
            ResetTrigger(flag.turnOffHash);
            ResetTrigger(flag.snapOnHash);
            ResetTrigger(flag.snapOffHash);

            if (animate)
                SetTrigger(value ? flag.turnOnHash : flag.turnOffHash);
            else
                SetTrigger(value ? flag.snapOnHash : flag.snapOffHash);
        }

        /// <summary>
        /// Whether the <see cref="Animator"/> is in a state where parameters can be set at all. It is not while the
        /// controller is missing, or while the object is inactive and the playable graph has been torn down.
        /// </summary>
        protected bool AnimatorUsable => animator != null && animator.playableGraph.IsValid();

        private void OnInputModeChanged()
        {
            SetBool(hashPointerMode, UiInputMode.IsPointer);
            ApplyFocus(true);
        }

        /// <summary>
        /// The cursor belongs on the selected widget only while the player is navigating. Applying the value it
        /// already holds is skipped, so the way in is not replayed every time something else moves the mode.
        /// </summary>
        private void ApplyFocus(bool animate)
        {
            // Nothing was recorded if nothing could be told, so that the resting pose still lands once the animator
            // catches up.
            if (!AnimatorUsable)
                return;

            bool focus = selected && !UiInputMode.IsPointer;
            if (focusKnown && focusApplied == focus)
                return;

            focusApplied = focus;
            focusKnown = true;
            ApplyFlag(focusedFlag, focus, animate);
        }

        private void SetTrigger(int hash)
        {
            if (Declared(hash))
                animator.SetTrigger(hash);
        }

        private void ResetTrigger(int hash)
        {
            if (Declared(hash))
                animator.ResetTrigger(hash);
        }

        private void SetBool(int hash, bool value)
        {
            if (AnimatorUsable && Declared(hash))
                animator.SetBool(hash, value);
        }

        /// <summary>
        /// Whether the running controller declares a parameter, so that setting one it never heard of is skipped
        /// rather than logged. Only call while <see cref="AnimatorUsable"/> holds.
        /// </summary>
        private bool Declared(int hash)
        {
            if (!parametersCached)
            {
                declaredParameters.Clear();
                foreach (AnimatorControllerParameter parameter in animator.parameters)
                {
                    declaredParameters.Add(parameter.nameHash);
                }
                parametersCached = true;
            }

            return declaredParameters.Contains(hash);
        }

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            base.DoStateTransition(state, instant);
            switch (state)
            {
                case SelectionState.Disabled:
                    //Disabled state could be triggered without clearing out unused triggers
                    if (AnimatorUsable)
                    {
                        SetTrigger(hashDisabled);
                    }
                    break;
                default:
                    if (currentState == SelectionState.Disabled)
                    {
                        ClearAndTrigger(hashNormal);
                    }
                    break;
            }
            currentState = state;
        }
    }
}
