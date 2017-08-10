using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// This class can instantiate MonoBehaviour to host a coroutine, automatic destroying on finish, resistant to scene load.
/// </summary>
public class CoroutineHost : MonoBehaviour
{
    private static CoroutineHost GetHostObject()
    {
        GameObject coroutineHost = new GameObject("CoroutineHost");
        CoroutineHost coroutineHostComponent = coroutineHost.AddComponent<CoroutineHost>();
        return coroutineHostComponent;
    }

	/// <summary>
	/// This method survives scene changes and moreover wait for the target scene to complete loading.
	/// </summary>
	/// <param name="action">Using lambda recommended</param>
    public static void StartDelayedActionOneFrame(Action action)
    {
        CoroutineHost coroutineHostComponent = GetHostObject();
        coroutineHostComponent.DelayedActionOneFrame(action);
        GameObject.DontDestroyOnLoad(coroutineHostComponent.gameObject);
    }

    public static void Host(IEnumerator coroutine)
    {
        CoroutineHost coroutineHostComponent = GetHostObject();
        coroutineHostComponent.StartWithIEnumerator(coroutine);
    }

    private IEnumerator coroutine;
    public void StartWithIEnumerator(IEnumerator coroutine)
    {
        this.coroutine = coroutine;
        StartCoroutine(coroutine);
    }

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
        if (coroutine != null && coroutine.MoveNext() == false)
        {
            //Debug.Log("Finished!");
            Destroy(gameObject);
        }
    }


}
