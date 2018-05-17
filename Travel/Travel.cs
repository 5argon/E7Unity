//#define TRAVEL_DEBUG

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Jobs;
using Unity.Collections;

public class Travel 
{

    public struct DataIndexOfTimeJob : IJob
    {
        [ReadOnly] public NativeArray<TravelEvent> travelEvents;
        [ReadOnly] public float time;
        [WriteOnly] public NativeArray<int> output;

        public void Execute()
        {
            for (int i = 0; i < travelEvents.Length; i++)
            {
                if (travelEvents[i].IsTimeInRange(time))
                {
                    output[0] = i;
                    return;
                }
            }
            output[0] = -1;
        }
    }


    public struct DataIndexOfPositionJob : IJob
    {
        [ReadOnly] public NativeList<TravelEvent> EventList;
        [ReadOnly] public float position;
        public NativeArray<int> rememberAndOutput;

        public void Execute()
        {
            if (EventList.Length == 0)
            {
                rememberAndOutput[1] = -1;
                return;
            }
            //Debug.Log($"Starting job {EventList.Length}");
            int plusIndex = -1;
            int minusIndex = 0;
            int useIndex;
            int rememberIndex = rememberAndOutput[0];

            for (int i = 0; i < EventList.Length; i++)
            {
                if ((math.notEqual(math.mod(i, 2), 0)))
                {
                    if (math.greaterThanEqual(rememberIndex - (minusIndex + 1), 0))
                    {
                        minusIndex++;
                        useIndex = rememberIndex - minusIndex;
                    }
                    else
                    {
                        plusIndex++;
                        useIndex = rememberIndex + plusIndex;
                    }
                }
                else
                {
                    if (math.lessThan(rememberIndex + (plusIndex + 1), EventList.Length))
                    {
                        plusIndex++;
                        useIndex = rememberIndex + plusIndex;
                    }
                    else
                    {
                        minusIndex++;
                        useIndex = rememberIndex - minusIndex;
                    }
                }

                if (EventList[useIndex].IsPositionInRange(position))
                {
                    rememberAndOutput[0] = useIndex;
                    rememberAndOutput[1] = useIndex;
                    return;
                }
            }
            //Debug.Log("Ending with minus one");
            rememberAndOutput[1] = -1;
            return;
        }
    }

}

public class Travel<T> : System.IDisposable
{
    public void Dispose()
    {
        travelEvents.Dispose();
        rememberAndOutput.Dispose();
    }

    private bool HasEventAtZero { get; set; }

    private List<T> datas { get; }
    private NativeList<TravelEvent> travelEvents { get; }
    private int travelRememberIndex;

    //This two are for jobs. When you Add it needs to be realloc so if possible add everything before start using.
    NativeArray<int> rememberAndOutput;

    public Travel()
    {
        datas = new List<T>();
        travelEvents = new NativeList<TravelEvent>(Allocator.Persistent);
        rememberAndOutput = new NativeArray<int>(2, Allocator.Persistent);
    }

    public void Clear()
    {
        HasEventAtZero = false;
        travelRememberIndex = 0;
        datas.Clear();
        travelEvents.Clear();
    }


    public T FirstData => datas.Count > 0 ? datas[0] : default(T);
    public T LastData => datas.Count > 0 ? datas[datas.Count - 1] : default(T);

    public TravelEvent FirstEvent
    {
        get
        {
            return travelEvents.Length > 0 ? travelEvents[0] : TravelEvent.INVALID;
        }
    }

    public TravelEvent LastEvent
    {
        get
        {
            return travelEvents.Length > 0 ? travelEvents[travelEvents.Length - 1] : TravelEvent.INVALID;
        }
    }

    private void SetLastEvent(TravelEvent te)
    {
        travelEvents.RemoveAtSwapBack(travelEvents.Length - 1);
        travelEvents.Add(te);
    }

    public bool NoEvent => travelEvents.Length == 0;

    public (T data, TravelEvent travelEvent) DataEventOfPosition(float position)
    {
        //Debug.Log("Finding of " + position);
        int dataIndex = DataIndexOfPosition(position);
        return DataEventOfPositionFromIndex(dataIndex);
    }

    public (T data, TravelEvent travelEvent) DataEventOfPositionFromIndex(int dataIndex) => dataIndex == -1 ? (default(T), TravelEvent.INVALID) : (datas[dataIndex], travelEvents[dataIndex]);

    public (T data, TravelEvent travelEvent) DataEventOfTime(float time)
    {
        int index = DataIndexOfTime(time);
        return index != -1 ? (datas[index], travelEvents[index]) : (default(T), TravelEvent.INVALID);
    }

    public (Travel.DataIndexOfPositionJob job, JobHandle handle) DataIndexOfPositionRunJob(float position)
    {
        rememberAndOutput[0] = travelRememberIndex;
        var job = new Travel.DataIndexOfPositionJob
        {
            EventList = travelEvents,
            position = position,
            rememberAndOutput = rememberAndOutput
        };
        JobHandle handle = job.Schedule();
        return (job, handle);
    }

    private int DataIndexOfPosition(float position)
    {
        if (travelEvents.Length == 0)
        {
            return -1;
        }
        (var job, var handle) = DataIndexOfPositionRunJob(position);
        handle.Complete();
        travelRememberIndex = job.rememberAndOutput[0];
        int output = job.rememberAndOutput[1];
        // for(int i = 0; i < travelEvents.Length; i++)
        // {
        //     Debug.Log($"{travelEvents[i].Position} {travelEvents[i].Time}");
        // }
        // Debug.Log("output is " + output);
        return output;
    }
    public (Travel.DataIndexOfTimeJob job, JobHandle handle) DataIndexOfTimeRunJob(float time)
    {
        rememberAndOutput[0] = -1;
        var job = new Travel.DataIndexOfTimeJob
        {
            travelEvents = travelEvents,
            time = time,
            output = rememberAndOutput
        };
        var jobHandle = job.Schedule();
        return (job, jobHandle);
    }

