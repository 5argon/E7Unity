using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Pushes the sub-directors driven by this Timeline's control tracks to their final frame once playback stops,
    /// so a stalled frame cannot leave one of them stranded mid-animation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Timeline samples the world at one instant per frame and keeps no memory of the previous one. Most tracks
    /// tolerate that: an animation clip set to hold past its end stays under the playhead forever, and an
    /// activation track recomputes itself from the current playhead every frame. A control track driving another
    /// <see cref="PlayableDirector"/> does neither. It writes the target's time from inside the clip's own frame
    /// processing, clamped to the target's duration, and once the playhead is past the clip nothing recomputes it.
    /// Whatever partial time the last evaluation wrote is what the target keeps.
    /// </para>
    /// <para>
    /// That final time is only correct if some frame landed on the stretch of clip beyond the target's own content,
    /// which is where the clamp takes effect. A frame long enough to step over that stretch skips it entirely, and
    /// the target holds a pose from the middle of its sequence.
    /// </para>
    /// <para>
    /// Attach this to the driving director and mark each sub-director that needs its last frame with
    /// <see cref="SettleAtEnd"/>. Targets without that component are left alone, so a sub-director whose ending is
    /// a handover back to component-driven playback keeps its handover.
    /// </para>
    /// <para>
    /// The work happens on <see cref="PlayableDirector.stopped"/>, which is raised by the change of state rather
    /// than read off the playhead, so no amount of stalling can miss it. It also lands after the playhead has left
    /// every clip, which matters: settling a target while its control clip is still under the playhead is undone on
    /// the next frame, when the clip notices the target is out of sync and drags it back.
    /// </para>
    /// <para>
    /// A director set to <see cref="DirectorWrapMode.Hold"/> never stops and so never raises the event. It does not
    /// need to — holding keeps re-asserting the last instant of the Timeline every frame, which is its own way of
    /// pinning the ending. Note that the same never-stopping is why a held director never runs any track's teardown
    /// either.
    /// </para>
    /// </remarks>
    [RequireComponent(typeof(PlayableDirector))]
    public class DirectorSettle : MonoBehaviour
    {
        /// <summary>
        /// The director whose control tracks are settled. Filled in with the director on this game object when left
        /// empty.
        /// </summary>
        public PlayableDirector settleTarget;

        /// <summary>
        /// Defaults <see cref="settleTarget"/> to the <see cref="PlayableDirector"/> on this game object.
        /// </summary>
        public void OnValidate()
        {
            if (settleTarget == null)
            {
                settleTarget = GetComponent<PlayableDirector>();
            }
        }

        /// <summary>
        /// Starts listening for the end of playback.
        /// </summary>
        public void OnEnable()
        {
            OnValidate();
            if (settleTarget != null)
            {
                settleTarget.stopped += OnDirectorStopped;
            }
        }

        /// <summary>
        /// Stops listening for the end of playback.
        /// </summary>
        public void OnDisable()
        {
            if (settleTarget != null)
            {
                settleTarget.stopped -= OnDirectorStopped;
            }
        }

        private void OnDirectorStopped(PlayableDirector stopped) => SettleControlTracks(stopped);

        /// <summary>
        /// Walks every control track of the Timeline on <paramref name="director"/> and moves each target director
        /// marked with <see cref="SettleAtEnd"/> to its own final frame, then does the same for that target's own
        /// control tracks, and so on down the chain.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The descent matters whenever control tracks nest. Evaluating a target at its own duration does not
        /// settle anything the target itself drives: at that instant a control clip ending on the duration is
        /// already outside the half-open interval Timeline queries with, so it is not processed and never writes
        /// its own target's time. Each level has to be settled explicitly.
        /// </para>
        /// <para>
        /// Only targets that were settled are descended into. A sub-director left alone for want of
        /// <see cref="SettleAtEnd"/> keeps everything below it untouched as well, so marking an intermediate is how
        /// you reach past it.
        /// </para>
        /// </remarks>
        /// <param name="director">The director whose control tracks are walked. Its bindings resolve the targets.</param>
        public static void SettleControlTracks(PlayableDirector director)
        {
            HashSet<PlayableDirector> visited = new HashSet<PlayableDirector>();
            Descend(director, visited);
        }

        private static void Descend(PlayableDirector director, HashSet<PlayableDirector> visited)
        {
            if (director == null || !visited.Add(director))
            {
                return;
            }

            TimelineAsset timeline = director.playableAsset as TimelineAsset;
            if (timeline == null)
            {
                return;
            }

            // Collected before any settling, because settling descends and would otherwise reuse this list.
            List<PlayableDirector> targets = new List<PlayableDirector>();
            List<PlayableDirector> onObject = new List<PlayableDirector>();

            foreach (TrackAsset track in timeline.GetOutputTracks())
            {
                if (!(track is ControlTrack) || track.muted)
                {
                    continue;
                }

                foreach (TimelineClip clip in track.GetClips())
                {
                    ControlPlayableAsset control = clip.asset as ControlPlayableAsset;
                    if (control == null || !control.updateDirector)
                    {
                        continue;
                    }

                    // The driving director doubles as the table that resolves the clip's exposed binding, the same
                    // way the control clip itself resolves it at runtime.
                    GameObject source = control.sourceGameObject.Resolve(director);
                    if (source == null)
                    {
                        continue;
                    }

                    // Mirrors how a control clip gathers its own targets, which reaches into children only when the
                    // clip asks it to.
                    onObject.Clear();
                    if (control.searchHierarchy)
                    {
                        source.GetComponentsInChildren(true, onObject);
                    }
                    else
                    {
                        source.GetComponents(onObject);
                    }

                    for (int i = 0; i < onObject.Count; i++)
                    {
                        PlayableDirector target = onObject[i];
                        if (target == null || target == director)
                        {
                            continue;
                        }

                        // A clip pointed at its own director drives the Timeline that contains it, which control
                        // clips refuse for the same reason.
                        if (target.playableAsset == director.playableAsset)
                        {
                            continue;
                        }

                        if (target.GetComponent<SettleAtEnd>() == null)
                        {
                            continue;
                        }

                        targets.Add(target);
                    }
                }
            }

            for (int i = 0; i < targets.Count; i++)
            {
                PlayableDirector target = targets[i];
                target.time = target.duration;
                target.Evaluate();
                Descend(target, visited);
            }
        }
    }
}
