using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_FLASH
#else
using System.Reflection;
#endif

/// <summary>
/// Provides base functionality for all Orthello display classes.
/// </summary>
[ExecuteInEditMode]
public class OTObject : MonoBehaviour
{
    //-----------------------------------------------------------------------------
    // Enum types
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Pivot point location.
    /// </summary>
    public enum Pivot
    {
        
        TopLeft,
        
        Top,
        
        TopRight,
        
        Left,
        
        Center,
        
        Right,
        
        BottomLeft,
        
        Bottom,
        
        BottomRight,
        
        Custom
    };

    //-----------------------------------------------------------------------------
    // Enum types
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Physical type
    /// </summary>
    public enum Physics
    {
        /// <summary>
        /// For input capture or custom collision detection
        /// </summary>
        Trigger,
        /// <summary>
        /// Physical floating object
        /// </summary>
        NoGravity,
        /// <summary>
        /// Physical rigid body
        /// </summary>
        RigidBody,
        /// <summary>
        /// Physical static body that collides with all 
        /// </summary>
        StaticBody,
        /// <summary>
        /// Physical static body that has a collison size of 10
        /// </summary>
        StaticRigidBody,
        /// <summary>
        /// custom adding/removing/configration of colliders and physical components.
        /// </summary>
        Custom
    };

    //-----------------------------------------------------------------------------
    // Enum types
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Type of collider to use
    /// </summary>
    public enum ColliderType
    {
        /// <summary>
        /// Use a Box collider object
        /// </summary>
        Box,
        /// <summary>
        ///  use a Spherical collider object
        /// </summary>
        Sphere
    };

    //-----------------------------------------------------------------------------
    // Editor settings
    //-----------------------------------------------------------------------------
    
    public string _name = "";
    
    public Vector2 _position = Vector2.zero;
    
    public Vector2 _size = new Vector2(100, 100);
    
    public float _rotation = 0;
    
    public Pivot _pivot = Pivot.Center;
    
    public Vector2 _pivotPoint = Vector2.zero;
    
    public bool _registerInput = false;
    
    public int _depth = 0;
    
    public bool _collidable = false;
    
    public Physics _physics = Physics.Trigger;
    
    public ColliderType _colliderType = ColliderType.Box;
    
    public int _collisionDepth = 0;
    
    public float _collisionSize = -1;

    /// <summary>
    /// Makes this OTObject draggable
    /// </summary>
    /// <remarks>
    /// When an OTObject is draggable it will fire the <see cref="onDragStart" /> and <see cref="onDragEnd" /> event. The receiving OTObject (sprite) must have <see cref="registerInput" /> set to true and
    /// will fire the <see cref="onReceiveDrop" /> event. 
    /// </remarks>
    public bool draggable = false;

    /// <summary>
    /// Mousebutton that will activate dragging the sprite.
    /// </summary>
    [HideInInspector]
    public int dragButton = 0;
    
    [HideInInspector]
    public bool init = true;	
	
    
    public Rect _worldBounds = new Rect(0, 0, 0, 0);

    /// <summary>
    /// World boundary for this object
    /// </summary>
    /// <remarks>
    /// If world boundary is set this will restrict this object
    /// from leaving this boundary box. Setting this to Rect(0,0,0,0) 
    /// will impose no  boundary restriction.
    /// This is specified in pixel (world space) coordinates.
    /// </remarks>
    public Rect worldBounds
    {
        get
        {
            return _worldBounds;
        }
        set
        {
            _worldBounds = value;
            Update();
        }
    }

    /// <summary>
    /// Check and handle setting changes while playing.
    /// </summary>   
    /// <remarks>
    /// When the system checks/validates settings, it will for example
    /// add a collider and a rigidbody when you set collidable to true.
    /// Normally you wont need this functionality while the application
    /// is playing but you can set this to true when you are building
    /// up sprites from scratch. This will cost about 10fps (very rough estimate).
    /// </remarks>
    public bool dirtyChecks
    {
        get
        {
            return _dirtyChecks;
        }
        set
        {
            _dirtyChecks = value;
        }
    }

    //-----------------------------------------------------------------------------
    // Fields
    //-----------------------------------------------------------------------------
    
    [HideInInspector]
    public string protoType = "";
    
    [HideInInspector]
    public string baseName = "";
    
    [HideInInspector]
    public bool _isPrefab = false;

