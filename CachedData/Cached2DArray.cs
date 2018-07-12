using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Cached2DArray<T> {

    public T[,] datas;
    private bool[,] dirty;

    public Cached2DArray(int sizeX, int sizeY)
    {
        datas = new T[sizeX,sizeY];
        dirty = new bool[sizeX,sizeY];
        SetDirty();
    }

    public T this[int indexX, int indexY]
    {
        get
        {
            if(dirty[indexX, indexY])
            {
                throw new Exception("Ouch! Why not refresh?");
            }
            return datas[indexX, indexY];
        }
        set
        {
            datas[indexX, indexY] = value;
            dirty[indexX, indexY] = false; //clean! not dirty!
        }
    }

    public bool[,] IsDirty
    {
        get{ return dirty; }
    }

    public void SetDirty()
    {
        for( int i = 0; i < dirty.GetLength(0); i++)
        {
            for( int j = 0; j < dirty.GetLength(1); j++)
            {
                dirty[i,j] = true;
            }
        }
    }

}
