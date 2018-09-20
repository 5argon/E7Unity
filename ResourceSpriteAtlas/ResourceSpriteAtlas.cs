using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;
using UnityEngine.AddressableAssets;

public class ResourceSpriteAtlas
{
    private string Address { get; }

    private SpriteAtlas loadedAtlas;
    private SpriteAtlas Atlas
    {
        get
        {
            if (loadedAtlas == null)
            {
                var async = Addressables.LoadAsset<SpriteAtlas>(Address); //does not consume memory, loading is fast
                while (async.IsDone == false) { }
                loadedAtlas = async.Result;
            }
            return loadedAtlas;
        }
    }
    private Sprite[] allSprites;
    public bool Loaded { get; private set; }

    /// <param name="address">Put the string of Addressable Asset System here.</param>
    public ResourceSpriteAtlas(string address)
    {
        this.Address = address;
    }

    /// <summary>
    /// Use this so getting sprite is fast on the first time.
    /// </summary>
    public void LoadTextures()
    {
        allSprites = new Sprite[Atlas.spriteCount];
        Atlas.GetSprites(allSprites);
        Loaded = true;
    }

    public void UnloadTextures()
    {
        Addressables.ReleaseAsset(loadedAtlas);
        Loaded = false;
    }

    public Sprite GetSpriteNamed(string spriteName)
    {
        if(!Loaded)
        {
            LoadTextures();
        }
        return Atlas.GetSprite(spriteName);
    }

}