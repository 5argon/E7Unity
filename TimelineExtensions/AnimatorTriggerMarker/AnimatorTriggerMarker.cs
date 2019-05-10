using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity
{
    //[CustomStyle("AnimatorTrigger")]
    [DisplayName(nameof(E7.E7Unity) + "/" + nameof(AnimatorTriggerMarker))]
    public class AnimatorTriggerMarker : Marker, INotification
    {
        public string trigger;
        public PropertyName id => trigger.GetHashCode();
    }

}