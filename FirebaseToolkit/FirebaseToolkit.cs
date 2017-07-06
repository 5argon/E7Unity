/* 
using Firebase.Storage;
using System;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;


public abstract class FirebaseToolkit<ITSELF> where ITSELF : FirebaseToolkit<ITSELF>, new()
{
    /// <summary>
    /// Format : "gs://my-custom-bucket"
    /// </summary>
    protected abstract string BucketName { get; }

    //All the C# abstract cannot be static so those are useless to the abstract methods.. because of this we have to make a singleton of AWSToolkit just for accessing those abstracts. lol
    //But this is internal use singleton, so it is private.
    private static ITSELF instance;
    private static ITSELF Instance
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

    private static FirebaseStorage storage;
    protected static FirebaseStorage Storage 
    { 
        get 
        {
            if(storage == null)
            {
                storage = FirebaseStorage.GetInstance(Instance.BucketName);
            }
            return storage;
        }
    }

    /// <summary>
    /// You cannot store files in Firebase root. Folder argument is a must.
    /// </summary>
    public static void UploadFromPersistent(string localFileName, string folder, string destinationFileName, Action<Task<StorageMetadata>> onSuccess = null, Action<Task<StorageMetadata>> onFailure = null)
    {
        StorageReference uploadReference = Storage.RootReference.Child(folder).Child(destinationFileName);

        using (FileStream stream = new FileStream(Application.persistentDataPath + Path.DirectorySeparatorChar + localFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
        {
            uploadReference.PutStreamAsync(stream).ContinueWith((Task<StorageMetadata> task) =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                if (onFailure == null)
                {
                    DefaultFailure("Upload " + localFileName,task);
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
        });

        }
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
*/