using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity
{
    //[CustomStyle("AnimatorTrigger")]
    public class AnimatorTriggerMarker : Marker, INotification
    {
        public string trigger;
        public PropertyName id => trigger.GetHashCode();
    }

}