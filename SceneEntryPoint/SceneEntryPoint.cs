using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace E7.E7Unity
{
    public interface ISceneEntryPoint
    {
        void EntryPoint();
    }

    /// <summary>
    /// Instead of starting the scene immediately with Awake, use `EntryPoint()` instead.
    /// In editor, this will delay for a bit of frames to separate out the scene loading lag from the actual entry point.
    /// </summary>
    public class SceneEntryPoint : MonoBehaviour
    {
        /// <summary>
        /// In editor only lags once.
        /// </summary>
        private static bool used;

        public void Awake()
        {
#if UNITY_EDITOR
            if (!used)
            {
                used = true;
                StartCoroutine(LagCombatRoutine());
                return;
            }
#endif
            GetComponent<ISceneEntryPoint>().EntryPoint();
        }

        IEnumerator LagCombatRoutine()
        {
            //yield return new WaitForSeconds(0.5f);
            for (int i = 0; i < 8; i++)
            {
                yield return null;
            }
            GetComponent<ISceneEntryPoint>().EntryPoint();
        }

    }
}