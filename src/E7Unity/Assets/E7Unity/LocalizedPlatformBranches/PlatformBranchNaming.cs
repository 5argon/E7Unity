#if E7UNITY_LOCALIZATION
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Localization.Metadata;

namespace E7.E7Unity
{
    /// <summary>
    /// The naming convention that pairs a branch root entry with the per-platform entries it branches to, added to
    /// the Localization Settings' metadata under <c>E7Unity</c>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A string that branches per platform is authored as one entry per platform plus a root entry that holds the
    /// <c>PlatformOverride</c> metadata. Suffixing them consistently — <c>…Generic</c>, <c>…iOS</c>,
    /// <c>…Android</c> — makes the branching legible from the key alone, which matters at the places a table cannot
    /// reach: the component inspector, a prefab's serialized data, a code reference.
    /// </para>
    /// <para>
    /// The convention only ever generates names and reports mismatches. It never decides where a branch actually
    /// goes, because that is recorded explicitly in the metadata and an entry is free to disagree with the naming.
    /// Adding this metadata is optional; the defaults below apply when it is absent, so nothing needs configuring
    /// before the branching inspector works.
    /// </para>
    /// </remarks>
    [Metadata(AllowedTypes = MetadataType.LocalizationSettings, AllowMultiple = false,
        MenuItem = "E7Unity/Platform Branch Naming")]
    [Serializable]
    public class PlatformBranchNaming : IMetadata
    {
        /// <summary>One platform, and the suffix its branch entries carry.</summary>
        [Serializable]
        public class PlatformSuffix
        {
            [SerializeField] RuntimePlatform m_Platform = RuntimePlatform.IPhonePlayer;
            [SerializeField] string m_Suffix = "iOS";

            /// <summary>The platform this suffix names.</summary>
            public RuntimePlatform Platform => m_Platform;

            /// <summary>The suffix appended to the root key for this platform's entry.</summary>
            public string Suffix => m_Suffix;

            public PlatformSuffix() { }

            public PlatformSuffix(RuntimePlatform platform, string suffix)
            {
                m_Platform = platform;
                m_Suffix = suffix;
            }
        }

        [SerializeField]
        [Tooltip("Suffix on the entry that holds the Platform Override metadata and is referenced by components.")]
        string m_RootSuffix = "Generic";

        [SerializeField]
        [Tooltip("The platforms a string may branch to, and the suffix each branch entry carries.")]
        List<PlatformSuffix> m_Platforms = new List<PlatformSuffix>
        {
            new PlatformSuffix(RuntimePlatform.IPhonePlayer, "iOS"),
            new PlatformSuffix(RuntimePlatform.Android, "Android"),
        };

        /// <summary>Suffix carried by the entry that holds the metadata and that components reference.</summary>
        public string RootSuffix => m_RootSuffix;

        /// <summary>The platforms a string may branch to, in the order they are presented.</summary>
        public IReadOnlyList<PlatformSuffix> Platforms => m_Platforms;

        /// <summary>
        /// The key a branch entry for <paramref name="platform" /> should be called, given a root key. Returns
        /// <see langword="null" /> when the platform is not one of <see cref="Platforms" />.
        /// </summary>
        public string BranchKey(string rootKey, RuntimePlatform platform)
        {
            string stem = Stem(rootKey);
            foreach (PlatformSuffix p in m_Platforms)
            {
                if (p.Platform == platform) return stem + p.Suffix;
            }
            return null;
        }

        /// <summary>
        /// The part of <paramref name="key" /> before its suffix, so that branch keys can be built from a root key
        /// whether or not it already carries one.
        /// </summary>
        public string Stem(string key)
        {
            if (string.IsNullOrEmpty(key)) return key;
            if (EndsWith(key, m_RootSuffix)) return key.Substring(0, key.Length - m_RootSuffix.Length);
            foreach (PlatformSuffix p in m_Platforms)
            {
                if (EndsWith(key, p.Suffix)) return key.Substring(0, key.Length - p.Suffix.Length);
            }
            return key;
        }

        /// <summary>True when <paramref name="key" /> is named as a branch root.</summary>
        public bool IsRootKey(string key) => EndsWith(key, m_RootSuffix);

        static bool EndsWith(string key, string suffix)
            => !string.IsNullOrEmpty(suffix) && !string.IsNullOrEmpty(key)
               && key.EndsWith(suffix, StringComparison.Ordinal);
    }
}
#endif
