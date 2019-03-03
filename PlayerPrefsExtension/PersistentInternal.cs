using System;
using UnityEngine;

namespace E7.PlayerPrefsExtension
{
    internal static class PersistentInternal
    {
        internal const string stateSuffix = "-state";
        internal const string indexSuffix = "-index";

        /// <summary>
        /// Reminder that you are exposing your assembly qualified name of your enum to the public because it would show up in `PlayerPref`'s plist file.
        /// </summary>
        internal static string EnumToKey<ENUM>(ENUM e)
        where ENUM : Enum
        {
            Type enumType = e.GetType();
            string key = $"{Enum.GetName(enumType, e)}, {enumType.AssemblyQualifiedName}";
            //Debug.Log($"{key}");
            return key;
        }

    }

}