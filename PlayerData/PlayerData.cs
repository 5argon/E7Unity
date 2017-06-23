using UnityEngine;
using System.Collections.Generic;
using System;
using System.IO;
using System.Text;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography;

/* 
A class to make a save file system! The whole class will be binary serialized.
Basics are PlayerData.Local.Save and PlayerData.Load
Local means it will load the file in your device.

It's a partial not inheritance, because I want the custom data you want
to include serialized together with basic data in this class.
You can edit them if they are useless to you.

Include your game-specific things in the other side of the partial.

This class will not compile if you see the 3 commented lines below!
You should move that to your partial then define those.

Note that if a hacker gain access of your key and iv they probably can
hack your save file. And as you see below, they are plain text in your code.
(hackable)

So change it to something else like server query if you care about security.

*/


[System.Serializable]
public partial class PlayerData {

    //public static readonly string playerDataFileName  = "SaveFile.sav";
    //private static readonly byte[] key = Encoding.ASCII.GetBytes("EightChr");
    //private static readonly byte[] iv = Encoding.ASCII.GetBytes("EightChr");

    private static PlayerData local;

    //Something you might want to know

    //it fetches only once and cache it. Even if you modify your save and save it, this local is the one that you fetches earilier.

    //But of course if you have modify your local before saving, your local must also be current. So there's no need to call the expensive .Load everytime you want your data.

    //But of course there is a LocalReload in case if you want to revert to the binary file, or just have replaced the save via backup.
    public static PlayerData Local
    {
        get
        {
            if(local == null)
            {
                //Load from binary
                local = PlayerData.Load();
            }
            return local;
        }
    }

    private string displayName;
    private string userId;
    private string email;

    public string FormattedUserId
    {
        get 
        { 
            if(userId == null)
            {
                return "???-???-???";
            }
            else
            {
                return
                userId.Substring(0,3) +
                "-" +
                userId.Substring(3,3) +
                "-" +
                userId.Substring(6,3)
                ; 
            }
        }
    }

    public string DisplayName
    {
        get 
        { 
            if(displayName == null)
            {
                return "???";
            }
            else
            {
                return displayName; 
            }
        }
        set
        {
            if(value.Length > 0 && value.Length <= 10)
            {
                displayName = value;
                Save();
            }
        }
    }

    public bool IsInitialized()
    {
        if( displayName == null || userId == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public bool IsAttachedOnline()
    {
        if(email == null)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    public void Initialize(string displayName, string email)
    {
        this.displayName = displayName;
        this.email = email;
        //in here, please make your own user ID generation methods. Currently it is very stupid.
        this.userId = E7PlayerDataUtility.GenerateUserId();
        Save();
    }

    public void Save()
    {
        FileStream file = File.Create(Application.persistentDataPath + "/" + playerDataFileName);
        DESCryptoServiceProvider des = new DESCryptoServiceProvider();
        using(var cryptoStream = new CryptoStream(file, des.CreateEncryptor(key, iv), CryptoStreamMode.Write))
        {
            BinaryFormatter bf = new BinaryFormatter();
            bf.Serialize(cryptoStream, PlayerData.Local);
        }
        file.Close();
        //PlayerData.Local.DebugDump();
    }

    public static void LocalReload()
    {
        local = Load();
    }

    private static PlayerData Load()
    {
        //Debug.Log(Application.persistentDataPath);
        Environment.SetEnvironmentVariable("MONO_REFLECTION_SERIALIZER", "yes");

        if(File.Exists(Application.persistentDataPath + "/" + playerDataFileName))
        {
            FileStream file = File.Open(Application.persistentDataPath + "/" + playerDataFileName, FileMode.Open);
            DESCryptoServiceProvider des = new DESCryptoServiceProvider();
            PlayerData loadedData = new PlayerData();
            using(var cryptoStream = new CryptoStream(file, des.CreateDecryptor(key, iv), CryptoStreamMode.Read))
            {
                BinaryFormatter bf = new BinaryFormatter();
                loadedData = (PlayerData)bf.Deserialize(cryptoStream);
            }
            file.Close();
            return loadedData;
        }
        else
        {
            return new PlayerData(); 
        }
    }

    //SUPER DESTRUCTIVE OPERATION please be careful!
    public static void DebugReset()
    {
        local = new PlayerData();
        local.Save();
    }

}
