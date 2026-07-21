#if E7UNITY_LOCALIZATION
using TMPro;
using UnityEngine;
using UnityEngine.Localization;
using UnityEngine.Localization.Components;

namespace E7.E7Unity
{
    /// <summary>
    /// Swaps a TextMeshPro component's font asset per locale. Put it on the same GameObject as a
    /// <see cref="TMP_Text"/> and point it at an asset table entry that holds a
    /// <see cref="TMP_FontAsset"/> for each language; the font then follows the active locale.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Because the fonts live in an asset table, adding a language is a data-only change — fill in
    /// that entry's column, with no code or per-object rewiring. Put the component on a base text
    /// prefab and every instance inherits the behaviour.
    /// </para>
    /// <para>
    /// It runs in edit mode as well (<see cref="ExecuteAlways"/>) so the font previews in the
    /// scene, and it registers the properties it writes with Localization's property driver, so
    /// the swap neither dirties the scene nor shows up as a prefab override.
    /// </para>
    /// <para>
    /// Only compiled when the Unity Localization package (<c>com.unity.localization</c>) is
    /// present, gated by the <c>E7UNITY_LOCALIZATION</c> define.
    /// </para>
    /// </remarks>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public class LocalizeTmpFontAsset : LocalizedAssetBehaviour<TMP_FontAsset, LocalizedTmpFont>
    {
        TMP_Text _tmp;

        /// <summary>Applies <paramref name="font"/> to the sibling <see cref="TMP_Text"/> when the locale resolves.</summary>
        protected override void UpdateAsset(TMP_FontAsset font)
        {
            if (font == null) return;
            if (_tmp == null) _tmp = GetComponent<TMP_Text>();
            if (_tmp == null) return;

            // Assigning the font runs LoadFontAsset, which also writes m_sharedMaterial.
            EditorPropertyDriver.RegisterProperty(_tmp, "m_fontAsset");
            EditorPropertyDriver.RegisterProperty(_tmp, "m_sharedMaterial");
            _tmp.font = font;
        }
    }
}
#endif
