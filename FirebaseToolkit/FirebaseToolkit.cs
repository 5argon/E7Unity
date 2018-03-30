using Firebase;
using Firebase.Storage;
using Firebase.Database;
using Firebase.Auth;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif
#if UNITY_EDITOR || UNITY_STANDALONE
using Firebase.Unity.Editor;
#endif

/// <summary>
/// A small toolkit I made for common operations.
/// Currently it can download-upload from persistent data path. 
/// REQUIRES : Firebase Unity SDK 4.5.0 It assumes Desktop Workflow is usable.
/// </summary>
/// <returns></returns>
public abstract class FirebaseToolkit<ITSELF> where ITSELF : FirebaseToolkit<ITSELF>, new()
{
    /// <summary>
    /// Format : "gs://my-custom-bucket"
    /// Bucket name from the config file is the one with (default) in Firebase. You cannot change it in the console.
    /// This setting can make default bucket becomes something else. Even if you are using your default please put it here.
    /// </summary>
    protected abstract string BucketName { get; }

    /// <summary>
    /// Format : "https://yourgame.firebaseio.com/"
    /// DatabaseUrl can't be read off the config file in editor. You must provide the string for editor use.
    /// </summary>
    protected abstract string DatabaseUrl { get; }

#if UNITY_EDITOR || UNITY_STANDALONE

    /// <summary>
    /// Format : "askfjsdafkj.p12"
    /// </summary>
    protected abstract string P12FileName { get; }

    /// <summary>
    /// Format : "firebase-adminsdk-mpvab@your-game.iam.gserviceaccount.com"
    /// </summary>
    protected abstract string ServiceAccountEmail { get; }

    /// <summary>
    /// In editor you can't use Auth, therefore you can't get UID from Firebase starting from e-mail and password.
    /// This dict will transform e-mail to UID. It indicates all the possible login you can do from the editor.
    /// </summary>
    public abstract Dictionary<string, string> EditorEmailToUID { get; }
#endif

    //All the C# abstract cannot be static so those are useless to the abstract methods.. because of this we have to make a singleton of FirebaseToolkit just for accessing those abstracts. lol
    protected static ITSELF instance;
    protected static ITSELF Instance
    {
        get
        {
            if (instance == null)
            {
                instance = new ITSELF();
            }
            return instance;
        }
    }



//When doing a PlayMode test and real device PlayMode test, the test use this to check the Firebase configuration variables.
#if UNITY_EDITOR || DEVELOPMENT_BUILD
    public static ITSELF TestFirebaseApp => Instance;

    public static int instanceCount = 1;

    public static void TestSetUp()
    {
        //Apparently currentFirebaseApp.Dispose() crashes Unity, so we are instantiating a new one...
        instanceCount++;
        currentFirebaseApp = FirebaseApp.Create(FirebaseApp.DefaultInstance.Options, firebaseToolkitInstanceName + instanceCount.ToString());

        auth = null;
        database = null;
        storage = null;
    }

#endif

    public const string firebaseToolkitInstanceName = "FirebaseToolkit-Instance";

    private static FirebaseApp currentFirebaseApp;
    public static FirebaseApp CurrentFirebaseApp
    {
        get
        {
            if (currentFirebaseApp == null)
            {
                currentFirebaseApp = FirebaseApp.Create(FirebaseApp.DefaultInstance.Options, firebaseToolkitInstanceName);
            }
            return currentFirebaseApp;
        }
    }

    private static FirebaseAuth auth;
    public static FirebaseAuth Auth 
    { 
        get 
        {
            if(auth == null)
            {
                auth = FirebaseAuth.GetAuth(CurrentFirebaseApp);
            }
            return auth;
        }
    }

    private static FirebaseDatabase database;
    public static FirebaseDatabase Database
    {
        get
        {
            if (database == null)
            {
                if (IsSignedIn == false)
                {
                    throw new Exception("No! You must login before using the database in editor!");
                }
                database = FirebaseDatabase.GetInstance(CurrentFirebaseApp,Instance.DatabaseUrl);
            }

            return database;
        }
    }

