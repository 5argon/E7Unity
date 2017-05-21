using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CachedArray<T> {

    public T[] datas;
    private bool[] dirty;

    public CachedArray(int size)
    {
        datas = new T[size];
        dirty = new bool[size];
        SetDirty();
    }

    public T this[int index]
    {
        get
        {
            if(dirty[index])
            {
                throw new Exception("Ouch! Why not refresh?");
            }
            return datas[index];
        }
        set
        {
            datas[index] = value;
            dirty[index] = false; //clean! not dirty!
        }
    }

    public bool[] IsDirty
    {
        get{ return dirty; }
    }

    public void SetDirty()
    {
        for( int i = 0; i < dirty.Length; i++)
        {
            dirty[i] = true;
        }
    }

}
