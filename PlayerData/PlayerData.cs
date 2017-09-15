using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

/* 
A class to make a save file system! The whole class will be binary serialized by Google Protobuf + additional encryption and compression

How to create your own data for your game is to write your own .proto file.

This comes with a base .proto file too. Generated class will be "partial", so it will merge nicely with your game's. (As long as you also name the class PlayerData)

Basics are PlayerData.Local.Save and PlayerData.Load
Local means it will load the file in your device.

It's a partial not inheritance, because I want the custom data you want
to include serialized together with basic data in this class by protobuf.
You can edit them if they are useless to you.

Include your game-specific things in the other side of the partial.

You should move commented varialbes to your partial then define those.

Note that if a hacker gain access of your key and iv they probably can
hack your save file. And as you see below, they are plain text in your code.
(hackable)

So change it to something else like server query if you care about security.

*/

public partial class PlayerData {

    //implement this on your partial class

    //public static readonly string playerDataFileName  = "SaveFile.sav";
    //private static readonly byte[] key = Encoding.ASCII.GetBytes("EightChr");
    //private static readonly byte[] iv = Encoding.ASCII.GetBytes("EightChr");

    //implement this on your partial class

    //private readonly ulong shortenAlgorithmX;
    //private readonly ulong shortenAlgorithmM;

    public static readonly int MaxDisplayNameLength = 10; 
    public static readonly string backupSuffix = ".backup";
    public static readonly string defaultNamePrefix = "Player";
    private static PlayerData local;

    //Something you might want to know

    //it fetches only once and cache it. Even if you modify your save and save it, this local is the one that you fetches earilier.

    //But of course if you have modify your local before saving, your local must also be current. So there's no need to call the expensive .Load everytime you want your data.

    //There is a LocalReload in case if you want to revert to the binary file, or just have replaced the save via backup.
    public static PlayerData Local
    {
        get
        {
            try
            {
                if(local == null)
                {
                    //Load from binary
                    local = PlayerData.Load();
                }
            }
            catch(CryptographicException ce1)
            {
                Debug.LogError(ce1);
                Debug.LogWarning("Possible old save data or corrupt save data found, trying to migrate.");
                try
                {
                    local = Migration();
                    local.Save(); //after the migration it should overwrite the old save immediately.
                    Debug.Log("Migration complete");
                }
                catch(CryptographicException ce2)
                {
                    Debug.LogError(ce2);
                    local = new PlayerData(); //you get an empty save if migration also throws crypto
                    Debug.Log("Could not migrate. Creating a new save file.");
                }
            }
            return local;
        }
    }

// These guys are in protobuf
/* 
    private string displayName;
    private string playerId; //GUID string
    private int playerIdHash;
    private string shortPlayerId; //Short form of that GUID string
    private string email;
*/


/// <summary>
/// Converts GUID to more readable name using a simple algorithm.
/// Modify it depending on your game's format.
/// Such case is unlikely, but the real GUID should be kept just for that.
/// </summary>
/// <returns></returns>
    public string FormattedShortPlayerId
    {
        get 
        { 
            if(string.IsNullOrEmpty(PlayerId))
            {
                return "???-???-???";
            }
            else
            {
                return
                ShortPlayerId.Substring(0,3) +
                "-" +
                ShortPlayerId.Substring(3,3) +
                "-" +
                ShortPlayerId.Substring(6,3)
                ; 
            }
        }
    }

    public string DisplayNameString
    {
        get 
        { 
            if(DisplayName == null || DisplayName == "")
            {
                return "???";
            }
            else
            {
                return DisplayName; 
            }
        }
        set
        {
            if(value.Length > 0 && value.Length <= MaxDisplayNameLength)
            {
                DisplayName = value;
                Save();
            }
        }
    }

