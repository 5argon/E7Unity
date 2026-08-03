using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// The animator parameters behind a state that persists rather than passes.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A trigger suits a moment — a press, a click. It cannot describe being checked or being selected, which have
    /// to survive every momentary animation playing over them and stay true for conditions elsewhere in the
    /// controller. A flag covers both: a bool holding what is currently true, and four triggers reaching the two
    /// resting poses.
    /// </para>
    /// <para>
    /// <see cref="turnOn"/> and <see cref="turnOff"/> are the authored ways in and out, each running a one-shot
    /// state that falls through to the pose it was heading for. <see cref="snapOn"/> and <see cref="snapOff"/> reach
    /// those same two poses with nothing played in between, which is what enabling, restoring saved values, and
    /// corrections a group makes on its own all want.
    /// </para>
    /// </remarks>
    /// <seealso cref="SelectableAnimatorUi"/>
    public readonly struct AnimatorFlag
    {
        /// <summary>Bool parameter holding what is currently true.</summary>
        public readonly string value;

        /// <summary>Trigger playing the authored way in.</summary>
        public readonly string turnOn;

        /// <summary>Trigger playing the authored way out.</summary>
        public readonly string turnOff;

        /// <summary>Trigger arriving at the on pose without playing anything.</summary>
        public readonly string snapOn;

        /// <summary>Trigger arriving at the off pose without playing anything.</summary>
        public readonly string snapOff;

        public readonly int valueHash;
        public readonly int turnOnHash;
        public readonly int turnOffHash;
        public readonly int snapOnHash;
        public readonly int snapOffHash;

        public AnimatorFlag(string value, string turnOn, string turnOff, string snapOn, string snapOff)
        {
            this.value = value;
            this.turnOn = turnOn;
            this.turnOff = turnOff;
            this.snapOn = snapOn;
            this.snapOff = snapOff;

            valueHash = Animator.StringToHash(value);
            turnOnHash = Animator.StringToHash(turnOn);
            turnOffHash = Animator.StringToHash(turnOff);
            snapOnHash = Animator.StringToHash(snapOn);
            snapOffHash = Animator.StringToHash(snapOff);
        }
    }
}
