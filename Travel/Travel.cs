using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Travel<T>
{
    private List<TravelEvent<T>> EventList { get; }

    public Travel()
    {
        EventList = new List<TravelEvent<T>>();
    }

    public TravelEvent<T> Find(System.Predicate<TravelEvent<T>> criteria)
    {
        return EventList.Find(criteria);
    }

    public TravelEvent<T> FirstEvent => EventList.Count == 0 ? null : EventList[0];
    public TravelEvent<T> LastEvent => EventList.Count == 0 ? null : EventList[EventList.Count - 1];

    /// <summary>
    /// Get the most recent event before the position on the timeline.
    /// </summary>
    public TravelEvent<T> EventOfPosition(float position)
    {
        return Find((te) => te.IsPositionInRange(position));
    }

    /// <summary>
    /// Get the most recent event before the time on the timeline.
    /// </summary>
    public TravelEvent<T> EventOfTime(float time)
    {
        return Find((te) => te.IsTimeInRange(time));
    }

    public void Add(float position, float timeElapsed, T data)
    {
        TravelEvent<T> newTimeEvent = new TravelEvent<T>(position, (LastEvent?.Time ?? 0) + timeElapsed, data);
        LastEvent?.LinkToNext(newTimeEvent);
        EventList.Add(newTimeEvent);
    }
}

/// <summary>
/// In addition to position and time, call .Data to get any kind of data you want.
/// </summary>
public class TravelEvent<T>
{
        public float Position { get; }
        public float PositionNext { get; private set; }
        public float Time { get; }
        public float TimeNext { get; private set;}
        public T Data {get;}

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
            //Debug.Log("Link result : " + this.ToString());
        }

        public bool IsPositionInRange(float position)
        {
            //Debug.LogFormat("Pos Range is {0} - {1}",AbsolutePosition,AbsolutePositionNext);
            return (position >= Position) && (position < PositionNext);
        }

        public bool IsTimeInRange(float time)
        {
            //Debug.LogFormat("Time Range is {0} - {1}",Time,TimeNext);
            return (time >= Time) && (time < TimeNext);
        }

        public override string ToString()
        {
            return string.Format("TE : P {0}-{2} T {1}-{3} {4}", Position, Time, PositionNext, TimeNext, Data.ToString());
        }
}

public static class ListExtension
{
    /// <summary>
    /// WIP
    /// </summary>
    public static T ParallelFind<T>(this List<T> list, int startIndex, System.Predicate<T> criteria)
    {
        int listCount = list.Count;
        for (int i = 0; i < listCount; i++)
        {
        }
        return list[0];
    }
}