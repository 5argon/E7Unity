using UnityEngine;
using System.Collections.Generic;

public class SizedQueue<T> : Queue<T>
{
    public int FixedCapacity { get; }
    public SizedQueue(int fixedCapacity)
    {
        this.FixedCapacity = fixedCapacity;
    }

    /// <summary>
    /// If the total number of item exceed the capacity, the oldest ones automatically dequeues.
    /// </summary>
    /// <returns>The dequeued value, if any.</returns>
    public new T Enqueue(T item)
    {
        base.Enqueue(item);
        if (base.Count > FixedCapacity)
        {
            return base.Dequeue();
        }
        return default;
    }
}