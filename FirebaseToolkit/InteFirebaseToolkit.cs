using System.Collections;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections.Generic;

using NUnit.Framework;

using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Unity.Editor;

using Newtonsoft.Json;

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

*/

/// <summary>
/// This class works together with FirebaseToolkit, testing it in the process.
/// After overriding FirebaseToolkit, you also override this one and put your own FirebaseToolkit class as this class's generic.
/// </summary>
public abstract class InteFirebaseToolkit<FIREBASE_TOOLKIT_SUBCLASS> : InteBase where FIREBASE_TOOLKIT_SUBCLASS : FirebaseToolkit<FIREBASE_TOOLKIT_SUBCLASS>, new()
{
    /// <summary>
    /// Look in your project's Settings.
    /// </summary>
    protected abstract string ProjectID {get;}

    /// <summary>
    /// Look in your project's Settings.
    /// </summary>
    protected abstract string AndroidAppID {get;}

    /// <summary>
    /// When pressing "Run in player" on Unity's PlayMode test runner, the app will be forced to com.UnityTestRunner.UnityTestRunner package name.
    /// Since Firebase ties package name to the App ID, we need to create a separate app for the test runner to use.
    /// Go create an Android app with com.UnityTestRunner.UnityTestRunner package name and put it here.
    /// </summary>
    protected abstract string AndroidTestRunnerAppID {get;}

    /// <summary>
    /// Look in your project's Settings.
    /// </summary>
    protected abstract string iOSAppID {get;}

    /// <summary>
    /// When pressing "Run in player" on Unity's PlayMode test runner, the app will be forced to com.UnityTestRunner.UnityTestRunner package name.
    /// Since Firebase ties package name to the App ID, we need to create a separate app for the test runner to use.
    /// Go create an iOS app with com.UnityTestRunner.UnityTestRunner package name and put it here.
    /// </summary>
    protected abstract string iOSTestRunnerAppID {get;}

    protected string AppID
    {
        get
        {
#if UNITY_EDITOR
            switch (Application.platform)
            {
                case RuntimePlatform.IPhonePlayer: return iOSAppID;
                case RuntimePlatform.Android: return AndroidAppID;
            }
            throw new System.Exception("Editor PlayMode test must be running on iOS or Android!");
#elif UNITY_ANDROID
                return AndroidTestRunnerAppID;
#elif UNITY_IOS
                return iOSTestRunnerAppID;
#endif
        }
    }

    /// <summary>
    /// Enter this without the "gs://"
    /// The name of a bucket with (default) in the Firebase console. That information is in Google config file so we can compare with this.
    /// </summary>
    protected abstract string DefaultBucketName {get;}

    /// <summary>
    /// Enter this without the "gs://"
    /// The bucket you actually want to use in the test. Should be different from DefaultBucketName.
    /// </summary>
    protected abstract string BucketName {get;}

    /// <summary>
    /// https://something.firebaseio.com/
    /// </summary>
    protected abstract string DatabaseUrl { get;}

    /// <summary>
    /// With the new Desktop Workflow Implementation, we can use Auth with the real account instead of a service account.
    /// </summary>
    protected abstract string AccountIdentifier { get; }

    protected abstract string AccountPassword { get; }

    /// <summary>
    /// The test will check if UID from Firebase matches with this or not.
    /// </summary>
    /// <returns></returns>
    protected abstract string AccountUID { get; }

    private Task ValidWrite()
    {
        Debug.Log("Writing with correct data structure...");
        var toWrite = new { numberOnly = 5555 };
        return FirebaseDatabase.DefaultInstance.RootReference.Child("sandbox").SetRawJsonValueAsync(JsonConvert.SerializeObject(toWrite));
    }

    private Task InvalidWrite()
    {
        Debug.Log("Writing with wrong data structure...");
        var toWrite = new { impossible = "is possible", impossible2 = "is also possible" };
        return FirebaseDatabase.DefaultInstance.RootReference.Child("sandbox").SetRawJsonValueAsync(JsonConvert.SerializeObject(toWrite));
    }

