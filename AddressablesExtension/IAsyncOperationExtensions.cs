using UnityEngine.ResourceManagement.AsyncOperations;
using System.Runtime.CompilerServices;
using System;

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

        public object GetResult() 
        {
            if(_operation.Status == AsyncOperationStatus.Failed)
            {
                throw _operation.OperationException;
            }
            return _operation.Result;
        }

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

        public T GetResult()
        {
            if(_operation.Status == AsyncOperationStatus.Failed)
            {
                throw _operation.OperationException;
            }
            return _operation.Result;
        }

    }
}
