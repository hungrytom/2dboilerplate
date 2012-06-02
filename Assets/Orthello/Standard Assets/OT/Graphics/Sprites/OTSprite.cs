using UnityEngine;
using System.Collections;
 
/// <summary>
/// Provides functionality to use static sprites (images) to your scenes.
/// </summary>
public class OTSprite : OTObject
{
    //-----------------------------------------------------------------------------
    // Editor settings
    //-----------------------------------------------------------------------------
    
    public bool _flipHorizontal = false;
    
    public bool _flipVertical = false;
    
    public bool _transparent = true;
    
    public bool _additive = false;
    
    public string _materialReference = "transparent";
    
    public Color _tintColor = Color.white;
    
    public float _alpha = 1.0f;
    
    public Texture _image = null;
    
    public int _frameIndex = 0;
    
    public OTContainer _spriteContainer;

    //-----------------------------------------------------------------------------
    // public attributes (get/set)
    //-----------------------------------------------------------------------------

    /// <summary>
    /// Flips sprite image horizontally
    /// </summary>
    public bool flipHorizontal
    {
        get
        {
            return _flipHorizontal;
        }
        set
        {
            _flipHorizontal = value;
            meshDirty = true;
            _flipHorizontal_ = _flipHorizontal;
            Update();
        }
    }

    /// <summary>
    /// Flips sprite image verically
    /// </summary>
    public bool flipVertical
    {
        get
        {
            return _flipVertical;
        }
        set
        {
            _flipVertical = value;
            meshDirty = true;
            _flipVertical_ = _flipVertical;
            Update();
        }
    }
	
    /// <summary>
    /// Sprite needs transparency support
    /// </summary>
    public bool transparent
    {
        get
        {
            return _transparent;
        }
        set
        {
            _transparent = value;
            Clean();
        }
    }
    /// <summary>
    /// Sprite needs additive transparency support
    /// </summary>
    public bool additive
    {
        get
        {
            return _additive;
        }
        set
        {
            _additive = value;
            Clean();
        }
    }

    /// <summary>
    /// Current texture of the sprite (image or spriteContainer)
    /// </summary>
    public Texture texture
    {
        get
        {
            if (spriteContainer != null)
                return spriteContainer.GetTexture();
            else
                return image;
        }
    }

    /// <summary>
    /// Default image texture for this sprite.
    /// </summary>
    public Texture image
    {
        get
        {
            return _image;
        }
        set
        {
            _image = value;
            Clean();
        }
    }

    /// <summary>
    /// Sprite Container that will provide image information for this sprite
    /// </summary>
    public OTContainer spriteContainer
    {
        get
        {
            return _spriteContainer;
        }
        set
        {
            _spriteContainer = value;
            Clean();
        }
    }
    /// <summary>
    /// Index of the frame that is used to get image information from the Sprite Container
    /// </summary>
    public int frameIndex
    {
        get
        {
            return _frameIndex;
        }
        set
        {
            _frameIndex = value;
            Clean();
        }
    }

    
    public Material material
    {
        get
        {
            if (Application.isPlaying)
                return renderer.material;
            else
                return renderer.sharedMaterial;
        }
        set
        {
            assignedMaterial = true;
            if (Application.isPlaying)
                renderer.material = value;
            else
                renderer.sharedMaterial = value;
        }
    }

    /// <summary>
    /// Reference name of material for this sprite
    /// </summary>
    public string materialReference
    {
        get
        {
            return _materialReference;
        }
        set
        {
            _materialReference = value;
            Clean();
        }
    }

    /// <summary>
    /// Tinting color of this sprite.
    /// </summary>
    /// <remarks>
    /// This setting will only work if this sprite's materialReference can work with color tinting.
    /// </remarks>
    public Color tintColor
    {
        get
        {
            return _tintColor;
        }
        set
        {
            _tintColor = value;
            Clean();
        }
    }
    /// <summary>
    /// Alpha channel for this sprite.
    /// </summary>
    /// <remarks>
    /// This setting will only work if this sprite's materialReference can work with alpha channels/color.
    /// </remarks>
    public float alpha
    {
        get
        {
            return _alpha;
        }
        set
        {
            _alpha = value;
            Clean();
        }
    }
	
	
	[HideInInspector]
	public string _containerName = "";
	
