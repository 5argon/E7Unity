/// 
/// InteFirebaseToolkit.cs
/// 5argon / Exceed7 Experiments
/// 

//PlayMode test in editor or PlayMode test in real device. Remember that PlayMode test to device automatically set DEVELOPMENT_BUILD.
//You don't need to worry leaking a sensitive Firebase data in the real game code.
#if UNITY_EDITOR || (DEVELOPMENT_BUILD && ( UNITY_IOS  || UNITY_ANDROID) )

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;
using System.Collections;

using NUnit.Framework;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Storage;
using Firebase.Unity.Editor; //We don't need this anymore after v4.5.0 !

using System.Threading.Tasks;

/*  
On your sandbox bucket, enter a rule like this :

      "sandbox": {
      ".validate": "newData.hasChildren(['numberOnly'])",
      "numberOnly": {
        ".validate": "newData.isNumber()"
      },
      "$other": {
        ".validate": "false"
      },
      ".read": "true",
      ".write": "auth.uid == 'your-test-account-uid'"
    }

    "test_public": {
      ".read": "true",
      ".write": "true"
    },

*/

/// <summary>
/// This class works together with FirebaseToolkit, testing it in the process.
/// FT is FirebaseToolkit's subclass.
/// After overriding FirebaseToolkit, you also override this one and put your own FirebaseToolkit class as this class's generic.
/// </summary>
public abstract class InteFirebaseToolkit<FT> : InteBase where FT : FirebaseToolkit<FT>, new()
{
    /// <summary>
    /// Look in your project's Settings.
    /// </summary>
    protected abstract string ProjectID { get; }

    /// <summary>
    /// Look in your project's Settings.
    /// </summary>
    protected abstract string AndroidAppID { get; }

    /// <summary>
    /// When pressing "Run in player" on Unity's PlayMode test runner, the app will be forced to com.UnityTestRunner.UnityTestRunner package name.
    /// Since Firebase ties package name to the App ID, we need to create a separate app for the test runner to use.
    /// Go create an Android app with com.UnityTestRunner.UnityTestRunner package name and put it here.
    /// </summary>
    protected abstract string AndroidTestRunnerAppID { get; }

    /// <summary>
    /// Look in your project's Settings.
    /// </summary>
    protected abstract string iOSAppID { get; }

    /// <summary>
    /// When pressing "Run in player" on Unity's PlayMode test runner, the app will be forced to com.UnityTestRunner.UnityTestRunner package name.
    /// Since Firebase ties package name to the App ID, we need to create a separate app for the test runner to use.
    /// Go create an iOS app with com.UnityTestRunner.UnityTestRunner package name and put it here.
    /// </summary>
    protected abstract string iOSTestRunnerAppID { get; }

    protected string AppID
    {
        get
        {
#if UNITY_EDITOR
            switch (EditorUserBuildSettings.activeBuildTarget)
            {
                case BuildTarget.iOS: return iOSAppID;
                case BuildTarget.Android: return AndroidAppID;
            }
            throw new System.Exception("Editor PlayMode test must be running on iOS or Android!");
#elif UNITY_ANDROID
                return AndroidTestRunnerAppID;
#elif UNITY_IOS
                return iOSTestRunnerAppID;
#endif
        }
    }

    private const string gsPrefix = "gs://";

    /// <summary>
    /// gs://something
    /// The name of a bucket with (default) in the Firebase console. That information is in Google config file so we can compare with this.
    /// </summary>
    protected abstract string DefaultBucketName { get; }
    protected string DefaultBucketNameNoGs => DefaultBucketName.Replace(gsPrefix, "");

    /// <summary>
    /// gs://something
    /// The bucket you actually want to use in the test. Should be different from DefaultBucketName and not the one you use in the real game. The test will clean them!
    /// </summary>
    protected abstract string TestBucketName { get; }
    protected string TestBucketNameNoGs => TestBucketName.Replace(gsPrefix, "");

    /// <summary>
    /// With the new Desktop Workflow Implementation, we can use Auth with the real account instead of a service account.
    /// The test will send you a verification e-mail if a certain test fails. Make sure that e-mail exists.
    /// </summary>
    protected abstract string TestAccountIdentifier { get; }

    /// <summary>
    /// This whole code will be preprocessed out in the non-development build. No need to fear that this will leak out. (Unless someone is standing behind you)
    /// </summary>
    protected abstract string TestAccountPassword { get; }

