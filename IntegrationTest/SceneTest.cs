//When integration testing using "on device" button DEVELOPMENT_BUILD is automatically on.
using UnityEngine;
using NUnit.Framework;
using UnityEngine.SceneManagement;

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

    [SetUp]
    public void StartScene()
    {
        SceneManager.LoadScene(Scene, LoadSceneMode.Single);
    }
}