    //-----------------------------------------------------------------------------
    // protected and private  fields
    //-----------------------------------------------------------------------------
    OTContainer _spriteContainer_ = null;
    int _frameIndex_ = 0;
    bool _flipHorizontal_ = false;
    bool _flipVertical_ = false;
    Texture _image_ = null;
    bool _transparent_ = true;
    Color _tintColor_ = Color.white;
    float _alpha_ = 1;
    bool _additive_ = false;
    string _materialReference_ = "transparent";
    string lastMatName = "";
    Material lastMat = null;
    OTMatRef mr;
    bool assignedMaterial = false;
	bool spriteInvalid  = false;

    
    protected bool useUV = true;


    //-----------------------------------------------------------------------------
    // public methods
    //-----------------------------------------------------------------------------

    /// <summary>
    /// Retrieve frame data of the sprite's current frame. This data will include the
    /// texture scale, texture offset and uv coordinates that are needed to get the
    /// current frame's image.
    /// </summary>
    /// <returns>frame data of sprite's current frame</returns>
    public OTContainer.Frame CurrentFrame()
    {
        if (spriteContainer != null && spriteContainer.isReady)
            return spriteContainer.GetFrame(frameIndex);
        else
        {
            if (spriteContainer == null)
                throw new System.Exception("No Sprite Container available [" + name + "]");
            else
                throw new System.Exception("Sprite Container not ready [" + name + "]");
        }
    }

    
    public override void StartUp()
    {
        isDirty = true;
        Material mat = LookupMaterial();
        if (mat != null)
        {
            renderer.material = mat;
            HandleUV(mat);
        }
        base.StartUp();
    }

    
    public override void Assign(OTObject protoType)
    {
        base.Assign(protoType);
        OTSprite pSprite = protoType as OTSprite;
        tintColor = pSprite.tintColor;
        alpha = pSprite.alpha;
        image = pSprite.image;
        spriteContainer = pSprite.spriteContainer;
        frameIndex = pSprite.frameIndex;
        materialReference = pSprite.materialReference;
    }

	
	protected float mLeft = 0,mRight = 0,mTop = 0, mBottom = 0;
	
	protected Vector2 _meshsize_ = Vector2.one;
	
	protected Mesh initMesh()
	{
        Mesh mesh = new Mesh();		
        _meshsize_ = Vector2.one;

        if (objectParent)
        {
            _meshsize_ = size;
            _pivotPoint = Vector2.Scale(_pivotPoint, size);
        }
        else
        {
            _meshsize_ = Vector2.one;
            _pivotPoint = pivotPoint;
        }

		float dx = (_meshsize_.x/2);
		float dy = (_meshsize_.y/2);
		float px =  _pivotPoint.x;
		float py =  _pivotPoint.y;						
		
		mTop = dy - py;
		mLeft = -dx - px;
		mBottom = -dy - py;
		mRight = dx - px;
		
		return mesh;		
	}

    //-----------------------------------------------------------------------------
    // overridden subclass methods
    //-----------------------------------------------------------------------------
    