    /// <summary>
    /// The test will check if UID from Firebase matches with this or not.
    /// </summary>
    protected abstract string TestAccountUID { get; }

    /// <summary>
    /// Like test account but it does not need to have a valid e-mail. Just create an ID in the console.
    /// </summary>
    protected abstract string TestAccountUnverifiedIdentifier { get; }
    protected abstract string TestAccountUnverifiedPassword { get; }
    protected abstract string TestAccountUnverifiedUID { get; }

    /// <summary>
    /// Many of FirebaseToolkit are static, it will preserves across tests if we don't reset them.
    /// </summary>
    [SetUp]
    public void SetUpFirebaseToolkit() => FirebaseToolkit<FT>.TestSetUp();

    [TearDown]
    public void TearingDown() => Debug.Log("Teardown");

    private DatabaseReference TestPublic => FirebaseToolkit<FT>.Database.RootReference.Child("test_public");
    private DatabaseReference TestPrivate => FirebaseToolkit<FT>.Database.RootReference.Child("test");

    private FirebaseApp App => FirebaseToolkit<FT>.CurrentFirebaseApp;
    private FirebaseAuth Auth => FirebaseToolkit<FT>.Auth;
    private FirebaseDatabase Database => FirebaseToolkit<FT>.Database;
    private FirebaseStorage Storage => FirebaseToolkit<FT>.Storage;

    /// <summary>
    /// Currently (31/03/2018 - 2017.4.0f1) Firebase logs (not throw) an Error log when doing realtime DB with any Auth login causing tests to fail.
    /// </summary>
    private void ExpectFirebaseAuthBug() => LogAssert.Expect(LogType.Error, new System.Text.RegularExpressions.Regex("IOException during handshake"));

    // ===============================================================================
    // ===============================================================================

    [UnityTest]
    public IEnumerator T_ConfigFileWereReadToDefaultInstance()
    {
        Assert.That(FirebaseApp.DefaultInstance.Options.StorageBucket, Is.EqualTo(DefaultBucketNameNoGs), "Firebase in editor reads Google service files successfully.");
        Assert.That(FirebaseApp.DefaultInstance.Options.AppId, Is.EqualTo(AppID), "Firebase in editor reads Google service files successfully.");
        Assert.That(FirebaseApp.DefaultInstance.Options.ProjectId, Is.EqualTo(ProjectID), "Firebase in editor reads Google service files successfully.");
        Assert.That(FirebaseApp.DefaultInstance.Name, Is.EqualTo("__FIRAPP_DEFAULT"), "Google has __FIRAPP_DEFAULT app name by default.");
        yield break;
    }

    [UnityTest]
    public IEnumerator T_NewAppCanBeCreatedFromDefaultApp()
    {
        const string testInstanceName = "FirebaseApp-TestInstance";
        FirebaseApp TestFirebaseApp = FirebaseApp.Create(FirebaseApp.DefaultInstance.Options, testInstanceName);
        Assert.That(TestFirebaseApp.Options.StorageBucket, Is.EqualTo(DefaultBucketNameNoGs), "Same as the default instance.");
        Assert.That(TestFirebaseApp.Options.AppId, Is.EqualTo(AppID), "Same as the default instance.");
        Assert.That(TestFirebaseApp.Options.ProjectId, Is.EqualTo(ProjectID), "Same as the default instance.");
        Assert.That(TestFirebaseApp.Name, Is.EqualTo(testInstanceName), "We can assign a new name to cloned instance");
        yield break;
    }

    [UnityTest]
    public IEnumerator T_GetInstanceManyTimes()
    {
        FirebaseApp fa = FirebaseToolkit<FT>.CurrentFirebaseApp;
        FirebaseApp fa2 = FirebaseToolkit<FT>.CurrentFirebaseApp;
        FirebaseApp fa3 = FirebaseToolkit<FT>.CurrentFirebaseApp;

        Assert.That(object.ReferenceEquals(fa, fa2), "It does not creates a new instance on every call.");
        Assert.That(object.ReferenceEquals(fa2, fa3), "It does not creates a new instance on every call.");

        FirebaseToolkit<FT>.TestSetUp();

        FirebaseApp newApp = FirebaseToolkit<FT>.CurrentFirebaseApp;

        Assert.That(object.ReferenceEquals(fa, newApp), Is.Not.True, $"After a {nameof(FirebaseToolkit<FT>.TestSetUp)} now we get a different instance.");
        Assert.That(fa.Name, Is.Not.EqualTo(newApp.Name));

        yield break;
    }