    private void SetToCorrectUID()
    {
        Debug.Log("Set to correct UID");
        // FirebaseApp.DefaultInstance.SetEditorAuthUserId(correctUid);
        // Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(correctUid));
        // Assert.That(FirebaseDatabase.DefaultInstance.App.GetEditorAuthUserId(), Is.EqualTo(correctUid)); //also check what the DB see to be sure. you will see that it pass everytime, signifying that the sticky effect is not stored here since it change accordingly.
        // Assert.That(FirebaseAuth.DefaultInstance.App.GetEditorAuthUserId(), Is.EqualTo(correctUid));
    }

    private void SetToWrongUID()
    {
        Debug.Log("Set to wrong UID");
        // FirebaseApp.DefaultInstance.SetEditorAuthUserId(correctUid + "555");
        // Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(correctUid + "555"));
        // Assert.That(FirebaseDatabase.DefaultInstance.App.GetEditorAuthUserId(), Is.EqualTo(correctUid + "555"));
        //THIS!
        //FirebaseAuth fa = FirebaseAuth.DefaultInstance;
        //Assert.That(FirebaseAuth.DefaultInstance.App.GetEditorAuthUserId(), Is.EqualTo(correctUid + "555"));
    }

    private void SetToEmptyUID()
    {
        Debug.Log("Set to empty UID");
        FirebaseApp.DefaultInstance.SetEditorAuthUserId("");
    }

    private void TheWriteShouldPass(Task t)
    {
        Assert.That(t.IsFaulted, Is.Not.True);
    }

    private void TheWriteShouldFail(Task t)
    {
        Assert.That(t.IsFaulted);
    }

    // ===============================================================================
    // ===============================================================================

    [UnityTest]
    [UnityEditorPlatform]
    public IEnumerator T1_ConfigFileWereRead()
    {
        Assert.That(FirebaseApp.DefaultInstance.Options.StorageBucket, Is.EqualTo(DefaultBucketName), "Firebase in editor reads Google service files successfully.");
        Assert.That(FirebaseApp.DefaultInstance.Options.AppId, Is.EqualTo(AppID), "Firebase in editor reads Google service files successfully.");
        Assert.That(FirebaseApp.DefaultInstance.Options.ProjectId, Is.EqualTo(ProjectID), "Firebase in editor reads Google service files successfully.");
        yield break;
    }

    [UnityTest][UnityEditorPlatform]
    public IEnumerator TestDefaultOptionsEditor()
    {
        Assert.That(FirebaseApp.DefaultInstance.Options.StorageBucket, Is.Not.Null);
        Assert.That(FirebaseApp.DefaultInstance.Options.AppId, Is.Not.Null);
        Assert.That(FirebaseApp.DefaultInstance.Options.ProjectId, Is.Not.Null);

        Assert.That(FirebaseApp.DefaultInstance.Options.DatabaseUrl, Is.EqualTo(DatabaseUrl), "SetEditor___ methods works.");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorDatabaseUrl(), Is.EqualTo(DatabaseUrl), "SetEditor___ methods works.");

#if UNITY_EDITOR
        FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("gs://something/");
        Assert.That(FirebaseApp.DefaultInstance.Options.DatabaseUrl, Is.EqualTo(DatabaseUrl), "Not possible to set editor DB the second time.");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorDatabaseUrl(), Is.EqualTo(DatabaseUrl), "Not possible to set editor DB the second time.");
#else
        Assert.That(FirebaseApp.DefaultInstance.Options.DatabaseUrl, Is.EqualTo(DatabaseUrl), "In real device we do not need to fake, the DB URL should already be there.");