    protected override Mesh GetMesh()
    {
		Mesh mesh = initMesh();
		
        mesh.vertices = new Vector3[] { 
                new Vector3(mLeft, mTop, 0),		// topleft
                new Vector3(mRight, mTop, 0),		// topright
                new Vector3(mRight, mBottom, 0),		// botright
                new Vector3(mLeft, mBottom, 0)	// botleft
            };        
		mesh.triangles = new int[] { 
                0,1,2,2,3,0
            };

        Vector2[] meshUV = new Vector2[] { 
            new Vector2(0,1), new Vector2(1,1), 
            new Vector2(1,0), new Vector2(0,0) };

        if (flipHorizontal)
        {
            Vector2 v;
            v = meshUV[0];
            meshUV[0] = meshUV[1]; meshUV[1] = v;
            v = meshUV[2];
            meshUV[2] = meshUV[3]; meshUV[3] = v;
        }

        if (flipVertical)
        {
            Vector2 v;
            v = meshUV[0];
            meshUV[0] = meshUV[3]; meshUV[3] = v;
            v = meshUV[1];
            meshUV[1] = meshUV[2]; meshUV[2] = v;
        }

        mesh.uv = meshUV;

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
		
        return mesh;
    }

    
    protected override string GetTypeName()
    {
        return "Sprite";
    }

    
    protected override void AfterMesh()
    {
        base.AfterMesh();
        // reset size because mesh has been created with a size (x/y) of 1/1
        size = _size;
        _frameIndex_ = -1;
        isDirty = true;
    }

    
    public virtual string GetMatName()
    {
        string matName = "";

        if (spriteContainer != null)
        {
            if (!useUV)
                matName += "spc:" + spriteContainer.name + ":" + frameIndex + ":" + materialReference;
            else
                matName += "spc:" + spriteContainer.name + ":" + materialReference;
        }
        else
            if (image != null)
                matName += "img:" + _image.GetInstanceID() + " : " +
                    materialReference;
        if (matName == "") matName = materialReference;

        if (mr != null)
        {
            if (mr.fieldColorTint != "")
                matName += " : " + tintColor.ToString();
            if (mr.fieldAlphaChannel != "" || mr.fieldAlphaColor != "")
                matName += " : " + alpha;
        }
        lastMatName = matName;
        return matName;
    }

    Material LookupMaterial()
    {
        return OT.LookupMaterial(GetMatName());
    }

    void RegisterMaterial()
    {
        OT.RegisterMaterial(GetMatName(), material);
    }

