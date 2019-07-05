#if HAS_AAS
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using System;

#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

#if ODIN_INSPECTOR
[InlineProperty]
#endif
[Serializable]
public class AddressableSpriteAtlas
{

#pragma warning disable 0649
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    [SerializeField] private AssetReference atlasAddress;
#pragma warning restore 0649

    private SpriteAtlas loadedAtlas;

    /// <summary>
    /// - If called once before, get the sprite immediately.
    /// - If some other AddressablesSpriteAtlas loaded the same sheet before also get the sprite immediately.
    /// </summary>
    public AsyncOperationHandle<Sprite> LoadSprite(string spriteName)
    {
        //Debug.Log($"Loading {spriteName}");
        if (atlasAddress == null || !atlasAddress.RuntimeKeyIsValid())
        {
            //Returns null sprite when it is empty
            return Addressables.ResourceManager.CreateCompletedOperation<Sprite>(null, "Atlas address is null");
        }

        //Debug.Log($"Ok {spriteName} is not null!");
        Sprite sp;

        if (loadedAtlas == null)
        {
            var atlLoad = atlasAddress.LoadAssetAsync<SpriteAtlas>();

            return Addressables.ResourceManager.CreateChainOperation<Sprite, SpriteAtlas>(atlLoad, (atl) =>
            {
                loadedAtlas = atl.Result;

                sp = loadedAtlas.GetSprite(spriteName);
                if (sp == null)
                {
                    throw new System.Exception("Loading sprite atlas " + atl.Result.name + " succeeded but sprite named " + spriteName + "is not in it.");
                }

                return Addressables.ResourceManager.CreateCompletedOperation<Sprite>(sp, string.Empty);
            });
        }

        sp = loadedAtlas.GetSprite(spriteName);
        if (sp == null)
        {
            throw new System.Exception("Sprite named " + spriteName + "is not in the loaded atlas " + loadedAtlas.name);
        }
        return Addressables.ResourceManager.CreateCompletedOperation<Sprite>(sp, "no error");
    }

    /// <summary>
    /// Decrease the reference count for the associating sprite atlas.
    /// </summary>
    public void ReleaseAtlas()
    {
        Addressables.Release(loadedAtlas);
        loadedAtlas = null;
    }
}
#endif