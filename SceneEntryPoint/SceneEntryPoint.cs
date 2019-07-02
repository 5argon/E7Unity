using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Playables;

namespace E7.E7Unity
{
    public interface ISceneEntryPoint
    {
        void EntryPoint();
    }

    /// <summary>
    /// Instead of starting the scene immediately with `Start`, use `EntryPoint()` instead.
    /// By use this a a single starting hub per scene, we could add some functionality commonly to all scenes at once.
    /// 
    /// - Lag combat routine (editor only)
    /// 
    /// In editor, this will delay for a bit of frames to separate out the scene loading lag from the actual entry point.
    /// Plus you could still use `Awake` for setup before this happen.
    /// 
    /// - Current screen Firebase Analytics integration (TODO)
    /// 
    /// You could set the current screen so that all reported logs will have a screen information. Unity game has no concept of usual
    /// app's screen. With this, you could automatically set the screen.
    /// </summary>
    public class SceneEntryPoint : MonoBehaviour
    {
        [Tooltip("While in editor lag combat routine, it could make a canvas group not blocking raycast so you can't accidentally cause impossible action that is impossible in real build.")]
        public CanvasGroup lagCombatUninteractable;
        public UnityEvent entryPoint;

        [Tooltip("This director is special because it will be Play() AND Evaluate() on the first frame, so you don't see a flash of unintended state on the first frame.")]

        public PlayableDirector entryDirector;

        /// <summary>
        /// In editor only lags once.
        /// </summary>
        private static bool used;

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
            entryPoint.Invoke();
            if(entryDirector != null)
            {
                entryDirector.Play();
                entryDirector.Evaluate();
            }
            GetComponent<ISceneEntryPoint>().EntryPoint();
        }

    }
}