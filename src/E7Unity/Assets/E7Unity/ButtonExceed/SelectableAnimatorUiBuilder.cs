#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Builds the starting-point <see cref="AnimatorController"/> that every <see cref="SelectableAnimatorUi"/>
    /// shares: an interaction layer carrying the <c>Normal</c>/<c>Down</c>/<c>Up</c>/<c>Disabled</c> states, a click
    /// layer overlaying a one-shot <c>Click</c> on top of whichever of those is showing, and a focus layer holding
    /// the keyboard cursor steady underneath both.
    /// </summary>
    /// <remarks>
    /// Each widget adds its own layers after those — <see cref="ButtonAnimatorUi"/> an idle layer,
    /// <see cref="ToggleAnimatorUi"/> a layer holding the on and off poses and the two paths between them.
    /// </remarks>
    internal static class SelectableAnimatorUiBuilder
    {
        internal const string clickLayer = "Click Effect Layer";
        internal const string focusLayer = "Focus Layer";

        /// <summary>
        /// Index of the first layer a widget adds for itself, after the three every widget shares.
        /// </summary>
        internal const int firstExtraLayer = 3;

        /// <summary>
        /// Ask where to save the controller. Returns an empty string when the dialog is cancelled.
        /// </summary>
        internal static string AskSavePath(GameObject target)
        {
            string defaultName = target.name;
            string message = $"Create a new animator for the game object '{defaultName}':";
            return EditorUtility.SaveFilePanelInProject("New Animation Controller", defaultName, "controller", message);
        }

        /// <summary>
        /// Create a controller at <paramref name="path"/> with the interaction, click and focus layers filled in,
        /// plus one empty layer per name in <paramref name="extraLayers"/> for the caller to populate.
        /// </summary>
        internal static AnimatorController Create(string path, params string[] extraLayers)
        {
            AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            AssetDatabase.ImportAsset(path);

            controller.AddLayer(clickLayer);
            controller.AddLayer(focusLayer);
            foreach (string extraLayer in extraLayers)
            {
                controller.AddLayer(extraLayer);
            }

            // Layer weights have to be written back through the array property, since reading `layers` hands out
            // copies rather than the live layers.
            AnimatorControllerLayer[] layers = controller.layers;
            for (int i = 1; i < layers.Length; i++)
            {
                layers[i].defaultWeight = 1;
            }
            controller.layers = layers;

            controller.AddParameter(SelectableAnimatorUi.boolHighlighted, AnimatorControllerParameterType.Bool);
            controller.AddParameter(SelectableAnimatorUi.boolPressed, AnimatorControllerParameterType.Bool);
            controller.AddParameter(SelectableAnimatorUi.boolSelected, AnimatorControllerParameterType.Bool);
            controller.AddParameter(SelectableAnimatorUi.boolPointerMode, AnimatorControllerParameterType.Bool);

            BuildClickLayer(controller);
            BuildInteractionLayer(controller);
            BuildFlagLayer(controller, 2, SelectableAnimatorUi.focusedFlag,
                "Unfocused", "Focused", "Focusing", "Unfocusing");
            return controller;
        }

        /// <summary>
        /// Fill a layer with the four states an <see cref="AnimatorFlag"/> moves between: a resting pose at each end,
        /// and a one-shot state for each authored way across that falls through to the pose it was heading for. The
        /// snap triggers reach the resting poses directly.
        /// </summary>
        internal static void BuildFlagLayer(AnimatorController controller, int layer, in AnimatorFlag flag,
            string offState, string onState, string turningOnState, string turningOffState)
        {
            AnimatorStateMachine machine = controller.layers[layer].stateMachine;

            controller.AddParameter(flag.value, AnimatorControllerParameterType.Bool);
            controller.AddParameter(flag.turnOn, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(flag.turnOff, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(flag.snapOn, AnimatorControllerParameterType.Trigger);
            controller.AddParameter(flag.snapOff, AnimatorControllerParameterType.Trigger);

            AnimatorState off = controller.AddMotion(OneShotClip(controller, offState), layer);
            AnimatorState on = controller.AddMotion(OneShotClip(controller, onState), layer);
            AnimatorState turningOn = controller.AddMotion(OneShotClip(controller, turningOnState), layer);
            AnimatorState turningOff = controller.AddMotion(OneShotClip(controller, turningOffState), layer);
            machine.defaultState = off;

            AnyStateTransition(machine, turningOn, flag.turnOn);
            AnyStateTransition(machine, turningOff, flag.turnOff);
            AnyStateTransition(machine, on, flag.snapOn);
            AnyStateTransition(machine, off, flag.snapOff);

            ExitTimeTransition(turningOn, on);
            ExitTimeTransition(turningOff, off);
        }

        /// <summary>
        /// A clip that plays through once and holds its last frame, added as a sub-asset of the controller.
        /// </summary>
        internal static AnimationClip OneShotClip(AnimatorController controller, string name)
        {
            AnimationClip clip = AnimatorController.AllocateAnimatorClip(name);
            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            clip.wrapMode = WrapMode.Once;
            AssetDatabase.AddObjectToAsset(clip, controller);
            return clip;
        }

        /// <summary>
        /// A state on <paramref name="layer"/> playing a one-shot clip of the same name, with a trigger parameter of
        /// that name declared on the controller.
        /// </summary>
        internal static AnimatorState AddTriggerState(AnimatorController controller, string triggerName, int layer)
        {
            AnimatorState state = controller.AddMotion(OneShotClip(controller, triggerName), layer);
            controller.AddParameter(triggerName, AnimatorControllerParameterType.Trigger);
            return state;
        }

        /// <summary>
        /// An immediate transition from anywhere into <paramref name="to"/> whenever <paramref name="trigger"/> fires.
        /// </summary>
        internal static AnimatorStateTransition AnyStateTransition(AnimatorStateMachine machine, AnimatorState to, string trigger)
        {
            AnimatorStateTransition transition = machine.AddAnyStateTransition(to);
            transition.hasExitTime = false;
            transition.duration = 0;
            transition.AddCondition(AnimatorConditionMode.If, 0, trigger);
            return transition;
        }

        /// <summary>
        /// An immediate transition between two states whenever <paramref name="trigger"/> fires.
        /// </summary>
        internal static AnimatorStateTransition TriggerTransition(AnimatorState from, AnimatorState to, string trigger)
        {
            AnimatorStateTransition transition = from.AddTransition(to, false);
            transition.hasExitTime = false;
            transition.duration = 0;
            transition.AddCondition(AnimatorConditionMode.If, 0, trigger);
            return transition;
        }

        /// <summary>
        /// A transition taken once <paramref name="from"/> has played all the way through, so a one-shot state can
        /// fall through to the resting state that follows it.
        /// </summary>
        internal static AnimatorStateTransition ExitTimeTransition(AnimatorState from, AnimatorState to)
        {
            AnimatorStateTransition transition = from.AddTransition(to, false);
            transition.hasExitTime = true;
            transition.exitTime = 1;
            transition.duration = 0;
            return transition;
        }

        private static void BuildClickLayer(AnimatorController controller)
        {
            AnimatorStateMachine machine = controller.layers[1].stateMachine;

            controller.AddParameter(SelectableAnimatorUi.triggerClick, AnimatorControllerParameterType.Trigger);
            AnimatorState click = controller.AddMotion(OneShotClip(controller, SelectableAnimatorUi.triggerClick), 1);

            machine.defaultState = machine.AddState("Wait State");
            AnyStateTransition(machine, click, SelectableAnimatorUi.triggerClick);
        }

        private static void BuildInteractionLayer(AnimatorController controller)
        {
            AnimatorStateMachine machine = controller.layers[0].stateMachine;

            AnimatorState normal = AddTriggerState(controller, SelectableAnimatorUi.triggerNormal, 0);
            AnimatorState down = AddTriggerState(controller, SelectableAnimatorUi.triggerDown, 0);
            AnimatorState up = AddTriggerState(controller, SelectableAnimatorUi.triggerUp, 0);
            AnimatorState disabled = AddTriggerState(controller, SelectableAnimatorUi.triggerDisabled, 0);

            AnyStateTransition(machine, normal, SelectableAnimatorUi.triggerNormal);
            AnyStateTransition(machine, disabled, SelectableAnimatorUi.triggerDisabled);

            TriggerTransition(normal, down, SelectableAnimatorUi.triggerDown);
            TriggerTransition(down, up, SelectableAnimatorUi.triggerUp).name = "Down -> Up by Up";
            TriggerTransition(down, up, SelectableAnimatorUi.triggerClick).name = "Down -> Up by Click";
            TriggerTransition(up, down, SelectableAnimatorUi.triggerDown);
            ExitTimeTransition(up, normal);
        }
    }
}
#endif