    //-----------------------------------------------------------------------------
    // Delegates
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Is used to delegate a message from a specific owner object.
    /// </summary>
    public delegate void ObjectDelegate(OTObject owner);
    /// <summary>
    /// Input control delegate
    /// </summary>
    /// <remarks>
    ///  The onInput delegate will be called if this object 'registerInput' setting is set to true and if the 
    ///  user interacts with this object. Use this to capture user-input (clicks/touches).
    /// </remarks>
    /// <example> 
    /// The <strong>C# sample</strong> shows how the onInput delegate can be used.
    /// <code>
    ///  void Start()
    ///  {
    ///     // get our sprite
    ///     OTObject mySprite = OT.ObjectByName("mySprite");
    ///     // assign onInput delegate
    ///     if (mySprite!=null) mySprite.onInput = OnInput;
    ///  }
    ///  
    ///  void OnInput(OTObject owner)
    ///  {
    ///     // check for the left mouse button
    ///     if (Input.GetMouseDown(0))
    ///         Debug.Log("Left Button clicked On "+owner.name);
    ///  }
    /// </code>
    /// <i style="font-size:90%">Double-click on example code to select all code.</i>
    /// <br></br><br></br><br></br>
    ///  The <strong>Javascript sample</strong> shows how a sprite can be instructed to give an 
    ///  OnInput(OTObject owner) callback.
    ///  <code>
    ///  function Start():void
    ///  {
    ///     // get our sprite
    ///     var mySprite:OTObject = OT.ObjectByName("mySprite");
    ///     // instruct our sprite to give callbacks
    ///     if (mySprite!=null) mySprite.InitCallBacks(this);
    ///  }
    ///  
    ///  // All sprites that are instructed to give callbacks using {sprite}.InitCallBacks(this);
    ///  // will call OnInput automaticly when user input is captured.
    ///  function OnInput(owner:OTObject):void
    ///  {
    ///     // check for the left mouse button
    ///     if (Input.GetMouseDown(0))
    ///         Debug.Log("Left Button clicked On "+owner.name);
    ///  }
    ///  </code>
    /// <i style="font-size:90%">Double-click on example code to select all code.</i>
    /// </example>   
    public ObjectDelegate onInput = null;
    /// <summary>
    /// Is called when an object moves out of view.
    /// </summary>
    public ObjectDelegate onOutOfView = null;
    /// <summary>
    /// Is called when an object moves into view.
    /// </summary>
    public ObjectDelegate onIntoView = null;
    /// <summary>
    /// Is called when user starts dragging this object.
    /// </summary>
    /// <remarks>
    /// To drag an object you must set <see cref="draggable" /> to true on the object that you want to drag.
    /// </remarks>
    public ObjectDelegate onDragStart = null;
    /// <summary>
    /// Is called when user drops this object after dragging it.
    /// </summary>
    /// <remarks>
    /// To drag an object you must set <see cref="draggable" /> to true on the object that you want to drag.
    /// <see cref="dropTarget" /> will contain the sprite where this one was dropped upon.
    /// </remarks>
    public ObjectDelegate onDragEnd = null;
    /// <summary>
    /// Is called when another object is drag'n dropped on this object
    /// </summary>
    /// <remarks>
    /// This receiving object must have <see cref="registerInput" /> set to true. To drag an object you must set <see cref="draggable" /> to true on the object that you want to drag.
    /// <see cref="dropTarget" /> will contain the sprite that was dropped upon this one.
    /// </remarks>
    public ObjectDelegate onReceiveDrop = null;
    /// <summary>
    /// Orthello mouse movement handler
    /// </summary>
    public ObjectDelegate onMouseMoveOT = null;
    /// <summary>
    /// Orthello mouse enter handler
    /// </summary>
    public ObjectDelegate onMouseEnterOT = null;
    /// <summary>
    /// Orthello mouse exit handler
    /// </summary>
    public ObjectDelegate onMouseExitOT = null;
    /// <summary>
    /// Delegate to check collisions
    /// </summary>
    /// <remarks>
    ///  The onCollision delegate will be called if this object 'collidable' setting is set to true and if the 
    ///  this OTObject collides with another 'collidable' OTObject. 
    /// </remarks>
    public ObjectDelegate onCollision = null;
    /// <summary>
    /// Collision 'enter' delegate
    /// </summary>
    /// <remarks>
    ///  The onEnter delegate will be called if this object 'collidable' setting is set to true and if the 
    ///  this OTObject enters (starts to overlap) another 'collidable' OTObject. 
    /// </remarks>
    public ObjectDelegate onEnter = null;
    /// <summary>
    /// Collision 'exit' delegate
    /// </summary>
    /// <remarks>
    ///  The onExit delegate will be called if this object 'collidable' setting is set to true and if the 
    ///  this OTObject exits (stops to overlap) another 'collidable' OTObject. 
    /// </remarks>
    public ObjectDelegate onExit = null;
    /// <summary>
    /// Collision 'stay' delegate
    /// </summary>
    /// <remarks>
    ///  The onStay delegate will be called if this object 'collidable' setting is set to true and if the 
    ///  this OTObject is overlapping another 'collidable' OTObject. 
    /// </remarks>
    public ObjectDelegate onStay = null;

