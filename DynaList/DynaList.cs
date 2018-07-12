using System.Collections.Generic;
using UnityEngine;

/* 
DynaList is a List that acts kind of like a dictionary, but
it is a List so you can serialize it along with your binary save file.

Moreover, if you Get with an undefined key it will automatically create
a new entry. So you have forward compatibility.

In Duel Otters, when I add new games the save will correctly registers a new game's data. In Mel Cadence, same goes for new songs.
*/

[System.Serializable]
public abstract class DynaList<LISTTYPE,KEYTYPE> where KEYTYPE : class{

    //use something from content
    protected abstract LISTTYPE New(KEYTYPE key);
    protected abstract KEYTYPE Key(LISTTYPE item);
    protected abstract bool KeyEqualityTest(KEYTYPE item1, KEYTYPE item2);

    protected List<LISTTYPE> data;

    public DynaList()
    {
        data = new List<LISTTYPE>();
    }

    public LISTTYPE Get(KEYTYPE key)
    {
        foreach(LISTTYPE item in data)
        {
            if(KeyEqualityTest(Key(item),key))
            {
                return item;
            }
        }
        //create new
        LISTTYPE newItem = New(key);
        data.Add(newItem);
        return newItem;
    }
}