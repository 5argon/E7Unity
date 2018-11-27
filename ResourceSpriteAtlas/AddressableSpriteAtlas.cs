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
        //Debug.Log($"Loading {spriteName}");
        if(atlasAddress == null || atlasAddress.RuntimeKey == Hash128.Parse(""))
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
                loadedAtlas = atl;
                
                sp = loadedAtlas.GetSprite(spriteName);
                if(sp == null)
                {
                    throw new System.Exception("Loading sprite atlas " + atl.name + " succeeded but sprite named " + spriteName + "is not in it.");
                }

                return new CompletedOperation<Sprite>().Start(null, null,sp );
            });
        }

        sp = loadedAtlas.GetSprite(spriteName);
        if(sp == null)
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
