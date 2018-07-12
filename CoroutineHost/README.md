# CoroutineHost

This class can automatically instantiate a `MonoBehaviour` to host a coroutine which is resistant to scene load. You just call the static method and forget, or yo can also have `Task` to check if it finishes or not in a modern C# way. It also a way to do things on the main thread from your other thread. 

*If you don't have an access to `Task` please **comment out the preprocessor at the first line**.

## Methods

Just `CoroutineHost.Host`! But it has several overloads.

```csharp
    //IEnumerator
    CoroutineHost.Host(CoroutineFunctionThatReturnsIEnumerator());

    //Action
    CoroutineHost.Host(AnyMethodWithoutArgument);
    CoroutineHost.Host(()=>AnyMethod(555));
    CoroutineHost.Host(()=>{ AnyMethod(555); MoreMethod(555);});
    CoroutineHost.Host(()=>DelayBeforeThis(), new WaitForSeconds(3));

    //Func
    CoroutineHost.Host<int>(()=> { return MethodReturnsInt(); });
    int returnValue = await CoroutineHost.Host<int>(()=> { return MethodReturnsInt(); });
```


## Simple Usages

### Starting a coroutine

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

## Advanced Usages

### Call cross-thread with .NET 4.6 / `System.Threading.Tasks`

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

### Starting a lambda function on the main thread

```csharp
    Task.Run(()=>{
        //We are in non-main thread now

        //Application.persistentDataPath does not work on non-main thread so we have the main thread host it.
        //This lambda will becomes a coroutine inside.
        CoroutineHost.Host(() => Debug.Log(Application.persistentDataPath));
    });
```


### Order the main thread to execute lambda with a return value getting it back when the main thread finishes executing the function

```csharp
    Task.Run(()=>{
        //We are in a non-main thread now

        //This Host<T> signifies lambda with return value, but unfortunately does not affects return type. It is still an "object".
        //But it can make sure that your lambda return T or compile will error.
        Task<object> t = CoroutineHost.Host<string>(() => { return Application.persistentDataPath; });

        //So you will need to cast it again. (It's Unity's fault for not allowing generic with MonoBehaviour where the return value was sent back.)
        t.ContinueWith( t => Debug.Log((string)t.Result);
    });
```

### Usage with C# 5's `async/await`

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

### `async/await` makes the return value lambda simpler

```csharp
    Task.Run(async ()=>{
        //We are in non-main thread now

        //await automatically turns Task<object> into object
        string result = (string) await CoroutineHost.Host<string>(() => { return Application.persistentDataPath; });
        Debug.Log(result);
    });
```

## Explanations

### Use it as a convenient way of starting a coroutine without any game objects (but you are in the play mode)

You can run a coroutine from static methods or other place this way as this will create a "host" for you to run that routine.

### Use it as a way to run things in main thread from a child thread

Many things in Unity such as instantiating a game object or creating/firing `UnityWebRequest` is not possible on the child thread. It will results in a runtime error.

If you want to use it from non-main thread, call `CoroutineHost.PrepareCrossThread()` first because game object can't be instantiated in other thread. Many of your threads can call to this without worries as I have placed a `lock` keyword to prevent possible race conditions.

Also in that case you might have `System.Threading.Tasks`, the return of `.Host` is of type `Task`. You can use `.ContinueWith` or others as you like. If you also have `async`/`await` available (.NET 4.6, C# 5+) you can `await CoroutineHost.Host(...)` to wait for your main thread from your child thread.

## Thread Ninja?
If you know about [Thread Ninja](https://www.assetstore.unity3d.com/en/#!/content/15717), a plugin in the asset store since Unity 3.4 that can start a coroutine on new thread + utility to switch back to main thread on the fly mid-coroutine. You might want to know how this compares to it.

- Both tools are not of the same purpose but can achieve similar results.
- CoroutineHost cannot start a task in the new thread. You must be already in a child thread and use CoroutineHost to issue task on the main thread, then you can use `await` to wait for the command. Thread Ninja use procedural style programming and internal that supports .NET 3.5 (that is, using Threading but not Tasks) so that both of your main thread work and your child thread work seems to be magically in the same coroutine separated by a line of `Ninja.` call.
- To use CoroutineHost like Thread Ninja, start a task with standard Task Parallel Library call like Task.Run(). In Thread Ninja, you start a new thread with a coroutine. With TPL, you can start a new thread with any code because it is a part of standard C#. This has an advantage of allowing more natural programming, does not require you to encapsulate your work in a function that returns `IEnumerator`.
- I don't know about the performace of TPL in .NET 4.6 vs. bare bone `Threading` in .NET 3.5. But [according to Microsoft](https://docs.microsoft.com/en-us/dotnet/standard/parallel-programming/task-based-asynchronous-programming), they said TPL uses `ThreadPool` eventually but is backed by efficient load balancing algorithms. So, I think using TPL + CoroutineHost for "ninja jump" to the main thread seems like a great idea.
- However if you can't use .NET 4.6 then Thread Ninja is definitely the way to go. The power of TPL + CoroutineHost is unlocked by `Task` and `async`/`await`.

## Possible improvements

Because we can get `Task` from `.Host` it should be possible to send a cancellation token to `StopCoroutine` in the host. I don't need to stop my routine at the moment, so this feature is still just an idea.

## Rules

It follows a normal Unity's `YieldInstruction` flow, it runs after the `Update` of that game object + all game objects. In this case that game object was created specially for hosting routine but it will still be after all of your scripts's `Update`.

## License

MIT

5argon / Exceed7 Experiments