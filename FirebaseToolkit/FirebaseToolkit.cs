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
/// </summary>
/// <returns></returns>
public abstract class FirebaseToolkit<ITSELF> where ITSELF : FirebaseToolkit<ITSELF>, new()
{

    /// <summary>
    /// Format : "gs://my-custom-bucket"
    /// For storage you can't set a new default from Firebase. I think setting here is easier than changing the config file manually.
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

#if UNITY_EDITOR || UNITY_STANDALONE
    private static readonly string defaultInstanceName = "default";
    private static string currentInstanceName = defaultInstanceName;
    protected static string CurrentInstanceName { get { return currentInstanceName; } }
    private static List<string> createdInstanceList = new List<string>() { defaultInstanceName };

    /// <returns>false means the instance is already created.</returns>
    private static bool SwitchInstance(string toName)
    {
        bool isAlreadyCreated = createdInstanceList.Contains(toName);
        if(currentInstanceName == defaultInstanceName && toName == defaultInstanceName)
        {
            return false;
        }
        if (!isAlreadyCreated)
        {
            //Debug.Log("Created new FirebaseApp instance named " + toName);
            FirebaseApp.Create(DefaultFirebaseOption, toName);
            createdInstanceList.Add(toName);
        }

        if (currentInstanceName == toName)
        {
            //This helps you code your UI better so it should not lead to double login
            throw new Exception("We should not have switch to the same instance! Stupid! (" + currentInstanceName + "->" + toName + ")");
        }

        currentInstanceName = toName;

        //Reset cache so all of them gets a new one from the ground up
        currentFirebaseApp = null;
        database = null;
        storage = null;
        return !isAlreadyCreated;
    }
#endif

    private static FirebaseApp currentFirebaseApp;
    public static FirebaseApp CurrentFirebaseApp
    {
        get
        {
            if (currentFirebaseApp == null)
            {
#if UNITY_EDITOR || UNITY_STANDALONE
                currentFirebaseApp = FirebaseApp.GetInstance(currentInstanceName);
#else
                currentFirebaseApp = FirebaseApp.DefaultInstance;
#endif
            }
            return currentFirebaseApp;
        }
    }

    private static AppOptions DefaultFirebaseOption
    {
        get { return FirebaseApp.DefaultInstance.Options; }
    }

    //I prevent the use of Auth in the editor since the state after using it is very unpredictable
    //See my research : https://gametorrahod.com/unity-firebase-realtime-database-gotchas-4359ff576026
#if !UNITY_EDITOR
    private static FirebaseAuth auth;
    public static FirebaseAuth Auth 
    { 
        get 
        {
            if(auth == null)
            {
                auth = FirebaseAuth.DefaultInstance;
            }
            return auth;
        }
    }
#endif

    private static FirebaseDatabase database;
    public static FirebaseDatabase Database
    {
        get
        {
            if (database == null)
            {
                if (IsLoggedIn == false)
                {
                    throw new Exception("No! You must login before using the database in editor!");
                }
#if UNITY_EDITOR || UNITY_STANDALONE
                CurrentFirebaseApp.SetEditorDatabaseUrl(Instance.DatabaseUrl);
                database = FirebaseDatabase.GetInstance(CurrentFirebaseApp);
#else
                database = FirebaseDatabase.GetInstance(CurrentFirebaseApp,Instance.DatabaseUrl);
#endif
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
        //In editor... to support freely switching user, each username would need a separate Firebase instance
        //since you cannot take it out once you made the first DB call.

        //In the real game we would need only one instance.
        bool switchCreatesNewInstance = SwitchInstance(username); //Every CurrentFirebaseApp will change!

        if (switchCreatesNewInstance)
        {
            CurrentFirebaseApp.SetEditorP12FileName(Instance.P12FileName);
            CurrentFirebaseApp.SetEditorServiceAccountEmail(Instance.ServiceAccountEmail);
            CurrentFirebaseApp.SetEditorP12Password("notasecret");
            CurrentFirebaseApp.SetEditorAuthUserId(uid);
        }

        Debug.LogFormat("EDITOR login as {0}", uid);
    }
#endif

    public static void Logout()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        SwitchInstance(defaultInstanceName);
#else
        Auth.SignOut();
#endif
    }


    public static bool IsLoggedIn
    {
        get
        {
#if UNITY_EDITOR || UNITY_STANDALONE
            if (currentInstanceName != defaultInstanceName)
            {
                return true;
            }
            else
            {
                //We have made sure the default ones can't have login information even in editor.
                return false;
            }
#else
            return (Auth.CurrentUser != null && Auth.CurrentUser.UserId != "");
#endif
        }
    }

    protected static string UserEmail
    {
        get
        {
            if (IsLoggedIn == false)
            {
                throw new Exception("Cannot get E-mail while not logged in");
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            return CurrentFirebaseApp.Name; //The instance name is an e-mail
#else
            return Auth.CurrentUser.Email;
#endif
        }
    }

    protected static string UserID
    {
        get
        {
            if (IsLoggedIn == false)
            {
                throw new Exception("Cannot get ID while not logged in");
            }
#if UNITY_EDITOR || UNITY_STANDALONE
            return CurrentFirebaseApp.GetEditorAuthUserId();
#else
            return Auth.CurrentUser.UserId;
#endif
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
    /// Firebase Task might not play well with Unity's Coroutine workflow. You can now yield on the task with this.
    /// </summary>
    public static IEnumerator YieldWait(this Task task)
    {
        while (task.IsCompleted == false)
        {
            yield return null;
        }
    }
}