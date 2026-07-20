using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

namespace E7.E7Unity
{
    /// <summary>
    /// A touch-oriented button that separates pressing down, lifting up, and clicking into three distinct events
    /// instead of collapsing them into one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The separation matters on touch screens. A press should usually be felt immediately — a click sound, a dent in
    /// the graphic — while the action itself should only happen on a genuine click, that is, when the finger lifts
    /// while still inside the button. Lifting outside is the player changing their mind, and deserves a cosmetic
    /// response without the action. <see cref="onDown"/>, <see cref="onUp"/> and <see cref="onClick"/> map onto
    /// exactly those three moments.
    /// </para>
    /// <para>
    /// Animation follows the same split. Rather than the <c>Normal</c>/<c>Pressed</c> pair of a stock button, it
    /// drives five explicit <see cref="Animator"/> triggers — <c>Normal</c>, <c>Down</c>, <c>Up</c>, <c>Click</c> and
    /// <c>Disabled</c> — so a click and a mere release can look different. Only one is ever pending: setting any of
    /// them resets the rest.
    /// </para>
    /// <para>
    /// The "selected" state is not supported, and neither is the built-in transition system —
    /// <see cref="Selectable.transition"/> is forced to <see cref="Selectable.Transition.None"/> because animation is
    /// driven by the triggers above.
    /// </para>
    /// <para>
    /// To get started, right-click the component header and choose <b>Create Animator</b>. It builds a controller
    /// wired with all five triggers, a click layer and an idle layer, as a reasonable starting point.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(Animator))]
    public class ButtonEx : Selectable, IPointerClickHandler
    {
        /// <summary>
        /// Invoked on a genuine click: the finger or cursor lifted while still inside the button's rectangle. This is
        /// where the button's actual action belongs.
        /// </summary>
        [Tooltip("On Click only invoke if you up inside the button's rectangle.")]
        public UnityEvent onClick;

        /// <summary>
        /// Invoked whenever the press is released, whether inside or outside the button's rectangle. Suited to
        /// cosmetic responses, since a release outside means the player backed out.
        /// </summary>
        [Tooltip("On Up will invoke regardless if you up inside or outside the button's rectangle.")]
        public UnityEvent onUp;

        /// <summary>
        /// Invoked the moment the button is pressed. Suited to immediate feedback such as a press sound.
        /// </summary>
        public UnityEvent onDown;

        private SelectionState currentState = SelectionState.Normal;

        private const string triggerNormal = "Normal";
        private const string triggerDown = "Down";
        private const string triggerUp = "Up";
        private const string triggerClick = "Click";
        private const string triggerDisabled = "Disabled";

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

        [ContextMenu("Create Animator")]
        void CreateAnimator()
        {
            var path = GetSaveControllerPath(this);
            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            AssetDatabase.ImportAsset(path);
            animator.runtimeAnimatorController = controller;

            var normalState = GenerateTriggerableTransition(triggerNormal, controller);
            var downState = GenerateTriggerableTransition(triggerDown, controller);
            var upState = GenerateTriggerableTransition(triggerUp, controller);
            var disabledState = GenerateTriggerableTransition(triggerDisabled, controller);

            controller.AddLayer("Click Effect Layer");
            controller.AddLayer("Idle Layer");

            controller.AddParameter(triggerClick, AnimatorControllerParameterType.Trigger);
            var clickClip = AnimatorController.AllocateAnimatorClip(triggerClick);
            AnimationClipSettings s = AnimationUtility.GetAnimationClipSettings(clickClip);
            s.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clickClip, s);

            var idleClip = AnimatorController.AllocateAnimatorClip("Idle");
            AssetDatabase.AddObjectToAsset(clickClip, controller);
            AssetDatabase.AddObjectToAsset(idleClip, controller);
            AssetDatabase.ImportAsset(path);

            var ly0 = controller.layers[0].stateMachine;
            var ly1 = controller.layers[1].stateMachine;
            var ly2 = controller.layers[2].stateMachine;

            controller.layers[1].defaultWeight = 1;
            controller.layers[2].defaultWeight = 1;

            var clickState = controller.AddMotion(clickClip, 1);
            var clickWaitState = ly1.AddState("Wait State");
            ly1.defaultState = clickWaitState;
            var clickTransition = ly1.AddAnyStateTransition(clickState);
            clickTransition.AddCondition(AnimatorConditionMode.If, 0, triggerClick);
            clickTransition.hasExitTime = false;
            clickTransition.duration = 0;

            var idleState = controller.AddMotion(idleClip, 2);
            ly2.defaultState = idleState;
            idleClip.wrapMode = WrapMode.Loop;

            var tran = ly0.AddAnyStateTransition(normalState);
            tran.AddCondition(AnimatorConditionMode.If, 0, triggerNormal);
            tran.hasExitTime = false;
            tran.duration = 0;

            tran = ly0.AddAnyStateTransition(disabledState);
            tran.AddCondition(AnimatorConditionMode.If, 0, triggerDisabled);
            tran.hasExitTime = false;
            tran.duration = 0;

            tran = normalState.AddTransition(downState, false);
            tran.hasExitTime = false;
            tran.duration = 0;
            tran.AddCondition(AnimatorConditionMode.If, 0, triggerDown);

