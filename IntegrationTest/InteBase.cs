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
    /// We likely do a scene load after starting a test. Scene load with Single mode won't destroy the test runner game object with this.
    /// </summary>
    [SetUp]
    public void ProtectTestRunner()
    {
        GameObject g = GameObject.Find("Code-based tests runner");
        Debug.Log($"Protecting test runner {g} {g.name}");
        GameObject.DontDestroyOnLoad(g);
    }
}
//#endif