using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using System;
using UnityEngine.ResourceManagement;
using Sirenix.OdinInspector;

#if ODIN_INSPECTOR
[InlineProperty]
#endif
[Serializable]
public class AddressableSpriteAtlas
{
#if ODIN_INSPECTOR
    [HideLabel]
#endif
    [SerializeField] private AssetReference atlasAddress;
    private SpriteAtlas loadedAtlas;

    /// <summary>
    /// - If called once before, get the sprite immediately.
    /// - If some other AddressablesSpriteAtlas loaded the same sheet before also get the sprite immediately.
    /// </summary>
    public IAsyncOperation<Sprite> LoadSprite(string spriteName)
    {
        if(atlasAddress == null || atlasAddress.RuntimeKey == Hash128.Parse(""))
        {
            //Returns null sprite when it is empty
            return new CompletedOperation<Sprite>().Start(null, null, null);
        }

        if (loadedAtlas == null)
        {
            var atlLoad = atlasAddress.LoadAsset<SpriteAtlas>();

            return new ChainOperation<Sprite, SpriteAtlas>().Start(null, null, atlLoad, (atl) =>
            {
                loadedAtlas = atl;
                return new CompletedOperation<Sprite>().Start(null, null, loadedAtlas.GetSprite(spriteName));
            });
        }
        return new CompletedOperation<Sprite>().Start(null, null, loadedAtlas.GetSprite(spriteName));
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
