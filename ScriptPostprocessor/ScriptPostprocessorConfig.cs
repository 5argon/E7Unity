using UnityEngine;
using System.Collections.Generic;

namespace E7.E7Unity.ScriptPostprocessor
{
    [CreateAssetMenu]
    public class ScriptPostprocessorConfig : ScriptableObject
    {
        [SerializeField] private ScriptPostprocessorOrder[] orders;
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