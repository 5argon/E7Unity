using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class CsprojMod
{
    private static Dictionary<string,string> modPairs = new Dictionary<string, string>()
    {
        ["<LangVersion>7</LangVersion>"] = "<LangVersion>7.2</LangVersion>",
        ["<LangVersion>default</LangVersion>"] = "<LangVersion>7.2</LangVersion>",
    };

    [UnityEditor.Callbacks.DidReloadScripts]
    static public void ModTheProj()
    {
        //Debug.Log("mod the proj!!");
        string projectDirectory = System.IO.Directory.GetParent(Application.dataPath).FullName;
        string[] files = Directory.GetFiles(projectDirectory, "*.csproj");
        foreach (var f in files)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                string line = null;

                using (StreamReader sr = new StreamReader(f))
                {
                    while ((line = sr.ReadLine()) != null)
                    {
                        foreach(var pair in modPairs)
                        {
                            line = line.Replace(pair.Key, pair.Value);
                        }
                        sb.AppendLine(line);
                    }
                }
                using (StreamWriter sw = new StreamWriter(f, false, Encoding.UTF8))
                {
                    sw.Write(sb.ToString());
                }
            }
            catch (System.Exception)
            {
                throw;
            }
        }
    }
}