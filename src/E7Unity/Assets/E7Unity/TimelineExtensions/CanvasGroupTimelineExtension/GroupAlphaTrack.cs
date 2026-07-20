using System.ComponentModel;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

namespace E7.E7Unity.Timeline
{
    /// <summary>
    /// Drives the alpha of a bound <see cref="CanvasGroup"/> from clip weight, fading an entire uGUI subtree as one.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Fading a whole tree through its <see cref="CanvasGroup"/> avoids the cost of the obvious alternative: an
    /// Activation Track toggles the objects themselves, which re-runs layout and wakes every component each time it
    /// flips. Alpha leaves the hierarchy alone and merely stops drawing it.
    /// </para>
    /// <para>
    /// Alpha comes from clip weight, so the ordinary Timeline gestures for shaping weight are the ones that shape the
    /// fade: hold <c>Cmd</c>/<c>Ctrl</c> and drag a clip's edge to slope it, or overlap two clips to cross-fade.
    /// Overlapping clips are summed and clamped, and each clip can cap its own contribution through
    /// <see cref="GroupAlphaClipBehaviour.alphaScale"/>.
    /// </para>
    /// <para>Bind the track to the <see cref="CanvasGroup"/> you want to fade.</para>
    /// </remarks>
    [TrackBindingType(typeof(CanvasGroup))]
    [TrackColor(0.4f, 0, 0)]
    [TrackClipType(typeof(GroupAlphaClip))]
    [DisplayName(nameof(E7.E7Unity) + "/" + nameof(GroupAlphaTrack))]
    public class GroupAlphaTrack : TrackAsset
    {
        /// <inheritdoc/>
        public override Playable CreateTrackMixer(PlayableGraph graph, GameObject go, int inputCount)
        {
            return ScriptPlayable<GroupAlphaMixerBehaviour>.Create(graph, inputCount);
        }

        /// <summary>
        /// Registers the bound <see cref="CanvasGroup"/>'s alpha with Timeline's preview system, so scrubbing in the
        /// editor does not leave the alpha permanently modified once preview ends.
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
                    driver.AddFromName<CanvasGroup>(cg.gameObject, "m_Alpha");
                }
            }
        }
    }
}
