using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;
using UniRx.Async;
using UnityEngine.ResourceManagement.AsyncOperations;

/// <summary>
/// TODO : Make it so that getting sprite name is able to load Addressable from multiple atlas source.
/// </summary>
public class ResourceSpriteAtlas
{
    private string Address { get; }

    private SpriteAtlas loadedAtlas;
    private Sprite[] allSprites;

    public bool NotLoaded => loadedAtlas == null;

    //Because we cannot check valid status of IAsync
    private bool loadTextureCalled = false;
    AsyncOperationHandle<SpriteAtlas> loadTextureOperation;

    /// <param name="address">Put the string of Addressable Asset System here.</param>
    public ResourceSpriteAtlas(string address)
    {
        this.Address = address;
    }

    /// <summary>
    /// Calling on this multiple times is safe. There will be no load on subsequent calls.
    /// </summary>
    public async UniTask LoadTextures()
    {
        if (loadTextureCalled == false)
        {
            loadTextureCalled = true;
            loadTextureOperation = Addressables.LoadAsset<SpriteAtlas>(Address); //should be fast
            loadedAtlas = await loadTextureOperation;
            if (loadedAtlas == null)
            {
                throw new System.IO.IOException($"The address {Address} returns null atlas!");
            }
            allSprites = new Sprite[loadedAtlas.spriteCount];
            loadedAtlas.GetSprites(allSprites); //slow
        }
        else
        {
            await loadTextureOperation;
        }
    }

    public void UnloadTextures()
    {
        if (loadedAtlas != null && loadTextureCalled )
        {
            Addressables.ReleaseAsset(loadedAtlas);
            loadedAtlas = null;
            loadTextureCalled = false;
        }
    }

    /// <summary>
    /// If you didn't call LoadTextures() first this will be an error!!
    /// </summary>
    public Sprite GetSpriteNamed(string spriteName)
    {
        if(loadedAtlas == null)
        {
            throw new System.IO.IOException("You must call LoadTexture() first!");
        }
        Sprite sp = loadedAtlas.GetSprite(spriteName);

        return sp ? sp : throw new System.IO.IOException($"{spriteName} is not in the atlas {loadedAtlas.name}! Oh no!");
    }

}