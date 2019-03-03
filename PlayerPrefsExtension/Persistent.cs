using System;
using UnityEngine;

namespace E7.PlayerPrefsExtension
{
    /// <summary>
    /// Use `enum` to read a specific `PlayerPrefs` data.
    /// </summary>
    public static class Persistent
    {
        /// <summary>
        /// Default to `false` if no such state.
        /// </summary>
        /// <typeparam name="ENUM">Type of the enum key.</typeparam>
        public static bool State<ENUM>(ENUM e)
        where ENUM : struct, Enum
            => Index(e) == 1 ? true : false;

        /// <summary>
        /// Used to check the state that is holding an `enum` inside instead of `bool`
        /// Note that the serialized thing is the string of that enum's name (the English name), not an integer or string-as-integer.
        /// If 2 `enum` have the same value English name, one `enum`'s serialized name will be usable with the other.
        /// </summary>
        /// <typeparam name="ENUM">Type of the enum key.</typeparam>
        /// <typeparam name="ENUM2">Type of the desired state.</typeparam>
        public static ENUM2 State<ENUM, ENUM2>(ENUM e)
        where ENUM : struct, Enum
        where ENUM2 : struct, Enum
        {
            string enumString = PlayerPrefs.GetString(PersistentInternal.EnumToKey(e));
            return Enum.TryParse<ENUM2>(enumString, out var parsed) ? parsed : default;
        }

        /// <summary>
        /// Default to 0 if no such index.
        /// </summary>
        /// <typeparam name="ENUM">Type of the enum key.</typeparam>
        public static int Index<ENUM>(ENUM e)
        where ENUM : struct, Enum
            => PlayerPrefs.GetInt(PersistentInternal.EnumToKey(e));

        /// <summary>
        /// It works like <see cref="PlayerPrefs.DeleteKey(string)"> but with `enum`.
        /// </summary>
        public static void Delete<ENUM>(ENUM e)
            where ENUM : struct, Enum
            => PlayerPrefs.DeleteKey(PersistentInternal.EnumToKey(e));

    }

}