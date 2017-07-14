using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class can instantiate MonoBehaviour to host a coroutine, automatic destroying on finish, resistant to scene load.
/// </summary>
public class CoroutineHost : MonoBehaviour
{
	/// <summary>
	/// This method survives scene changes and moreover wait for the target scene to complete loading.
	/// </summary>
	/// <param name="action">Using lambda recommended</param>
    public static void StartDelayedActionOneFrame(Action action)
    {
        GameObject coroutineHost = new GameObject("CoroutineHost");
        CoroutineHost coroutineHostComponent = coroutineHost.AddComponent<CoroutineHost>();
        coroutineHostComponent.DelayedActionOneFrame(action);
        GameObject.DontDestroyOnLoad(coroutineHost);
    }

    private IEnumerator coroutine;
    public void DelayedActionOneFrame(Action action)
    {
        coroutine = DelayedOneFrameRoutine(action);
        StartCoroutine(coroutine);
    }

    private static IEnumerator DelayedOneFrameRoutine(Action delayedAction)
    {
        yield return null;
        delayedAction.Invoke();
    }

    private void Update()
    {
        if (coroutine != null || coroutine.MoveNext() == false)
        {
            //Debug.Log("Finished!");
            Destroy(gameObject);
        }
    }


}
