#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using System;
using System.Linq;

namespace E7.E7Unity.ScriptPostprocessor
{
    public class ScriptPostprocessor : AssetPostprocessor
    {
        private static bool anyChanges = false;
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            anyChanges = false;
            foreach (string str in importedAssets)
            {
                //Debug.Log($"Importing {str}");
                if (Postprocess(str) == true)
                {
                    anyChanges = true;
                }
            }

            if (anyChanges)
            {
                Debug.Log(nameof(ScriptPostprocessor));
                AssetDatabase.Refresh();
            }
        }

        private static string postprocessedString = "//--Script Postprocessed--";

        private static bool Postprocess(string path)
        {
            string[] find = AssetDatabase.FindAssets("ScriptPostprocessor t:ScriptableObject");
            if (find.Length == 0)
            {
                return false;
            }

            //Debug.Log($"Found config file {AssetDatabase.GUIDToAssetPath(find[0])}");

            ScriptPostprocessorConfig sps = AssetDatabase.LoadAssetAtPath<ScriptPostprocessorConfig>(AssetDatabase.GUIDToAssetPath(find[0]));
            bool changes = false;
            foreach (ScriptPostprocessorOrder spo in sps.Orders)
            {
                if (spo.fileName == Path.GetFileName(path))
                {
                    bool foundLine = false;
                    List<string> lines = File.ReadAllLines(path).ToList();
                    List<string> modifiedLines = new List<string>();
                    if(lines.Count > 0  && lines[0] == postprocessedString)
                    {
                        //Debug.Log($"[ScriptPostprocessor] {spo.fileName} already postprocessed.");
                        continue;
                    }
                    Debug.Log($"[ScriptPostprocessor] Detected changes of {spo.fileName}.");

                    foreach (string line in lines)
                    {
                        foreach (ScriptPostprocessorLine spl in spo.lines)
                        {
                            if (line.Trim() == spl.line.Trim())
                            {
                                foreach (string add in spl.addToAbove)
                                {
                                    Debug.Log($"[ScriptPostprocessor] Adding {add} above {line} in {spo.fileName}");
                                    modifiedLines.Add(add);
                                    foundLine = true;
                                }
                            }
                        }
                        modifiedLines.Add(line);
                    }
                    if (foundLine)
                    {
                        modifiedLines.Insert(0, postprocessedString);
                        File.WriteAllLines(path, modifiedLines);
                        changes = true;
                    }
                }
            }
            return changes;
        }

    }
}
#endif