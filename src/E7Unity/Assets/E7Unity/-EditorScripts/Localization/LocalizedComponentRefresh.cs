#if E7UNITY_LOCALIZATION
using System.Reflection;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
using UnityEngine.SceneManagement;

namespace E7.E7Unity.Localization
{
    /// <summary>
    /// Makes every loaded Localization component look its entry up again, for the cases where something other
    /// than the selected locale decided which entry answers.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A locale change fans out through Localization's own events, but a <c>PlatformOverride</c> is read inside
    /// the table entry lookup, so changing which platform is simulated leaves already-resolved components
    /// showing the entry they picked earlier.
    /// </para>
    /// <para>
    /// The lookup is re-run through <c>LocalizedReference.ForceUpdate</c>, which is the only path that reaches
    /// <c>GetTableEntryAsync</c> and therefore the only one that re-evaluates an entry override. Its public
    /// neighbours do not: <c>LocalizeStringEvent.RefreshString</c> re-formats the entry already held by the
    /// current loading operation, and every property setter that would force an update returns early when
    /// assigned its existing value. <c>ForceUpdate</c> itself is <c>protected internal</c>, so it is reached by
    /// reflection, applied to every <see cref="LocalizedReference" /> a component exposes so that strings,
    /// sprites, textures and asset references are all covered by one pass.
    /// </para>
    /// <para>
    /// Re-running the lookup is cheap — tables stay cached and only the entry is resolved again — and the
    /// values it writes go through Localization's property drivers, so they do not count as scene or prefab
    /// modifications.
    /// </para>
    /// </remarks>
    public static class LocalizedComponentRefresh
    {
        static readonly MethodInfo ForceUpdateMethod = typeof(LocalizedReference).GetMethod(
            "ForceUpdate", BindingFlags.Instance | BindingFlags.NonPublic);

        /// <summary>
        /// Refreshes the components of every loaded scene, plus the contents of an open prefab stage, which
        /// lives in a preview scene of its own.
        /// </summary>
        public static void RefreshLoadedComponents()
        {
            if (ForceUpdateMethod == null) return;

            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                Scene scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (GameObject root in scene.GetRootGameObjects())
                {
                    RefreshBranch(root);
                }
            }

            PrefabStage stage = PrefabStageUtility.GetCurrentPrefabStage();
            if (stage != null)
            {
                RefreshBranch(stage.prefabContentsRoot);
            }
        }

        static void RefreshBranch(GameObject root)
        {
            foreach (LocalizedMonoBehaviour behaviour in root.GetComponentsInChildren<LocalizedMonoBehaviour>(true))
            {
                Refresh(behaviour);
            }
        }

        static void Refresh(LocalizedMonoBehaviour behaviour)
        {
            foreach (PropertyInfo property in behaviour.GetType().GetProperties(
                BindingFlags.Instance | BindingFlags.Public))
            {
                if (!typeof(LocalizedReference).IsAssignableFrom(property.PropertyType)) continue;
                if (property.GetIndexParameters().Length != 0) continue;

                if (property.GetValue(behaviour) is LocalizedReference reference)
                {
                    ForceUpdateMethod.Invoke(reference, null);
                }
            }
        }
    }
}
#endif
