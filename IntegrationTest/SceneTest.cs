//When integration testing using "on device" button DEVELOPMENT_BUILD is automatically on.
using UnityEngine;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using System.Collections;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.ResourceProviders;

/// <summary>
/// A play mode test which starts on a specific scene by loading solely that scene.
/// This class will ensure the test runner is preserved.
/// </summary>
public abstract class SceneTest : InteBase
{
    /// <summary>
    /// Scene to start on each test's [SetUp]
    /// </summary>
    protected abstract string Scene { get; }

    protected SceneInstance SceneInstance {get; private set;}

    [UnitySetUp]
    public IEnumerator StartScene()
    {
        var handle = Addressables.LoadSceneAsync(Scene, loadMode: LoadSceneMode.Single, activateOnLoad: false);
        yield return handle;
        SceneInstance = handle.Result;
    }
    
}