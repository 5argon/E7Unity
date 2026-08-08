using UnityEngine;
using UnityEngine.UI;

namespace E7.E7Unity
{
    /// <summary>
    /// Empties uGUI's static registry of selectables as the runtime starts.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <see cref="Selectable"/> keeps every enabled selectable in a static array with a static count, and clears
    /// neither. With editor Domain Reload turned off both outlive a play session, so whatever the previous one left
    /// behind is what the next one starts from.
    /// </para>
    /// <para>
    /// That matters because the pair can be driven apart and never recover. <c>OnEnable</c> increments the count
    /// before it marks the selectable as enabled, and the two statements have a <c>DoStateTransition</c> call
    /// between them; an exception from that call leaves the count raised while <c>OnDisable</c> — which returns
    /// early unless the selectable marked itself enabled — declines to lower it again. The array grows only when
    /// the count is exactly equal to its length, so once the count has passed the length it can never grow, and
    /// every selectable enabled afterwards writes past the end of the array. One exception, anywhere, and the rest
    /// of the editor session throws <see cref="System.IndexOutOfRangeException"/> on every enable.
    /// </para>
    /// <para>
    /// Clearing both at <see cref="RuntimeInitializeLoadType.SubsystemRegistration"/> gives every play session the
    /// state a fresh domain would have. Nothing has registered by then — no scene has loaded — so there is nothing
    /// to lose. The fix covers every selectable in the project, not only the ones in this package, since the
    /// registry is shared.
    /// </para>
    /// <para>
    /// The class exists to reach those two fields, which are <c>protected static</c> and so visible only from a
    /// subclass. It is abstract because it is never meant to be added to anything.
    /// </para>
    /// </remarks>
    internal abstract class SelectableRegistryReset : Selectable
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ClearRegistry()
        {
            s_Selectables = new Selectable[10];
            s_SelectableCount = 0;
        }
    }
}
