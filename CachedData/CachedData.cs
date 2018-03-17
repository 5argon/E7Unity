using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Common point of all cacheables, so you can program something without knowing its T generic class type.
/// </summary>
public interface ICacheableData
{
    bool IsDirty { get; }
    void SetDirty();
}

public class CachedData<T> : ICacheableData {

    private T data;

    private bool dirty;
    public bool IsDirty => dirty;
    public void SetDirty() => dirty = true;

    public CachedData()
    {
        SetDirty();
    }

    /// <summary>
    /// Assign the data at the same time. Useful if T is a reference type and you don't want it to start as null.
    /// </summary>
    public CachedData(T instance) : this()
    {
        data = instance;
    }

    /// <summary>
    /// Data is allowed to get only if it is not dirty, which is achieved by setting something to it.
    /// Data can be set anytime, doing so will remove the dirty status.
    /// </summary>
    public T Data
    {
        get
        {
            if(dirty)
            {
                throw new Exception("Ouch! Why not refresh?");
            }
            return data;
        }
        set
        {
            data = value;
            dirty = false; //clean! not dirty!
        }
    }

    /// <summary>
    /// Use this if you want to do something to the data as an assignment and make it not dirty at the same time.
    /// Be careful of null reference. And it has no dirty check.
    /// </summary>
    /// <returns></returns>
    public T Assignment
    {
        get
        {
            dirty = false;
            return data;
        }
    }


}
