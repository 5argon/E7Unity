using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement;
using System;
using Sirenix.OdinInspector;

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
        if (atlasAddress == null || atlasAddress.RuntimeKey == Hash128.Parse(""))
        {
            //Returns null sprite when it is empty
            return new CompletedOperation<Sprite>().Start(null, null, null);
        }

        //Debug.Log($"Ok {spriteName} is not null!");
        Sprite sp;

        if (loadedAtlas == null)
        {
            var atlLoad = atlasAddress.LoadAsset<SpriteAtlas>();

            return new ChainOperation<Sprite, SpriteAtlas>().Start(null, null, atlLoad, (atl) =>
            {
                loadedAtlas = atl.Result;

                sp = loadedAtlas.GetSprite(spriteName);
                if (sp == null)
                {
                    throw new System.Exception("Loading sprite atlas " + atl.Result.name + " succeeded but sprite named " + spriteName + "is not in it.");
                }

                return new CompletedOperation<Sprite>().Start(null, null, sp);
            });
        }

        sp = loadedAtlas.GetSprite(spriteName);
        if (sp == null)
        {
            throw new System.Exception("Sprite named " + spriteName + "is not in the loaded atlas " + loadedAtlas.name);
        }
        return new CompletedOperation<Sprite>().Start(null, null, sp);
    }

    /// <summary>
    /// Decrease the reference count for the associating sprite atlas.
    /// </summary>
    public void ReleaseAtlas()
    {
        Addressables.ReleaseAsset(loadedAtlas);
        loadedAtlas = null;
    }
}
