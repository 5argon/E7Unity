//When integration testing using "on device" button DEVELOPMENT_BUILD is automatically on.
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !UNITY_EDITOR)
using UnityEngine;
using System.Collections;
using NUnit.Framework;
using System;
using System.Threading.Tasks;

public static class TaskExtensionTest
{
    public const int timeOut = 25;

    /// <summary>
    /// Make a Task yieldable, but there is a time out so it is suitable for running tests.
    /// </summary>
    public static IEnumerator YieldWaitTest(this Task task)
    {
        float timeTaken = 0;
        while (task.IsCompleted == false)
        {
            if(task.IsCanceled)
            {
                Assert.Fail("Task canceled!");
            }
            yield return null;
            timeTaken += Time.deltaTime;
            if(timeTaken > timeOut)
            {
                Assert.Fail("Time out!");
            }
        }
        Assert.That(task.IsFaulted,Is.Not.True,task.Exception?.ToString());
        Debug.Log("Task time taken : " + timeTaken);
    }

    public static async Task ShouldThrow<T>(this Task asyncMethod) where T : Exception
    {
        await ShouldThrow<T>(asyncMethod,"");
    }

    public static async Task ShouldThrow<T>(this Task asyncMethod, string message) where T : Exception
    {
        try
        {
            await asyncMethod; //Should throw..
        }
        catch (T)
        {
            //Task should throw Aggregate but add this just in case.
            Debug.Log("Caught an exception : " + typeof(T).FullName + " !!");
            return;
        }
        catch (AggregateException ag)
        {
            foreach (Exception e in ag.InnerExceptions)
            {
                Debug.Log("Caught an exception : " + e.GetType().FullName + " !!");
                if (message != "")
                {
                    //Fails here if we find any other inner exceptions
                    Assert.That(e, Is.TypeOf<T>(), message + " | " + e.ToString());
                }
                else
                {
                    //Fails here also
                    Assert.That(e, Is.TypeOf<T>(), e.ToString() + " " + "An exception should be of type " + typeof(T).FullName);
                }
            }
            return;
        }
        Assert.Fail("Expected an exception of type " + typeof(T).FullName + " but no exception was thrown."  );
    } 
}
#endif