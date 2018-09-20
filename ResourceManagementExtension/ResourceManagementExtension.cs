using UnityEngine.ResourceManagement;

public static class ResourceManagementExtension
{
    /// <summary>
    /// Make a `ChainOperation` in a fluent way from any `IAsyncOperation`. Context and key are null.
    /// </summary>
    public static IAsyncOperation<Obj> ChainTo<Obj, Dep>(this IAsyncOperation<Dep> ops, System.Func<Dep, IAsyncOperation<Obj>> func)
    {
        return new ChainOperation<Obj, Dep>().Start(null, null, ops, func);
    }

    /// <summary>
    /// Make a `ChainOperation` in a fluent way from any IAsyncOperation. Context and key are null.
    /// This overload returns a `CompletedOperation` since the function does not return `IAsyncOperation`.
    /// </summary>
    public static IAsyncOperation<Obj> ChainTo<Obj, Dep>(this IAsyncOperation<Dep> ops, System.Func<Dep, Obj> func)
    {
        return new ChainOperation<Obj, Dep>().Start(null, null, ops, (dep) =>  
        {
            return new CompletedOperation<Obj>().Start(null, null, func(dep));
        });
    }

    /// <summary>
    /// Make a `ChainOperation` in a fluent way from any IAsyncOperation. Context and key are null.
    /// This overload allows you to use a lambda that does not return anything, the `IAsyncOperation` will
    /// wait on the prior operation. Returns a `CompletedOperation`.
    /// </summary>
    public static IAsyncOperation<Dep> ChainTo<Dep>(this IAsyncOperation<Dep> ops, System.Action<Dep> func)
    {
        return new ChainOperation<Dep, Dep>().Start(null, null, ops, (dep) =>  
        {
            func(dep);
            return new CompletedOperation<Dep>().Start(null, null, dep);
        });
    }
}
