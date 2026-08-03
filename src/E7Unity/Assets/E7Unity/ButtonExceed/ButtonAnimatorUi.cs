using UnityEngine;
using UnityEngine.Events;

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
    /// See <see cref="SelectableAnimatorUi"/> for the three moments and the five animation triggers they drive.
    /// <see cref="onClick"/> is the one that carries the button's actual action.
    /// </para>
    /// <para>
    /// To get started, right-click the component header and choose <b>Create Animator</b>. It builds a controller
    /// wired with all five triggers, a click layer, a focus layer and an idle layer, as a reasonable starting
    /// point.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(Animator))]
    public class ButtonAnimatorUi : SelectableAnimatorUi
    {
        /// <summary>
        /// Invoked on a genuine click: the finger or cursor lifted while still inside the button. This is
        /// where the button's actual action belongs.
        /// </summary>
        [Tooltip("On Click only invoke if you up inside the button's rectangle.")]
        public UnityEvent onClick;

        private const string idleLayer = "Idle Layer";

        protected override void OnClicked()
        {
            onClick.Invoke();
        }

    #if UNITY_EDITOR
        [ContextMenu("Create Animator")]
        void CreateAnimator()
        {
            string path = SelectableAnimatorUiBuilder.AskSavePath(gameObject);
            if (string.IsNullOrEmpty(path))
                return;

            AnimatorController controller = SelectableAnimatorUiBuilder.Create(path, idleLayer);
            animator.runtimeAnimatorController = controller;

            AnimationClip idleClip = AnimatorController.AllocateAnimatorClip("Idle");
            AssetDatabase.AddObjectToAsset(idleClip, controller);
            idleClip.wrapMode = WrapMode.Loop;

            int layer = SelectableAnimatorUiBuilder.firstExtraLayer;
            AnimatorStateMachine machine = controller.layers[layer].stateMachine;
            machine.defaultState = controller.AddMotion(idleClip, layer);

            AssetDatabase.ImportAsset(path);
        }
    #endif
    }
}
