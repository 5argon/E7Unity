using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.U2D;

public class ResourceSpriteAtlas
{
    private string ResourcePath { get; }

    public SpriteAtlas Atlas => Resources.Load<SpriteAtlas>(ResourcePath); //does not consume memory, loading is fast
    private Sprite[] allSprites;
    public bool Loaded { get; private set; }

    public ResourceSpriteAtlas(string resourcePath)
    {
        this.ResourcePath = resourcePath;
    }

    /// <summary>
    /// Overload for only one layer of folder in Resources.
    /// </summary>
    public ResourceSpriteAtlas(string folder, string atlasName) : this(folder + Path.DirectorySeparatorChar + atlasName) { }

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
        foreach(Sprite s in allSprites)
        {
            Resources.UnloadAsset(s.texture);
        }
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