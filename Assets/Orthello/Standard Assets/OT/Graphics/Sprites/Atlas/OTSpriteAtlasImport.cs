using UnityEngine;
using System.Collections;
using System.Xml;

/// <summary>
/// Base class for importing sprite atlasses
/// </summary>
public class OTSpriteAtlasImport : OTSpriteAtlas
{
    
    /// <exclude />
    public TextAsset _atlasDataFile = null;
    /// <summary>
    /// Will reload the atlas data
    /// </summary>
    public bool reloadData = false;
	
	[HideInInspector]
	public int bytesDataFile = 0;
	
    /// <summary>
    /// Atlas data file to import framedata from
    /// </summary>
    public TextAsset atlasDataFile
    {
        get
        {
            return _atlasDataFile;
        }
        set
        {
            _atlasDataFile = value;
            Update();
        }
    }
	
    /// <exclude />
    public bool reloadFrame
    {
        get
        {
            return _reloadFrame;
        }
    }

    private TextAsset _atlasDataFile_ = null;	
	private bool _reloadFrame = false;
	
    /// <exclude />
    new protected void Start()
    {
		if (atlasDataFile!=null && atlasData.Length>0)
        	_atlasDataFile_ = atlasDataFile;		
		else
			_reloadFrame = true;
        base.Start();
    }

    /// <summary>
    /// Override this Import method to load the atlas data from the xml
    /// </summary>
    /// <returns>Array with atlas frame data</returns>
    protected virtual OTAtlasData[] Import()
    {
        return new OTAtlasData[] { };
    }

    /// <exclude />
    new protected void Update()
    {
        if (_atlasDataFile_!=atlasDataFile || reloadData || (atlasDataFile!=null && bytesDataFile!=atlasDataFile.bytes.Length))
        {
            _atlasDataFile_ = atlasDataFile;
            if (atlasDataFile != null)
            {
				bytesDataFile = atlasDataFile.bytes.Length;
                atlasReady = false;
                atlasData = Import();
                atlasReady = true;
            }
			else
				bytesDataFile = 0;
			
            if (reloadData)
                reloadData = false;
#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif										
			
        }

        base.Update();
    }
}