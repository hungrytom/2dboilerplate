using UnityEngine;
using System.Collections;
 
/// <summary>
/// Provides functionality to use sprites in your scenes that are filled with a pattern image.
/// </summary>
public class OTGradientSprite : OTSprite
{
	public enum GradientOrientation { Vertical, Horizontal }
	
    //-----------------------------------------------------------------------------
    // Editor settings
    //-----------------------------------------------------------------------------
	public GradientOrientation _gradientOrientation = GradientOrientation.Vertical;
	/// <summary>
	/// Gradient position (0-100) and colors RGBA
	/// </summary>
    public OTGradientSpriteColor[] _gradientColors;
	
    //-----------------------------------------------------------------------------
    // public attributes (get/set)
    //-----------------------------------------------------------------------------
    public GradientOrientation gradientOrientation
	{
		get
		{
			return _gradientOrientation;
		}
		set
		{
			if (value!=_gradientOrientation)
			{
				_gradientOrientation = value;			
				meshDirty = true;
				isDirty = true;
			}
		}
	}
    public OTGradientSpriteColor[] gradientColors
	{
		get
		{
			return _gradientColors;
		}
		set
		{
			_gradientColors = value;			
			meshDirty = true;
			isDirty = true;
		}
	}
	
    private OTGradientSpriteColor[] _gradientColors_;
	private GradientOrientation _gradientOrientation_ = GradientOrientation.Vertical;
		
    protected override Mesh GetMesh()
    {
        Mesh mesh = new Mesh();

        Vector2 _meshsize_ = Vector2.one;

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
		
		int count = _gradientColors.Length;		
		for (int vr=0; vr<_gradientColors.Length; vr++)
			if (_gradientColors[vr].size>0)
				count++;
										
		float dx = (_meshsize_.x/2);
		float dy = (_meshsize_.y/2);
		float px =  _pivotPoint.x;
		float py =  _pivotPoint.y;		
		
		Vector3[] verts = new Vector3[count * 2];
		int vp = 0;
		for (int vr=0; vr<_gradientColors.Length; vr++)
		{
			if (_gradientOrientation == GradientOrientation.Horizontal)
			{
				float dd = (_meshsize_.x/100) * _gradientColors[vr].position;							
				verts[vp * 2] = new Vector3(-dx - px + dd, dy - py , 0); 	// top
				verts[(vp * 2) +1] = new Vector3(-dx - px + dd, -dy - py , 0);		// bottom
			}
			else
			{
				float dd = (_meshsize_.y/100) * _gradientColors[vr].position;
				verts[vp * 2] = new Vector3(-dx - px, dy - py - dd , 0); 	// left
				verts[(vp * 2) +1] = new Vector3(dx - px, dy - py - dd , 0);		// right
			}
			vp++;	
			if (_gradientColors[vr].size>0)
			{
				if (_gradientOrientation == GradientOrientation.Horizontal)
				{
					float dd = (_meshsize_.x/100) * (_gradientColors[vr].position+_gradientColors[vr].size);
					verts[vp * 2] = new Vector3(-dx - px + dd, dy - py , 0); 	// top
					verts[(vp * 2) +1] = new Vector3(-dx - px + dd, -dy - py , 0);		// bottom
				}
				else
				{
					float dd = (_meshsize_.y/100) * (_gradientColors[vr].position + _gradientColors[vr].size);
					verts[vp * 2] = new Vector3(-dx - px, dy - py - dd , 0); 	// left
					verts[(vp * 2) +1] = new Vector3(dx - px, dy - py - dd , 0);		// right
				}
				vp++;
			}
		}		
        mesh.vertices = verts;

		int[] tris = new int[(count-1) * 6];
		for (int vr=0; vr<count-1; vr++)
		{
			int vv = vr*2;
			if (_gradientOrientation == GradientOrientation.Horizontal)
			{
				int[] _tris = new int[] { vv,vv+2,vv+3,vv+3,vv+1,vv };
				_tris.CopyTo(tris, vr * 6);
			}
			else
			{
				int[] _tris = new int[] { vv,vv+1,vv+3,vv+3,vv+2,vv };
				_tris.CopyTo(tris, vr * 6);
			}
		}		
		
        mesh.triangles = tris;

		float[] gradientPositions = new float[count];
		vp = 0;
		for(int g = 0; g<gradientColors.Length; g++)
		{
			gradientPositions[vp] = gradientColors[g].position;					
			vp++;
			if (gradientColors[g].size>0)
			{
				gradientPositions[vp] = gradientColors[g].position + gradientColors[g].size;									
				vp++;
			}
		}
				
        mesh.uv = SpliceUV(
			new Vector2[] { 
				new Vector2(0,1), new Vector2(1,1), new Vector2(1,0), new Vector2(0,0)
			},gradientPositions, _gradientOrientation == GradientOrientation.Horizontal);

        mesh.RecalculateBounds();
        mesh.RecalculateNormals();
		
        return mesh;
    }
	
	
	void CloneGradientColors()
	{
		_gradientColors_ = new OTGradientSpriteColor[_gradientColors.Length];
		for (int c=0; c<_gradientColors.Length; c++)
		{
			_gradientColors_[c] = new OTGradientSpriteColor();
			_gradientColors_[c].position = _gradientColors[c].position;
			_gradientColors_[c].size = _gradientColors[c].size;
			_gradientColors_[c].color = _gradientColors[c].color;
		}		
	}
	
