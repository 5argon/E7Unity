using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace E7.E7Unity
{
    /// <summary>
    /// Loads a scene by name from something that cannot call code directly — a
    /// <see cref="UnityEngine.Events.UnityEvent"/> wired in the inspector, or a
    /// <see cref="SceneTransitionMarker"/> on a Timeline.
    /// </summary>
    /// <remarks>
    /// Deliberately trivial. Its whole purpose is to exist as a component so that a button, an animation event, or
    /// the end of a Timeline can trigger a scene load without a bespoke script per scene.
    /// </remarks>
    public class SceneTransition : MonoBehaviour, INotificationReceiver
    {
        /// <summary>
        /// Loads <paramref name="sceneName"/> with <see cref="LoadSceneMode.Single"/>, replacing the current scene.
        /// </summary>
        /// <param name="sceneName">Name of the scene to load. It must be in the build's scene list.</param>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        /// <summary>
        /// Receives Timeline notifications and loads the scene named by any <see cref="SceneTransitionMarker"/>
        /// among them. Other notifications are ignored.
        /// </summary>
        /// <param name="origin">The playable that raised the notification.</param>
        /// <param name="notification">The notification, acted on only when it is a <see cref="SceneTransitionMarker"/>.</param>
        /// <param name="context">User-defined context supplied by the sender.</param>
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is SceneTransitionMarker stm)
            {
                LoadScene(stm.sceneName);
            }
        }
    }
}
