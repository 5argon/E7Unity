using UnityEngine;
using UnityEngine.Playables;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Listens for <see cref="AnimatorTriggerMarker"/> notifications from a Timeline and turns each one into
    /// <see cref="Animator.SetTrigger(string)"/> on a target <see cref="Animator"/>.
    /// </summary>
    /// <remarks>
    /// Put this on the game object bound to the animation track carrying the markers. Timeline delivers
    /// notifications to the bound object, so a marker elsewhere never reaches this receiver.
    /// </remarks>
    [RequireComponent(typeof(Animator))]
    public class AnimatorTriggerReceiver : MonoBehaviour, INotificationReceiver
    {
        /// <summary>
        /// The animator that receives the triggers. Filled in with the animator on this game object when left empty.
        /// </summary>
        public Animator triggerTarget;

        /// <summary>
        /// Defaults <see cref="triggerTarget"/> to the <see cref="Animator"/> on this game object.
        /// </summary>
        public void OnValidate()
        {
            if (triggerTarget == null)
            {
                triggerTarget = GetComponent<Animator>();
            }
        }

        /// <summary>
        /// Sets the trigger named by any <see cref="AnimatorTriggerMarker"/> received. Other notifications are
        /// ignored.
        /// </summary>
        /// <param name="origin">The playable that raised the notification.</param>
        /// <param name="notification">The notification, acted on only when it is an <see cref="AnimatorTriggerMarker"/>.</param>
        /// <param name="context">User-defined context supplied by the sender.</param>
        public void OnNotify(Playable origin, INotification notification, object context)
        {
            if (notification is AnimatorTriggerMarker atm)
            {
                triggerTarget.SetTrigger(atm.trigger);
            }
        }
    }
}
