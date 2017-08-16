#define COROUTINEHOST_TASKS //this option grants you ability to wait/await on the Task that indicates completion of your coroutine

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if COROUTINEHOST_TASKS
using System.Threading.Tasks;
#endif

public class CoroutineHost : MonoBehaviour
{
    private static CoroutineHost crossThreadHost;
    private static CoroutineHost CrossThreadHost{
        get{
            if(crossThreadHost == null)
            {
                crossThreadHost = InstantiateCoroutineHost(); //error when on non-main thread. Call PrepareCrossthread beforehand.
            }
            return crossThreadHost;
        }
    }

    private static CoroutineHost InstantiateCoroutineHost()
    {
        GameObject coroutineHost = new GameObject("Coroutine Host");
        CoroutineHost coroutineHostComponent = coroutineHost.AddComponent<CoroutineHost>();
        GameObject.DontDestroyOnLoad(crossThreadHost); 
        return coroutineHostComponent;
    }

    /// <summary>
    /// Because we can't instantiate game objects from other thread, call this to pre-instantiate the host in the main thread first.
    /// </summary>
    public static void PrepareCrossThread()
    {
        crossThreadHost = InstantiateCoroutineHost();
    }

    /// <summary>
    /// The point of this is for starting an action on the moment we are changing scene. On the next scene, the one frame wait will Awake everything
    /// and grants you access for things in that scene you might need.
    /// </summary>
    /// <param name="action">Using lambda recommended</param>

#if COROUTINEHOST_TASKS
    public static Task StartDelayedActionOneFrame(Action action)
#else
    public static void StartDelayedActionOneFrame(Action action)
#endif 
    {
        IEnumerator ienum = DelayedOneFrameRoutine(action);
#if COROUTINEHOST_TASKS
        return Host(ienum);
#else
        Host(ienum);
#endif
    }

    /// <summary>
    /// If you have some lambdas you want to be run on the main thread then use this one.
    /// </summary>
#if COROUTINEHOST_TASKS
    public static Task Host(Action action)
#else
    public static void Host(Action action)
#endif
    {
#if COROUTINEHOST_TASKS
        return Host(ConvertActionToCoroutine(action));
#else
        return Host(ConvertActionToCoroutine(action));
#endif
    }

    private static IEnumerator ConvertActionToCoroutine(Action action)
    {
        action.Invoke();
        yield break;
    }

    /// <summary>
    /// Think of this as just StartCoroutine() that is able to use from outside of MonoBehaviour or static methods.
    /// </summary>
#if COROUTINEHOST_TASKS
    public static Task Host(IEnumerator coroutine)
#else
    public static void Host(IEnumerator coroutine)
#endif
    {
#if COROUTINEHOST_TASKS
        return CrossThreadHost.QueueCoroutine(coroutine);
#else
        CrossThreadHost.QueueCoroutine(coroutine);
#endif
    }

    private static IEnumerator DelayedOneFrameRoutine(Action delayedAction)
    {
        yield return null;
        delayedAction.Invoke();
    }

    // ---- Non-static  ----

    private Queue<IEnumerator> coroutineQueue;
#if COROUTINEHOST_TASKS
    private Dictionary<IEnumerator, TaskCompletionSource<object>> taskCompletionSources;
    private List<IEnumerator> rememberRemoval;
#endif
    public void Awake()
    {
        coroutineQueue = new Queue<IEnumerator>();
#if COROUTINEHOST_TASKS
        taskCompletionSources = new Dictionary<IEnumerator, TaskCompletionSource<object>>();
        rememberRemoval = new List<IEnumerator>();
#endif
    }

#if COROUTINEHOST_TASKS
    private Task QueueCoroutine(IEnumerator coroutine)
#else
    private void QueueCoroutine(IEnumerator coroutine)
#endif
    {
        lock (coroutineQueue)
        {
            coroutineQueue.Enqueue(coroutine);
#if COROUTINEHOST_TASKS
            TaskCompletionSource<object> tcs = new TaskCompletionSource<object>();
            taskCompletionSources.Add(coroutine,tcs);
            return tcs.Task;
#endif
        }
    }

    private void Update()
    {

        lock (coroutineQueue)
        {
            while (coroutineQueue.Count > 0)
            {
                StartCoroutine(coroutineQueue.Dequeue());
            }
        }
#if COROUTINEHOST_TASKS
        foreach (KeyValuePair<IEnumerator, TaskCompletionSource<object>> kvp in taskCompletionSources)
        {
            if (kvp.Key != null && kvp.Key.MoveNext() == false)
            {
                kvp.Value.SetResult(null);
                rememberRemoval.Add(kvp.Key);
            }
        }
        if (rememberRemoval.Count > 0)
        {
            foreach (IEnumerator ie in rememberRemoval)
            {
                taskCompletionSources.Remove(ie);
                Debug.Log("Removing...");
            }
            rememberRemoval.Clear();
        }
#endif
    }

}
