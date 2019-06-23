using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

namespace E7.E7Unity
{
    /// <summary>
    /// A stupid but useful script so you could connect something like <see cref="UnityEvent"> with the methods in here.
    /// </summary>
    public class SceneTransition : MonoBehaviour, INotificationReceiver
    {
        /// <summary>
        /// Do a <see cref="LoadSceneMode.Single"> with <paramref name="sceneName">.
        /// </summary>
        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }

        public void LoadSceneAddressables(string sceneName)
        {
            Addressables.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        }

        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if(notification is SceneTransitionMarker stm)
            {
                LoadScene(stm.sceneName);
            }
        }
    }
}