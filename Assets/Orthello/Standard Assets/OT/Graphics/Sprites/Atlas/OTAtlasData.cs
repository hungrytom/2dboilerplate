using UnityEngine;
using System.Collections;

/// <summary>
/// Stores data of a specific atlas frame.
/// </summary>
[System.Serializable]
public class OTAtlasData
{
    /// <summary>
    /// This frame's name
    /// </summary>
    public string name = "";
    /// <summary>
    /// This frame's atlas position
    /// </summary>
    public Vector2 position = Vector2.zero;
    /// <summary>
    /// This frame's position offset
    /// </summary>
    public Vector2 offset = Vector2.zero;
    /// <summary>
    /// if this image is rotated on Atlas
    /// </summary>
    public bool rotated = false;
    /// <summary>
    /// This frame's atlas size
    /// </summary>
    public Vector2 size = Vector2.zero;
    /// <summary>
    /// atlas frame size
    /// </summary>
    public Vector2 frameSize = Vector2.zero;
}