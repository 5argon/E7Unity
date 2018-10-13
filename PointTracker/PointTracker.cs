//Uncomment this to debug native shenanigans that might happen
//#define DEBUG_POINT_TRACKER

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Collections;
using Unity.Jobs;

/// <summary>
/// From a sequence of events that might lacks an ID but having previous point, 
/// or having just previous point but lacking an ID, we can infer more states of the touch.
/// 
/// Plus it can keep an arbitrary boolean state for each holding down touch.
/// </summary>
public struct PointTracker : System.IDisposable
{
    private NativeList<float2> registeredPoints;
    private NativeHashMap<float2, Bool> registeredStates;
    private NativeHashMap<float2, int> registeredTouchId;
    private NativeArray<int> touchIdRunnerMemory;
    private int touchIdRunner
    {
        get => touchIdRunnerMemory[0];
        set => touchIdRunnerMemory[0] = value;
    }

    /// <summary>
    /// Make sure even if player use all of fingers and toes he still could not crash the game...
    /// </summary>
    public const int maximumTouch = 21;

    public void Dispose()
    {
        registeredPoints.Dispose();
        registeredStates.Dispose();
        registeredTouchId.Dispose();
        touchIdRunnerMemory.Dispose();
    }

    public PointTracker(Allocator allocator)
    {
        registeredPoints = new NativeList<float2>(allocator);
        registeredStates = new NativeHashMap<float2, Bool>(maximumTouch, allocator);
        registeredTouchId = new NativeHashMap<float2, int>(maximumTouch, allocator);
        touchIdRunnerMemory = new NativeArray<int>(1, allocator);
        touchIdRunner = 0;
    }

    /// <summary>
    /// All points in this list are currently "down". Not array for performance reason so don't modify the list! Just read it!
    /// </summary>
	public IEnumerable<float2> CurrentPoints
    {
        get
        {
            for (int i = 0; i < registeredPoints.Length; i++)
            {
                yield return registeredPoints[i];
            }
        }
    }

    /// <summary>
    /// If you are in a job get this and iterate on it instead of `CurrentPoints`.
    /// </summary>
	public NativeList<float2> CurrentPointsBurst => registeredPoints;

    /// <summary>
    /// You can keep whatever state you want with a bool per point.
    /// </summary>
    public Bool StateOfPoint(float2 point)
    {
        point = RoundVector(point);
        Bool ret;
        if (registeredStates.TryGetValue(point, out ret))
        {
            //DebugLog($"State of {point.x} {point.y} is {ret}", LogType.Log);
            return ret;
        }
        else
        {
            //DebugLog($"State of {point.x} {point.y} not found", LogType.Log);
            return false;
        }
    }

    /// <summary>
    /// Each touch gets a unique generated ID that is carried over from point to point.
    /// So you know a new touch in other frame is perhaps the same ones from earlier frame.
    /// </summary>
    public int IdOfPoint(float2 point)
    {
        point = RoundVector(point);
        int ret;
        if (registeredTouchId.TryGetValue(point, out ret))
        {
            //DebugLog($"Id of {point.x} {point.y} is {ret}", LogType.Log);
            return ret;
        }
        else
        {
            //DebugLog($"Id of {point.x} {point.y} not found", LogType.Log);
            return -1;
        }
    }

    public void Reset()
    {
        registeredPoints.Clear();
        registeredStates.Clear();
        registeredTouchId.Clear();
    }

    public void Down(float2 pointDown)
    {
        pointDown = RoundVector(pointDown);
#if DEBUG_POINT_TRACKER
        DebugLog($"Down {pointDown.x} {pointDown.y} ID : {touchIdRunner}", LogType.Log);
#endif
        registeredPoints.Add(pointDown);
        registeredStates.TryAdd(pointDown, false);
        registeredTouchId.TryAdd(pointDown, touchIdRunner);
        touchIdRunner = touchIdRunner + 1;
    }