            tran = downState.AddTransition(upState, false);
            tran.hasExitTime = false;
            tran.duration = 0;
            tran.name = "Down -> Up by Up";
            tran.AddCondition(AnimatorConditionMode.If, 0, triggerUp);

            tran = downState.AddTransition(upState, false);
            tran.hasExitTime = false;
            tran.duration = 0;
            tran.name = "Down -> Up by Click";
            tran.AddCondition(AnimatorConditionMode.If, 0, triggerClick);

            tran = upState.AddTransition(downState, false);
            tran.hasExitTime = false;
            tran.duration = 0;
            tran.AddCondition(AnimatorConditionMode.If, 0, triggerDown);

            tran = upState.AddTransition(normalState, false);
            tran.hasExitTime = true;
            tran.exitTime = 1;
            tran.duration = 0;


            string GetSaveControllerPath(Selectable target)
            {
                var defaultName = target.gameObject.name;
                var message = string.Format("Create a new animator for the game object '{0}':", defaultName);
                return EditorUtility.SaveFilePanelInProject("New Animation Contoller", defaultName, "controller", message);
            }

            AnimatorState GenerateTriggerableTransition(string name, AnimatorController cont)
            {
                // Create the clip
                var clip = AnimatorController.AllocateAnimatorClip(name);
                AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
                settings.loopTime = false;
                AnimationUtility.SetAnimationClipSettings(clip, settings);
                clip.wrapMode = WrapMode.Once;
                AssetDatabase.AddObjectToAsset(clip, cont);

                // Create a state in the animatior controller for this clip
                var state = cont.AddMotion(clip);

                // Add a transition property
                cont.AddParameter(name, AnimatorControllerParameterType.Trigger);

                // // Add an any state transition
                // var stateMachine = cont.layers[0].stateMachine;
                // var transition = stateMachine.AddAnyStateTransition(state);
                // transition.AddCondition(UnityEditor.Animations.AnimatorConditionMode.If, 0, name);
                return state;
            }
        }
    #endif

        protected override void OnEnable()
        {
            base.OnEnable();
        }

        /// <summary>
        /// Fires the <c>Down</c> animation trigger and <see cref="onDown"/>. Does nothing while the button is
        /// inactive or not interactable.
        /// </summary>
        /// <param name="eventData">Pointer data supplied by the event system.</param>
        public override void OnPointerDown(PointerEventData eventData)
        {
            base.OnPointerDown(eventData);
            if (!IsActive() || !IsInteractable())
                return;
            ClearAndTrigger(triggerDown);
            onDown.Invoke();
        }

        /// <summary>
        /// Fires the <c>Up</c> animation trigger and <see cref="onUp"/> on release, regardless of whether the pointer
        /// is still inside the button. Does nothing while the button is inactive or not interactable.
        /// </summary>
        /// <param name="eventData">Pointer data supplied by the event system.</param>
        public override void OnPointerUp(PointerEventData eventData)
        {
            base.OnPointerUp(eventData);
            if (!IsActive() || !IsInteractable())
                return;
            ClearAndTrigger(triggerUp);
            onUp.Invoke();
        }

        //TODO: It is possible to down while uninteractable, then up when interactable and trigger this click..
        /// <summary>
        /// Fires the <c>Click</c> animation trigger and <see cref="onClick"/> when the pointer is released inside the
        /// button. Does nothing while the button is inactive or not interactable.
        /// </summary>
        /// <param name="eventData">Pointer data supplied by the event system.</param>
        public virtual void OnPointerClick(PointerEventData eventData)
        {
            if (!IsActive() || !IsInteractable())
                return;

            ClearAndTrigger(triggerClick);
            onClick.Invoke();
        }


        protected override void InstantClearState()
        {
            base.InstantClearState();
            ClearAndTrigger(triggerNormal);
        }

        /// <summary>
        /// Clear out any unused trigger and trigger the next one.
        /// </summary>
        private void ClearAndTrigger(string trigger)
        {
            if (AnimatorUsable)
            {
                animator.ResetTrigger(triggerNormal);
                animator.ResetTrigger(triggerDown);
                animator.ResetTrigger(triggerUp);
                animator.ResetTrigger(triggerClick);
                animator.ResetTrigger(triggerDisabled);

                animator.SetTrigger(trigger);
            }
        }

        private bool AnimatorUsable => animator != null && animator.playableGraph.IsValid();

        protected override void DoStateTransition(SelectionState state, bool instant)
        {
            //Debug.Log($"Transition to state {state}");
            base.DoStateTransition(state, instant);
            switch (state)
            {
                // case SelectionState.Normal:
                //     break;
                case SelectionState.Disabled:
                    //Disabled state could be triggered without clearing out unused triggers
                    if(AnimatorUsable)
                    {
                        animator.SetTrigger(triggerDisabled);
                    }
                    //ClearAndTrigger(triggerDisabled);
                    break;
                // case SelectionState.Highlighted:
                //     break;
                // case SelectionState.Pressed:
                //     break;
                // case SelectionState.Selected:
                //     break;
                default:
                    if (currentState == SelectionState.Disabled)
                    {
                        ClearAndTrigger(triggerNormal);
                    }
                    break;

            }
            currentState = state;
        }
    }
}
