using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// A Timeline marker that hands a named trigger back to an <see cref="Animator"/>, so the animator can resume
    /// driving itself at a chosen point rather than the moment the Timeline happens to end.
    /// </summary>
    /// <remarks>
    /// <para>
    /// While a Timeline animates an <see cref="Animator"/> it takes the animator over completely, overriding its
    /// controller — Timeline borrows the animator's ability to pose things, but not its state machine. When the
    /// sequence is done the animator should go back to running its own controller, and this marker is how that
    /// handover is scheduled precisely.
    /// </para>
    /// <para>
    /// Place it on the animation track bound to the object you want to trigger, so that object receives the
    /// notification. Markers are not actually bound to a track, so it can be placed elsewhere, but then nothing is
    /// listening and nothing happens.
    /// </para>
    /// <para>
    /// The listener is <see cref="AnimatorTriggerReceiver"/>, which turns this marker into
    /// <see cref="Animator.SetTrigger(string)"/>.
    /// </para>
    /// </remarks>
    [DisplayName(nameof(E7.E7Unity) + "/" + nameof(AnimatorTriggerMarker))]
    public class AnimatorTriggerMarker : Marker, INotification
    {
        /// <summary>
        /// Name of the <see cref="Animator"/> trigger parameter to set when this marker is passed.
        /// </summary>
        public string trigger;

        /// <summary>
        /// Identifies this notification by the trigger name it carries.
        /// </summary>
        public PropertyName id => trigger.GetHashCode();
    }
}