    //-----------------------------------------------------------------------------
    // overridden subclass methods
    //-----------------------------------------------------------------------------	
    /// <exclude />
    protected override void CheckSettings()
    {
        base.CheckSettings();
		
		
		if (_gradientColors.Length<2)
		{
			System.Array.Resize(ref _gradientColors,2);
		}
		_gradientColors[0].position = 0;
		_gradientColors[_gradientColors.Length-1].position = 100;			
		
		if (_gradientColors.Length!=_gradientColors_.Length || GradientMeshChanged() || _gradientOrientation_ != _gradientOrientation)
			meshDirty = true;
		else
		if (GradientColorChanged())
		{
			isDirty = true;
		}
		
		if (meshDirty || isDirty)
		{
			CloneGradientColors();
			_gradientOrientation_ = _gradientOrientation;
		}
    }

    /// <exclude />
    protected override string GetTypeName()
    {
        return "Gradient";
    }
			
    protected override void HandleUV(Material mat)
    {
        if (useUV && spriteContainer != null && spriteContainer.isReady)
        {
            OTContainer.Frame frame = spriteContainer.GetFrame(frameIndex);
            mat.mainTextureScale = new Vector2(1, 1);
            mat.mainTextureOffset = new Vector2(0, 0);
            // adjust this sprites UV coords
            if (frame.uv != null && mesh != null)
            {								
				int count = _gradientColors.Length;		
				for (int vr=0; vr<_gradientColors.Length; vr++)
					if (_gradientColors[vr].size>0)
						count++;
				
				// get positions for UV splicing
				float[] gradientPositions = new float[count];
				int vp = 0;
				for(int g = 0; g<gradientColors.Length; g++)
				{
					gradientPositions[vp] = gradientColors[g].position;					
					vp++;
					if (gradientColors[g].size>0)
					{
						gradientPositions[vp] = gradientColors[g].position + gradientColors[g].size;									
						vp++;
					}
				}
				// splice UV that we got from the container.
                mesh.uv = SpliceUV(frame.uv.Clone() as Vector2[],gradientPositions,gradientOrientation == GradientOrientation.Horizontal);
            }
        }
    }
	
	protected override void Clean()
	{
		base.Clean();

		var colors = new Color[mesh.vertexCount];
		int vp = 0;
		for (int c=0; c<_gradientColors.Length; c++)
		{
			if (vp < mesh.vertexCount/2)
			{			
				colors[(vp*2)] = _gradientColors[c].color;
				colors[(vp*2)+1] = _gradientColors[c].color;								
			}
			vp++;
			if (_gradientColors[c].size>0 && vp < mesh.vertexCount/2)
			{
				colors[(vp*2)] = _gradientColors[c].color;
				colors[(vp*2)+1] = _gradientColors[c].color;								
				vp++;
			}			
		}
				
        MeshFilter mf = GetComponent<MeshFilter>();
		mf.sharedMesh.colors = colors;
		
	}

    //-----------------------------------------------------------------------------
    // class methods
    //-----------------------------------------------------------------------------	
	bool GradientMeshChanged()
	{
		bool res = false;
		for (int c = 0; c < _gradientColors.Length; c++)
		{
			if (_gradientColors[c].position < 0) _gradientColors[c].position = 0;
			if (_gradientColors[c].position > 100) _gradientColors[c].position = 100;
			if (_gradientColors[c].size < 0) _gradientColors[c].size = 0;
			if (_gradientColors[c].size > 100) _gradientColors[c].size = 100;
			if (_gradientColors[c].position+_gradientColors[c].size > 100) _gradientColors[c].position = 100-_gradientColors[c].size;
			if (_gradientColors[c].position!=_gradientColors_[c].position || _gradientColors[c].size!=_gradientColors_[c].size)
				res = true;			
		}
		return res;	
	}
	
	bool GradientColorChanged()
	{
		for (int c = 0; c < _gradientColors.Length; c++)
		{
			if (!_gradientColors[c].color.Equals(_gradientColors_[c].color))
			{
				return true;			
			}
		}
		return false;	
	}
		
    /// <exclude />
    protected override void Awake()
    {
		CloneGradientColors();
		_gradientOrientation_ = _gradientOrientation;
        base.Awake();
    }


    new void Start()
    {
        base.Start();
    }
	
    // Update is called once per frame
    new void Update()
    {
        base.Update();
    }
}

[System.Serializable]
public class OTGradientSpriteColor
{
	public int position = 0;
	public int size = 0;
	public Color color = Color.white;
}