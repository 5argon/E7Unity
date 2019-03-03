//#define USE_DEFAULT_INSTANCE

using Firebase.Storage;
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

public abstract class FirebaseToolkit<ITSELF> where ITSELF : FirebaseToolkit<ITSELF>, new()
{

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
        StorageReference uploadReference = FirebaseStorage.DefaultInstance.RootReference.Child(firebaseFolder).Child(firebaseFileName);

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
        StorageReference downloadReference = FirebaseStorage.DefaultInstance.RootReference.Child(firebaseFolder).Child(firebaseFileName);
        return downloadReference.GetFileAsync(destination);
    }
}