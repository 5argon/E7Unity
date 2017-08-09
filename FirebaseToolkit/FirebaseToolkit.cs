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
#if UNITY_EDITOR || DEVELOPMENT_BUILD
using Firebase.Unity.Editor;
#endif

/// <summary>
/// A small toolkit I made for common operations.
/// Currently it can download-upload from persistent data path. 
/// </summary>
/// <returns></returns>
public abstract class FirebaseToolkit<ITSELF> where ITSELF : FirebaseToolkit<ITSELF>, new()
{
    private static readonly string defaultInstanceName = "default";

    /// <summary>
    /// Format : "gs://my-custom-bucket"
    /// For storage you can't set a new default from Firebase. I think setting here is easier than changing the config file manually.
    /// </summary>
    protected abstract string BucketName { get; }


//I will really make sure all of these does not leak out of your computer..!
#if UNITY_EDITOR
    /// <summary>
    /// Format : "https://yourgame.firebaseio.com/"
    /// DatabaseUrl can't be read off the config file in editor. You must provide the string for editor use.
    /// </summary>
    protected abstract string DatabaseUrl { get; }

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

#if UNITY_EDITOR
    private static string currentInstanceName = defaultInstanceName;
    private static List<string> createdInstanceList = new List<string>() { defaultInstanceName };
    private static void SwitchInstance(string toName)
    {
        if (createdInstanceList.Contains(toName) == false)
        {
            Debug.Log("Created new FirebaseApp instance named " + toName);
            FirebaseApp.Create(DefaultFirebaseOption, toName);
            createdInstanceList.Add(toName);
        }

        if (currentInstanceName == toName)
        {
            throw new Exception("We should not have switch to the same instance! Stupid! (" + currentInstanceName + "->" + toName + ")");
        }

        currentInstanceName = toName;

        //Reset cache so all of them gets a new one from the ground up
        currentFirebaseApp = null;
        database = null;
        storage = null;
// #if !UNITY_EDITOR
//         auth = null;
// #endif
    }
#endif

    private static FirebaseApp currentFirebaseApp;
    public static FirebaseApp CurrentFirebaseApp
    {
        get
        {
            if (currentFirebaseApp == null)
            {
#if UNITY_EDITOR
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
#if UNITY_EDITOR
                if (IsLoggedIn == false)
                {
                    throw new System.Exception("No! You must login before using the database in editor!");
                }
                CurrentFirebaseApp.SetEditorDatabaseUrl(Instance.DatabaseUrl);
                database = FirebaseDatabase.GetInstance(CurrentFirebaseApp);
#else
                database = FirebaseDatabase.GetInstance(CurrentFirebaseApp);
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
                storage = FirebaseStorage.GetInstance(CurrentFirebaseApp);
            }
            return storage;
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// Call this from your Login in Editor instead of Auth. Maybe use #if UNITY_EDITOR to help.
    /// </summary>
    protected static void EditorLogin(string username, string password, string uid)
    {
        //In editor... to support freely switching user, each username would need a separate Firebase instance
        //since you cannot take it out once you made the first DB call.

        //In the real game we would need only one instance.
        SwitchInstance(username); //Every CurrentFirebaseApp will change!

        CurrentFirebaseApp.SetEditorP12FileName(Instance.P12FileName);
        CurrentFirebaseApp.SetEditorServiceAccountEmail(Instance.ServiceAccountEmail);
        CurrentFirebaseApp.SetEditorP12Password("notasecret");
        CurrentFirebaseApp.SetEditorAuthUserId(uid);

        Debug.LogFormat("EDITOR login as {0}", uid);
    }
#endif

    public static void Logout()
    {
#if UNITY_EDITOR
        SwitchInstance(defaultInstanceName);
#else
        Auth.SignOut();
#endif
    }

    public static string LoginInformation
    {
        get{
            string info = 
            @"UserId : {0}
            E-mail : {1}
            PlayerId : {2}
            FormattedPlayerId : {3}";

            #if UNITY_EDITOR
            return string.Format(info,
            CurrentFirebaseApp.GetEditorAuthUserId(),
            currentInstanceName,
            PlayerData.Local.PlayerId,
            PlayerData.Local.FormattedShortPlayerId);
            #else
            return string.Format(info,
            Auth.CurrentUser.Email,
            Auth.CurrentUser.UserId,
            PlayerData.Local.PlayerId,
            PlayerData.Local.FormattedShortPlayerId);
            #endif
        }
    }


    public static bool IsLoggedIn
    {
        get
        {
#if UNITY_EDITOR
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
#if UNITY_EDITOR
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
#if UNITY_EDITOR
            return CurrentFirebaseApp.GetEditorAuthUserId();
#else
            return Auth.CurrentUser.UserId;
#endif
        }
    }

    /// <summary>
    /// You cannot store files in Firebase root. Folder argument is a must.
    /// </summary>
    public static void UploadFromPersistent(string localFileName, string folder, string destinationFileName, Action<Task<StorageMetadata>> onSuccess = null, Action<Task<StorageMetadata>> onFailure = null)
    {
        StorageReference uploadReference = Storage.RootReference.Child(folder).Child(destinationFileName);

        FileStream stream = new FileStream(Application.persistentDataPath + Path.DirectorySeparatorChar + localFileName, FileMode.Open, FileAccess.Read, FileShare.Read);
        uploadReference.PutStreamAsync(stream).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                if (onFailure == null)
                {
                    DefaultFailure("Upload " + localFileName, task);
                }
                else
                {
                    onFailure.Invoke(task);
                }
            }
            else
            {
                if (onSuccess == null)
                {
                    DefaultUploadSuccess(task);
                }
                else
                {
                    onSuccess.Invoke(task);
                }
            }
            //Cannot use "using" since it is based on callback. The stream will already be disposed by then.
            stream.Dispose();
        });

    }

    private static void DefaultFailure(string fileName, Task task)
    {
        Debug.LogError("Firebase Failure : " + fileName);
        Debug.LogError(task.Exception.ToString());
    }

    private static void DefaultUploadSuccess(Task<StorageMetadata> task)
    {
        Debug.Log("Firebase Upload Success : " + task.Result.Name);
    }

    private static void DefaultDownloadSuccess(string fileName, Task task)
    {
        Debug.Log("Firebase Download Success : " + fileName);
    }

    public static void DownloadToPersistent(string folder, string storageFileName, string saveFileName, Action<Task> onSuccess = null, Action<Task> onFailure = null)
    {
        StorageReference downloadReference = Storage.RootReference.Child(folder).Child(storageFileName);
        downloadReference.GetFileAsync(Application.persistentDataPath + Path.DirectorySeparatorChar + saveFileName).ContinueWith(
(Task task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                if (onFailure == null)
                {
                    DefaultFailure("Download " + downloadReference.Path, task);
                }
                else
                {
                    onFailure.Invoke(task);
                }
            }
            else
            {
                if (onSuccess == null)
                {
                    DefaultDownloadSuccess("Download " + downloadReference.Path, task);
                }
                else
                {
                    onSuccess.Invoke(task);

                }
            }
        }
        );
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
        if(task.IsFaulted)
        {
            throw task.Exception;
        }
    }
}