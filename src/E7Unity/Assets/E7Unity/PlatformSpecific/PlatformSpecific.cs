using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Keeps its game object active only on the listed runtime platforms, and deactivates it everywhere else.
    /// </summary>
    /// <remarks>
    /// The decision is made once in <c>Awake</c> by comparing <see cref="Application.platform"/> against
    /// <see cref="platforms"/>, so the game object may be left active in the scene while authoring and still
    /// disappear on the platforms it is not meant for.
    /// </remarks>
    public class PlatformSpecific : MonoBehaviour
    {
        /// <summary>
        /// Platforms on which the game object stays active. On any platform outside this list it is deactivated.
        /// </summary>
        public RuntimePlatform[] platforms;

        void Awake()
        {
            foreach (var p in platforms)
            {
                if (Application.platform == p)
                {
                    this.gameObject.SetActive(true);
                    return;
                }
            }
            this.gameObject.SetActive(false);
        }
    }
}
