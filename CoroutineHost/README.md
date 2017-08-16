# CoroutineHost

This class can automatically instantiate a `MonoBehaviour` to host a coroutine which is resistant to scene load. You just call the static method and forget, or yo can also have `Task` to check if it finishes or not in a modern C# way. It also a way to do things on the main thread from your other thread. 

*If you don't have an access to `Task` please **comment out the preprocessor at the first line**.

## Usage

```csharp
public static void SomeInconvenientPlace()
{
    //StartCoroutine() is not possible at this point.
    CoroutineHost.Host(YourRoutine());
}

IEnumerator MyRoutine()
{
    yield return new WaitForSeconds(5);
}
```

## Advanced usage with .NET 4.6 / `System.Threading.Tasks`

```csharp
public void MyMethod()
{
    CoroutineHost.PrepareCrossThread();
    Task childThread = Task.Run(()=> {
        CoroutineHost.Host(MyRoutine());
    });
}

IEnumerator MyRoutine()
{
    yield return new WaitForSeconds(10);
    GameObject go = new GameObject("Hi"); //This is not possible to call in a child thread.
}
```

## Advanced usage with C# 5's `async/await`

```csharp
public async Task MyAsyncMethodOnMainThread()
{
    CoroutineHost.PrepareCrossThread();
    Task<int> asyncOnChildThread = Task.Run(async ()=> {
        await CoroutineHost.Host(MyHeavyRoutine());
        Debug.Log("Yes!");
        return 555;
    });
    int result = await asyncOnChildThread;
    //etc.
}

IEnumerator MyHeavyRoutine()
{
    for(int i = 0; i < 100000; i++)
    {
        GameObject go = new GameObject("Hi"); //This is not possible to call in a child thread.
        yield return null;
    }
}
```

## Explanations

### Use it as a convenient way of starting a coroutine without any game objects (but you are in the play mode)

You can run a coroutine from static methods or other place this way as this will create a "host" for you to run that routine.

### Use it as a way to run things in main thread from a child thread

Many things in Unity such as instantiating a game object or creating/firing `UnityWebRequest` is not possible on the child thread. It will results in a runtime error.

If you want to use it from non-main thread, call `CoroutineHost.PrepareCrossThread()` first because game object can't be instantiated in other thread. Many of your threads can call to this without worries as I have placed a `lock` keyword to prevent possible race conditions.

Also in that case you might have `System.Threading.Tasks`, the return of `.Host` is of type `Task`. You can use `.ContinueWith` or others as you like. If you also have `async`/`await` available (.NET 4.6, C# 5+) you can `await CoroutineHost.Host(...)` to wait for your main thread from your child thread.

## Possible improvements

Because we can get `Task` from `.Host` it should be possible to send a cancellation token to `StopCoroutine` in the host. I don't need to stop my routine at the moment, so this feature is still just an idea.

## License

MIT

5argon / Exceed7 Experiments