    public bool IsInitialized
    {
        get
        {
            if (DisplayName == null || DisplayName == "" || PlayerId == null || PlayerId == "")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }

    /// <summary>
    /// Does not destroy your save file
    /// </summary>
    public void Initialize()
    {
        if(IsInitialized)
        {
            //If you want this to work again you have to call Reset()
            throw new Exception("Already initialized");
        }
        this.StartPlaying = DateTime.UtcNow.ToString("s");
        this.DisplayName = defaultNamePrefix + UnityEngine.Random.Range(0,9999).ToString("0000");
        bool isShortUserIdGood = false;
        while(isShortUserIdGood == false)
        {
            //GUID based user ID generation
            Guid guid = Guid.NewGuid();
            this.PlayerId = guid.ToString();
            this.PlayerIdHash = guid.GetHashCode();
            this.ShortPlayerId = PlayerDataUtility.ShortenGUID(guid,shortenAlgorithmX,shortenAlgorithmM);
            isShortUserIdGood = PlayerDataUtility.IsShortUserIdGood(FormattedShortPlayerId);
        }
        Initialize2();
        Debug.Log("Initialized with Name : " + DisplayName + " ID : " + PlayerId + " SID : " + FormattedShortPlayerId);
        Save();
    }

//Move this to your partial and write something
/* 
    private void Initialize2()
    {

    }
*/

    public void Save()
    {
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");
        LastUpdated = DateTime.UtcNow.ToString("s");
        SaveAs(playerDataFileName);
    }

    public void Backup()
    {
        SaveAs(playerDataFileName + backupSuffix);
    }

    public void RestoreBackup()
    {
        PlayerDataUtility.ApplySaveFileFromPersistent("", playerDataFileName + backupSuffix);
    }

    private void SaveAs(string name)
    {
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes"); //So that iOS don't complain about protobuf's JITing 
        Debug.Log("Saved : " + Application.persistentDataPath);
        using (FileStream file = File.Create(Application.persistentDataPath + "/" + name))
        {
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            using (var cryptoStream = new CryptoStream(file, des.CreateEncryptor(key, iv), CryptoStreamMode.Write))
            {
                using (Google.Protobuf.CodedOutputStream cos = new Google.Protobuf.CodedOutputStream(cryptoStream))
                {
                    local.WriteTo(cos);
                }
            }
        }
    }

    public static void LocalReload()
    {
        local = Load();
    }

    private static PlayerData Load()
    {
        if (File.Exists(Application.persistentDataPath + "/" + playerDataFileName))
        {
            using(FileStream fileStream = File.Open(Application.persistentDataPath + "/" + playerDataFileName, FileMode.Open))
            {
                PlayerData loaded = PlayerDataFromStream(fileStream);
                return loaded;
            }
        }
        else
        {
            return new PlayerData();
        }
    }

    private static PlayerData PlayerDataFromStream(Stream stream)
    {
		Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes"); //So that iOS don't complain about protobuf's JITing 
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        PlayerData loadedData = new PlayerData();
        using (var cryptoStream = new CryptoStream(stream, des.CreateDecryptor(key, iv), CryptoStreamMode.Read))
        {
            using (Google.Protobuf.CodedInputStream cos = new Google.Protobuf.CodedInputStream(cryptoStream))
            {
                loadedData = PlayerData.Parser.ParseFrom(cos);
                return loadedData;
            }
        }
    }

    /// <summary>
    /// Currently it is not merge but a complete replace.. move it to your partial and make something cool!
    /// </summary>
    // public static void MergeSave(PlayerData toMergeWith)
    // {
    //     local = toMergeWith;
    //     PlayerData.Local.Save(); //you might not want auto-save on merge.
    // }

    /// <summary>
    /// Currently it is not merge but a complete replace.. move it to your partial and make something cool!
    /// </summary>
    // public static PlayerData Migration()
    // {
    //     return new PlayerData();
    // }

    /// <summary>
    /// For example getting a save restore as a JSON.
    /// </summary>
    public static PlayerData PlayerDataFromBase64(string base64String)
    {
        byte[] byteData = Convert.FromBase64String(base64String);
        MemoryStream memStream = new MemoryStream(byteData);
        return PlayerDataFromStream(memStream);
    }

    /// <summary>
    /// If you want to put the entire save in JSON this is useful
    /// </summary>
    public static string GeneratePlayerDataBase64()
    {
        PlayerData.Local.Save();
        if (File.Exists(Application.persistentDataPath + "/" + playerDataFileName))
        {
            return Convert.ToBase64String(File.ReadAllBytes(Application.persistentDataPath + "/" + playerDataFileName));
        }
        else
        {
            throw new Exception("Save file does not exist!");
        }
    }

    /// <summary>
    /// SUPER DESTRUCTIVE OPERATION please be careful!
    /// </summary>
    public static void Reset()
    {
        local = new PlayerData();
        local.Save();
    }


}
