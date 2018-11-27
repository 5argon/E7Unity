#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;
using UniRx.Async;
using System;

public static class UniTaskCustomExtension
{
}

public static class IAsyncOperationExtensions
{
    public static AsyncOperationAwaiter GetAwaiter(this IAsyncOperation operation)
    {
        return new AsyncOperationAwaiter(operation);
    }

    public static AsyncOperationAwaiter<T> GetAwaiter<T>(this IAsyncOperation<T> operation) 
    {
        return new AsyncOperationAwaiter<T>(operation);
    }

    public readonly struct AsyncOperationAwaiter : INotifyCompletion
    {

        readonly IAsyncOperation _operation;

        public AsyncOperationAwaiter(IAsyncOperation operation)
        {
            _operation = operation;
        }

        public bool IsCompleted => _operation.Status != AsyncOperationStatus.None;

        public void OnCompleted(Action continuation) => _operation.Completed += (op) => continuation?.Invoke();

        public object GetResult() => _operation.Result;

    }

    public readonly struct AsyncOperationAwaiter<T> : INotifyCompletion 
    {

        readonly IAsyncOperation<T> _operation;

        public AsyncOperationAwaiter(IAsyncOperation<T> operation)
        {
            _operation = operation;
        }

        public bool IsCompleted => _operation.Status != AsyncOperationStatus.None;

        public void OnCompleted(Action continuation) => _operation.Completed += (op) => continuation?.Invoke();

        public T GetResult() => _operation.Result;

    }
}

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
