using System;
using UnityEngine;
using UnityEngine.UI;

namespace E7.E7Unity
{
    /// <summary>
    /// Puts uGUI's static registry of selectables back into a state it can survive, without discarding anything
    /// registered in it.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Selectable"/> keeps every enabled selectable in a static array alongside a static count, and
    /// clears neither. With editor Domain Reload turned off both outlive a play session, so whatever the previous
    /// one left behind is what the next one starts from.
    /// </para>
    /// <para>
    /// The two can be driven apart and never recover. <c>OnEnable</c> raises the count before marking the selectable
    /// enabled, with a <c>DoStateTransition</c> call between the two, while <c>OnDisable</c> returns early unless
    /// that mark was set — so an exception out of that call leaves the count raised with nothing that will ever
    /// lower it. The array grows only when the count is exactly its length, so a count that has passed the length
    /// can never grow again, and every selectable enabled from then on writes past the end.
    /// </para>
    /// <para>
    /// Emptying the registry looks like the obvious repair and is a trap. A prefab opened for editing keeps its
    /// preview scene alive across a play-mode transition, so selectables can already be registered when a play
    /// session begins; dropping the count to zero underneath them sends it negative as they disable, which fails
    /// the same way from the other end. What this does instead is widen the array to fit the count and lift a
    /// negative count back to zero. Neither throws a registration away, and neither state is one the registry can
    /// reach on its own.
    /// </para>
    /// <para>
    /// The class exists to reach those two fields, which are <c>protected static</c> and so visible only from a
    /// subclass. It is abstract because it is never meant to be added to anything.
    /// </para>
    /// </remarks>
    internal abstract class SelectableRegistryRepair : Selectable
    {
        /// <summary>
        /// Make the registry safe to write to. Cheap enough to call before every enable, and does nothing at all
        /// unless the count and the array have come apart.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        internal static void Repair()
        {
            if (s_Selectables == null)
                s_Selectables = new Selectable[10];

            if (s_SelectableCount < 0)
                s_SelectableCount = 0;

            if (s_SelectableCount < s_Selectables.Length)
                return;

            Selectable[] widened = new Selectable[Mathf.Max(s_Selectables.Length * 2, s_SelectableCount + 1)];
            Array.Copy(s_Selectables, widened, s_Selectables.Length);
            s_Selectables = widened;
        }
    }
}
