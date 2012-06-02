using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// OT Helper class to store object prefab references.
/// </summary>
[System.Serializable]
public class OTObjectType {
    /// <summary>
    /// Object type name.
    /// </summary>
    public string name = "";
    /// <summary>
    /// Object prototype.
    /// </summary>
    public GameObject prototype;

    /// <summary>
    /// Lookup table to find an object prototype by name.
    /// </summary>
    public static Dictionary<string, GameObject> lookup = new Dictionary<string, GameObject>();

    /// <exclude />
    public static string Sprite
    {
        get
        {
            return "Sprite";
        }
    }
    /// <exclude />
    public static string FilledSprite
    {
        get
        {
            return "FilledSprite";
        }
    }
    /// <exclude />
    public static string AnimatingSprite
    {
        get
        {
            return "AnimatingSprite";
        }
    }
    /// <exclude />
    public static string Animation
    {
        get
        {
            return "Animation";
        }
    }
    /// <exclude />
    public static string SpriteSheet
    {
        get
        {
            return "SpriteSheet";
        }
    }
    /// <exclude />
    public static string SpriteBatch
    {
        get
        {
            return "SpriteBatch";
        }
    }
    /// <exclude />
    public static string SpriteAtlas
    {
        get
        {
            return "SpriteAtlas";
        }
    }

}