    //-----------------------------------------------------------------------------
    // public attributes (get/set)
    //-----------------------------------------------------------------------------
    /// <summary>
    /// Name of this object
    /// </summary>
    /// <remarks>
    /// Changing this name will reflect immediately on the name of the associated Unity GameObject and
    /// vice versa.
    /// </remarks>
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
                OT.RegisterLookup(this, old);
            }
        }
    }

    /// <summary>
    /// Display depth of this object. -1000 (=front) to 1000 (=back)
    /// </summary>
    /// <remarks>
    /// The depth is a integer value from -1000 to 1000. Lower values will be layered on top of higher values, so by 
    /// increasing the depth you will send your object further back.
    /// </remarks>
    public int depth
    {
        get
        {
            return _depth;
        }
        set
        {
            _depth = value;
            _paintingDepth = depth + (position.y / 1000) + (position.x / 10000);
            transform.position = new Vector3(transform.position.x, transform.position.y, _paintingDepth);
            SetCollider();
        }
    }

    /// <summary>
    /// Depth of collision layer
    /// </summary>
    /// <remarks>
    /// 'Collidable' objects will collide only with other 'collidable' object with the same
    /// collision depth.
    /// </remarks>
    public int collisionDepth
    {
        get
        {
            return _collisionDepth;
        }
        set
        {
            _collisionDepth = value;
            SetCollider();
        }
    }

    /// <summary>
    /// Depth (z) size of physical collider
    /// </summary>
    /// <remarks>
    /// If you are using Physics you can tweek the depth/z value of the collider
    /// using this setting. By setting this to high values, objects on other
    /// collision layers, where the collidersizes overlap can cause collisions.
    /// </remarks>
    public float collisionSize
    {
        get
        {
            return _collisionSize;
        }
        set
        {
            _collisionSize = value;
            SetCollider();
        }
    }

    /// <summary>
    /// Pivot location
    /// </summary>
    public Pivot pivot
    {
        get
        {
            return _pivot;
        }
        set
        {
            _pivot = value;
            switch (_pivot)
            {
                case Pivot.TopLeft: pivotPoint = new Vector2(-.5f, .5f); break;
                case Pivot.Top: pivotPoint = new Vector2(0, .5f); break;
                case Pivot.TopRight: pivotPoint = new Vector2(.5f, .5f); break;
                case Pivot.Left: pivotPoint = new Vector2(-.5f, 0); break;
                case Pivot.Center: pivotPoint = new Vector2(0, 0); break;
                case Pivot.Right: pivotPoint = new Vector2(.5f, 0); break;
                case Pivot.BottomLeft: pivotPoint = new Vector2(-.5f, -.5f); break;
                case Pivot.Bottom: pivotPoint = new Vector2(0, -.5f); break;
                case Pivot.BottomRight: pivotPoint = new Vector2(.5f, -.5f); break;
            }
        }
    }
    /// <summary>
    /// Custom pivot location
    /// </summary>
    /// <remarks>
    /// This Vector2 (x/y) 0/0 marks the pivot relative to the center of the object.<br></br><br></br>
    /// <strong>IMPORTANT</strong> : a sprite is always created with a size (x/y) of 1/1.
    /// </remarks>
    public Vector2 pivotPoint
    {
        get
        {
            return _pivotPoint;
        }
        set
        {
            if (!Vector2.Equals(value, _pivotPoint))
            {
                Vector2 nOff = Vector2.Scale(_pivotPoint, size);
                if (rotation != 0)
                {
                    Matrix4x4 m = new Matrix4x4();
                    m.SetTRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 0, rotation)), Vector3.one);
                    nOff = m.MultiplyPoint3x4(nOff);
                }
                position -= nOff;
                _pivotPoint = value;
                nOff = Vector2.Scale(_pivotPoint, size);
                if (rotation != 0)
                {
                    Matrix4x4 m = new Matrix4x4();
                    m.SetTRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 0, rotation)), Vector3.one);
                    nOff = m.MultiplyPoint3x4(nOff);
                }

                offset = _baseOffset;

                position += nOff;
                meshDirty = true;
            }
        }
    }

    /// <summary>
    /// Indicates if this object is out of view
    /// </summary>
    public bool outOfView
    {
        get
        {
            //return (!inView);
            return !OT.view.Contains(this);
        }
    }

    /// <summary>
    /// This object's rectangle in world space
    /// </summary>
    public Rect rect
    {
        get
        {
            Rect r = new Rect(
                renderer.bounds.center.x - (size.x / 2),
                renderer.bounds.center.y + (size.y / 2),
                size.x,
                size.y * -1);
            return r;
        }
    }

    /// <summary>
    /// Monitor user input yes/no
    /// </summary>
    /// <remarks>
    /// When registerInput is set to true, this OTObject is monitored for user input. When this object
    /// receives some user input the 'onInput' delegate/callback is called.
    /// </remarks>
    public bool registerInput
    {
        get
        {
            return _registerInput;
        }
        set
        {
            _registerInput = value;
            Update();
        }
    }

    /// <summary>
    /// Monitor collisions yes/no
    /// </summary>
    /// <remarks>
    /// When collidable is set to true, this OTObject is monitored for collisions. When this object collides with another
    /// 'collidable' object, the onCollision delegate/callback is called. In addition to the onCollision delegate, the
    /// onEnter, onExit and onStay delegates/callbacks are called as well as a collidable object enters, exits or resides within 
    /// another collidable object.
    /// </remarks>
    public bool collidable
    {
        get
        {
            return _collidable;
        }
        set
        {
            _collidable = value;
            Update();
        }
    }

    /// <summary>
    /// Physics type of this object
    /// </summary>
    public Physics physics
    {
        get
        {
            return _physics;
        }
        set
        {
            _physics = value;
            Update();
        }
    }

    /// <summary>
    /// Physics Collider type of this object
    /// </summary>
    public ColliderType colliderType
    {
        get
        {
            return _colliderType;
        }
        set
        {
            _colliderType = value;
            Update();
        }
    }

    /// <summary>
    /// Current collision object
    /// </summary>
    /// <remarks>
    /// The collisionObject is the destination OTObject of a collision
    /// </remarks>
    public OTObject collisionObject
    {
        get
        {
            return _collisionObject;
        }
    }
	
    /// <summary>
    /// Current collision
    /// </summary>
    /// <remarks>
    /// The collision is only used when rigidBodys collide when using physics
    /// </remarks>
    public Collision collision
    {
        get
        {
            return _collision;
        }
    }

    /// <summary>
    /// User input hit point
    /// </summary>
    /// <remarks>
    /// The hit point is the point of user input 'inpact' when a user clicks or touches
    /// this OTObject. the x and y coordinates are relative to this objects position.
    /// </remarks>
    public Vector2 hitPoint
    {
        get
        {
            return _hitPoint;
        }
    }

    /// <summary>
    /// Target of the drag and drop action with thius sprite.
    /// </summary>
    /// <remarks>
    /// If this is the dragging object, the dropTarget will be set to the object that received the dragged sprite.
    /// If this is the receiving object (<see cref="registerInput" /> must be true), dropTarget will hold the sprite
    /// that was dragged and dropped.
    /// </remarks>
    public OTObject dropTarget
    {
        get
        {
            return _dropTarget;
        }
    }

    /// <summary>
    /// Position (x/y) in pixels
    /// </summary>
    /// <remarks>
    /// This Vector2 object reflects the x/y values of this OTObject's associated GameObject 
    /// transform posotion Vector3 object. Z value will be controlled by this OTObject's depth.
    /// </remarks>
    public Vector2 position
    {
        get
        {
            if (transform != null)
                return new Vector2(transform.position.x - offset.x, transform.position.y + offset.y);
            else
                return Vector2.zero;
        }
        set
        {
            Vector2 pos = value;
            if (worldBounds.width != 0)
            {
                float minX = _worldBounds.xMin;
                float maxX = _worldBounds.xMax;
                float minY = _worldBounds.yMin;
                float maxY = _worldBounds.yMax;
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
            }
            _paintingDepth = depth + (pos.y / 1000) + (pos.x / 10000);
            transform.position = new Vector3(pos.x + offset.x, pos.y - offset.y, _paintingDepth);
            _position = pos;
            _position_ = pos;
        }
    }

    /// <summary>
    /// Size (x/y) in pixels
    /// </summary>
    public Vector2 size
    {
        get
        {
            if (transform != null && !objectParent)
                return new Vector2(transform.localScale.x, transform.localScale.y);
            else
                if (objectParent)
                    return _size;
                else
                    return Vector2.zero;
        }
        set
        {
            float x = value.x;
            float y = value.y;
            if (OT.view.customSize == 0)
            {
                if (x < 1 || y < 1)
                {
                    value = new Vector2((x <= 0) ? 1 : x, (y <= 0) ? 1 : y);
                }
            }
            if (!objectParent)
                transform.localScale = new Vector3(value.x, value.y, 1 * OT.view.sizeFactor);
            else
                meshDirty = true;
            _size = value;
            _size_ = value;
            oSize = _size;
            Clean();
        }
    }

    /// <summary>
    /// Rotation in degrees
    /// </summary>
    /// <remarks>
    /// this reflects the z value values of this OTObject's associated GameObject 
    /// transform rotation Vector3 object (euler). x/y rotation will always be 0.
    /// </remarks>
    public float rotation
    {
        get
        {
            if (transform != null)
            {
                float val = transform.rotation.eulerAngles.z - offsetRotation;
                //if (val < 0.001f) val = 0;
                return val;
            }
            else
                return 0;
        }
        set
        {
            float val = value;
            // keep this rotation within 0-360
            if (val < 0) val += 360.0f;
            else
                if (val >= 360) val -= 360.0f;
            transform.rotation = Quaternion.Euler(0, 0, val + offsetRotation);
            _rotation = val;
        }
    }

    
    public Vector2[] uv
    {
        get
        {
            if (mesh != null)
                return mesh.uv;
            return null;
        }
    }

    /// <summary>
    /// Hides and shows an Orthello object
    /// </summary>
    /// <remarks>
    /// All children of this object, that have renderers, will be automaticly hidden as well.
    /// </remarks>
    public bool visible
    {
        get
        {
            return renderer.enabled;
        }
        set
        {
            if (value != renderer.enabled)
            {
                renderer.enabled = value;
                if (!value)
                {
                    if (transform.childCount > 0)
                    {
                        Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
                        for (int r = 0; r < renderers.Length; r++)
                        {
                            hiddenChildren.Add(renderers[r].gameObject);
                            renderers[r].enabled = false;
                        }
                    }
                }
                else
                {
                    if (hiddenChildren.Count > 0)
                    {
                        for (int r = 0; r < hiddenChildren.Count; r++)
                            hiddenChildren[r].renderer.enabled = true;
                        hiddenChildren.Clear();
                    }
                }
            }
        }
    }

    //-----------------------------------------------------------------------------
    // private and protected fields
    //-----------------------------------------------------------------------------
    
    protected int _collisionDepth_ = 0;
    
    protected float _collisionSize_ = 0.4f;
    
    protected string _name_ = "";
    
    protected bool objectParent = false;
    bool _registerInput_ = false;
    bool _collidable_ = false;
    Physics _physics_ = Physics.Trigger;
    ColliderType _colliderType_ = ColliderType.Box;
    Pivot _pivot_ = Pivot.Center;
    Vector2 _pivotPoint_ = Vector2.zero;
    
    protected bool isCopy = false;
    List<GameObject> hiddenChildren = new List<GameObject>();
    
    [HideInInspector]
    public Vector2 oSize;

    
    Vector2 _position_ = Vector2.zero;
    
    protected Vector2 _size_ = new Vector2(100, 100);
    
    float _rotation_ = 0;
    float _paintingDepth = 0;
    
    protected object[] callBackParams;

    
    [HideInInspector]
    public Vector2 imageSize = Vector2.zero;
    
    protected Vector2 offset
    {
        get
        {
            return _offset;
        }
        set
        {
            _baseOffset = value;
            Vector2 nOffset = value;
            if (!Vector2.Equals(Vector2.zero, imageSize) && !Vector2.Equals(size, imageSize))
            {
                float off = pivotPoint.x + 0.5f;
                if (off > 0)
                    nOffset.x = (value.x + (size.x * off)) - (imageSize.x * off);

                off = (pivotPoint.y * -1) + 0.5f;
                if (off > 0)
                    nOffset.y = (value.y + (size.y * off)) - (imageSize.y * off);

                if (rotation != 0)
                {
                    Matrix4x4 m = new Matrix4x4();
                    m.SetTRS(Vector3.zero, Quaternion.Euler(new Vector3(0, 0, rotation * -1)), Vector3.one);
                    nOffset = m.MultiplyPoint3x4(nOffset);
                }
            }
            _offset = nOffset;
        }
    }
    
    [HideInInspector]
    public Vector2 _offset = Vector2.zero;
    
    [HideInInspector]
    public Vector2 _baseOffset = Vector2.zero;
    
    protected float offsetRotation = 0;

    
    bool _dirtyChecks = false;
    
    protected Mesh mesh;
    
    protected bool meshDirty = false;
    
    protected bool isDirty = false;
    
    protected OTObject _dropTarget = null;
    bool isDragging = false;

    Vector2 _hitPoint;
    OTObject _collisionObject = null;
	Collision _collision = null;
    List<Component> callBackTargets = new List<Component>();
    int _depth_ = 0;

    
    protected bool inView = true;

    private List<OTController> controllers = new List<OTController>();
    private Dictionary<System.Type, OTController> controllerLookupType = new Dictionary<System.Type, OTController>();
    private Dictionary<string, OTController> controllerLookupName = new Dictionary<string, OTController>();

    // -----------------------------------------------------------------
    // virtual methods
    // -----------------------------------------------------------------
    /// <summary>
    /// Overridable virtual method that will provide object's mesh
    /// </summary>
    /// <returns>object's mesh</returns>
    protected virtual Mesh GetMesh()
    {
        return null;
    }

    /// <summary>
    /// Overridable virtual method that will provide object's type name
    /// </summary>
    /// <returns>object's type name</returns>
    protected virtual string GetTypeName()
    {
        return "Object";
    }

    void SetCollider()
    {
        if (physics == Physics.Custom)
            return;

        if (colliderType == ColliderType.Box)
        {
            BoxCollider b = GetComponent<BoxCollider>();
            if (b != null)
            {
                if (collidable)
                {
                    if (b.size.x == 0 || b.size.y == 0)
                        b.size = new Vector3(1, 1, collisionSize);
                    else
                        b.size = new Vector3(b.size.x, b.size.y, collisionSize);
                    b.center = (pivotPoint * -1);
                    b.center += new Vector3(0, 0, collisionDepth - depth);
                    b.isTrigger = true;

                    if (physics == Physics.Trigger)
                        b.isTrigger = true;
                    else
                        b.isTrigger = false;
                }
                else
                {
                    if (b.size.x == 0 || b.size.y == 0)
                        b.size = new Vector3(1, 1, 0);
                    else
                        b.size = new Vector3(b.size.x, b.size.y, 0);
                    b.center = pivotPoint * -1;
                    b.isTrigger = false;
                }
            }

        }
        else
        {
            SphereCollider s = GetComponent<SphereCollider>();
            if (s != null)
            {
                if (s.radius == 0)
                    s.radius = 0.5f;
                if (collidable)
                {
                    s.center = (pivotPoint * -1);
                    s.center += new Vector3(0, 0, collisionDepth - depth);
                    s.isTrigger = true;

                    if (physics == Physics.Trigger)
                        s.isTrigger = true;
                    else
                        s.isTrigger = false;
                }
                else
                {
                    s.center = pivotPoint * -1;
                    s.isTrigger = false;
                }
            }
        }
    }

    void CheckInputCollidable()
    {
        if (physics == Physics.Custom)
        {
            if (registerInput)
                OT.InputTo(this);
            else
                OT.NoInputTo(this);
            return;
        }

        BoxCollider b = GetComponent<BoxCollider>();
        SphereCollider sp = GetComponent<SphereCollider>();
        Rigidbody bd = GetComponent<Rigidbody>();

        if (!collidable && physics != Physics.Trigger)
        {
            _collidable = true; _collidable_ = true;
        }

        if (collidable)
        {
            if (colliderType == ColliderType.Box)
            {
                if (b == null)
                    b = gameObject.AddComponent<BoxCollider>();
                if (sp != null)
                    DestroyImmediate(sp);
            }
            else
            {
                if (sp == null)
                    sp = gameObject.AddComponent<SphereCollider>();
                if (b != null)
                    DestroyImmediate(b);
            }

            if (bd == null)
                bd = gameObject.AddComponent<Rigidbody>();

            switch (physics)
            {
                case Physics.Trigger:
                    bd.useGravity = false;
                    bd.isKinematic = true;
                    break;
                case Physics.RigidBody:
                    bd.useGravity = true;
                    bd.isKinematic = false;
                    break;
                case Physics.NoGravity:
                    bd.useGravity = false;
                    bd.isKinematic = false;
                    break;
                case Physics.StaticBody:
                    bd.useGravity = true;
                    bd.isKinematic = true;
                    collisionDepth = 0;
                    break;
                case Physics.StaticRigidBody:
                    bd.useGravity = true;
                    bd.isKinematic = true;
                    break;
            }
            bd.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
        }
        else
        {
            if (bd != null)
                DestroyImmediate(bd);
            if (colliderType == ColliderType.Box && sp != null)
            {
                DestroyImmediate(sp);
                sp = null;
            }
            if (colliderType == ColliderType.Sphere && b != null)
            {
                DestroyImmediate(b);
                b = null;
            }
        }

        if (registerInput)
        {
            if (colliderType == ColliderType.Box && b == null)
                b = gameObject.AddComponent<BoxCollider>();
            if (colliderType == ColliderType.Sphere && sp == null)
                sp = gameObject.AddComponent<SphereCollider>();
            OT.InputTo(this);
        }
        else
        {
            OT.NoInputTo(this);
            if (!registerInput && b != null && !collidable)
                DestroyImmediate(b);
            if (!registerInput && sp != null && !collidable)
                DestroyImmediate(sp);
        }
        SetCollider();
    }

    /// <summary>
    /// Overridable virtual method that will check object's editor settings
    /// </summary>
    protected virtual void CheckSettings()
    {
        if (collidable != _collidable_ && !collidable)
        {
            if (_physics != Physics.Custom)
                _physics = Physics.Trigger;
        }

        if (physics != _physics_ || _collisionSize == -1)
        {
            switch (physics)
            {
                case Physics.Custom:
                    _collisionSize = 0;
                    break;
                case Physics.Trigger:
                    _collisionSize = 0.4f;
                    break;
                case Physics.RigidBody:
                    _collisionSize = 10f;
                    break;
                case Physics.NoGravity:
                    _collisionSize = 10f;
                    break;
                case Physics.StaticBody:
                    _collisionSize = 2000f;
                    break;
                case Physics.StaticRigidBody:
                    _collisionSize = 10f;
                    break;
            }
        }

        if (registerInput != _registerInput_ || collidable != _collidable_ || collisionDepth != _collisionDepth_ || physics != _physics_ ||
            colliderType != _colliderType_ || collisionSize != _collisionSize_)
        {
            _registerInput_ = registerInput;
            _collidable_ = collidable;
            _collisionDepth_ = collisionDepth;
            _physics_ = physics;
            _collisionSize_ = collisionSize;
            _colliderType_ = colliderType;
            CheckInputCollidable();
        }

        if (depth < -1000) depth = -1000;
        if (depth > 1000) depth = 1000;

        if (_depth_ != depth)
        {
            _depth_ = depth;
            _paintingDepth = depth + (position.y / 1000) + (position.x / 10000);
        }

        if (size.x < 0 || size.y < 0)
            size = new Vector2((size.x < 0) ? 0 : size.x, (size.y < 0) ? 0 : size.y);

        if (name != _name_)
        {
            OT.RegisterLookup(this, _name_);
            _name_ = name;
            gameObject.name = name;
            baseName = name;
        }
        else
            if (name != gameObject.name)
            {
                _name = gameObject.name;
                OT.RegisterLookup(this, _name_);
                _name_ = name;
                baseName = name;
#if UNITY_EDITOR
				if (!Application.isPlaying)
					UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif		
            }
						
        if (!Vector2.Equals(_size_, _size) || !Vector2.Equals(_position_, _position) || _rotation_ != _rotation)
        {
            Vector2 pos = _position;
            if (worldBounds.width != 0)
            {
                float minX = _worldBounds.xMin;
                float maxX = _worldBounds.xMax;
                float minY = _worldBounds.yMin;
                float maxY = _worldBounds.yMax;
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                if (!Vector2.Equals(_position, pos))
                    _position = pos;
            }

            if (!Vector2.Equals(_size_, _size))
            {
                float x = _size.x;
                float y = _size.y;
                if (OT.view.customSize == 0)
                    if (x < 1 || y < 1)
                    {
                        _size = new Vector2((x <= 0) ? 1 : x, (y <= 0) ? 1 : y);
                    }
            }

            if (!Vector2.Equals(_size_, _size))
            {
                size = _size;
                _size_ = _size;
            }
            if (!Vector2.Equals(_position_, _position))
            {
                position = _position;
                _position_ = _position;
            }
            if (!Vector2.Equals(_rotation_, _rotation))
            {
                rotation = _rotation;
                _rotation_ = _rotation;
            }
        }

        if (!Vector2.Equals(size, _size) || !Vector2.Equals(position, _position) || rotation != _rotation)
        {
            Vector2 pos = position;
            if (worldBounds.width != 0)
            {
                float minX = _worldBounds.xMin;
                float maxX = _worldBounds.xMax;
                float minY = _worldBounds.yMin;
                float maxY = _worldBounds.yMax;
                pos.x = Mathf.Clamp(pos.x, minX, maxX);
                pos.y = Mathf.Clamp(pos.y, minY, maxY);
                if (!Vector2.Equals(position, pos))
                    position = pos;
            }
            if (!Vector2.Equals(size, _size))
            {
                _size = size;
                _size_ = size;
            }
            if (!Vector2.Equals(position, _position))
            {
                _position = position;
                _position_ = position;
            }
            if (!Vector2.Equals(rotation, _rotation))
            {
                _rotation = rotation;
                _rotation_ = rotation;
            }
#if UNITY_EDITOR
			if (!Application.isPlaying)
				UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif		
            _paintingDepth = depth + (position.y / 1000) + (position.x / 10000);
        }

        if (transform.position.z != _paintingDepth)
            transform.position = new Vector3(transform.position.x, transform.position.y, _paintingDepth);

    }

    /// <summary>
    /// Overridable virtual method that will check Objects mesh settings
    /// </summary>
    protected virtual void CheckDirty()
    {
        if (_pivot_ != _pivot)
        {
            pivot = _pivot;
            _pivot_ = _pivot;
        }
        if (_pivotPoint_ != _pivotPoint)
        {
            _pivotPoint_ = pivotPoint;
            meshDirty = true;
        }
    }

    /// <summary>
    /// Overridable virtual method that will clean the dirty object
    /// </summary>
    protected virtual void Clean()
    {
    }

    
    public virtual void Assign(OTObject protoType)
    {
        rotation = protoType.rotation;
        position = protoType.position;
        size = protoType.size;
        if (pivot != protoType.pivot || pivotPoint != protoType.pivotPoint)
        {
            pivot = protoType.pivot;
            pivotPoint = protoType.pivotPoint;
            Update();
        }
    }

    /// <summary>
    /// Overridable virtual method that is called after create the object's mesh
    /// </summary>
    protected virtual void AfterMesh()
    {
        size = _size;
        SetCollider();
    }

    
    public virtual void OnInput(Vector2 hitPoint)
    {
        _hitPoint = hitPoint - position;

        if (onInput != null)
            onInput(this);
        if (!CallBack("onInput", callBackParams))
            CallBack("OnInput", callBackParams);

        return;
    }
    
    public virtual void OnMouseMove(Vector2 hitPoint)
    {
        _hitPoint = OT.view.mouseWorldPosition - position;

        if (onMouseMoveOT != null)
            onMouseMoveOT(this);
        if (!CallBack("onMouseMoveOT", callBackParams))
            CallBack("OnMouseMoveOT", callBackParams);

        return;
    }
    
    public virtual void OnMouseEnter()
    {
        _hitPoint = OT.view.mouseWorldPosition - position;

        if (onMouseEnterOT != null)
            onMouseEnterOT(this);
        if (!CallBack("onMouseEnterOT", callBackParams))
            CallBack("OnMouseEnterOT", callBackParams);

        return;
    }
    
    public virtual void OnMouseDrag()
    {
        if (!draggable) return;
        if (!Input.GetMouseButton(dragButton)) return;

        if (!isDragging)
        {
            if (onDragStart != null)
                onDragStart(this);
            if (!CallBack("onDragStart", callBackParams))
                CallBack("OnDragStart", callBackParams);
            isDragging = true;
            _hitPoint = OT.view.mouseWorldPosition - position;
        }
        position = OT.view.mouseWorldPosition - _hitPoint;
    }

    
    public virtual void OnMouseUp()
    {
        if (isDragging)
        {
            if (onDragEnd != null)
                onDragEnd(this);
            if (!CallBack("onDragEnd", callBackParams))
                CallBack("OnDragEnd", callBackParams);

            _dropTarget = OT.ObjectUnderPoint(Input.mousePosition, new OTObject[] { }, new OTObject[] { this });
            if (_dropTarget != null)
            {
                _dropTarget.SetDropped(this);
                if (_dropTarget.onReceiveDrop != null)
                    _dropTarget.onReceiveDrop(_dropTarget);
            }
            isDragging = false;
        }
    }
    
    public virtual void OnMouseExit()
    {
        _hitPoint = OT.view.mouseWorldPosition - position;

        if (onMouseExitOT != null)
            onMouseExitOT(this);
        if (!CallBack("onMouseExitOT", callBackParams))
            CallBack("OnMouseExitOT", callBackParams);

        return;
    }

    // -----------------------------------------------------------------
    // class methods
    // -----------------------------------------------------------------
    /// <summary>
    /// Object has to use callback functions.
    /// </summary>
    /// <param name="target">target class that will receive the callbacks.</param>
    public void InitCallBacks(Component target)
    {
        callBackTargets.Add(target);
    }

    /// <summary>
    /// Rotate so we are heading (up vector) to a specific position.
    /// </summary>
    /// <param name="toPosition">Position that we will be heading to.</param>
    public void RotateTowards(Vector2 toPosition)
    {
        Vector2 upVector = toPosition - position;
        transform.up = upVector.normalized;
        _rotation = rotation;
        _rotation_ = _rotation;
    }

    
    public void SetDropped(OTObject o)
    {
        _dropTarget = o;
    }

    
    public virtual void StartUp()
    {
        if (!OT.IsRegistered(this))
            OT.Register(this);
        if (registerInput)
            OT.InputTo(this);
        else
            OT.NoInputTo(this);

        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf != null) mesh = mf.mesh;

        inView = true;
    }

    // -----------------------------------------------------------------
    // class methods
    // -----------------------------------------------------------------

    void OnTriggerEnter(Collider c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
        if (_collisionObject != null)
        {
            if (_collisionObject.collidable)
            {
                if (onCollision != null)
                    onCollision(this);
                if (!CallBack("onCollision", callBackParams))
                    CallBack("OnCollision", callBackParams);
            }
            if (onEnter != null)
                onEnter(this);
            if (!CallBack("onEnter", callBackParams))
                CallBack("OnEnter", callBackParams);
        }
    }

    void OnTriggerExit(Collider c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
        if (onExit != null)
            onExit(this);
        if (!CallBack("onExit", callBackParams))
            CallBack("OnExit", callBackParams);
    }

    void OnTriggerStay(Collider c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
        if (onStay != null)
            onStay(this);
        if (!CallBack("onStay", callBackParams))
            CallBack("OnStay", callBackParams);
    }

    void OnCollisionEnter(Collision c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
		_collision = c;
        if (_collisionObject != null)
        {
            if (_collisionObject.collidable)
            {
                if (onCollision != null)
                    onCollision(this);
                if (!CallBack("onCollision", callBackParams))
                    CallBack("OnCollision", callBackParams);
            }
            if (onEnter != null)
                onEnter(this);
            if (!CallBack("onEnter", callBackParams))
                CallBack("OnEnter", callBackParams);
        }
    }

    void OnCollisionExit(Collision c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
		_collision = c;
        if (onExit != null)
            onExit(this);
        if (!CallBack("onExit", callBackParams))
            CallBack("OnExit", callBackParams);
    }

    void OnCollisionStay(Collision c)
    {
        _collisionObject = c.gameObject.GetComponent<OTObject>();
		_collision = c;
        if (onStay != null)
            onStay(this);
        if (!CallBack("onStay", callBackParams))
            CallBack("OnStay", callBackParams);
    }
	
    
    protected bool CallBack(string handler, object[] param)
    {
		// if flash we cant use reflection to activate a callback
#if UNITY_FLASH
#else
        for (int t = 0; t < callBackTargets.Count; t++)
        {
            MethodInfo mi = callBackTargets[t].GetType().GetMethod(handler);
            if (mi != null)
            {
                mi.Invoke(callBackTargets[t], param);
                return true;
            }
        }
#endif
        return false;
    }

    
    protected virtual void Awake()
    {
#if UNITY_EDITOR
		if (!Application.isPlaying)
			UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif		
        // initialize attributes
        _position_ = position;
        _depth_ = depth;
        _size_ = size;
        _rotation_ = rotation;
        _pivot_ = pivot;
        _pivotPoint_ = pivotPoint;
        _name_ = name;
        _collidable_ = collidable;
        _collisionDepth_ = collisionDepth;
        _collisionSize_ = collisionSize;
        _physics_ = physics;
        _paintingDepth = depth + (position.y / 1000) + (position.x / 10000);				
    }

    // -----------------------------------------------------------------
    // Use this for initialization
    
    protected OTObject copyObject = null;
	
    
    protected virtual void Start()
    {				
		callBackParams = new object[] { this };		
        if (!OT.isValid)
        {
            Debug.LogError("Orthello : Orthello main OT system object not available!");
            Debug.Log("Please remove this Orthello object and add the OT object first.");
        }
		
        isCopy = (name != "" && OT.ObjectByName(name) != null && OT.ObjectByName(name) != this);
        if (isCopy)
            copyObject = OT.ObjectByName(name);
				
        if (_name == "" || isCopy || _isPrefab)
        {
            if (copyObject != null || _isPrefab)
            {
				if (copyObject !=null)
				{
	                baseName = copyObject.baseName;
	                if (baseName == "")
	                    baseName = name;
				}				
				if (baseName!="")
				{
	                int baseIdx = 1;
	                while (GameObject.Find(baseName + "-" + baseIdx))
	                    baseIdx++;
	                name = baseName + "-" + baseIdx;
#if UNITY_EDITOR
					if (!Application.isPlaying)
						UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif			
				}
            }
			if (name == "")
			{
                name = GetTypeName() + " (id=" + this.gameObject.GetInstanceID() + ")";
#if UNITY_EDITOR
				if (!Application.isPlaying)
					UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(this);
#endif											
			}
			_isPrefab = false;
        }

        // check if we have a meshfilter
        MeshFilter mf = GetComponent<MeshFilter>();
        if (mf == null)
            mf = gameObject.AddComponent<MeshFilter>();
        // check if we have a mesh renderer
        MeshRenderer mr = GetComponent<MeshRenderer>();
        if (mr == null)
            mr = gameObject.AddComponent<MeshRenderer>();

        // check if we have to generate a mesh for this object
        if (Application.isPlaying) mesh = mf.mesh;
        else mesh = mf.sharedMesh;

        if (mesh == null || mesh.vertexCount == 0 || isCopy)
        {
            Mesh m = null;
            if (Application.isPlaying) m = mf.mesh;
            else m = mf.sharedMesh;

            mesh = GetMesh();
            if (mesh != null)
            {
                if (Application.isPlaying) mf.mesh = mesh;
                else mf.sharedMesh = mesh;
                AfterMesh();

                if (!isCopy && m != null)
                {
                    if (m.uv.Length > 0)
                        mesh.uv = m.uv;
                    DestroyImmediate(m);
                }
                meshDirty = false;
            }
        };

        if (!OT.IsRegistered(this))
            OT.Register(this);

        CheckInputCollidable();
        if (Application.isPlaying)
            oSize = Vector2.zero;
        else
        {
            if (init)
            {
                if (OT.view.customSize > 0)
                    _size *= OT.view.sizeFactor;
                init = false;
            }
        }
    }
		
    
    public void ForceUpdate()
    {
        Update();
    }
	
    // Update is called once per frame
    
    /// 
    protected virtual void Update()
    {
        if (!OT.isValid || transform == null || gameObject == null) return;

		
        if (!Application.isPlaying || dirtyChecks || OT.dirtyChecks)
        {
            if (registerInput != _registerInput_ && !registerInput && draggable)
                draggable = false;

            if (draggable && !registerInput)
                _registerInput = true;

            if (!OT.recordMode)
            {
                CheckSettings();
                CheckDirty();
            }			
        }

        if (meshDirty)
        {						
            transform.localScale = Vector3.one;

            MeshFilter mf = GetComponent<MeshFilter>();
            Mesh m = null;
            if (Application.isPlaying)
            {
                m = mf.mesh;
                mesh = GetMesh();
                if (mesh != null)
                    mf.mesh = mesh;
            }
            else
            {
                m = mf.sharedMesh;
                mesh = GetMesh();
                if (mesh != null)
                    mf.sharedMesh = mesh;
            }
            if (mesh != null)
            {
                if (m != null && !isCopy)
                {
                    if (Application.isPlaying)
                        Destroy(m);
                    else
                        DestroyImmediate(m);
                }
                meshDirty = false;
                AfterMesh();
            }
        }

        if (isDirty)
            Clean();


        if (isCopy)
            isCopy = false;

        if (controllers.Count > 0)
        {
            for (int c = 0; c < controllers.Count; c++)
            {
                OTController co = controllers[c];
                if (co.enabled)
                    co.Update(Time.deltaTime);
            }
        }
		
    }

    void OnBecameVisible()
    {
        inView = true;
        if (onIntoView != null)
            onIntoView(this);
        if (!CallBack("onIntoView", callBackParams))
            CallBack("OnIntoView", callBackParams);
    }

    void OnBecameInvisible()
    {
        inView = false;
        if (onOutOfView != null)
            onOutOfView(this);
        if (!CallBack("onOutOfView", callBackParams))
            CallBack("OnOutOfView", callBackParams);
    }

    /// <summary>
    /// Get a Controller from this object with a specific type
    /// </summary>
    /// <param name="controllerType">Type of controller to find</param>
    /// <returns>Controller found and null if none was found</returns>
    public OTController Controller(System.Type controllerType)
    {
        OTController c = null;
        if (controllerLookupType.ContainsKey(controllerType))
            c = controllerLookupType[controllerType];
        else
        {
            for (int ci = 0; ci < controllers.Count; ci++)
            {
                OTController co = controllers[ci];
                if (co.GetType().IsSubclassOf(controllerType))
                    return co;
            }
        }
        return c;
    }
    /// <summary>
    /// Get a Controller from this object with a specific name
    /// </summary>
    /// <param name="name">Name of controller to find</param>
    /// <returns>Controller found and null if none was found</returns>
    public OTController Controller(string name)
    {
        OTController c = null;
        if (controllerLookupName.ContainsKey(name))
            c = controllerLookupName[name];
        return c;
    }
    /// <summary>
    /// Get a Controller from this object with a specific type
    /// </summary>
    /// <returns>Controller found and null if none was found</returns>
    public OTController Controller<T>()
    {
        return Controller(typeof(T));
    }

    /// <summary>
    /// Adds a controller to this object
    /// </summary>
    /// <param name="c">Controller to add</param>
    public void AddController(OTController c)
    {
        if (!controllers.Contains(c))
        {
            controllers.Add(c);
            controllerLookupName.Add(c.name, c);
            controllerLookupType.Add(c.GetType(), c);
            c.SetOwner(this);
        }
    }
    /// <summary>
    /// Removes a controller from this object
    /// </summary>
    /// <param name="c">Controller to remove</param>
    public void RemoveController(OTController c)
    {
        if (c == null) return;

        if (controllerLookupType.ContainsKey(c.GetType()))
            controllerLookupType.Remove(c.GetType());
        if (controllerLookupName.ContainsKey(c.name))
            controllerLookupName.Remove(c.name);
        if (controllers.Contains(c))
            controllers.Remove(c);
    }
    /// <summary>
    /// Removes all controllers
    /// </summary>
    public void ClearControllers()
    {
        while (controllers.Count > 0)
            RemoveController(controllers[0]);
    }
		
	public virtual void Dispose()
	{
		callBackTargets.Clear();
		ClearControllers();
	}
	    
    protected void OnDestroy()
    {
        if (mesh != null)
            DestroyImmediate(mesh);

        if (OT.isValid)
            OT.RemoveObject(this);

    }

}