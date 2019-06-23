using System;
using UnityEngine;
using UnityEngine.Playables;

namespace E7.Timeline
{
    [Serializable]
    public class GroupAlphaClipBehaviour : PlayableBehaviour
    {
        [Range(0f, 1f)]
        [Tooltip("Use this if you want to use the clip's weight yet want the maximum weight to be something other than maximum alpha.")]
        public float alphaScale = 1;
    }
}
