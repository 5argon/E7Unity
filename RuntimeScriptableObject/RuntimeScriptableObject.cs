#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Callbacks;
#endif
using UnityEngine;
using System.Collections;
using System.IO;

public abstract class RuntimeScriptableObject<T> : ScriptableObject where T : ScriptableObject
{
    private static string[] guids;
    private static string path;
    private static T cached;
    private const string resourcesFolderName = "Resources/RSO";

    public static T Get
    {
        get
        {
            if (cached != null)
            {
                //Debug.Log("[RSO] Get from cache.");
                return cached;
            }

            T obj = (T)Resources.Load(resourcesFolderName + "/" + typeof(T).Name);
            if (obj != null)
            {
                cached = obj;
            }
            else
            {
                cached = ScriptableObject.CreateInstance<T>();
                //Debug.LogWarning("[RSO] Created new temporary data file at runtime! This is not permanent!");
            }
            //Debug.Log("[RSO] Get from resources.");
            return cached;
        }
    }

}
