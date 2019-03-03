#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;
using UniRx.Async;

public static class AddressablesExtension
{
    public static bool IsNullOrEmpty(this AssetReference aref)
    {
        return aref == null || aref.RuntimeKey == Hash128.Parse("");
    }

    /// <summary>
    /// Use the Addressables system if in real play, use `editorAsset` if in edit mode.
    /// </summary>
    public static async UniTask<T> LoadAssetX<T>(this AssetReference aref)
    where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (!Application.isPlaying) 
        {
            return (T)aref.editorAsset;
        }
#endif
        var op = aref.LoadAsset<T>();
        var result = await op;
        //Debug.Log($"{op.Status} {object.ReferenceEquals(null, op.Result)} {op.IsDone} {op.IsValid} {op.OperationException}");
        return result;
    }

    /// <summary>
    /// Use the Addressables system if in real play, use `AssetDatabase` if in edit mode.
    /// </summary>
    /// <param name="key">Addressable key</param>
    /// <param name="pathForEditor">This starts with "Assets/..." and you need the file extension as well.</param>
    public static async UniTask<T> LoadAssetX<T>(string key, string pathForEditor)
    where T : UnityEngine.Object
    {
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            return AssetDatabase.LoadAssetAtPath<T>(pathForEditor);
        }
#endif
        return await Addressables.LoadAsset<T>(key);
    }
}