    private static FirebaseStorage storage;
    public static FirebaseStorage Storage
    {
        get
        {
            if (storage == null)
            {
                storage = FirebaseStorage.GetInstance(CurrentFirebaseApp,Instance.BucketName);
            }
            return storage;
        }
    }

#if UNITY_EDITOR || UNITY_STANDALONE
    /// <summary>
    /// Call this from your Login in Editor instead of Auth. Maybe use #if UNITY_EDITOR to help.
    /// </summary>
    protected static void EditorLogin(string username, string password, string uid)
    {
    }
#endif

    public static bool IsSignedIn => Auth.CurrentUser != null && Auth.CurrentUser.UserId != "";

    public static bool IsSignedInAndVerified => IsSignedIn && Auth.CurrentUser.IsEmailVerified;

    protected static string UserEmail => Auth.CurrentUser.Email;

    protected static string UserID
    {
        get
        {
            if (IsSignedIn == false)
            {
                throw new Exception("Cannot get ID while not logged in");
            }
            return Auth.CurrentUser.UserId;
        }
    }

    /// <summary>
    /// You cannot store files in Firebase root. Folder argument is a must.
    /// </summary>
    protected static Task UploadFromPersistent(string localFileName, string firebaseFolder, string firebaseFileName)
    {
        return UploadCommon(Application.persistentDataPath + Path.DirectorySeparatorChar + localFileName, firebaseFolder,firebaseFileName);
    }

#if UNITY_EDITOR
    protected static Task UploadFromAssetFolder(string pathFromAssetsFolder, string firebaseFolder, string firebaseFileName)
    {
        return UploadCommon(Application.dataPath + Path.DirectorySeparatorChar + pathFromAssetsFolder, firebaseFolder,firebaseFileName);
    }
#endif

#if UNITY_STANDALONE
    protected static Task UploadFromAppLocation(string pathFromAppLocation, string firebaseFolder, string firebaseFileName)
    {
        return UploadCommon(Path.GetFullPath(".") + Path.DirectorySeparatorChar + pathFromAppLocation, firebaseFolder, firebaseFileName);
    }
#endif

    private static Task UploadCommon(string uploadFromPath, string firebaseFolder, string firebaseFileName)
    {
        StorageReference uploadReference = Storage.RootReference.Child(firebaseFolder).Child(firebaseFileName);

        FileStream stream = new FileStream(uploadFromPath, FileMode.Open, FileAccess.Read, FileShare.Read);
        return uploadReference.PutStreamAsync(stream).ContinueWith(uploadTask => { stream.Close(); });
    }

    protected static Task DownloadToPersistent(string firebaseFolder, string firebaseFileName, string fileNameToSave)
    {
        return DownloadCommon(firebaseFolder, firebaseFileName, Application.persistentDataPath + Path.DirectorySeparatorChar + fileNameToSave);
    }

#if UNITY_EDITOR
    protected static Task DownloadToAssetFolder(string firebaseFolder, string firebaseFileName, string fileNameToSaveFromAssetFolder)
    {
        Task t = DownloadCommon(firebaseFolder, firebaseFileName,Application.dataPath + Path.DirectorySeparatorChar + fileNameToSaveFromAssetFolder);
        return t;
    }
#endif

#if UNITY_STANDALONE
    protected static Task DownloadToAppLocation(string firebaseFolder, string firebaseFileName, string fileNameToSaveFromAppLocation)
    {
        return DownloadCommon(firebaseFolder, firebaseFileName, Path.GetFullPath(".")+ Path.DirectorySeparatorChar + fileNameToSaveFromAppLocation);
    }
#endif

    private static Task DownloadCommon(string firebaseFolder, string firebaseFileName, string destination)
    {
        StorageReference downloadReference = Storage.RootReference.Child(firebaseFolder).Child(firebaseFileName);
        return downloadReference.GetFileAsync(destination);
    }
}

public static class TaskExtension
{
    /// <summary>
    /// Firebase Task might not play well with Unity's Coroutine workflow. You can now yield on the task with this. Useful in a test.
    /// </summary>
    public static IEnumerator YieldWait(this Task task)
    {
        while (task.IsCompleted == false)
        {
            yield return null;
        }
    }
}