    private void SetMatReference()
    {
        if (transparent)
            _materialReference = "transparent";
        else
            if (additive)
                _materialReference = "additive";
            else
                if (_materialReference == "additive" || _materialReference == "transparent" || _materialReference == "")
                    _materialReference = "solid";
    }

    
    override protected void CheckDirty()
    {
        base.CheckDirty();
        if (spriteContainer != null)
        {
            if (spriteContainer.isReady)
            {
                if (_spriteContainer_ != spriteContainer || _frameIndex_ != frameIndex)
                    isDirty = true;
            }
        }
        else
            if (_spriteContainer_ != null || image != _image_)
                isDirty = true;

        if (flipHorizontal != _flipHorizontal_ || flipVertical != _flipVertical_)
        {
            _flipHorizontal_ = flipHorizontal;
            _flipVertical_ = flipVertical;
            meshDirty = true;
        }

        if (!Application.isPlaying)
        {
            if (!isDirty && spriteContainer != null && material.mainTexture != spriteContainer.GetTexture())
                isDirty = true;
        }

        if (transparent != _transparent_ && transparent)
        {
            _additive = false;
            _additive_ = additive;
            _transparent_ = transparent;
            SetMatReference();
        }
        else
            if (additive != _additive_ && additive)
            {
                _transparent = false;
                _additive_ = additive;
                _transparent_ = transparent;
                SetMatReference();
            }
            else
                if (!_additive && !_transparent)
                {
                    _additive_ = additive;
                    _transparent_ = transparent;
                    if (_materialReference == "transparent" || _materialReference == "additive")
                        _materialReference = "solid";
                }



        if (materialReference != _materialReference_)
        {
            mr = OT.GetMatRef(materialReference);
            if (_materialReference == "transparent")
            {
                _transparent = true;
                _additive = false;
            }
            else
                if (_materialReference == "additive")
                {
                    _transparent = false;
                    _additive = true;
                }
                else
                {
                    _transparent = false;
                    _additive = false;
                }
            isDirty = true;
        }

        if (mr != null)
        {
            if (_tintColor_ != tintColor)
            {
                if (mr.fieldColorTint != "")
                    isDirty = true;
                else
                {
                    _tintColor = Color.white;
                    _tintColor_ = _tintColor;
                    Debug.LogWarning("Orthello : TintColor can not be set on this materialReference!");
                }
            }
            if (_alpha_ != alpha)
            {
                if (mr.fieldAlphaColor != "" || mr.fieldAlphaChannel != "")
                    isDirty = true;
                else
                {
                    _alpha = 1;
                    _alpha_ = 1;
                    Debug.LogWarning("Orthello : Alpha value can not be set on this materialReference!");
                }
            }
        }

    }
	
	
	protected Vector2[] SpliceUV(Vector2[] uv, float[] pos, bool horizontal)
	{
		if (pos.Length<=2)
			return uv;
		
		Vector2 tl = uv[0];
		Vector2 tr = uv[1];
		Vector2 bl = uv[3];
																			
		Vector2[] _uv = new Vector2[pos.Length * 2];
		for (int vr=0; vr<pos.Length; vr++)
		{
			float vi = pos[vr];
			if (vr==0) vi=0;
			if (vr == pos.Length-1) vi=100;
			
			if (horizontal)
			{
				float dd = ((tr.x-tl.x)/100f) * vi;
				
				int vp = vr;
				if (flipHorizontal) 
					vp = (pos.Length-1-vr);
				
				Vector2 p1 = new Vector2(tl.x+dd,tl.y);
				Vector2 p2 = new Vector2(bl.x+dd,bl.y);
				if (flipVertical)
				{
					Vector2 ps = p1;
					p1 = p2;
					p2 = ps;
				}
								
				_uv[vp * 2] = p1;
				_uv[vp * 2+1] = p2;
			}
			else
			{
				float dd = ((tl.y-bl.y)/100f) * vi;
				
				int vp = vr;
				if (flipVertical) 
					vp = (pos.Length-1-vr);
				
				Vector2 p1 = new Vector2(tl.x,tl.y-dd);
				Vector2 p2 = new Vector2(tr.x,tr.y-dd);
								
				if (flipHorizontal)
				{
					Vector2 ps = p1;
					p1 = p2;
					p2 = ps;
				}
									
				_uv[vp * 2] = p1;
				_uv[vp * 2+1] = p2;
			}
		}										
		return _uv;
	}
		
	
    protected virtual void HandleUV(Material mat)
    {
        if (useUV && spriteContainer != null && spriteContainer.isReady)
        {			
            OTContainer.Frame frame = spriteContainer.GetFrame(frameIndex);
            mat.mainTextureScale = new Vector2(1, 1);
            mat.mainTextureOffset = new Vector2(0, 0);
            // adjust this sprites UV coords
            if (frame.uv != null && mesh != null)
            {
                Vector2[] meshUV = frame.uv.Clone() as Vector2[];
                if (flipHorizontal)
                {
                    Vector2 v;
                    v = meshUV[0];
                    meshUV[0] = meshUV[1]; meshUV[1] = v;
                    v = meshUV[2];
                    meshUV[2] = meshUV[3]; meshUV[3] = v;
                }

                if (flipVertical)
                {
                    Vector2 v;
                    v = meshUV[0];
                    meshUV[0] = meshUV[3]; meshUV[3] = v;
                    v = meshUV[1];
                    meshUV[1] = meshUV[2]; meshUV[2] = v;
                }

                mesh.uv = meshUV;
            }
        }
    }

    
    protected virtual Material InitMaterial()
    {
        if (spriteContainer != null && !spriteContainer.isReady)
        {
            lastMat = material;
            assignedMaterial = false;
            RegisterMaterial();
            return lastMat;
        }
		
        Material spMat = OT.GetMaterial(_materialReference, tintColor, alpha);
        if (spMat == null) spMat = OT.materialTransparent;
        if (spMat == null) return null;
        Material mat = new Material(spMat);

        if (spriteContainer != null && spriteContainer.isReady)
        {
            Texture tex = spriteContainer.GetTexture();
            if (mat.mainTexture != tex)
                mat.mainTexture = tex;
            HandleUV(mat);
        }
        else
            if (image != null)
            {
                if (mat != null)
                {
                    mat.mainTexture = image;
                    mat.mainTextureScale = Vector2.one;
                    mat.mainTextureOffset = Vector3.zero;
                }
            }

        if (mat != null)
        {
            if (lastMatName != "" && lastMat != null)
                OT.MatDec(lastMat, lastMatName);

            if (lastMat == null && !assignedMaterial)
            {
                if (!isCopy)
                {
                    if (!Application.isPlaying)
                        DestroyImmediate(renderer.sharedMaterial, true);
                    else
                        Destroy(renderer.material);
                }
            }

            if (Application.isPlaying)
                renderer.material = mat;
            else
                renderer.sharedMaterial = mat;

            lastMat = mat;
            assignedMaterial = false;
            RegisterMaterial();
        }
        return mat;
    }

    
    [HideInInspector]
    protected override void Clean()
    {
        if (!OT.isValid) return;
        base.Clean();

        if (_spriteContainer_ != spriteContainer ||
            _frameIndex_ != frameIndex ||
            _image_ != image ||
            _tintColor_ != tintColor ||
            _alpha_ != alpha ||
            _materialReference_ != _materialReference ||
            isCopy)
        {

            if (spriteContainer != null && spriteContainer.isReady)
            {
                if (frameIndex < 0) _frameIndex = 0;
                if (frameIndex > spriteContainer.frameCount - 1) _frameIndex = spriteContainer.frameCount - 1;
                if (spriteContainer is OTSpriteAtlas)
                {
                    OTContainer.Frame fr = CurrentFrame();
                    if ((spriteContainer as OTSpriteAtlas).offsetSizing)
                    {
                        if (Vector2.Equals(oSize, Vector2.zero))
                        {
                            oSize = fr.size * OT.view.sizeFactor;
                            Vector2 nOffset = fr.offset * OT.view.sizeFactor;
                            if (_baseOffset.x != nOffset.x || _baseOffset.y != nOffset.y)
                            {
                                offset = nOffset;
                                position = _position;
                                imageSize = fr.imageSize * OT.view.sizeFactor;
                            }
                        }
                        if (_frameIndex_ != frameIndex || _spriteContainer_ != spriteContainer)
                        {
                            Vector2 sc = new Vector2((size.x / oSize.x) * fr.size.x * OT.view.sizeFactor, (size.y / oSize.y) * fr.size.y * OT.view.sizeFactor);
                            Vector3 sc3 = new Vector3(sc.x, sc.y, 1);

                            _size = sc;
                            if (!Vector3.Equals(transform.localScale, sc3))
                               transform.localScale = sc3;
                            oSize = fr.size * OT.view.sizeFactor;
                            imageSize = fr.imageSize * OT.view.sizeFactor;
                            Vector2 nOffset = fr.offset * OT.view.sizeFactor;
                            if (_baseOffset.x != nOffset.x || _baseOffset.y != nOffset.y)
                            {
                                offset = nOffset;
                                position = _position;
                            }
                        }
                    }
                    else
                    {
                        Vector3[] verts = fr.vertices.Clone() as Vector3[];
                        verts[0] -= new Vector3(pivotPoint.x, pivotPoint.y, 0);
                        verts[1] -= new Vector3(pivotPoint.x, pivotPoint.y, 0);
                        verts[2] -= new Vector3(pivotPoint.x, pivotPoint.y, 0);
                        verts[3] -= new Vector3(pivotPoint.x, pivotPoint.y, 0);
                        mesh.vertices = verts;

                        _size = fr.size;
                        Vector3 sc3 = new Vector3(_size.x, _size.y, 1);
                        if (!Vector3.Equals(transform.localScale, sc3))
                            transform.localScale = sc3;
                    }
                }
            }
			
            Material mat = LookupMaterial();
            if (mat == null)
            {
                mat = InitMaterial();
            }
            else
            {
                renderer.material = mat;
                HandleUV(mat);
            }
            OT.MatInc(mat);

            _spriteContainer_ = spriteContainer;
            _materialReference_ = materialReference;
            _frameIndex_ = frameIndex;
            _image_ = image;
            _tintColor_ = tintColor;
            _alpha_ = alpha;
        }

        isDirty = false;
        if (spriteContainer != null && !spriteContainer.isReady)
            isDirty = true;
		
#if UNITY_EDITOR
		if (!Application.isPlaying)
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif			
		
    }

    
    new protected void OnDestroy()
    {
        if (lastMatName != "" && lastMat != null)
            OT.MatDec(lastMat, lastMatName);
        else
            DestroyImmediate(material);
        base.OnDestroy();
    }

    
    protected Vector2 baseSize = Vector2.zero;
	bool frameReloaded  = false;
    
