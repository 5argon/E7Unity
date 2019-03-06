using UnityEngine;
using System.Collections.Generic;

namespace E7.E7Unity.ScriptPostprocessor
{
    [CreateAssetMenu]
    public class ScriptPostprocessorConfig : ScriptableObject
    {
#pragma warning disable 0649
        [SerializeField] private ScriptPostprocessorOrder[] orders;
#pragma warning restore 0649
        public IEnumerable<ScriptPostprocessorOrder> Orders => orders;
    }

    [System.Serializable]
    public struct ScriptPostprocessorOrder
    {
        public string fileName;
        public ScriptPostprocessorLine[] lines;
    }

    [System.Serializable]
    public struct ScriptPostprocessorLine
    {
        public string line;
        public string[] addToAbove;
    }

}