    public void SetState(float2 pointNow, bool toState)
    {
        pointNow = RoundVector(pointNow);
        if (registeredPoints.Contains(pointNow) && registeredStates.TryGetValue(pointNow, out _))
        {
            //Debug.Log($"Set state OK {point.x} {point.y} {toState}");
            registeredStates.Remove(pointNow);
            registeredStates.TryAdd(pointNow, toState);
        }
#if DEBUG_POINT_TRACKER
        else
        {
#if DEBUG_POINT_TRACKER
            DebugLog($"Set state fail {pointNow.x} {pointNow.y} {toState}", LogType.Log);
#endif
        }
#endif
    }

    /// <summary>
    /// This is just to fight with Unity's floating point weirdness
    /// </summary>
    private float2 RoundVector(float2 vector) => new float2(math.round(vector.x), math.round(vector.y));

    public bool Move(float2 pointNow, float2 pointPrevious)
    {
        pointNow = RoundVector(pointNow);
        pointPrevious = RoundVector(pointPrevious);

#if DEBUG_POINT_TRACKER
        DebugLog($"Move {pointNow.x} {pointNow.y} {pointPrevious.x} {pointPrevious.y}", LogType.Log);
#endif

#if UNITY_IOS
        if (pointNow == pointPrevious)
        {
            //This weird bug iOS reports happen after an errornous Up.. we interpret this as Down.

#if DEBUG_POINT_TRACKER
            DebugLog($"Error Move!! {pointNow.x} {pointNow.y} {pointPrevious.x} {pointPrevious.y}", LogType.Error);
#endif
            Down(pointNow);
            return true;
        }
#endif

        Bool state;
        int touchId;

        Bool containsPrevious = registeredPoints.Contains(pointPrevious);

        if (containsPrevious)
        {
            float2 victim = pointPrevious;
            if (registeredStates.TryGetValue(victim, out state) && registeredTouchId.TryGetValue(victim, out touchId))
            {
                registeredPoints.RemoveAtSwapBack(registeredPoints.IndexOf(victim));
                registeredStates.Remove(victim);
                registeredTouchId.Remove(victim);
                registeredPoints.Add(pointNow);
                if (!registeredStates.TryGetValue(pointNow, out _) && !registeredTouchId.TryGetValue(pointNow, out _)) //somehow ArgumentException crash happen below!!
                {
                    registeredStates.TryAdd(pointNow, state); //copy state
                    registeredTouchId.TryAdd(pointNow, touchId); //copy touch ID too
                }
                return true;
            }
        }


#if DEBUG_POINT_TRACKER
        DebugLog($"No such previous point! (move) {pointPrevious.x} x {pointPrevious.y}", LogType.Error);
#endif
        return false;
    }

    public bool Up(float2 pointUp, float2 pointPrevious)
    {
        pointUp = RoundVector(pointUp);
        pointPrevious = RoundVector(pointPrevious);

#if DEBUG_POINT_TRACKER
        DebugLog($"Up {pointUp.x} {pointUp.y} {pointPrevious.x} {pointPrevious.y}", LogType.Log);
#endif

        //It has the same problem as Down
        bool containsPrevious = registeredPoints.Contains(pointPrevious);
        bool containsUp = false;

        if (!containsPrevious)
        {
            containsUp = registeredPoints.Contains(pointUp);
        }

        if (containsPrevious || containsUp )
        {
            float2 victim = containsUp ? pointUp : pointPrevious;
            if (registeredStates.TryGetValue(victim, out _) && registeredTouchId.TryGetValue(victim, out _))
            {
                registeredPoints.RemoveAtSwapBack(registeredPoints.IndexOf(victim));
                registeredStates.Remove(victim);
                registeredTouchId.Remove(victim);
                return true;
            }
        }

#if DEBUG_POINT_TRACKER
        DebugLog($"No such previous point! (up) {pointPrevious.x} x {pointPrevious.y}", LogType.Error);
#endif
        return false;
    }

    private void DebugLog(string message, LogType logType)
    {
#if DEBUG_POINT_TRACKER
        switch (logType)
        {
            case LogType.Log: { Debug.Log(message); return; }
            case LogType.Warning: { Debug.LogWarning(message); return; }
            case LogType.Error: { Debug.LogError(message); return; }
            default: { Debug.Log(message); return; }
        }
#endif
    }

}