#endif
        yield break;
    }

    [UnityTest]
    public IEnumerator TestDepencency1_RetainsInstanceInformationInbetweenTests()
    {
        Debug.Log("The first one...");
        string original = ";askf;aleasdlkj";

        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(),Is.Null);

        FirebaseApp.DefaultInstance.SetEditorAuthUserId(original);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(original));

        yield break;
    }

    [UnityTest]
    public IEnumerator TestDependency2_FirebaseRetainsInstanceInformationInbetweenTests()
    {
        Debug.Log("The second one...");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.Null);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.Empty);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.TypeOf<string>());

        yield break;
    }

    [UnityTest]
    public IEnumerator TestDependency3_EvenDestroyingTheFirebaseObjectCannotRemoveTheInstance()
    {
        GameObject.Destroy(GameObject.Find("Firebase Services"));
        yield return null;
        FirebaseApp.DefaultInstance.Dispose();

        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(),Is.Not.Null);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.TypeOf<string>());

        yield break;
    }

    [UnityTest]
    public IEnumerator NormalUsage_ValidWriteWithCorrectUidIsValid()
    {
        SetToCorrectUID();
        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t);
    }

    [UnityTest]
    public IEnumerator NormalUsage_ValidWriteWithWrongUidIsInvalid()
    {
        SetToWrongUID();
        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);
    }

    [UnityTest]
    public IEnumerator NormalUsage_InValidWriteWithCorrectUidIsInvalid()
    {
        SetToCorrectUID();
        Task t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);
    }

    [UnityTest]
    public IEnumerator NormalUsage_InValidWriteWithWrongUidIsInvalid()
    {
        SetToWrongUID();
        Task t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);
    }

    [UnityTest]
    public IEnumerator NoUidIsSudo_CanValidWrite()
    {
        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t);
    }

    [UnityTest]
    public IEnumerator NoUidIsSudo_CanInvalidWrite()
    {
        Task t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t);
    }

    [UnityTest]
    public IEnumerator NoUidExplicitIsSudo_CanValidWrite()
    {
        SetToEmptyUID();
        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t);
    }

    [UnityTest]
    public IEnumerator NoUidExplicitIsSudo_CanInvalidWrite()
    {
        SetToEmptyUID();
        Task t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t);
    }
 
    [UnityTest]
    public IEnumerator SetUidMultipleTimes_CorrectToWrongRemainsCorrect()
    {
        yield return NormalUsage_ValidWriteWithCorrectUidIsValid();

        SetToWrongUID();
        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //Still pass even if now the UID is wrong??

        yield return NormalUsage_InValidWriteWithWrongUidIsInvalid(); //The old ones also still pass
    }

    [UnityTest]
    public IEnumerator SetUidMultipleTimes_WrongToCorrectRemainsWrong()
    {
        yield return NormalUsage_ValidWriteWithWrongUidIsInvalid();

        SetToCorrectUID();
        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); //Still fail even if we have corrected everything

        yield return NormalUsage_ValidWriteWithWrongUidIsInvalid(); //still fail, which is as expected
    }

    [UnityTest]
    public IEnumerator SetUidMultipleTimes_NotMakingACallThenYouCanStillChangeYourMind()
    {
        SetToEmptyUID();
        SetToCorrectUID();
        SetToWrongUID();
        SetToEmptyUID();
        SetToWrongUID();

        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); //This is the behaviour of wrong UID

        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);  //Now we can't correct it anymore..
    }

    [UnityTest]
    public IEnumerator SetUidMultipleTimes_NoUidToWrongRemainsSudo()
    {
        yield return NoUidIsSudo_CanInvalidWrite(); //so this means it does not really depends on how many times you set but on the first time you use it

        SetToWrongUID();
        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //Still has the sudo power from the first write even without explicit set?

        yield return NoUidIsSudo_CanInvalidWrite(); //sudo still can do everything
        yield return NoUidIsSudo_CanValidWrite(); //sudo still can do everything
    }

    [UnityTest]
    public IEnumerator SetUidMultipleTimes_NoUidExplicitToWrongRemainsSudo()
    {
        yield return NoUidExplicitIsSudo_CanInvalidWrite();

        SetToWrongUID();
        Task t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //Still has the sudo power from the first write even without explicit set?

        yield return NoUidIsSudo_CanInvalidWrite(); //sudo still can do everything
        yield return NoUidIsSudo_CanValidWrite(); //sudo still can do everything
    }

    [UnityTest]
    public IEnumerator CanChangeAuthManyTime() //Until you make the first DB call, then you can change it to no effect
    {
        string original = ";askf;aleasdlkj";
        string changeTo = "WASDFAEOKFEOKF";

        FirebaseApp.DefaultInstance.SetEditorAuthUserId(original);
        string rememberedUid = FirebaseApp.DefaultInstance.GetEditorAuthUserId();

        //You can change it as much as you like... 
        FirebaseApp.DefaultInstance.SetEditorAuthUserId(changeTo);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(changeTo));

        FirebaseApp.DefaultInstance.SetEditorAuthUserId(rememberedUid);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(rememberedUid));

        FirebaseApp.DefaultInstance.SetEditorAuthUserId("");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(""));

        FirebaseApp.DefaultInstance.SetEditorAuthUserId(rememberedUid);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(rememberedUid));

        yield break;
    }

    [UnityTest]
    public IEnumerator ForceSticky_WhatIsEnough()
    {
        Task t = null;

        //Just getting default instance is not enough to make it sticks. This is the wrong behaviour
        SetToCorrectUID();
        FirebaseDatabase fd = FirebaseDatabase.DefaultInstance;
        SetToWrongUID();

        t = ValidWrite(); // <-- Sticks here
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 
    }

    [UnityTest]
    public IEnumerator ForceSticky_WhatIsEnough2()
    {
        Task t = null;

        //Just getting root reference is not enough to make it sticks. This is the wrong behaviour
        SetToCorrectUID();
        DatabaseReference dr = FirebaseDatabase.DefaultInstance.RootReference;
        SetToWrongUID();

        t = ValidWrite(); // <-- Sticks here
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 
    }

    [UnityTest]
    public IEnumerator ForceSticky_WhatIsEnough3()
    {
        Task t = null;

        SetToCorrectUID();
        Task taskForStick = FirebaseDatabase.DefaultInstance.RootReference.GetValueAsync(); //<--- It sticks here! Just get some value for nothing
        yield return taskForStick.YieldWait();
        SetToWrongUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //Now pass

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        SetToWrongUID(); //How about to wrong?

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //Still pass

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 
    }

    [UnityTest]
    public IEnumerator ForceSticky_WhatIsEnough4()
    {
        Task t = null;

        SetToCorrectUID();
        //Task taskForStick = FirebaseDatabase.DefaultInstance.RootReference.GetValueAsync(); 
        FirebaseDatabase.DefaultInstance.RootReference.GetValueAsync(); 
        //yield return YieldWaitTest(taskForStick); <--- Missing the yield results in it does not stick
        SetToWrongUID();

        t = ValidWrite(); 
        yield return t.YieldWait(); //<--- Sticks here
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 
    }

    [UnityTest]
    public IEnumerator CheckMechanics_Register()
    {
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Null);

        string emailToRegister = "hi@yo.com";

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Not.Null);
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser.UserId,Is.EqualTo(emailToRegister));

        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Null);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Null);

        SetToCorrectUID();

        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister)); //<--- it forgets!
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Not.Null);
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser.UserId,Is.EqualTo(emailToRegister));

        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Null);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(AccountUID)); //<--- it remembers!
        yield break;
    }

    [UnityTest]
    public IEnumerator CheckMechanics_Signin()
    {
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Null);

        string emailToRegister = "hi@yo.com";

        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Not.Null);
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser.UserId,Is.EqualTo(emailToRegister));

        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Null);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Null);

        SetToCorrectUID();

        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister)); //<--- it forgets!
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Not.Null);
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser.UserId,Is.EqualTo(emailToRegister));

        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseAuth.DefaultInstance.CurrentUser,Is.Null);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(AccountUID)); //<--- it remembers!

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_RegisterCallOverwritesYourEditorAuth()
    {
        string emailToRegister = "hi@yo.com";

        SetToCorrectUID();
        string rememberedUid = FirebaseApp.DefaultInstance.GetEditorAuthUserId();

        //After the auth action it change to e-mail
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid));
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        //You can't change it anymore it will stay the same
        FirebaseApp.DefaultInstance.SetEditorAuthUserId(rememberedUid);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid)); 
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        //But sign out did restore it back!
        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(rememberedUid)); 

        //Now you can change it again
        SetToWrongUID();

        yield break;
    }

    /*
        Lesson from this :
        1. Sign in prevents the thing you set in EditorAuthUserId from sticking.
        2. You can sign out to restore the sticky effect you might have, or if you don't you can make your first call to make it stick
    */

    [UnityTest]
    public IEnumerator AuthInEditor_CallsWhileRegisterDoesNotCount()
    {
        //Because "the first write" was registered after the sign in
        Task t = null;
        string emailToRegister = "hi@yo.com";

        SetToWrongUID();
        SetToCorrectUID();


        string rememberedUid = FirebaseApp.DefaultInstance.GetEditorAuthUserId();

        //After the auth action it change to e-mail
        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid));
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        t = ValidWrite(); //<--- Does not have sticky effect. Does not count as "the first call"
        yield return t.YieldWait();
        TheWriteShouldFail(t); //It fails since using e-mail as UID is not making sense

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Try to change...");
        //You can't change it anymore it will stay the same
        FirebaseApp.DefaultInstance.SetEditorAuthUserId(rememberedUid);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid)); 
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Signing out...");
        //But sign out did restore it back!
        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(rememberedUid)); 

        t = ValidWrite(); //<----- The first call outside of sign in, has sticky effect
        yield return t.YieldWait();
        TheWriteShouldPass(t);  

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        //Now you can change it again
        Debug.Log("Try to change 2...");
        SetToWrongUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //The correct UID still sticks... what should fail still pass

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_CallsWhileSignInDoesNotCount() //is the same as register
    {
        //Because "the first write" was registered after the sign in
        Task t = null;
        string emailToRegister = "hi@yo.com";

        SetToWrongUID();
        SetToCorrectUID();

        string rememberedUid = FirebaseApp.DefaultInstance.GetEditorAuthUserId();

        //After the auth action it change to e-mail
        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.SignInWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid));
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        t = ValidWrite(); //<--- Does not have sticky effect. Does not count as "the first call"
        yield return t.YieldWait();
        TheWriteShouldFail(t); //It fails since using e-mail as UID is not making sense

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Try to change...");
        //You can't change it anymore it will stay the same
        FirebaseApp.DefaultInstance.SetEditorAuthUserId(rememberedUid);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid)); 
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Signing out...");
        //But sign out did restore it back!
        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(rememberedUid)); 

        t = ValidWrite(); //<----- The first call outside of sign in, has sticky effect
        yield return t.YieldWait();
        TheWriteShouldPass(t);  

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        //Now you can change it again
        Debug.Log("Try to change 2...");
        SetToWrongUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //The correct UID still sticks... what should fail still pass

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_CallsWhileRegisterDoesNotCount2()
    {
        Task t = null;
        string emailToRegister = "hi@yo.com";

        SetToWrongUID();
        SetToCorrectUID();

        string rememberedUid = FirebaseApp.DefaultInstance.GetEditorAuthUserId();

        //After the auth action it change to e-mail
        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid));
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); //It fails since using e-mail as UID is not making sense

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Try to change...");
        //You can't change it anymore it will stay the same
        FirebaseApp.DefaultInstance.SetEditorAuthUserId(rememberedUid);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid)); 
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Signing out...");
        //But sign out did restore it back!
        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(rememberedUid)); 

        SetToWrongUID(); // <---- We "change the mind" here and it will be in effect since we haven't made the first write

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);  

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        //Now you can change it again
        Debug.Log("Try to change 2...");
        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); //The fail sticks

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_CallsWhileRegisterDoesNotCount3()
    {
        Task t = null;
        string emailToRegister = "hi@yo.com";

        //Have it sticks to wrong first

        SetToWrongUID();

        t = ValidWrite(); //<----- Now we try to write before the register to have it stick to wrong
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        SetToCorrectUID();//<---- no effect

        t = ValidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t); //<---- same

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        string rememberedUid = FirebaseApp.DefaultInstance.GetEditorAuthUserId();

        //After the auth action it change to e-mail
        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid));
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));


        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); //It fails since using e-mail as UID is not making sense

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Try to change...");
        //You can't change it anymore it will stay the same
        FirebaseApp.DefaultInstance.SetEditorAuthUserId(rememberedUid);
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.Not.EqualTo(rememberedUid)); 
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(emailToRegister));

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Signing out...");
        //But sign out did restore it back!
        FirebaseAuth.DefaultInstance.SignOut();
        Assert.That(FirebaseApp.DefaultInstance.GetEditorAuthUserId(), Is.EqualTo(rememberedUid)); 

        //it remembers the wrong effect
        SetToCorrectUID(); //no effect

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);  

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        SetToCorrectUID(); //no effect

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);  

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        Debug.Log("Try to change 2...");
        SetToWrongUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); 

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_CallsWhileRegisterDoesNotCount4()
    {
        Task t = null;
        string emailToRegister = "hi@yo.com";

        //Have it sticks to correct first

        SetToCorrectUID();

        t = ValidWrite(); //<----- Now we try to write before the register to have it stick to CORRECT
        yield return t.YieldWait();
        TheWriteShouldPass(t);

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        SetToWrongUID();

        t = ValidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldPass(t); //<---- No effect, it sticks

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Debug.Log("IMMEDIATELY Signing out...");
        FirebaseAuth.DefaultInstance.SignOut();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); //<--- Now it correctly fails!? WHY! Somehow it can remembers the set to wrong from before the login and correct itself after the sign out

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Try to change 2...");

        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);  //<--- and stick again too

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_CallsWhileRegisterDoesNotCount5()
    {
        Task t = null;
        string emailToRegister = "hi@yo.com";

        //Have it sticks to correct first

        SetToCorrectUID();

        t = ValidWrite(); //<----- Now we try to write before the register to have it stick to CORRECT
        yield return t.YieldWait();
        TheWriteShouldPass(t);

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        // SetToWrongUID();

        // t = ValidWrite(); 
        // yield return t.YieldWait();
        // TheWriteShouldPass(t); //<---- No effect, it sticks

        // t = InvalidWrite(); 
        // yield return t.YieldWait();
        // TheWriteShouldFail(t);

        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Debug.Log("IMMEDIATELY Signing out...");
        FirebaseAuth.DefaultInstance.SignOut();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //<--- We commented that out, now it remembers the CORRECT

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Try to change 2...");

        SetToWrongUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t);  //<--- and stick again too

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_CallsWhileRegisterDoesNotCount6()
    {
        Task t = null;
        string emailToRegister = "hi@yo.com";

        //Have it sticks to correct first

        SetToWrongUID();

        t = ValidWrite(); //<----- Now we try to write before the register to have it stick to WRONG 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        SetToCorrectUID();

        t = ValidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t); //<---- No effect, it sticks

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        //After the auth action it change to e-mail
        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Debug.Log("IMMEDIATELY Signing out...");
        FirebaseAuth.DefaultInstance.SignOut();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); //<---- BUT with correct before the sign in, after the signin it did not turn itself to correct like when I did it with wrong

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Try to change 2...");

        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);  //Still wrong

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_Poke1() //This is the same as DoesNotCount4, "poking" does nothing
    {
        FirebaseAuth useless = FirebaseAuth.DefaultInstance; // <---- What will happen if we just add this!!!!
        Task t = null;
        string emailToRegister = "hi@yo.com";

        //Have it sticks to correct first

        SetToCorrectUID();

        t = ValidWrite(); //<----- Now we try to write before the register to have it stick to CORRECT
        yield return t.YieldWait();
        TheWriteShouldPass(t);

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        SetToWrongUID();

        t = ValidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldPass(t); //<---- No effect, it sticks

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Debug.Log("IMMEDIATELY Signing out...");
        FirebaseAuth.DefaultInstance.SignOut();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t); //<--- Now it correctly fails!? WHY! Somehow it can remembers the set to wrong from before the login and correct itself after the sign out

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Try to change 2...");

        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);  //<--- and stick again too

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_Poke2() //This is the same as DoesNotCount5, poking does nothing
    {
        FirebaseAuth useless = FirebaseAuth.DefaultInstance; // <---- What will happen if we just add this!!!!
        Task t = null;
        string emailToRegister = "hi@yo.com";

        //Have it sticks to correct first

        SetToCorrectUID();

        t = ValidWrite(); //<----- Now we try to write before the register to have it stick to CORRECT
        yield return t.YieldWait();
        TheWriteShouldPass(t);

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        // SetToWrongUID();

        // t = ValidWrite(); 
        // yield return t.YieldWait();
        // TheWriteShouldPass(t); //<---- No effect, it sticks

        // t = InvalidWrite(); 
        // yield return t.YieldWait();
        // TheWriteShouldFail(t);

        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Debug.Log("IMMEDIATELY Signing out...");
        FirebaseAuth.DefaultInstance.SignOut();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //<--- We commented that out, now it remembers the CORRECT

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Try to change 2...");

        SetToWrongUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t);  //<--- and stick again too

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_Poke3() //This is the same as DoesNotCount6, but now...!
    {
        FirebaseAuth useless = FirebaseAuth.DefaultInstance; // <---- What will happen if we just add this!!!!
        Task t = null;
        string emailToRegister = "hi@yo.com";

        //Have it sticks to correct first

        SetToWrongUID();

        t = ValidWrite(); //<----- Now we try to write before the register to have it stick to WRONG 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        SetToCorrectUID();

        t = ValidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t); //<---- No effect, it sticks

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        //After the auth action it change to e-mail
        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Debug.Log("IMMEDIATELY Signing out...");
        FirebaseAuth.DefaultInstance.SignOut();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t); //<--- This pass now! Why is that??? Compare this with DoesNotCount6 the difference is just the first line

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Try to change 2...");

        SetToWrongUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldPass(t);  //It sticks too!

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        yield break;
    }

    [UnityTest]
    public IEnumerator AuthInEditor_Poke4() 
    {
        Task t = null;
        string emailToRegister = "hi@yo.com";

        //Have it sticks to correct first

        SetToWrongUID();

        t = ValidWrite(); //<----- Now we try to write before the register to have it stick to WRONG 

        FirebaseAuth useless = FirebaseAuth.DefaultInstance; // <---- Moved this magic line down. Here is the first point where the behaviour returns to normal
        //Actually I don't know what is normal anymore.. orz
        //That means, the poke change the behaviour if called before the first DB call. And the effect is only after the signout.
        //Previously, Auth has been poked the first time on sign in it might be that it remembers something there...

        yield return t.YieldWait();
        TheWriteShouldFail(t);

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);


        SetToCorrectUID();

        t = ValidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t); //<---- No effect, it sticks

        t = InvalidWrite(); 
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        //After the auth action it change to e-mail
        Debug.Log("Registering...");
        FirebaseAuth.DefaultInstance.CreateUserWithEmailAndPasswordAsync(emailToRegister, "sdfsfsfk");
        Debug.Log("IMMEDIATELY Signing out...");
        FirebaseAuth.DefaultInstance.SignOut();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);// <--- Did not correct itself 

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        Debug.Log("Try to change 2...");

        SetToCorrectUID();

        t = ValidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);  //Still wrong

        t = InvalidWrite();
        yield return t.YieldWait();
        TheWriteShouldFail(t);

        yield break;

    }
} 
