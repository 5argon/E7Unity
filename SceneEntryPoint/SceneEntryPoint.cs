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
    /// In editor, this will delay for a bit of frames to separate out the scene loading lag from the actual entry point.
    /// Plus you could still use `Awake` for setup before this happen.
    /// </summary>
    public class SceneEntryPoint : MonoBehaviour
    {
        public CanvasGroup lagCombatUninteractable;
        public UnityEvent entryPoint;

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
            GetComponent<ISceneEntryPoint>().EntryPoint();
        }

    }
}