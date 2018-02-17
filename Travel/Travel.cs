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

    public TravelEvent<T> FirstEvent => EventList.Count == 0 ? null : EventList[0];
    public TravelEvent<T> LastEvent => EventList.Count == 0 ? null : EventList[EventList.Count - 1];

    private int rememberIndex = 0;

    /// <summary>
    /// Get the most recent event before the position on the timeline.
    /// </summary>
    public TravelEvent<T> EventOfPosition(float position)
    {
        //return Find((te) => te.IsPositionInRange(position));
        int plusIndex = -1;
        int minusIndex = 0;
        int useIndex;
        for (int i = 0; i < EventList.Count; i++)
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
                if(rememberIndex + (plusIndex + 1) < EventList.Count)
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
                return EventList[useIndex];
            }
        }
        return null;
    }

    /// <summary>
    /// Get the most recent event before the time on the timeline.
    /// </summary>
    public TravelEvent<T> EventOfTime(float time)
    {
        for(int i = 0; i < EventList.Count ; i++)
        {
            if(EventList[i].IsTimeInRange(time))
            {
                return EventList[i];
            }
        }
        return null;
    }

    /// <summary>
    /// Adds if there is no event in the Travel, otherwise does nothing.
    /// This prevents the travel returning null for all positive time and position
    /// </summary>
    public void AddDefault(T data)
    {
        if(EventList.Count == 0)
        {
            Add(0,0,data);
        }
    }

    /// <summary>
    /// TIME IS TIME ELAPSED NOT ANY TIME YOU WANT!!
    /// </summary>
    public void Add(float position, float timeElapsed, T data)
    {
        if (EventList.Count != 0 && timeElapsed <= 0)
        {
            throw new System.Exception($"Time elapsed except the first one must be positive. position : {position} timeElapsed : {timeElapsed}");
        }
        TravelEvent<T> travelEvent = new TravelEvent<T>(position, (LastEvent?.Time ?? 0) + timeElapsed, data);
        LastEvent?.LinkToNext(travelEvent);
        EventList.Add(travelEvent);
    }
}

/// <summary>
/// In addition to position and time, call .Data to get any kind of data you want.
/// </summary>
public class TravelEvent<T>
{
        public float Position { get; }
        /// <summary>
        /// When this is the last event, this is Mathf.Infinity
        /// </summary>
        public float PositionNext { get; private set; }

        public float Time { get; }
        /// <summary>
        /// When this is the last event, this is Mathf.Infinity
        /// </summary>
        public float TimeNext { get; private set;}

        public T Data {get;}
        /// <summary>
        /// When this is the last event, this is null.
        /// </summary>
        public TravelEvent<T> Next {get; private set;}
        public TravelEvent<T> Previous {get; private set;}

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
            return string.Format($"P {Position}-{PositionNext} T {Time}-{TimeNext} {Data.ToString()}");
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