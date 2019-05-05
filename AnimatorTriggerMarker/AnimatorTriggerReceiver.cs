using E7.E7Unity;
using UnityEngine;
using UnityEngine.Playables;

[RequireComponent(typeof(Animator))]
public class AnimatorTriggerReceiver : MonoBehaviour, INotificationReceiver
{
    public Animator triggerTarget;
    public void OnNotify(Playable origin, INotification notification, object context)
    {
        if(notification is AnimatorTriggerMarker atm)
        {
            triggerTarget.SetTrigger(atm.trigger);
        }
    }
}
