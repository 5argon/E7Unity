using UnityEngine;
using System.Collections;
using System;
using System.IO;
using System.Text.RegularExpressions;

public class PlayerDataUtility {

    // Criteria

    // Generate locally
    // 9 Characters without 0 O 1 I
    // Formatted like this XXX-XXX-XXX
    // All 3-char string should not contain profanity words.

    // By calculation this supports up to 6.9 million players before 2 of them clash.
    // http://stackoverflow.com/questions/35857156/how-can-i-create-my-own-guid-algorithm-with-smaller-global

    // Thanks to : https://github.com/klhurley/ElementalEngine2/blob/master/Common/Databases/BadWords.dbx
    private static readonly string[] filters = {"fuc","fuk","fuq","fux","fck","cac","cak","caq","kac","kak","kaq","dck","pns","fag","fgt","ngr","cnt","knt","sht","dsh","twt","bch","cum","clt","kum","klt","suc","suk","suq","sck","lck","jzz","gay","gey","vag","vgn","sjv","fap","prn","jew","gvr","pus","pss","snm","fku","fcu","fqu","slt","jap","kyk","kyc","kyq","dyk","dyq","dyc","kkk","jyz","prk","prc","prq","myc","myk","myq","guc","guk","guq","gzz","sex","sxx","sxe","sxy","xxx","wak","waq","wck","thc","vaj","vjn","nut","std","lsd","azn","pcp","dmn","anl","ans","muf","mff","phk","phc","phq","xtc","mlf","rac","rak","raq","rck","sac","sak","saq","nad","ndz","nds","sfu"};

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

    public static bool IsShortUserIdGood(string formattedShortUserId)
    {
        string[] dashSplit = formattedShortUserId.Split('-');
        foreach (string s in dashSplit)
        {
            if (Array.IndexOf(filters, s) != -1)
            {
                return false;
            }
        }
        return true;
    }

    public static string ShortenGUID(Guid guid, ulong x, ulong m)
    {
        //We don't care about information lose. 
        //If the result conflicts we still have the GUID for check.
        //Read more : https://stackoverflow.com/questions/44765539/any-algorithm-that-use-a-number-as-a-feed-for-generating-random-string

        //Note that Hash might change on different .NET version!!
        //But for GUID class it is overridden, likely that it will stay the same.. I hope
        //On the server, etc. we will not have access to this function. So it is wise to include this along with the GUID string.
        uint i = (uint)guid.GetHashCode();

        ulong mmod = MultiplyModulo(i, x, m);

        //Now we convert to 
        string code = DecimalToArbitrarySystem(mmod, 32);

        code = PrefixUntilLength(code, 9, '2');

        //Debug.Log("Hash " + i + " -> Multiply modulo " + mmod + " -> Code " + code);

        //check the code for offensive words..

        return code;
    }

    private static string PrefixUntilLength(string input, int length, char prefix)
    {
        while(input.Length < length)
        {
            input = prefix + input;
        }
        return input;
    }

    private static ulong MultiplyModulo(ulong input, ulong x, ulong m)
    {
        //Debug.Log(input + " " + x + " " + m);
        return (input * x) % m;
    }

    /// <summary>
    /// Converts the given decimal number to the numeral system with the
    /// specified radix (in the range [2, 36]).
    /// </summary>
    /// <param name="decimalNumber">The number to convert.</param>
    /// <param name="radix">The radix of the destination numeral system (in the range [2, 36]).</param>
    /// <returns></returns>
    public static string DecimalToArbitrarySystem(ulong decimalNumber, ulong radix)
    {
        const int BitsInLong = 64;
        const string Digits = "23456789ABCDEFGHJKLMNPQRSTUVWXYZ";

        if (radix < 2 || radix > (ulong)Digits.Length)
            throw new ArgumentException("The radix must be >= 2 and <= " + Digits.Length.ToString());

        if (decimalNumber == 0)
            return "0";

        int index = BitsInLong - 1;
        ulong currentNumber = decimalNumber;
        char[] charArray = new char[BitsInLong];

        while (currentNumber != 0)
        {
            int remainder = (int)(currentNumber % radix);
            charArray[index--] = Digits[remainder];
            currentNumber = currentNumber / radix;
        }

        string result = new String(charArray, index + 1, BitsInLong - index - 1);
        if (decimalNumber < 0)
        {
            result = "-" + result;
        }

        return result;
    }

    public static void ApplySaveFileFromProject(string path, string name)
    {
        ApplySaveFile(Application.dataPath+ path, name);
    }

    public static void ApplySaveFileFromPersistent(string path, string name)
    {
        ApplySaveFile(Application.persistentDataPath + path, name);
    }

/// <summary>
/// This is for selectively load any save file of any name that you have. Useful for debugging or unit testing.
/// </summary>
    private static void ApplySaveFile(string saveFilesPath, string saveFileName)
    {
        string[] fileEntries = Directory.GetFiles(saveFilesPath);
        foreach (string filePath in fileEntries)
        {
            //Debug.Log(filePath);
            if(Path.GetExtension(filePath) == ".meta")
            {
                continue;
            }

            string fileName = Path.GetFileName(filePath);
            if(fileName == saveFileName)
            {
                //Debug.Log("Applying save : " + filePath);

                //It's just a copy-replace (old ones not removed)
                File.Copy(filePath, Application.persistentDataPath + "/" + PlayerData.playerDataFileName,true);

                PlayerData.LocalReload(); //reload the copied save
                return;
            }
        }
        throw new Exception("Cound not find the save " + saveFilesPath + " " + saveFileName);
    }

}
