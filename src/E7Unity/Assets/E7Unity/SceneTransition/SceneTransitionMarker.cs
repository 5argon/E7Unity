using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity
{
    /// <summary>
    /// A Timeline marker that asks for a scene load at the point it sits on, so an outro sequence can end by moving
    /// to the next scene without a script watching the director's clock.
    /// </summary>
    /// <remarks>
    /// It is only a request: something must be listening. Put a <see cref="SceneTransition"/> on the game object
    /// bound to the track holding this marker, and it will perform the load.
    /// </remarks>
    [DisplayName(nameof(E7.E7Unity) + "/" + nameof(SceneTransitionMarker))]
    public class SceneTransitionMarker : Marker, INotification
    {
        /// <summary>
        /// Name of the scene to load. It must be in the build's scene list.
        /// </summary>
        public string sceneName;

        /// <summary>
        /// Identifies this notification by the scene it asks for.
        /// </summary>
        public PropertyName id => sceneName;
    }
}
