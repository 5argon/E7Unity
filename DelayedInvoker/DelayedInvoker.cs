using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class DelayedInvoker {

    private Dictionary<int, IEnumerator> ActionIdToCoroutine = new Dictionary<int, IEnumerator>();
    private MonoBehaviour CoroutineHost { get; }

    /// <summary>
    /// Execution order in the first frame after specified delay time depends on Script Execution Order of this `coroutineHost`.
    /// </summary>
    public DelayedInvoker(MonoBehaviour coroutineHost)
    {
        this.CoroutineHost = coroutineHost;
    }

    private IEnumerator DelayedActionRoutine(int actionId, Action hitMethod, float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        hitMethod.Invoke();
        ActionIdToCoroutine.Remove(actionId);
    }

    /// <summary>
    /// Throws an exception if the same action Id is already scheduled. Use IsSchedulingAction to check.
    /// </summary>
    public void ScheduleAction(int actionId, Action action, float invokeAfterDelay)
    {
        if (ActionIdToCoroutine.ContainsKey(actionId) == false)
        {
            IEnumerator routine = DelayedActionRoutine(actionId, action, invokeAfterDelay);
            CoroutineHost.StartCoroutine(routine);
            ActionIdToCoroutine.Add(actionId, routine);
        }
        else
        {
            throw new ArgumentException($"Action ID {actionId} already scheduled!");
        }
    }

    public void CancelAction(int actionId)
    {
        IEnumerator routine;
        if(ActionIdToCoroutine.TryGetValue(actionId,out routine))
        {
            CoroutineHost.StopCoroutine(routine);
            ActionIdToCoroutine.Remove(actionId);
        }
    }

    public void CancelAllActions()
    {
        if (ActionIdToCoroutine.Count != 0)
        {
            foreach (IEnumerator ie in ActionIdToCoroutine.Values)
            {
                CoroutineHost.StopCoroutine(ie);
            }
            ActionIdToCoroutine.Clear();
        }
    }

    public bool IsSchedulingAction(int actionId) => ActionIdToCoroutine.ContainsKey(actionId);

}
