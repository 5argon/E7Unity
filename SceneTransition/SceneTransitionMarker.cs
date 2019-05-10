using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity
{
    [DisplayName(nameof(E7.E7Unity) + "/" + nameof(SceneTransitionMarker))]
    public class SceneTransitionMarker : Marker, INotification 
    {
        public string sceneName;

        public PropertyName id => sceneName;
    }
}