    override protected void CheckSettings()
    {
        base.CheckSettings();
        if (Application.isEditor || OT.dirtyChecks || dirtyChecks)
        {
            if (spriteContainer != null && spriteContainer.isReady)
            {				
				_containerName = spriteContainer.name;
								
				if (spriteContainer is OTSpriteAtlasImport && (spriteContainer as OTSpriteAtlasImport).reloadFrame && !frameReloaded)
				{
					_frameIndex_ = -1;	
					frameReloaded = true;
				}
													
                if (frameIndex < 0) _frameIndex = 0;
                if (frameIndex > spriteContainer.frameCount - 1) _frameIndex = spriteContainer.frameCount - 1;
				
				OTContainer.Frame fr = CurrentFrame();
				if (_spriteContainer_ == null && _containerName != "")
				{
					// set basesize to current frame size if we just had a lookup from a prefab					
					baseSize = fr.size;
				}								
                if (_spriteContainer_ != spriteContainer)
				{											
					if (!baseSize.Equals(Vector2.zero))							
                       size =  new Vector2 (size.x * (fr.size.x / baseSize.x) , size.y * (fr.size.y / baseSize.y) ) * OT.view.sizeFactor;
					else
                      size = fr.size * OT.view.sizeFactor;
				}
				baseSize = fr.size;
            }
            else
            {
                if (_image_ != image)
                    size = new Vector2(image.width, image.height) * OT.view.sizeFactor;
            }
            if (alpha < 0) _alpha = 0;
            else
                if (alpha > 1) _alpha = 1;
        }
}


