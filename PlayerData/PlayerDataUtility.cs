using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text.RegularExpressions;

public class E7PlayerDataUtility {

    public static bool IsDisplayNameGood(string name)
    {
        if(name.Length > 0 && name.Length <= 10)
        {
            Regex r = new Regex("[^A-Z0-9_]$");
            if (r.IsMatch(name)) {
                // validation failed
                return false;
            }
            else
            {
                return true;
            }
        }
        else
        {
            return false;
        }
    }

    // Criteria

    // Generate locally
    // 9 Characters without 0 O 1 I
    // Formatted like this XXX-XXX-XXX
    // All 3-char string should not contain profanity words.

    // By calculation this supports up to 6.9 million players before 2 of them clash.
    // http://stackoverflow.com/questions/35857156/how-can-i-create-my-own-guid-algorithm-with-smaller-global

    // Thanks to : https://github.com/klhurley/ElementalEngine2/blob/master/Common/Databases/BadWords.dbx
    //private readonly string[] filters = {"fuc","fuk","fuq","fux","fck","coc","cok","coq","kox","koc","kok","koq","cac","cak","caq","kac","kak","kaq","dic","dik","diq","dix","dck","pns","psy","fag","fgt","ngr","nig","cnt","knt","sht","dsh","twt","bch","cum","clt","kum","klt","suc","suk","suq","sck","lic","lik","liq","lck","jiz","jzz","gay","gey","gei","gai","vag","vgn","sjv","fap","prn","lol","jew","joo","gvr","pus","pis","pss","snm","tit","fku","fcu","fqu","hor","slt","jap","wop","kik","kyk","kyc","kyq","dyk","dyq","dyc","kkk","jyz","prk","prc","prq","mic","mik","miq","myc","myk","myq","guc","guk","guq","giz","gzz","sex","sxx","sxi","sxe","sxy","xxx","wac","wak","waq","wck","pot","thc","vaj","vjn","nut","std","lsd","poo","azn","pcp","dmn","orl","anl","ans","muf","mff","phk","phc","phq","xtc","tok","toc","toq","mlf","rac","rak","raq","rck","sac","sak","saq","pms","nad","ndz","nds","wtf","sol","sob","fob","sfu"};

    public static string GenerateUserId()
    {
        //Temp..
        return Guid.NewGuid().ToString("N").Substring(0,9);
    }

    public static void ApplySaveFile(string saveFilesPath, string saveFileName)
    {
        string[] fileEntries = Directory.GetFiles(Application.dataPath + "/" + saveFilesPath);
        foreach (string filePath in fileEntries)
        {
            if(Path.GetExtension(filePath) == ".meta")
            {
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(filePath);
            if(fileName == saveFileName)
            {
                Debug.Log("Applying save : " + filePath);
                File.Copy(filePath, Application.persistentDataPath + "/" + PlayerData.playerDataFileName,true);
                PlayerData.LocalReload(); //reload the copied save
                return;
            }
        }
        throw new Exception("Cound not find the save " + saveFilesPath + " " + saveFileName);

    }

}
