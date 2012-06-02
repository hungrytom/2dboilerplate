using UnityEngine;
using System.Collections;
 
/// <summary>
/// Provides base functionality to handle textures with multiple image frames.
/// </summary>
[ExecuteInEditMode]
public class OTContainer : MonoBehaviour
{
    /// <exclude />
    public string _name = "";
    bool registered = false;
    /// <exclude />    
    protected bool dirtyContainer = true;
    /// <exclude />
    protected string _name_ = "";	

    /// <summary>
    /// Stores texture data of a specific container frame.
    /// </summary>
    public struct Frame
    {
        /// <summary>
        /// This frame's name
        /// </summary>
        public string name;
        /// <summary>
        /// This frame's image scale modifier
        /// </summary>
        public Vector2 size;
        /// <summary>
        /// This frame's original image size
        /// </summary>
        public Vector2 imageSize;
        /// <summary>
        /// This frame's world position offset modifier
        /// </summary>
        public Vector2 offset;
        /// <summary>
        /// This frame's world rotation modifier
        /// </summary>
        public float rotation;
        /// <summary>
        /// Texture UV coordinates (x/y).
        /// </summary>
        public Vector2[] uv;
        /// <summary>
        /// Mesh vertices used when OffsetSizing = false (Atlas)
        /// </summary>
        public Vector3[] vertices;
    }

    Frame[] frames = { };

    /// <summary>
    /// Name of the container
    /// </summary>
    new public string name
    {
        get
        {
            return _name;
        }
        set
        {
            string old = _name;
            _name = value;
            gameObject.name = _name;
            if (OT.isValid)
            {
                _name_ = _name;
                OT.RegisterContainerLookup(this, old);
            }
        }
    }
    /// <summary>
    /// Container ready indicator.
    /// </summary>
    /// <remarks>
    /// Container frame data or container texture can only be accessed when a container is ready.
    /// Be sure to check this when retrieving data programmaticly.
    /// </remarks>
    public bool isReady
    {
        get
        {
            return frames.Length > 0;
        }
    }
    /// <summary>
    /// Number of frames in this container.
    /// </summary>
    public int frameCount
    {
        get
        {
            return frames.Length;
        }
    }
    /// <summary>
    /// Overridable virtal method to provide a container's texture
    /// </summary>
    /// <returns>Container's texture</returns>
    public virtual Texture GetTexture()
    {
        return null;
    }
    /// <summary>
    /// Overridable virtal method to provide the container's frames
    /// </summary>
    /// <returns>Container's array of frames</returns>
    protected virtual Frame[] GetFrames()
    {
        return new Frame[] { };
    }

    /// <summary>
    /// Return the frame number by its name or -1 if it doesn't exist. 
    /// </summary>
    public virtual int GetFrameIndex(string inName)
    {
        for (int i = 0; i < frames.Length; ++i)
        {
            if (frames[i].name == inName)
            {
                return i;
            }
        }

        return -1;
    }
	
	protected void Awake()
	{
#if UNITY_EDITOR
		if (!Application.isPlaying)
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif				
	}

    // Use this for initialization
    /// <exclude />
    protected void Start()
    {
        // initialize attributes
        // initialize attributes
        _name_ = name;
        if (name == "")
		{
            name = "Container (id=" + this.gameObject.GetInstanceID() + ")";
#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif										
		}

        RegisterContainer();
    }

    /// <summary>
    /// Retrieve a specific container frame.
    /// </summary>
    /// <remarks>
    /// The container frame contains data about each frame's texture offset and UV coordinates. The texture offset and scale 
    /// is used when this frame is mapped onto a single sprite. The UV coordinates are used when this images has to be mapped onto 
    /// a multi sprite mesh ( a SpriteBatch for example ).
    /// <br></br><br></br>
    /// When the index is out of bounce, an IndexOutOfRangeException  will be raised.
    /// </remarks>
    /// <param name="index">Index of container frame to retrieve. (starting at 0)</param>
    /// <returns>Retrieved container frame.</returns>
    public Frame GetFrame(int index)
    {
        if (frames.Length > index)
            return frames[index];
        else
        {
            throw new System.IndexOutOfRangeException("Frame index out of bounds ["+index+"]");
        }
    }

    void RegisterContainer()
    {
        if (OT.ContainerByName(name) == null)
        {
            OT.RegisterContainer(this);
            gameObject.name = name;
            registered = true;
        }
        if (_name_ != name)
        {
            OT.RegisterContainerLookup(this, _name_);
            _name_ = name;
            gameObject.name = name;
        }

        if (name != gameObject.name)
        {
            name = gameObject.name;
            OT.RegisterContainerLookup(this, _name_);
            _name_ = name;
 #if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif		
       }
    }

    // Update is called once per frame
    /// <exclude />
    protected void Update()
    {

        if (!OT.isValid) return;

        if (!registered || !Application.isPlaying)
            RegisterContainer();

        if (frames.Length == 0 && !dirtyContainer)
            dirtyContainer = true;

        if (dirtyContainer || !isReady)
        {
            frames = GetFrames();
            dirtyContainer = false;
        }
    }

    void OnDestroy()
    {
        if (OT.isValid)
            OT.RemoveContainer(this);
    }

}