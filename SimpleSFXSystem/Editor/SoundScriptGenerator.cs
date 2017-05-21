using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Text;
using System.IO;
using System;

public class SoundScriptGenerator : MonoBehaviour {

    [MenuItem("Edit/SimpleSFXSystem/Generate Script To Clipboard")]
    public static void GenerateScriptToClipboard()
    {
        string innerCode = GenerateInnerCode();
        EditorGUIUtility.systemCopyBuffer = innerCode;
        Debug.Log("Script copied to clipboard.");
    }

    [MenuItem("Edit/SimpleSFXSystem/New SoundEffectPlayer Wizard")]
    public static void NewSoundEffectPlayerWizard()
    {
        //string innerCode = GenerateInnerCode();
        //To do!
    }

    [MenuItem("Edit/SimpleSFXSystem/Update SoundEffectPlayer")]
    public static void UpdateSoundEffectPlayer()
    {
        //string innerCode = GenerateInnerCode();
        //To do!
    }

    private static string GenerateInnerCode()
    {
		string selected = EditorUtility.OpenFolderPanel("Select a folder that contains your SFXs categorized in folders.","Assets/","");
		string[] directories = Directory.GetDirectories(selected);
        return GenerateCode(directories);
    }

    private static string GenerateCode(string[] directories)
    {
        StringBuilder sb = new StringBuilder();

        foreach (string s in directories)
        {
            string[] soundNames = GetAllSoundNames(s);
            sb.Append("[Header(\"" + Path.GetFileName(s) + "\")]\r\n");
            foreach (string name in soundNames)
            {
                AppendDeclaration(sb, name);
            }
            Debug.Log("Found a folder " + Path.GetFileName(s) + " with " + soundNames.Length + " files.");
        }
        foreach (string s in directories)
        {
            string[] soundNames = GetAllSoundNames(s);
            foreach (string name in soundNames)
            {
                AppendPlayFunction(sb, name);
            }
        }

        return sb.ToString();


    }

    private static void AppendDeclaration(StringBuilder sb, string s)
    {
        sb.Append("\tpublic SoundEffect ");
        sb.Append(Char.ToLower(s[0]));
        sb.Append(s.Substring(1));
        sb.Append(";\r\n");
    }

    private static void AppendPlayFunction(StringBuilder sb, string s)
    {
		AppendPlayFunctionOverload1(sb,s);
		AppendPlayFunctionOverload2(sb,s);
    }

	private static void AppendPlayFunctionOverload1(StringBuilder sb, string s)
	{
        sb.Append("\r\n");
        sb.Append("\tpublic void Play");
        sb.Append(Char.ToUpper(s[0]));
        sb.Append(s.Substring(1));
        sb.Append("()\r\n");
        sb.Append("\t{\r\n");
        sb.Append("\t\tPlaySoundEffect(");
        sb.Append(Char.ToLower(s[0]));
        sb.Append(s.Substring(1));
        sb.Append(");\r\n");
        sb.Append("\t}\r\n");
	}

	private static void AppendPlayFunctionOverload2(StringBuilder sb, string s)
	{
        sb.Append("\r\n");
        sb.Append("\tpublic void Play");
        sb.Append(Char.ToUpper(s[0]));
        sb.Append(s.Substring(1));
        sb.Append("(float forceVolume)\r\n");
        sb.Append("\t{\r\n");
        sb.Append("\t\tPlaySoundEffect(");
        sb.Append(Char.ToLower(s[0]));
        sb.Append(s.Substring(1));
        sb.Append(",forceVolume);\r\n");
        sb.Append("\t}\r\n");
	}

    public static string[] GetAllSoundNames(string fullFolderPath)
    {
        string[] s = Directory.GetFiles(fullFolderPath, "*.???");
        for (int i = 0; i < s.Length; i++)
        {
            s[i] = Path.GetFileNameWithoutExtension(s[i]);
        }
        return s;
    }

}
