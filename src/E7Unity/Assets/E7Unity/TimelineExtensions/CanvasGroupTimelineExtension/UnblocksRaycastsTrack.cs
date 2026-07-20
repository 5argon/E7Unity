using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Locks a uGUI subtree out of receiving clicks while a sequence is playing, by driving the bound
    /// <see cref="CanvasGroup"/>'s <see cref="CanvasGroup.blocksRaycasts"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The point is to keep the player from poking at a UI that is still animating in or out. Beware the naming: the
    /// track "unblocks raycasts", meaning rays pass <i>through</i> the group — which is precisely when the player
    /// <b>cannot</b> click anything.
    /// </para>
    /// <para>Two arrangements are useful, and the track behaves differently for each:</para>
    /// <list type="bullet">
    ///   <item>
    ///     <description>
    ///     <b>With clips.</b> The UI is unclickable for the extent of every <see cref="UnblocksRaycastsClip"/>, and
    ///     clickable everywhere else on the track. A clip usually spans the whole sequence, but shortening it hands
    ///     control back to the player early.
    ///     </description>
    ///   </item>
    ///   <item>
    ///     <description>
    ///     <b>With no clips at all.</b> The whole track is unclickable for its entire duration. This is common enough
    ///     to be worth the special case, and is why the track still evaluates while empty.
    ///     </description>
    ///   </item>
    /// </list>
    /// <para>
    /// A new clip is created spanning the full timeline rather than at some default length, since covering the whole
    /// sequence is nearly always what is wanted.
    /// </para>
    /// <para>
    /// Where the group is left once playback ends is up to
    /// <see cref="UnblocksRaycastsTrackMixerBehaviour.PostPlaybackState"/>.
    /// </para>
    /// </remarks>
    [TrackBindingType(typeof(CanvasGroup))]
    [TrackColor(0.4f, 0, 0)]
    [TrackClipType(typeof(UnblocksRaycastsClip))]
    public class UnblocksRaycastsTrack : TrackAsset
    {
        /// <summary>
        /// Track-wide settings. Its fields are drawn directly in the track inspector rather than behind a foldout.
        /// </summary>
        public UnblocksRaycastsTrackMixerBehaviour template;

        /// <summary>
        /// Always reports the track as non-empty, so it keeps evaluating with no clips on it — that state is
        /// meaningful here and means "unclickable throughout".
        /// </summary>
        public override bool isEmpty => false;

        /// <inheritdoc/>
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            //Debug.Log($"Creating track mixer {Time.frameCount}");
            return ScriptPlayable<UnblocksRaycastsTrackMixerBehaviour>.Create(graph, template, inputCount);
        }

        /// <summary>
        /// Registers the bound <see cref="CanvasGroup"/>'s interactable and raycast flags with Timeline's preview
        /// system, so scrubbing in the editor does not leave them permanently modified once preview ends.
        /// </summary>
        /// <param name="director">The director previewing this track.</param>
        /// <param name="driver">Collector that records which properties this track writes to.</param>
        public override void GatherProperties(PlayableDirector director, IPropertyCollector driver)
        {
            if (director != null)
            {
                var binding = director.GetGenericBinding(this);
                if (binding is CanvasGroup cg)
                {
                    driver.AddFromName<CanvasGroup>(cg.gameObject, "m_Interactable");
                    driver.AddFromName<CanvasGroup>(cg.gameObject, "m_BlocksRaycasts");
                }
            }
        }

        /// <summary>
        /// Stretches a newly created clip across the whole timeline, since covering the entire sequence is the usual
        /// intent.
        /// </summary>
        /// <param name="clip">The clip just added to this track.</param>
        protected override void OnCreateClip(TimelineClip clip)
        {
            clip.start = 0;
            //Clip starts with some preset duration, we want it to stretch the full length but that preset duration ruins the timeline length.
            clip.duration = float.Epsilon;
            //Now we can ask for the correct timeline length before the clip came
            clip.duration = clip.GetParentTrack().parent.duration;
            if (clip.duration <= float.Epsilon)
            {
                clip.duration = 1;
            }
        }
    }
}
