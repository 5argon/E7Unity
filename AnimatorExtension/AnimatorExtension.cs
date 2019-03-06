using UniRx.Async;
using UnityEngine;

namespace E7.AnimatorExtension
{
    public static class AnimatorExtension
    {
        /// <summary>
        /// Set trigger, wait a frame to apply, then wait equals to the next state's duration. Works on the layer index 0.
        /// Only works if setting a trigger before PreLateUpdate, where it uses that trigger and transition to the next state.
        /// </summary>
        public static async UniTask SetTriggerAndWait(this Animator animator, string trigger)
        {
            animator.SetTrigger(trigger);
            await UniTask.Yield();

            bool noTransition = animator.GetAnimatorTransitionInfo(0).duration == 0;
            //Debug.Log($"TRANS LEN {animator.GetAnimatorTransitionInfo(0).duration}");

            float waitFor = 0;

            if (noTransition)
            {
                //No transition means we are already at the next state.
                var currentState = animator.GetCurrentAnimatorStateInfo(0);
                waitFor = currentState.length;
                //Debug.Log($"NTwait {waitFor}");
            }
            else
            {
                //Has transition means we are still in the same state, have to get the next one's time.
                var nextState = animator.GetNextAnimatorStateInfo(0);
                waitFor = nextState.length;
                //Debug.Log($"Twait {waitFor}");
            }

            if (float.IsInfinity(waitFor) || float.IsNaN(waitFor) || waitFor < 0)
            {
                return;
            }

            //Debug.Log($"Waiting for {waitFor}");
            await UniTask.Delay((int)(waitFor * 1000));
        }
    }
}