    //-----------------------------------------------------------------------------
    // class methods
    //-----------------------------------------------------------------------------
    // Use this for initialization

    
    protected override void Awake()
    {
        _spriteContainer_ = spriteContainer;
        _frameIndex_ = frameIndex;
        _image_ = image;
        _materialReference_ = materialReference;
        _transparent_ = transparent;
        _flipHorizontal_ = flipHorizontal;
        _flipVertical_ = flipVertical;
        _tintColor_ = _tintColor;
        _alpha_ = _alpha;
        isDirty = true;
        base.Awake();
    }


    
    protected override void Start()
    {
        base.Start();
        mr = OT.GetMatRef(materialReference);
        if (Application.isPlaying)
        {
            if (!assignedMaterial)
            {
                Material mat = LookupMaterial();
                if (mat != null)
                {
                    renderer.material = mat;
                    HandleUV(mat);
                }
                else
                    mat = InitMaterial();

                OT.MatInc(mat);
            }
        }
        else
        {
            Material mat = InitMaterial();
            OT.MatInc(mat);
        }

        if (Application.isPlaying)
            _frameIndex_ = -1;

    }
	
	
	public void  InvalidateSprite()
	{
		spriteInvalid = true;
		renderer.enabled = false;
	}
	
	bool SpriteValid()
	{
		if (spriteInvalid)
		{
			if (!Application.isPlaying || image != null || (spriteContainer!=null && spriteContainer.isReady))
			{
				spriteInvalid = false;
				renderer.enabled = true;
				return true;
			}
			return false;		
		}			
		return true;
	}
		
    // Update is called once per frame
    
    protected override void Update()
    {
		
        if (!OT.isValid) return;
		
		SpriteValid();		
		if (image == null && spriteContainer == null)
		{
			if (_containerName!="")
			{
				OTContainer c = OT.ContainerByName(_containerName);
				if (c!=null && c.isReady) 
					spriteContainer = c;					
			}
		}
							
        // check if no material has been assigned yet
        if (!Application.isPlaying)
        {
            Material mat = material;
            if (mat == null)
            {
                mat = new Material(OT.materialTransparent);
                material = mat;
                mat.mainTexture = texture;
            }	
			
        }
        base.Update();
    }

}