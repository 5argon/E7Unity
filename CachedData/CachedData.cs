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

}
