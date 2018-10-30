//When integration testing using "on device" button DEVELOPMENT_BUILD is automatically on.
//#if UNITY_EDITOR || (DEVELOPMENT_BUILD && !UNITY_EDITOR)
using UnityEngine;
using NUnit.Framework;

/// <summary>
/// InteBase by 5argon - Exceed7 Experiments
/// This is now based on Unity 5.6's test runner. Separate Integration scene no longer required.
/// </summary>
public abstract class InteBase {

    /// <summary>
    /// The only way to check if we are in a test or not. Because only test runner can activate [SetUp] and [TearDown].
    /// </summary>
    public static bool IsTesting { get; private set; } = false;

    [SetUp]
    public void IsTestingOn() => IsTesting = true;

    [TearDown]
    public void IsTestingOff() => IsTesting = false;


    /// <summary>
    /// We likely do a scene load after starting a test. Scene load with Single mode won't destroy the test runner game object with this.
    /// </summary>
    [SetUp]
    public void ProtectTestRunner()
    {
        GameObject g = GameObject.Find("Code-based tests runner");
        GameObject.DontDestroyOnLoad(g);
    }
}
//#endif