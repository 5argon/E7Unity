using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CachedData<T> {

    private T data;
    private bool dirty;

    public CachedData()
    {
        SetDirty();
    }

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

    public bool IsDirty
    {
        get{ return dirty; }
    }

    public void SetDirty()
    {
        dirty = true;
    }

}
