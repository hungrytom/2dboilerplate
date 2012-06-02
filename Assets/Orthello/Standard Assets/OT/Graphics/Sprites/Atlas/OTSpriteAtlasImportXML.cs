using UnityEngine;
using System.Collections;
using System.Xml;

/// <summary>
/// Base class for importing a sprite atlas from an XML file
/// </summary>
public class OTSpriteAtlasImportXML : OTSpriteAtlasImport
{
    /// <exclude />
    protected XmlDocument xml = new XmlDocument();
    /// <summary>
    /// Check if xml provided is valid
    /// </summary>
    /// <returns>Array with atlas frame data</returns>
    protected bool ValidXML()
    {
        try
        {
            xml.LoadXml(atlasDataFile.text);
            return true;
        }
        catch (System.Exception err)
        {
            Debug.LogError("Orthello : Atlas XML file could not be read!");
            Debug.LogError(err.Message);
        } 
        return false;
    }
}