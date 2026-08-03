using System;
using UnityEngine;

namespace E7.E7Unity
{
    /// <summary>
    /// Which kind of input the player is steering the interface with, so that only one kind of cursor is on screen
    /// at a time.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A mouse and a keyboard both work at once, and both want to show where they are — a hover under the pointer, a
    /// ring around the focused widget. Showing both reads as two cursors, and the player cannot tell which one
    /// Enter will hit. Whichever device was used last is the one that gets to point.
    /// </para>
    /// <para>
    /// <see cref="SelectableAnimatorUi"/> reports the events it already receives, so the mode follows the player
    /// without anything else being wired up: landing the pointer on a widget or pressing one counts as pointing, and
    /// navigating counts as directional. A game that reads its devices itself can report the switch earlier and
    /// more precisely through <see cref="ReportPointer"/> and <see cref="ReportDirectional"/> — a mouse crossing
    /// empty space, or the first arrow key pressed while nothing is selected, are moments no widget ever hears
    /// about.
    /// </para>
    /// </remarks>
    /// <seealso cref="SelectableAnimatorUi"/>
    public static class UiInputMode
    {
        public enum Mode
        {
            /// <summary>A pointing device is steering, so hover belongs on whatever sits under it.</summary>
            Pointer,

            /// <summary>Keyboard or gamepad navigation is steering, so the focused widget carries the cursor.</summary>
            Directional,
        }

        private static Mode current = Mode.Pointer;

        /// <summary>
        /// Raised when the player switches between pointing and navigating.
        /// </summary>
        public static event Action Changed;

        /// <summary>
        /// Which kind of input was used last.
        /// </summary>
        public static Mode Current => current;

        /// <summary>
        /// Whether a pointing device is the one currently steering.
        /// </summary>
        public static bool IsPointer => current == Mode.Pointer;

        /// <summary>
        /// Report that a pointing device was used.
        /// </summary>
        public static void ReportPointer()
        {
            Report(Mode.Pointer);
        }

        /// <summary>
        /// Report that keyboard or gamepad navigation was used.
        /// </summary>
        public static void ReportDirectional()
        {
            Report(Mode.Directional);
        }

        /// <summary>
        /// Report which kind of input was used, raising <see cref="Changed"/> only when that differs from the kind
        /// already steering.
        /// </summary>
        public static void Report(Mode mode)
        {
            if (current == mode)
                return;

            current = mode;
            Changed?.Invoke();
        }

        /// <summary>
        /// Static state outlives a play session wherever domain reload is turned off, so the mode and its listeners
        /// start over with the runtime rather than with the domain.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            current = Mode.Pointer;
            Changed = null;
        }
    }
}
