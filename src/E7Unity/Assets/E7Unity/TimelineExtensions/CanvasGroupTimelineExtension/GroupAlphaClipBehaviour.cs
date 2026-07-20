using System;
using UnityEngine;
using UnityEngine.Playables;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// The per-clip payload of a <see cref="GroupAlphaClip"/>, holding the alpha that clip contributes at full
    /// weight.
    /// </summary>
    [Serializable]
    public class GroupAlphaClipBehaviour : PlayableBehaviour
    {
        /// <summary>
        /// Alpha this clip contributes when its weight is 1, letting a fully-weighted clip settle at something other
        /// than fully opaque. The blended result is the sum of every clip's weight times its own scale.
        /// </summary>
        [Range(0f, 1f)]
        [Tooltip("Use this if you want to use the clip's weight yet want the maximum weight to be something other than maximum alpha.")]
        public float alphaScale = 1;
    }
}