    [UnityTest]
    public IEnumerator T_AuthSignIn()
    {
        //Test runner cannot start without IEnumerator method definition, but we want to test in a Task style where we can use await.
        yield return T_AuthSignIn_Task().YieldWaitTest();
    }

    public async Task T_AuthSignIn_Task()
    {
        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.Not.True, $"{nameof(FirebaseToolkit<FT>.IsSignedIn)} is false from the start.");

        FirebaseUser user = await Auth.SignInWithEmailAndPasswordAsync(TestAccountIdentifier, TestAccountPassword);

        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.True, "Real sign in works in both editor and real device.");

        Assert.That(user.UserId, Is.EqualTo(TestAccountUID));
        Assert.That(user.Email, Is.EqualTo(TestAccountIdentifier));

        if (user.IsEmailVerified == false)
        {
            Debug.LogError($"Test account is still unverified. A verification e-mail has been sent to {TestAccountIdentifier}. Verify it and run the test again!");
            await user.SendEmailVerificationAsync();
        }

        Assert.That(user.IsEmailVerified, Is.True);
        Assert.That(user.IsAnonymous, Is.Not.True);

        Debug.Log("Signed in");
    }

    [UnityTest]
    public IEnumerator AAAAA()
    {
        LogAssert.ignoreFailingMessages = true;

        yield return FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync("editor@exceed7.com", "ysmanDtwRQsDDtD").YieldWaitTest();
        var toWrite = new { numberOnly = 444};
        yield return FirebaseDatabase.DefaultInstance.RootReference.Child("test").SetJson(toWrite).YieldWaitTest();
        Debug.Log("DSFSADF");

    }

    [UnityTest]
    public IEnumerator T_AuthSignInUnverified()
    {
        yield return T_AuthSignInUnverified_Task().YieldWaitTest();
    }

    private async Task T_AuthSignInUnverified_Task()
    {
        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.Not.True, $"{nameof(FirebaseToolkit<FT>.IsSignedIn)} is false from the start.");
        FirebaseUser loggedInUser = await Auth.SignInWithEmailAndPasswordAsync(TestAccountUnverifiedIdentifier, TestAccountUnverifiedPassword);

        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.True, "You can also sign in to unverified address.");

        Assert.That(loggedInUser.UserId, Is.EqualTo(TestAccountUnverifiedUID));
        Assert.That(loggedInUser.Email, Is.EqualTo(TestAccountUnverifiedIdentifier));

        Assert.That(loggedInUser.IsEmailVerified, Is.Not.True, "We are signing in to an unverified account.");
        Assert.That(loggedInUser.IsAnonymous, Is.Not.True);

        Assert.That(FirebaseToolkit<FT>.IsSignedInAndVerified, Is.Not.True, $"We can use {FirebaseToolkit<FT>.IsSignedInAndVerified} property to check players that did not click the confirmation mail yet.");
    }

    [UnityTest]
    public IEnumerator T_AuthSignInOut()
    {
        yield return T_AuthSignIn();
        Auth.SignOut();
        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.Not.True, "Sign out works.");
    }

    [UnityTest]
    public IEnumerator T_TestSetUpRemovesSignin()
    {
        yield return T_AuthSignIn();
        FirebaseToolkit<FT>.TestSetUp();
        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.Not.True, $"We should be able to use TestSetUp to reset FirebaseToolkit so we can run multiple tests consecutively.");
        FirebaseToolkit<FT>.TestSetUp();
        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.Not.True, $"Running TestSetUp many times is OK");
    }

    [UnityTest]
    public IEnumerator T_AuthSignInWrongCredential()
    {
        yield return T_AuthSignInWrongCredential_Task().YieldWaitTest();
    }

    private async Task T_AuthSignInWrongCredential_Task()
    {
        await Auth.SignInWithEmailAndPasswordAsync(TestAccountIdentifier, TestAccountPassword + "asdf").ShouldThrow<FirebaseException>("Password should be wrong.");
        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.Not.True);
        await Auth.SignInWithEmailAndPasswordAsync(TestAccountIdentifier + "asdf", TestAccountPassword).ShouldThrow<FirebaseException>("Credential should not exist.");
        Assert.That(FirebaseToolkit<FT>.IsSignedIn, Is.Not.True);
    }

    [UnityTest]
    public IEnumerator T_WriteTestPublicNotLoggedIn()
    {
        yield return T_WriteTestPublicCommon_Task().YieldWaitTest();
    }

    private async Task T_WriteTestPublicCommon_Task()
    {
        var toWrite = new { numberOnly = "not a number!", hi = 555 };
        await TestPublic.SetJson(toWrite);

        DataSnapshot data = await TestPublic.GetValueAsync();
        Assert.That(data.ChildrenCount, Is.EqualTo(2), "Check if the value is really there");
        Assert.That(data.Child("numberOnly").Value, Is.EqualTo("not a number!"), "Check if the value is really there");
        Assert.That(data.Child("hi").Value, Is.EqualTo(555), "Check if the value is really there");
    }

    [UnityTest]
    public IEnumerator T_WriteTestPrivateNotLoggedIn()
    {
        yield return T_WriteTestPrivateNotLoggedIn_Task().YieldWaitTest();
    }

    private async Task T_WriteTestPrivateNotLoggedIn_Task()
    {
        var toWrite = new { numberOnly = 5555 };
        await TestPrivate.SetJson(toWrite).ShouldThrow<DatabaseException>("Can't write to private without logging in.");

        var toWriteWrongDataType = new { numberOnly = "not a number!"};
        await TestPrivate.SetJson(toWriteWrongDataType).ShouldThrow<DatabaseException>("Can't write to private without logging in.");
    }

    [UnityTest]
    public IEnumerator T_WriteTestPublicLoggedIn()
    {
        yield return T_WriteTestPublicLoggedIn_Task().YieldWaitTest();
    }

    public async Task T_WriteTestPublicLoggedIn_Task()
    {
        ExpectFirebaseAuthBug();
        await T_AuthSignIn_Task();
        await T_WriteTestPublicCommon_Task(); //Logged in or not it should be the same, but that IOException bug we have to take care..
    }

    [UnityTest]
    public IEnumerator T_WriteTestPrivateLoggedIn()
    {
        yield return T_WriteTestPrivateLoggedIn_Task().YieldWaitTest();
    }

    public async Task T_WriteTestPrivateLoggedIn_Task()
    {
        ExpectFirebaseAuthBug();
        await T_AuthSignIn_Task();

        var toWrite = new { numberOnly = 5555 };
        await TestPrivate.SetJson(toWrite);

        var toWriteWrongDataType = new { numberOnly = "not a number!"};
        await TestPrivate.SetJson(toWriteWrongDataType).ShouldThrow<DatabaseException>("Wrong data structure!");
    }

    [UnityTest]
    public IEnumerator T_RemoveTestPublicNotLoggedIn()
    {
        yield return T_RemoveTestPublicNotLoggedIn_Task().YieldWaitTest();
    }

    private async Task T_RemoveTestPublicNotLoggedIn_Task()
    {
        await T_WriteTestPublicCommon_Task();
        await TestPublic.RemoveValueAsync();

        DataSnapshot data = await TestPublic.GetValueAsync();
        Assert.That(data.HasChildren, Is.Not.True, "We have removed an entire test public tree.");
    }

    [UnityTest]
    public IEnumerator T_RemoveTestPublicLoggedIn()
    {
        yield return T_RemoveTestPublicLoggedIn_Task().YieldWaitTest();
    }

    private async Task T_RemoveTestPublicLoggedIn_Task()
    {
        ExpectFirebaseAuthBug();

        await T_AuthSignIn_Task(); // <---
        await T_WriteTestPublicCommon_Task();
        await TestPublic.RemoveValueAsync();

        DataSnapshot data = await TestPublic.GetValueAsync();
        Assert.That(data.HasChildren, Is.Not.True, "We have removed an entire test public tree while logged in.");
    }

    [UnityTest]
    public IEnumerator T_RemoveTestPrivateNotLoggedIn()
    {
        yield return T_RemoveTestPrivateNotLoggedIn_Task().YieldWaitTest();
    }

    private async Task T_RemoveTestPrivateNotLoggedIn_Task()
    {
        await T_WriteTestPrivateLoggedIn_Task();
        Auth.SignOut();
        await TestPrivate.RemoveValueAsync().ShouldThrow<DatabaseException>("We cannot remove private test without logging in");

        DataSnapshot data = await TestPrivate.GetValueAsync();
        Assert.That(data.HasChildren, Is.Not.True, "We have removed an entire test public tree.");
    }

} 

#endif