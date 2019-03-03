using System;
using UnityEngine;

namespace E7.PlayerPrefsExtension
{
    /// <summary>
    /// Use `enum` to write a specific `PlayerPrefs` data.
    /// </summary>
    public static class SetPersistent
    {
        /// <summary>
        /// Get a `bool` state of the `enum` key <param name="e">.
        /// </summary>
        public static void State<ENUM>(ENUM e, bool setTo)
        where ENUM : struct, Enum
            => Index(e, setTo ? 1 : 0);

        /// <summary>
        /// Use an another set of `enum` instead of `bool` for a state to remember something that has more than 2 choices but still finite.
        /// It is saved as a string representation of <param name="setTo">.
        /// </summary>
        /// <typeparam name="ENUM">Type of the enum key.</typeparam>
        /// <typeparam name="ENUM2">Type of the state to remember.</typeparam>
        public static void State<ENUM, ENUM2>(ENUM e, ENUM2 setTo)
        where ENUM : struct, Enum
        where ENUM2 : struct, Enum
            => PlayerPrefs.SetString(PersistentInternal.EnumToKey(e), setTo.ToString());

        /// <summary>
        /// Get an `int` index of the `enum` key <param name="e">.
        /// </summary>
        public static void Index<ENUM>(ENUM e, int setTo)
        where ENUM : struct, Enum
            => PlayerPrefs.SetInt(PersistentInternal.EnumToKey(e), setTo);
    }

}