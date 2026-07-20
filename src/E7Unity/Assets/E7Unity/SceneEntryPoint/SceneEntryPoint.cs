using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace E7.E7Unity
{
    /// <summary>
    /// Implemented by a component sitting on the same game object as a <see cref="SceneEntryPoint"/> to be started
    /// by it, instead of starting itself from <c>Start</c>.
    /// </summary>
    public interface ISceneEntryPoint
    {
        /// <summary>
        /// Called once by <see cref="SceneEntryPoint"/> when the scene is settled and ready to begin.
        /// </summary>
        void EntryPoint();
    }

    /// <summary>
    /// A single starting hub for a scene. Rather than letting every component begin in its own <c>Start</c>, one
    /// entry point decides when the scene truly begins, which gives every scene a common place to hang start-up
    /// behaviour on.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Three things start when the scene begins, in order: the <see cref="ISceneEntryPoint"/> on this game object if
    /// there is one, then the <see cref="entryPoint"/> event, then <see cref="entryDirector"/>.
    /// </para>
    /// <para>
    /// In the editor the first scene entered additionally waits a few frames before beginning, so the loading hitch
    /// of entering play mode is not mistaken for a hitch in the scene's own opening animation. Builds never wait, and
    /// <c>Awake</c> still runs at its normal time in both cases — only the entry point is deferred.
    /// </para>
    /// </remarks>
    public class SceneEntryPoint : MonoBehaviour
    {
        /// <summary>
        /// Blocked from receiving raycasts during the editor-only settling delay, so an input that would be
        /// impossible in a real build cannot be made in those few frames. Optional.
        /// </summary>
        [Tooltip("While in editor lag combat routine, it could make a canvas group not blocking raycast so you can't accidentally cause impossible action that is impossible in real build.")]
        public CanvasGroup lagCombatUninteractable;

        /// <summary>
        /// Invoked when the scene begins, after the <see cref="ISceneEntryPoint"/> on this game object and before
        /// <see cref="entryDirector"/>.
        /// </summary>
        public UnityEvent entryPoint;

        /// <summary>
        /// Played when the scene begins, and also evaluated immediately on that same frame so its first frame is
        /// already applied rather than showing one frame of the un-posed scene. Optional.
        /// </summary>
        [Tooltip("This director is special because it will be Play() AND Evaluate() on the first frame, so you don't see a flash of unintended state on the first frame.")]
        public PlayableDirector entryDirector;

        /// <summary>
        /// The editor-only settling delay applies to the first scene of a play session only.
        /// </summary>
        private static bool used;

        /// <summary>
        /// Begins the scene, or defers it by a few frames for the editor-only settling delay described on this class.
        /// </summary>
        public void Start()
        {
#if UNITY_EDITOR
            if (!used)
            {
                used = true;
                StartCoroutine(LagCombatRoutine());
                return;
            }
#endif
            Entry();
        }

        IEnumerator LagCombatRoutine()
        {
            if (lagCombatUninteractable != null)
            {
                lagCombatUninteractable.blocksRaycasts = false;
            }
            for (int i = 0; i < 8; i++)
            {
                yield return null;
            }
            if (lagCombatUninteractable != null)
            {
                lagCombatUninteractable.blocksRaycasts = true;
            }
            Entry();
        }

        private void Entry()
        {
            var entryPointComponent = GetComponent<ISceneEntryPoint>();
            if (entryPointComponent != null)
            {
                entryPointComponent.EntryPoint();
            }

            entryPoint.Invoke();

            if (entryDirector != null)
            {
                entryDirector.Play();
                entryDirector.Evaluate();
            }
        }
    }
}
