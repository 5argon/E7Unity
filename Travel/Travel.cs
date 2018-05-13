using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Transforms2D;
using Unity.Jobs;
using Unity.Collections;

public struct TravelData<T> : IComponentData
{
    public NativeArray<TravelEvent<T>> EventList;
}

public struct TravelRememberIndex : IComponentData
{
    public int rememberIndex;
}

public class Travel<T>
{
    private Entity entityToEventList;

    /// <summary>
    /// Would be most people's EM that would like the data to be in.
    /// </summary>
    private static EntityManager EntityManager => World.Active.GetExistingManager<EntityManager>();

    private static EntityArchetype archetype;
    private static void InitializeManager()
    {
        archetype = EntityManager.CreateArchetype(typeof(TravelData<T>), typeof(TravelRememberIndex));
    }

    public Travel()
    {
        if(!archetype.Valid)
        {
            InitializeManager();
        }

        //When creating a "Travel" we could allocate things in the manager instead.
        //This Travel then could be an interface to the entity.
        entityToEventList = EntityManager.CreateEntity(archetype);
    }

    private TravelData<T> GetTravelData => EntityManager.GetComponentData<TravelData<T>>(entityToEventList);
    private TravelRememberIndex GetRememberIndex => EntityManager.GetComponentData<TravelRememberIndex>(entityToEventList);
    private NativeArray<TravelEvent<T>> GetEventList => GetTravelData.EventList;

    public TravelEvent<T> FirstEvent => GetEventList[0];
    public TravelEvent<T> LastEvent => GetEventList[GetEventList.Length - 1];
    public bool NoEvent => GetEventList.Length == 0;

    /// <summary>
    /// Get the most recent event before the position on the timeline.
    /// </summary>
    public TravelEvent<T> EventOfPosition(float position)
    {
        NativeArray<TravelEvent<T>> EventList = GetTravelData.EventList;
        TravelRememberIndex tri = GetRememberIndex;
        int rememberIndex = tri.rememberIndex;

        //return Find((te) => te.IsPositionInRange(position));
        int plusIndex = -1;
        int minusIndex = 0;
        int useIndex;
        for (int i = 0; i < EventList.Length; i++)
        {
            if ((i % 2 != 0))
            {
                if (rememberIndex - (minusIndex + 1) >= 0)
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
                if(rememberIndex + (plusIndex + 1) < EventList.Length)
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

            //Debug.Log(i + " " + rememberIndex + " " + minusIndex + " " + plusIndex + "  " + useIndex);
            if(EventList[useIndex].IsPositionInRange(position))
            {
                rememberIndex = useIndex;
                //Debug.Log($"Ok we got {EventList[useIndex]}");

                //Save rememberIndex back to entity..
                tri.rememberIndex = rememberIndex;
                EntityManager.SetComponentData<TravelRememberIndex>(entityToEventList, tri);

                return EventList[useIndex];
            }
        }
        return TravelEvent<T>.INVALID;
    }

    /// <summary>
    /// Get the most recent event before the time on the timeline.
    /// </summary>
    public TravelEvent<T> EventOfTime(float time)
    {
        for(int i = 0; i < GetEventList.Length; i++)
        {
            if(GetEventList[i].IsTimeInRange(time))
            {
                return GetEventList[i];
            }
        }
        return TravelEvent<T>.INVALID;
    }

    /// <summary>
    /// Left this as non-ECS?
    /// </summary>
    private bool HasEventAtZero { get; set; }

    /// <summary>
    /// Adds event at position/time zero if there's nothing at zero yet.
    /// This prevents the travel returning null for all positive time and position
    /// </summary>
    public void AddDefaultAtZero(T data)
    {
        if(!HasEventAtZero)
        {
            //Debug.Log("Adding default " + data.ToString());
            Add(0,0,data);
        }
    }

    /// <summary>
    /// TIME IS TIME ELAPSED NOT ANY TIME YOU WANT!!
    /// </summary>
    public void Add(float position, float timeElapsed, T data)
    {
        //Debug.Log($"Adding {position} {timeElapsed} {data.ToString()}");
        if (GetEventList.Length != 0 && timeElapsed <= 0)
        {
            throw new System.Exception($"Time elapsed except the first one must be positive. position : {position} timeElapsed : {timeElapsed}");
        }
        TravelEvent<T> travelEvent = new TravelEvent<T>(position, (NoEvent ? 0 : LastEvent.Time) + timeElapsed, data);
        if(!NoEvent)
        {
            LastEvent.LinkToNext(travelEvent);
        }

        //Super costly Add haha
        TravelData<T> td = GetTravelData;
        NativeArray<TravelEvent<T>> eventList = td.EventList;
        NativeArray<TravelEvent<T>> biggerEventList = new NativeArray<TravelEvent<T>>(eventList.Length + 1, Allocator.Persistent);
        for(int i = 0; i < eventList.Length; i++)
        {
            biggerEventList[i] = eventList[i];
        }
        //Finally add the new one...
        biggerEventList[biggerEventList.Length-1] = travelEvent;
        eventList.Dispose();
        td.EventList = biggerEventList;
        EntityManager.SetComponentData<TravelData<T>>(entityToEventList, td);

        if(position == 0)
        {
            HasEventAtZero = true;
        }
    }
}

/// <summary>
/// In addition to position and time, call .Data to get any kind of data you want.
/// </summary>
public struct TravelEvent<T> 
{
    private static readonly TravelEvent<T> constINVALID = new TravelEvent<T>(Mathf.NegativeInfinity, float.NegativeInfinity, default(T));
    public static ref readonly TravelEvent<T> INVALID => ref constINVALID;

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

    public T Data { get; }
    /// <summary>
    /// When this is the last event, this is null.
    /// </summary>

    public TravelEvent<T> Next
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }
    public TravelEvent<T> Previous
    {
        get => throw new System.NotImplementedException();
        set => throw new System.NotImplementedException();
    }

    public TravelEvent(float absolutePosition, float time, T data)
    {
        this.Position = absolutePosition;
        this.PositionNext = Mathf.Infinity;
        this.Time = time;
        this.TimeNext = Mathf.Infinity;
        this.Data = data;
    }

    public void LinkToNext(TravelEvent<T> te)
    {
        this.TimeNext = te.Time;
        this.PositionNext = te.Position;
        this.Next = te;
        te.Previous = this;
        //Debug.Log("Link result : " + this.ToString());
    }

    public bool IsPositionInRange(float position)
    {
        //Debug.Log($"Is in range? {Position} - {position} - {PositionNext}");
        return (position >= Position) && (position < PositionNext);
    }

    public bool IsTimeInRange(float time)
    {
        //Debug.LogFormat("Time Range is {0} - {1}",Time,TimeNext);
        return (time >= Time) && (time < TimeNext);
    }

    public override string ToString()
    {
        return string.Format($"P {Position}-{PositionNext} T {Time}-{TimeNext} {Data.ToString()}");
    }
}