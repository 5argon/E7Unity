using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
#endif

/// <summary>
/// A special mobile-centric button with separated on down and up event, not just on click.
/// Useful when you want to play the button down sound, but take action on click, for example.
/// Up is useful for cosmetic reason, since click means the user wants to take action for that button.
/// 
/// Animator trigger behave more explicitly too : Normal, Down, Up, Click, Disabled.
/// With just "Pressed" and "Normal" of Button, you can't properly react differently to "Click" or just "Up", 
/// though to get the same behaviour you must make the transition for both Click and Up to the same way.
/// 
/// It doesn't support "selected" state.
/// 
/// Right click on the component's header and select Create Animator to get an Animator with required triggers,
/// plus reasonable setup as a starting point for you.
/// </summary>
[RequireComponent(typeof(Animator))]
public class ButtonEx : Selectable, IPointerClickHandler
{
    [Tooltip("On Click only invoke if you up inside the button's rectangle.")]
    public UnityEvent onClick;
    [Tooltip("On Up will invoke regardless if you up inside or outside the button's rectangle.")]
    public UnityEvent onUp;
    public UnityEvent onDown;

    private SelectionState currentState = SelectionState.Normal;

    private const string triggerNormal = "Normal";
    private const string triggerDown = "Down";
    private const string triggerUp = "Up";
    private const string triggerClick = "Click";
    private const string triggerDisabled = "Disabled";

#if UNITY_EDITOR
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
        tran.AddCondition(AnimatorConditionMode.If, 0, triggerUp);

        tran = downState.AddTransition(upState, false);
        tran.hasExitTime = false;
        tran.duration = 0;
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

    public override void OnPointerDown(PointerEventData eventData)
    {
        base.OnPointerDown(eventData);
        if (!IsActive() || !IsInteractable())
            return;
        onDown.Invoke();
        ClearAndTrigger(triggerDown);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);
        if (!IsActive() || !IsInteractable())
            return;
        onUp.Invoke();
        ClearAndTrigger(triggerUp);
    }

    public virtual void OnPointerClick(PointerEventData eventData)
    {
        if (!IsActive() || !IsInteractable())
            return;
        onClick.Invoke();
        ClearAndTrigger(triggerClick);
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
        if (animator != null && animator.playableGraph.IsValid())
        {
            animator.ResetTrigger(triggerNormal);
            animator.ResetTrigger(triggerDown);
            animator.ResetTrigger(triggerUp);
            animator.ResetTrigger(triggerClick);
            animator.ResetTrigger(triggerDisabled);

            animator.SetTrigger(trigger);
        }
    }

    protected override void DoStateTransition(SelectionState state, bool instant)
    {
        base.DoStateTransition(state, instant);
        switch (state)
        {
            // case SelectionState.Normal:
            //     break;
            case SelectionState.Disabled:
                ClearAndTrigger(triggerDisabled);
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