    private int DataIndexOfTime(float time)
    {
        if (travelEvents.Length == 0)
        {
            return -1;
        }
        (var job, var jobHandle) = DataIndexOfTimeRunJob(time);
        jobHandle.Complete();
        return rememberAndOutput[0];
    }

    /// <summary>
    /// Adds event at position/time zero if there's nothing at zero yet.
    /// This prevents the travel returning null for all positive time and position
    /// If you have something at zero already this does nothing.
    /// BUT if you don't have anything at zero but have other data... it will crash
    /// Travel does not support adding a data in backward direction.
    /// </summary>
    public void AddDefaultAtZero(T data)
    {
        if (!HasEventAtZero)
        {
#if TRAVEL_DEBUG
            Debug.Log("Adding default " + data.ToString());
#endif
            Add(0, 0, data);
        }
    }

    public T NextDataOf(TravelEvent travelEvent)
    {
        int nextIndex = travelEvent.DataIndex + 1;
        if (nextIndex < datas.Count)
        {
            return datas[nextIndex];
        }
        else
        {
            return default(T);
        }
    }

    public TravelEvent NextOf(TravelEvent te)
    {
        int nextIndex = te.DataIndex + 1;
        if (nextIndex < datas.Count)
        {
            return travelEvents[nextIndex];
        }
        else
        {
            return TravelEvent.INVALID;
        }
    }

    public TravelEvent PreviousOf(TravelEvent te)
    {
        int prevIndex = te.DataIndex - 1;
        if (prevIndex >= 0 && datas.Count > 0)
        {
            return travelEvents[prevIndex];
        }
        else
        {
            return TravelEvent.INVALID;
        }
    }

    /// <summary>
    /// TIME IS TIME ELAPSED NOT ANY TIME YOU WANT!!
    /// </summary>
    public void Add(float position, float timeElapsed, T data)
    {
#if TRAVEL_DEBUG
        Debug.Log($"Adding {position} {timeElapsed} {data.ToString()}");
#endif
        if (travelEvents.Length != 0 && timeElapsed <= 0)
        {
            throw new System.Exception($"Time elapsed except the first one must be positive. position : {position} timeElapsed : {timeElapsed}");
        }
        TravelEvent lastEvent = LastEvent;
        TravelEvent newTe = new TravelEvent(position, (NoEvent ? 0 : lastEvent.Time) + timeElapsed, travelEvents.Length);
        if (!NoEvent)
        {
            lastEvent.LinkToNext(newTe, this);
            //Now we have to save back the last event too?
            SetLastEvent(lastEvent);
        }

        travelEvents.Add(newTe);
        datas.Add(data);

        if (position == 0)
        {
            HasEventAtZero = true;
        }
    }
}

/// <summary>
/// To keep struct purity there's no data in here! Ask data with DataIndex from Travel!
/// </summary>
public struct TravelEvent 
{
    private static readonly TravelEvent constINVALID = new TravelEvent(Mathf.NegativeInfinity, float.NegativeInfinity, -1);
    public static ref readonly TravelEvent INVALID => ref constINVALID;

    public bool Invalid => Position == constINVALID.Position && Time == constINVALID.Time;
    public bool Valid => !Invalid;

    public float Position { get; }
    /// <summary>
    /// When this is the last event, this is Mathf.Infinity
    /// </summary>
    public float PositionNext { get; private set; }

    public float Time { get; }
    /// <summary>
    /// When this is the last event, this is Mathf.Infinity
    /// </summary>
    public float TimeNext { get; private set; }

    public int DataIndex { get;}

    /// <summary>
    /// When this is the last event, this is null.
    /// </summary>

    // public TravelEvent<T> Next
    // {
    //     get => throw new System.NotImplementedException();
    //     set => throw new System.NotImplementedException();
    // }
    // public TravelEvent<T> Previous
    // {
    //     get => throw new System.NotImplementedException();
    //     set => throw new System.NotImplementedException();
    // }

    public TravelEvent(float absolutePosition, float time, int dataIndex)
    {
        this.Position = absolutePosition;
        this.PositionNext = Mathf.Infinity;
        this.Time = time;
        this.TimeNext = Mathf.Infinity;
        this.DataIndex = dataIndex;
    }

    public void LinkToNext<T>(TravelEvent te, Travel<T> owningTravel)
    {
#if TRAVEL_DEBUG
        Debug.Log($"Linking to next : {te.Time} {te.Position}");
#endif
        this.TimeNext = te.Time;
        this.PositionNext = te.Position;
        //this.Next = te;
        //te.Previous = this;
        //Debug.Log("Link result : " + this.ToString());
    }

    public bool IsPositionInRange(float position)
    {
#if TRAVEL_DEBUG
        Debug.Log($"Is in range? {Position} - {position} - {PositionNext}");
#endif
        //return (position >= Position) && (position < PositionNext);
        return math.greaterThanEqual(position, Position) && math.lessThan(position, PositionNext);
    }

    public bool IsTimeInRange(float time)
    {
#if TRAVEL_DEBUG
        Debug.LogFormat($"Is time in range? {Time} - {time} - {TimeNext}");
#endif
        //return (time >= Time) && (time < TimeNext);
        return math.greaterThanEqual(time, Time) && math.lessThan(time, TimeNext);
    }

    public override string ToString()
    {
        return string.Format($"P {Position}-{PositionNext} T {Time}-{TimeNext}"); 
    }
}