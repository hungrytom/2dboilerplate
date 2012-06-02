using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Provides 'static' functionality to control the Orthello 2D Framework. 
/// </summary>
/// <remarks>
/// The OT class is used to offer some base 'static' framework functionality.
/// <br></br><br></br>
/// Use the OT class to :
/// <br></br><br></br>
/// <ul>
/// <li>Check if framework is initialized (isValid).</li>
/// <li>Find your objects, animations, (sprite)containers and materials programmaticly.</li>
/// <li>Set your default solid/transparent and additive material.</li>
/// <li>Create a sprite programmaticly.</li>
/// <li> a sprite programmaticly.</li>
/// </ul>
/// <br></br><br></br>
/// <strong style='color:red'>!IMPORTANT</strong> The OT class is already added as a script to the (prefab) <strong>/Orthello/Objects/OT object</strong> and is
/// only used on this object. The OT object has to be added to a new scene (with a main camera) to 
/// initialize the Orthello 2D framework. This <strong>must be done before adding other Orthello objects like sprites</strong> to your scene.
/// <br></br><br></br>
/// <strong style='color:red'>!IMPORTANT</strong> If you would like to <strong>use the OT class with the static functions from Javascript</strong>, make sure that you
/// create a root 'Standard Assets' folder in your project and  move the OT C# script into it. By doing so, it will be available in
/// your JS scripts and not give you a 'type not found' error.
/// </remarks>
[ExecuteInEditMode]
public class OT : MonoBehaviour
{
    static OT instance = null;
    
    
    public OTObjectType[] objectPrototypes;
    /// <summary>
    /// Material references known to the Orthello framework.
    /// </summary>
    /// <remarks>
    /// Orthello comes with several base materials that are pre-configured. These materials can be 
    /// expanded at your covenience. The base principle is to link a material name to a material
    /// object and use that material on an object by assigning <see cref="OTSprite.materialReference" />.
    /// The default materials : solid, transparent and additive can be activated by checking the
    /// setting the sprite properties <see cref="OTSprite.transparent" /> and <see cref="OTSprite.additive" />.
    /// </remarks>
    
    public OTMatRef[] materials;

   	
    /// <summary>
    /// Check and handle object setting changes while playing.
    /// </summary>   
    /// <remarks>
    /// When the system checks settings, it will for example
    /// add a collider and a rigidbody when you set collidable to true.
    /// Normally you wont need this functionality while the application
    /// is playing but you can set this to true when you are building
    /// up sprites from scratch. This will cost about 10fps (very rough estimate).
    /// </remarks>
    public static bool dirtyChecks = true;

    /// <summary>
    /// Uses object pooling when creating and destroying objects
    /// </summary>   
    /// <remarks>
    /// When objectPooling is set to true, Orthello uses object pooling when
    /// creating and destroying objects. This means that when an object is
    /// created using the OT.CreateObject(..) method is will be put in an
    /// object pool when it is destroyed. A new CreateObject request will than 
    /// re-use this object instead of creating another one.<br></br><br></br>
    /// If more objects need to created than available in the pool, the system
    /// will instantiate objects as normal.<br></br><br></br>
    /// You can use <see cref="OT.PreFabricate" />(prototype,number) to fill the object pool
    /// even before objects are requested using OT.CreateObject.
    /// </remarks>
    public static bool objectPooling = true;

    /// <summary>
    /// Show debug info with OT.Print(msg)
    /// </summary>
    public static bool debug = false;

    /// <summary>
    /// Check if all containers are ready.
    /// </summary>
    /// <returns>True if all containers are initialized and ready</returns>
    public static bool ContainersReady()
    {
        if (!OT.isValid) return false;
        for (int c = 0; c < containerCount; c++)
        {
            OTContainer co = instance.containerList[c];
            if (!co.isReady) return false;
        }
        return true;
    }


    /// <summary>
    /// Framework initialization indicator.
    /// </summary>
    /// <remarks>
    /// Check this setting to see if the framework is readyto be used.
    /// </remarks>
    public static bool isValid
    {
        get
        {
            if (instance == null)
            {
                GameObject OT = GameObject.Find("OT");
                if (OT != null)
                    instance = OT.GetComponent<OT>();
                if (instance==null)
                    return false;
            }

            if (materialSolid == null || materialTransparent == null || materialAdditive == null)
            {
               Debug.LogError("Orthello : Not all materials assigned on OT object!");
               return false;
            }

            // if we are in editor mode and just compiled source code
            // all dictionaries (lookups) will be cleared so we have to 
            // re-create the lookup tables
            if (!Application.isPlaying)
            {
                if (instance.objects.Count > 0 && instance.lookup.Count == 0)
                    instance.createLookups();
                if (instance.animationList.Count > 0 && instance.animations.Count == 0)
                    instance.createAnimationLookups();
                if (instance.containerList.Count > 0 && instance.containers.Count == 0)
                    instance.createContainerLookups();
				
				if (instance.objectPrototypes.Length>0 && OTObjectType.lookup.Count==0)
				{
	                for (int o = 0; o < instance.objectPrototypes.Length; o++)
					{
						if (!OTObjectType.lookup.ContainsKey(instance.objectPrototypes[o].name.ToLower()))
	                    	OTObjectType.lookup.Add(instance.objectPrototypes[o].name.ToLower(), instance.objectPrototypes[o].prototype);
					}
				}
            }

            return true;
        }
    }

    /// <summary>
    /// Current update frames per second
    /// </summary>
    public static float fps
    {
        get
        {
            if (instance != null)
                return instance._fps;
            else
                return 0;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static int objectCount
    {
        get
        {
            if (instance != null)
                return instance.objects.Count;
            else
                return 0;
        }
    }

    /// <summary>
    /// Number of registered containers
    /// </summary>
    public static int containerCount
    {
        get
        {
            if (instance != null)
                return instance.containerList.Count;
            else
                return 0;
        }
    }

    /// <summary>
    /// Number of registered animations
    /// </summary>
    public static int animationCount
    {
        get
        {
            if (instance != null)
                return instance.animationList.Count;
            else
                return 0;
        }
    }

    /// <summary>
    /// Main camera view controller
    /// </summary>
    public static OTView view
    {
        get
        {
            if (isValid)
                return instance._view;
            else
                return null;
        }
    }
    
    
    public static Material materialSolid
    {
        get
        {
            if (instance != null)
            {
                if (instance._materialSolid == null)
                    instance._materialSolid = instance._GetMaterial("solid",new Color(0.5f,0.5f,0.5f),1);
                return instance._materialSolid;
            }
            else
                return null;
        }
    }

    
    public static Material materialTransparent
    {
        get
        {
            if (instance != null)
            {
                if (instance._materialTransparent == null)
                    instance._materialTransparent = instance._GetMaterial("transparent",new Color(0.5f,0.5f,0.5f),1);
                return instance._materialTransparent;
            }
            else
                return null;
        }
    }

    
    public static Material materialAdditive
    {
        get
        {
            if (instance != null)
            {
                if (instance._materialAdditive == null)
                    instance._materialAdditive = instance._GetMaterial("additive",new Color(0.5f,0.5f,0.5f),1);
                return instance._materialAdditive;
            }
            else
                return null;
        }
    }

    /// <summary>
    /// Checks if we are over a specific GameObject
    /// </summary>
    /// <param name="g">GameObject to check</param>
    /// <returns>True if we are over the GameObject</returns>
    public static bool Over(GameObject g)
    {
        if (g.collider != null)
        {
            RaycastHit hit;
            return (g.collider.Raycast(view.camera.ScreenPointToRay(Input.mousePosition), out hit, 2500f));
        }
        return false;
    }
    /// <summary>
    /// Checks if we are over a specific Orthello object
    /// </summary>
    /// <param name="o">Orthello object to check</param>
    /// <returns>True if we are over the Orthello object</returns>
    public static bool Over(OTObject o)
    {
		return Over(o.gameObject);
    }

    /// <summary>
    /// Checks if we clicked the mouse with a specific button on a GameObject
    /// </summary>
    /// <param name="g">GameObject to check</param>
    /// <param name="button">Mouse button that has to be used</param>
    /// <returns>True if we clicked on the GameObject</returns>
    public static bool Clicked(GameObject g, int button)
    {
        if (g.collider != null && Input.GetMouseButtonDown(button))
        {
            RaycastHit hit;
            return (g.collider.Raycast(view.camera.ScreenPointToRay(Input.mousePosition), out hit, 2500f));
        }
        return false;
    }
    /// <summary>
    /// Checks if we clicked the mouse with the left button on a GameObject
    /// </summary>
    /// <param name="g">GameObject to check</param>
    /// <returns>True if we clicked on the GameObject</returns>
    public static bool Clicked(GameObject g)
    {
        return Clicked(g,0);
    }
    /// <summary>
    /// Checks if we touched a GameObject
    /// </summary>
    /// <param name="g">GameObject to check</param>
    /// <returns>True if we touched the GameObject</returns>
    public static bool Touched(GameObject g)
    {
        return Clicked(g);
    }
    /// <summary>
    /// Checks if we clicked the mouse with the left button on an Orthello object
    /// </summary>
    /// <param name="o">Orthello object to check</param>
    /// <returns>True if we clicked on the Orthello object</returns>
    public static bool Clicked(OTObject o)
    {
        return Clicked(o.gameObject,0);
    }
    /// <summary>
    /// Checks if we touched an Orthello object
    /// </summary>
    /// <param name="o">Orthello object to check</param>
    /// <returns>True if we touched the Orthello object</returns>
    public static bool Touched(OTObject o)
    {
        return Clicked(o.gameObject);
    }

    
    public static OTMatRef GetMatRef(string name)
    {
        if (instance != null)
        {
            return instance._GetMatRef(name);
        }
        else
            return null;
    }

    
    public static void MatInc(Material mat)
    {
        if (instance != null)
            instance._MatInc(mat);
    }
    
    public static void MatDec(Material mat, string matName)
    {
        if (instance != null)
            instance._MatDec(mat, matName);
    }

    
    public static Material GetMaterial(string name,float alpha)
    {
        return GetMaterial(name, new Color(0.5f,0.5f,0.5f), alpha);
    }
    
    public static Material GetMaterial(string name)
    {
        return GetMaterial(name, new Color(0.5f, 0.5f, 0.5f), 1);
    }
    
    public static Material GetMaterial(string name, Color tintColor)
    {
        return GetMaterial(name, tintColor, 1);
    }
    
    public static Material GetMaterial(string name, Color tintColor, float alpha)
    {
        if (isValid)
        {
            Material m = instance._GetMaterial(name, tintColor, alpha);
            if (m == null)
                m = LookupMaterial(name);
            return m;
        }
        return null;
    }

    
    public static Material LookupMaterial(string name)
    {
        if (isValid)
        {
            return instance._LookupMaterial(name);
        }
        return null;
    }

    
    public static bool IsRegistered(OTObject o)
    {
        if (isValid)
            return instance._IsRegistered(o);
        else
            return false;
    }

    
    public static void RegisterLookup(OTObject o, string oldName)
    {
        if (isValid)
            instance._RegisterLookup(o, oldName);
    }

    
    public static void Register(OTObject o)
    {
        if (isValid)
            instance._Register(o);
    }

    
    public static void RegisterContainer(OTContainer container)
    {
        if (isValid)
            instance._RegisterContainer(container);
    }

    
    public static void RegisterContainerLookup(OTContainer container, string oldName)
    {
        if (isValid)
            instance._RegisterContainerLookup(container, oldName);
    }

    /// <summary>
    /// Get a container using a name lookup
    /// </summary>
    /// <param name="name">Name of sprite container object to find</param>
    /// <returns>OTContainer or null is none was found</returns>
    public static OTContainer ContainerByName(string name)
    {
        if (isValid)
            return instance._ContainerByName(name);
        else
            return null;
    }

    
    public static void RegisterAnimation(OTAnimation animation)
    {
        if (isValid)
            instance._RegisterAnimation(animation);
    }

    
    public static void RegisterMaterial(string name, Material mat)
    {
        if (isValid)
            instance._RegisterMaterial(name, mat);
    }

    
    public static void RegisterAnimationLookup(OTAnimation animation, string oldName)
    {
        if (isValid)
            instance._RegisterAnimationLookup(animation, oldName);
    }

    /// <summary>
    /// Get an animation using a name lookup
    /// </summary>
    /// <param name="name">Name of animation object to find</param>
    /// <returns>OTAnimation object or null is none was found</returns>
    public static OTAnimation AnimationByName(string name)
    {
        if (isValid)
            return instance._AnimationByName(name);
        else
            return null;
    }

    /// <summary>
    /// Get an object using a name lookup
    /// </summary>
    /// <param name="name">Name of object to find</param>
    /// <returns>OTObject or null is none was found</returns>
    public static OTObject ObjectByName(string name)
    {
        if (isValid)
            return instance._ObjectByName(name);
        else
            return null;
    }


    
    public static void InputTo(OTObject o)
    {
        if (isValid)
            instance._InputTo(o);
    }
    
    public static void NoInputTo(OTObject o)
    {
        if (isValid)
            instance._NoInputTo(o);
    }

    
    public static bool recordMode
    {
        get
        {
            if (instance != null)
            {
				return instance._recordMode;
            }
            else
                return false;
        }
    }
	
	
    /// <summary>
    /// Creates an objectpool of a specific number of objectPrototype instances
    /// </summary>
    /// <param name="objectPrototype">Name of object prototype for objectpool</param>
    /// <param name="numberOfInstances">Objectpool size</param>
    public static void PreFabricate(string objectPrototype, int numberOfInstances)
    {
        if (isValid)
            instance._PreFabricate(objectPrototype, numberOfInstances);
        return ;
    }

    /// <summary>
    /// Creates a new gameobject from a registered prototype
    /// </summary>
    /// <param name="objectPrototype">Name of object prototype to create</param>
    /// <returns>Created or pooled GameObject</returns>
    public static GameObject CreateObject(string objectPrototype)
    {
        if (isValid)
            return instance._CreateObject(objectPrototype);
        return null;
    }

    /// <summary>
    /// Destroys an Orthello object and puts it back into the object pool
    /// </summary>
    public static void DestroyObject(OTObject o)
    {
        if (isValid)
            instance._DestroyObject(o);
    }

    /// <summary>
    /// Destroys an gameobject and puts it back into the object pool
    /// </summary>
    public static void DestroyObject(GameObject g)
    {
        if (isValid)
            instance._DestroyObject(g);
    }

    /// <summary>
    /// Destroys an Orthello container
    /// </summary>
    public static void DestroyContainer(OTContainer container)
    {
        if (isValid)
            instance._DestroyContainer(container);
    }

    /// <summary>
    /// Destroys an Orthello animation
    /// </summary>
    public static void DestroyAnimation(OTAnimation animation)
    {
        if (isValid)
            instance._DestroyAnimation(animation);
    }

    /// <summary>
    /// Destroys all Orthello objects
    /// </summary>
    public static void DestroyAll()
    {
        if (isValid)
            instance._DestroyAll();
    }

    /// <summary>
    /// Destroys all Orthello objects
    /// </summary>
    public static void Destroy()
    {
        if (isValid)
            instance._DestroyAll();
    }

    
    public static void RemoveObject(OTObject o)
    {
        if (isValid)
            instance._RemoveObject(o);
    }
    
    public static void RemoveObject(GameObject g)
    {
        if (isValid)
            instance._RemoveObject(g);
    }

    
    public static void RemoveAnimation(OTAnimation o)
    {
        if (isValid)
            instance._RemoveAnimation(o);
    }

    
    public static void RemoveContainer(OTContainer o)
    {
        if (isValid)
            instance._RemoveContainer(o);
    }
	
	/// <summary>
	/// We are going to create orthello objects and build them from scratch.
	/// </summary>
	/// <remarks>
	/// Because normaly, orthello objects are created using the Unity3D editor environment
	/// some property checking and handling only takes place in editor mode. Sometimes however,
	/// we will need that same handling in runtime mode, for example, when building orthello objects
	/// from scratch. Using this method, all property checking and handling will be activated 
	/// ( OT.dirtyChecks = true ) for a few (10 should be enough) update cycles.
	/// </remarks>
	public static void RuntimeCreationMode()
	{
        if (isValid)
            instance._RuntimeCreationMode();
	}

    /// <summary>
    /// Prints a message to the console when OT.debug = true
    /// </summary>
    /// <param name="msg"></param>
    public static void Print(string msg)
    {
        if (isValid)
            instance._Print(msg);
    }


    
    public static void RegisterForClick(OTObject o)
    {
        if (isValid)
            instance._RegisterForClick(o);
    }

    List<OTObject> objects = new List<OTObject>();
    List<OTObject> inputObjects = new List<OTObject>();
    Dictionary<string, OTObject> lookup = new Dictionary<string, OTObject>();

    List<OTContainer> containerList = new List<OTContainer>();
    Dictionary<string, OTContainer> containers = new Dictionary<string, OTContainer>();

    List<OTAnimation> animationList = new List<OTAnimation>();
    Dictionary<string, OTAnimation> animations = new Dictionary<string, OTAnimation>();

    Dictionary<string, Material> materialLookup = new Dictionary<string, Material>();
    Dictionary<Material, int> materialCount = new Dictionary<Material,int>();

    Dictionary<string, List<GameObject>> objectPool = new Dictionary<string, List<GameObject>>();
    Dictionary<string, GameObject> objectPoolContainer = new Dictionary<string, GameObject>();
    Dictionary<string, int> objectPoolIndexer = new Dictionary<string, int>();
    Dictionary<int, string> gameObjectProtoTypes = new Dictionary<int, string>();
	
	List<OTController> controllers = new List<OTController>();

    Material _materialSolid = null;
    Material _materialTransparent = null;
    Material _materialAdditive = null;

    bool _debug = false;


    float _fps;
    float fpsTime = 0;

    RaycastHit hit;
    RaycastHit[] hits;

    OTView _view = null;

    // Use this for initialization
    void Awake()
    {
        instance = this;
        // find view class as child of the OT main class
        for (int c = 0; c < transform.childCount; c++)
        {
            _view = transform.GetChild(c).gameObject.GetComponent<OTView>();
            if (_view != null)
                break;
        }
        if (view != null) view.InitView();

        if (Application.isPlaying)
        {
            OTObjectType.lookup.Clear();
            for (int o = 0; o < objectPrototypes.Length; o++)
                OTObjectType.lookup.Add(objectPrototypes[o].name.ToLower(), objectPrototypes[o].prototype);

            // check if we have an OT/Prototypes folder
            for (int c = 0; c < transform.childCount; c++)
            {
                string n = transform.GetChild(c).gameObject.name.ToLower();
                if (n == "prototype" || n == "prototypes")
                {
                    Transform t = transform.GetChild(c);
                    // object prototypes found so add them to the lookup table
                    for (int p = 0; p < t.childCount; p++)
                        OTObjectType.lookup.Add(t.GetChild(p).gameObject.name.ToLower(), t.GetChild(p).gameObject);
                    break;
                }
            }
        }

    }

    
    public OTMatRef _GetMatRef(string name)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            OTMatRef mref = materials[i];
            if (mref.name.ToLower() == name.ToLower())
                return mref;
        }
        return null;
    }

    
    public OTObject[] _ObjectsUnderPoint(Vector2 screenPoint, OTObject[] checkObjects, OTObject[] ignoreObjects)
    {
        List<OTObject> _ignoreObjects = new List<OTObject>(ignoreObjects);
        List<OTObject> _checkObjects = new List<OTObject>(checkObjects);
        List<OTObject> _foundObjects = new List<OTObject>();
        List<RaycastHit> _hits = new List<RaycastHit>();

        Ray ray = Camera.main.ScreenPointToRay(screenPoint);
        hits = Physics.RaycastAll(ray, 5000);
        if (hits.Length > 0)
        {
            for (int h = hits.Length - 1; h >= 0; h--)
            {
                OTObject o = hits[h].collider.gameObject.GetComponent<OTObject>();
                if (_ignoreObjects.Contains(o)) continue;
                if (o != null)
                {
                    if ((_checkObjects.Count > 0 && _checkObjects.Contains(o)) || _checkObjects.Count == 0)
                    {
                        _foundObjects.Add(o);
                        _hits.Add(hits[h]);
                    }
                }
            }
        }
        hits = _hits.ToArray();
        return _foundObjects.ToArray();
    }
    /// <summary>
    /// Get all orthello objects under a specific point
    /// </summary>
    /// <param name="screenPoint">point on screen</param>
    /// <param name="checkObjects">valid objects, if empty all object will be valid</param>
    /// <param name="ignoreObjects">igonore these objects</param>
    /// <returns>Array with found valid objects</returns>
    public static OTObject[] ObjectsUnderPoint(Vector2 screenPoint, OTObject[] checkObjects, OTObject[] ignoreObjects)
    {
        if (!isValid) return null;
        return instance._ObjectsUnderPoint(screenPoint, checkObjects, ignoreObjects);
    }
    /// <summary>
    /// Get all orthello objects under a specific point
    /// </summary>
    /// <param name="screenPoint">point on screen</param>
    /// <param name="checkObjects">valid objects, if empty all object will be valid</param>
    /// <returns>Array with found valid objects</returns>
    public static OTObject[] ObjectsUnderPoint(Vector2 screenPoint, OTObject[] checkObjects)
    {
        if (!isValid) return null;
        return instance._ObjectsUnderPoint(screenPoint, checkObjects, new OTObject[] { });
    }
    /// <summary>
    /// Get all orthello objects under a specific point
    /// </summary>
    /// <param name="screenPoint">point on screen</param>
    /// <returns>Array with found valid objects</returns>
    public static OTObject[] ObjectsUnderPoint(Vector2 screenPoint)
    {
        if (!isValid) return null;
        return instance._ObjectsUnderPoint(screenPoint, new OTObject[] { }, new OTObject[] { });
    }
    /// <summary>
    /// Get all orthello objects under the mouse pointer
    /// </summary>
    /// <returns>Array with found valid objects</returns>
    public static OTObject[] ObjectsUnderPoint()
    {
        if (!isValid) return null;
        return instance._ObjectsUnderPoint(Input.mousePosition, new OTObject[] { }, new OTObject[] { });
    }
    
    public OTObject _ObjectUnderPoint(Vector2 screenPoint, OTObject[] checkObjects, OTObject[] ignoreObjects)
    {
        List<OTObject> _foundObjects = new List<OTObject>(ObjectsUnderPoint(screenPoint, checkObjects, ignoreObjects));
        float depth = 9999;
        OTObject hitObject = null;
        hit = new RaycastHit();
        for (int f = 0; f < _foundObjects.Count; f++)
        {
            OTObject o = _foundObjects[f];
            if (o.depth<=depth)
            {
                hitObject = o;
                hit = hits[f];
                depth = hitObject.depth;
            }
        }
        return hitObject;
    }
    /// <summary>
    /// Get closest object (lowest depth) under a specific point
    /// </summary>
    /// <param name="screenPoint">point on screen</param>
    /// <param name="checkObjects">valid objects, if empty all object will be valid</param>
    /// <param name="ignoreObjects">igonore these objects</param>
    /// <returns>OTObject or null if none was found</returns>
    public static OTObject ObjectUnderPoint(Vector2 screenPoint, OTObject[] checkObjects, OTObject[] ignoreObjects)
    {
        if (!isValid) return null;
        return instance._ObjectUnderPoint(screenPoint, checkObjects, ignoreObjects);
    }
    /// <summary>
    /// Get closest object (lowest depth) under a specific point
    /// </summary>
    /// <param name="screenPoint">point on screen</param>
    /// <param name="checkObjects">valid objects, if empty all object will be valid</param>
    /// <returns>OTObject or null if none was found</returns>
    public static OTObject ObjectUnderPoint(Vector2 screenPoint, OTObject[] checkObjects)
    {
        if (!isValid) return null;
        return instance._ObjectUnderPoint(screenPoint, checkObjects, new OTObject[] { });
    }
    /// <summary>
    /// Get closest object (lowest depth) under a specific point
    /// </summary>
    /// <param name="screenPoint">point on screen</param>
    /// <returns>OTObject or null if none was found</returns>
    public static OTObject ObjectUnderPoint(Vector2 screenPoint)
    {
        if (!isValid) return null;
        return instance._ObjectUnderPoint(screenPoint, new OTObject[] { }, new OTObject[] { });
    }
    /// <summary>
    /// Get closest object (lowest depth) under the mouse pointer
    /// </summary>
    /// <returns>OTObject or null if none was found</returns>
    public static OTObject ObjectUnderPoint()
    {
        if (!isValid) return null;
        return instance._ObjectUnderPoint(Input.mousePosition, new OTObject[] { }, new OTObject[] { });
    }


    void HandleInput(Vector2 screenPoint)
    {
        OTObject hitObject = ObjectUnderPoint(screenPoint, inputObjects.ToArray());
        if (hitObject != null && hitObject.enabled)
           hitObject.OnInput(hit.point);
    }
	
	bool _recordMode = false;
#if UNITY_EDITOR
	float checkRecordModeFrequency = 0.5f;
	float recordModeCheckTime = 0;
    bool RecordMode(bool set, bool value)
    {
        UnityEditor.EditorWindow W = null;
        System.Type T = System.Type.GetType("UnityEditor.AnimationWindow,UnityEditor");
        Object[] allAniWindows = Resources.FindObjectsOfTypeAll(T);
        if (allAniWindows.Length > 0)
            W = (UnityEditor.EditorWindow)allAniWindows[0];

        if (T != null && W != null)
        {
            if (set)
                T.InvokeMember("SetAutoRecordMode", System.Reflection.BindingFlags.InvokeMethod, null, W, new object[] { value });
            System.Object Res = T.InvokeMember("GetAutoRecordMode", System.Reflection.BindingFlags.InvokeMethod, null, W, null);
            return ((bool)Res);
        }
        return false;
    }
#endif
	
    // Update is called once per frame
    void Update()
    {
        if (instance == null) instance = this;
				
#if UNITY_EDITOR	
		recordModeCheckTime+=Time.deltaTime;
		if (recordModeCheckTime > checkRecordModeFrequency)
		{
			_recordMode = RecordMode(false,false);
			recordModeCheckTime = 0;
		}
#endif
		
        if (Application.isEditor || dirtyChecks)
        {
            if (!Vector3.Equals(transform.position, Vector3.zero))
                transform.position = Vector3.zero;
			
			if (dirtyChecks && runTimeCreationMode && dirtyChecksUpdateCycles++ > 10)
			{
				dirtyChecks = false;
				runTimeCreationMode = false;
			}
			
        }

        // check for clicks
        if (inputObjects.Count > 0)
        {
            if (Input.touchCount > 0 ||
                Input.GetMouseButton(0) ||
                Input.GetMouseButton(1) ||
                Input.GetMouseButton(2) ||
                Input.GetMouseButtonDown(0) ||
                Input.GetMouseButtonDown(1) ||
                Input.GetMouseButtonDown(2) ||
                Input.GetMouseButtonUp(0) ||
                Input.GetMouseButtonUp(1) ||
                Input.GetMouseButtonUp(2))
            {
                if (Input.touchCount > 0)
                    HandleInput(Input.touches[0].position);
                else
                    HandleInput(Input.mousePosition);                     
            }
        }

        // calculate fps every 100ms
        fpsTime += Time.deltaTime;
        if (fpsTime >= 0.1f)
        {
            _fps = 1 / Time.deltaTime;
            fpsTime = 0;
        }
		
		
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

    bool _IsRegistered(OTObject o)
    {
        return lookup.ContainsKey(o.name);
    }

    void _RegisterLookup(OTObject o, string oldName)
    {
        if (objects.Contains(o))
        {
            if (lookup.ContainsKey(oldName.ToLower()) && lookup[oldName.ToLower()] == o)
                lookup.Remove(oldName.ToLower());
            if (lookup.ContainsKey(o.name.ToLower()))
                lookup.Remove(o.name.ToLower());
            lookup.Add(o.name.ToLower(), o);
        }
    }

    void _Register(OTObject o)
    {
        if (!objects.Contains(o))
            objects.Add(o);
        if (!lookup.ContainsKey(o.name.ToLower()))
            lookup.Add(o.name.ToLower(), o);
    }

    void _RegisterContainer(OTContainer container)
    {
        if (!containerList.Contains(container))
            containerList.Add(container);
        if (!containers.ContainsKey(container.name.ToLower()))
            containers.Add(container.name.ToLower(), container);
        else
        {
            if (containers[container.name.ToLower()] != container)
                Debug.LogError("More than one SpriteContainer with name '" + container.name + "'");
        }
		
		
		foreach (Transform child in transform)
		{
			if (child.name.ToLower() == "containers")
			{
				container.transform.parent = child.transform;
				break;
			}
		}
		
    }


    void _RegisterContainerLookup(OTContainer container, string oldName)
    {
        if (containerList.Contains(container))
        {
            if (containers.ContainsKey(oldName.ToLower()) && containers[oldName.ToLower()] == container)
                containers.Remove(oldName.ToLower());
            if (containers.ContainsKey(container.name.ToLower()))
                containers.Remove(container.name.ToLower());
            containers.Add(container.name.ToLower(), container);
        }
    }

    OTContainer _ContainerByName(string name)
    {
        if (containers.ContainsKey(name.ToLower()))
            return containers[name.ToLower()];
        else
            return null;
    }
    OTObject _ObjectByName(string name)
    {
        if (lookup.ContainsKey(name.ToLower()))
            return lookup[name.ToLower()];
        else
            return null;
    }

    void _Print(string msg)
    {
        if (_debug)
            Debug.Log(msg);
    }


    void _RegisterAnimation(OTAnimation animation)
    {
        if (!animationList.Contains(animation))
            animationList.Add(animation);
        if (!animations.ContainsKey(animation.name.ToLower()))
            animations.Add(animation.name.ToLower(), animation);
        else
        {
            if (animations[animation.name.ToLower()] != animation)
                Debug.LogError("More than one Animation with name '" + animation.name + "'");
        }
		
		foreach (Transform child in transform)
		{
			if (child.name.ToLower() == "animations")
			{
				animation.transform.parent = child.transform;
				break;
			}
		}
				
		
    }

    void _RegisterAnimationLookup(OTAnimation animation, string oldName)
    {
        if (animationList.Contains(animation))
        {
            if (animations.ContainsKey(oldName.ToLower()) && animations[oldName.ToLower()] == animation)
                animations.Remove(oldName.ToLower());
            if (animations.ContainsKey(animation.name.ToLower()))
                animations.Remove(animation.name.ToLower());
            animations.Add(animation.name.ToLower(), animation);
        }
    }

    OTAnimation _AnimationByName(string name)
    {
        if (animations.ContainsKey(name.ToLower()))
            return animations[name.ToLower()];
        else
            return null;
    }

    void SetInputTo(OTObject o, bool value)
    {
        bool inputTo = inputObjects.Contains(o);
        if (!value && inputTo)
        {
            inputObjects.Remove(o);
        }
        else
        {
            if (value && !inputTo)
                inputObjects.Add(o);
        }
    }
    void _InputTo(OTObject o)
    {
        SetInputTo(o, true);
    }
    void _NoInputTo(OTObject o)
    {
        SetInputTo(o, false);
    }

    void _PreFabricate(string objectProtoType, int numberOfInstances)
    {
        List<GameObject> gObjects;
        string proto = objectProtoType.ToLower();
        GameObject pool = null;
        if (!objectPool.ContainsKey(proto))
        {
            gObjects = new List<GameObject>();
            objectPool.Add(proto, gObjects);
            GameObject pools = GetChild(gameObject, "ObjectPools");
            pool = GetChild(pools, objectProtoType);
            objectPoolContainer.Add(proto, pool);
            objectPoolIndexer.Add(proto, 0);
        }
        else
        {
            gObjects = objectPool[proto];
            pool = objectPoolContainer[proto];
        }
        int index = objectPoolIndexer[proto];
        while(gObjects.Count<numberOfInstances)
        {
            GameObject g = _CreateObject(proto, false);
            g.active = false;
            g.name = objectProtoType + "-" + (++index);
            if (pool!=null)
                g.transform.parent = pool.transform;
            OTObject o = g.GetComponent<OTObject>();
            if (o != null)
                o.name = g.name;
            gObjects.Add(g);
        }
        objectPoolIndexer[proto] = index;
    }

    GameObject GetChild(GameObject parent, string childName)
    {
        Transform t = parent.transform.Find(childName);
        if (t == null)
        {
            GameObject g = new GameObject();
            g.name = childName;
            g.transform.parent = parent.transform;
            t = g.transform;
        }
        return t.gameObject;
    }

    void _ToObjectPool(string poolName, GameObject g)
    {
        g.active = false;
        string _poolName = poolName.ToLower();
        List<GameObject> gObjects;
        GameObject pool = null;
        if (!objectPool.ContainsKey(_poolName))
        {
            gObjects = new List<GameObject>();
            GameObject pools = GetChild(gameObject, "ObjectPools");
            pool = GetChild(pools, poolName);
            objectPool.Add(_poolName, gObjects);
            objectPoolContainer.Add(_poolName, pool);
        }
        else
        {
            gObjects = objectPool[_poolName];
            pool = objectPoolContainer[_poolName];
        }
        if (pool!=null)
            g.transform.parent = pool.transform;
        gObjects.Add(g);
    }

    GameObject _CreateObject(string objectProtoType)
    {
        return _CreateObject(objectProtoType, Application.isPlaying);
    }

    GameObject _CreateObject(string objectProtoType, bool fromPool)
    {
        string proto = objectProtoType.ToLower();
        if (OTObjectType.lookup.ContainsKey(proto))
        {
            if (!fromPool || !objectPooling)
            {
                var g = Instantiate(OTObjectType.lookup[proto]) as GameObject;
                g.name = OTObjectType.lookup[proto].name;
                OTObject o = g.GetComponent<OTObject>();
                if (o != null)
                    o.protoType = objectProtoType;
                else
                    gameObjectProtoTypes.Add(g.GetInstanceID(), proto);
                if (g.renderer != null)
				{
					if (!(o is OTSprite))
                    	g.renderer.enabled = true;
					else
                    	(o as OTSprite).InvalidateSprite();
				}
                return g;
            }
            else
            {
                if (!objectPool.ContainsKey(proto))
                    _PreFabricate(proto, 1);
                List<GameObject> gObjects = objectPool[proto];
                if (gObjects.Count > 0)
                {
                    GameObject g = gObjects[0];
                    gObjects.RemoveAt(0);

                    OTObject o = g.GetComponent<OTObject>();
                    if (o != null)
                    {
                        OTObject oproto = OTObjectType.lookup[proto].GetComponent<OTObject>();
                        if (oproto != null)
                            o.Assign(oproto);
                        o.StartUp();
                    }
                    g.transform.parent = null;
                    g.active = true;
                    if (g.renderer != null)
                        g.renderer.enabled = true;
                    return g;
                }
                else
                {
                    int index = (++objectPoolIndexer[proto]);
                    GameObject g = _CreateObject(objectProtoType, false);
                    g.name = objectProtoType + "-" + index;
                    OTObject o = g.GetComponent<OTObject>();
                    if (o != null)
                        o.name = g.name;
                    objectPoolIndexer[proto] = index;
                    return g;
                }
            }
        }
        return null;
    }

    void _RemoveObject(OTObject o)
    {
        if (objects.Contains(o))
        {
            string lname = o.name.ToLower();
            if (lookup.ContainsKey(lname))
                lookup.Remove(lname);
            if (objects.Contains(o))
                objects.Remove(o);
            if (inputObjects.Contains(o))
                inputObjects.Remove(o);
        }
        o.gameObject.active = false;
    }

    void _RemoveObject(GameObject g)
    {
        OTObject o = g.GetComponent<OTObject>();
        if (o != null)
            _RemoveObject(o);
        else
            g.active = false;
    }

    void _RemoveContainer(OTContainer container)
    {
        if (containerList.Contains(container))
        {
            string lname = container.name.ToLower();
            if (containers.ContainsKey(lname))
                containers.Remove(lname);
            if (containerList.Contains(container))
                containerList.Remove(container);
        }
    }
    void _RemoveAnimation(OTAnimation animation)
    {
        if (animationList.Contains(animation))
        {
            string lname = animation.name.ToLower();
            if (animations.ContainsKey(lname))
                animations.Remove(lname);
            if (animationList.Contains(animation))
                animationList.Remove(animation);
        }
    }

    void _DestroyObject(OTObject o, string pool)
    {
        _RemoveObject(o);
        if (pool != "" && objectPooling)
        {
            o.Dispose();
            _ToObjectPool(pool, o.gameObject);
        }
        else
        {
            if (o.gameObject != null)
                Destroy(o.gameObject);
        }
    }

    void _DestroyObject(OTObject o)
    {
        _DestroyObject(o, o.protoType);
    }

    void _DestroyObject(GameObject g)
    {
        OTObject o = g.GetComponent<OTObject>();
        if (o != null)
            _DestroyObject(o, o.protoType);
        else
        {
            _RemoveObject(g);
            string pool = "";
            if (gameObjectProtoTypes.ContainsKey(g.GetInstanceID()))
                pool = gameObjectProtoTypes[g.GetInstanceID()];

            if (pool != "" && objectPooling)
                _ToObjectPool(pool, g);
            else
               Destroy(g);
        }
    }

    void _DestroyContainer(OTContainer container)
    {
        _RemoveContainer(container);
        if (container.gameObject != null)
            Destroy(container.gameObject);
    }

    void _DestroyAnimation(OTAnimation animation)
    {
        _RemoveAnimation(animation);
        if (animation.gameObject != null)
            Destroy(animation.gameObject);
    }

    void _DestroyAll()
    {
        while (objects.Count > 0)
            _DestroyObject(objects[0]);
        while (containerList.Count > 0)
            _DestroyContainer(containerList[0]);
        while (animationList.Count > 0)
            _DestroyAnimation(animationList[0]);
    }

    void createLookups()
    {
        int o = 0;

        while (o < objects.Count)
        {
            if (objects[o] != null)
            {
                _RegisterLookup(objects[o], objects[o].name);
                o++;
            }
            else
                objects.RemoveAt(o);
        }
    }

    void createAnimationLookups()
    {
        int o = 0;
        while (o < animationList.Count)
        {
            if (animationList[o] != null)
            {
                _RegisterAnimationLookup(animationList[o], animationList[o].name);
                o++;
            }
            else
                animationList.RemoveAt(o);
        }
    }

    void createContainerLookups()
    {
        int o = 0;
        while (o < containerList.Count)
        {
            if (containerList[o] != null)
            {
                _RegisterContainerLookup(containerList[o], containerList[o].name);
                o++;
            }
            else
                containerList.RemoveAt(o);
        }
    }

    void _RegisterForClick(OTObject o)
    {
        if (!inputObjects.Contains(o))
            inputObjects.Add(o);
    }

    
    Material _GetMaterial(string name, Color tintColor, float alpha)
    {
        for (int i = 0; i < materials.Length; i++)
        {
            OTMatRef mref = materials[i];
            if (mref.name.ToLower() == name.ToLower())
            {
                Material mat = mref.material;
                if (mref.fieldColorTint != "")
                    mat.SetColor(mref.fieldColorTint, tintColor);
                if (mref.fieldAlphaChannel != "")
                    mat.SetFloat(mref.fieldAlphaChannel, alpha);
                else
                    if (mref.fieldAlphaColor != "")
                    {
                        Color alphaColor = mat.GetColor(mref.fieldAlphaColor);
                        alphaColor.a = alpha;
                        mat.SetColor(mref.fieldAlphaColor, alphaColor);
                    }
                return mat;
            }
        }
        return null;
    }

    void _MatInc(Material mat)
    {
        if (mat == null) return;
        if (materialCount.ContainsKey(mat))
            materialCount[mat]++;
        else
            materialCount.Add(mat, 1);
    }

    void _MatDec(Material mat, string matName)
    {
        if (mat == null) return;
        if (materialCount.ContainsKey(mat))
        {
            materialCount[mat]--;
            if (materialCount[mat] == 0)
            {
                materialCount.Remove(mat);
                materialLookup.Remove(matName);
                if (Application.isPlaying)
                    Destroy(mat);
                else
                    DestroyImmediate(mat);
            }
        }
    }

    void _RegisterMaterial(string name, Material mat)
    {
        string lname = name.ToLower();
        if (!materialLookup.ContainsKey(lname))
            materialLookup.Add(lname, mat);
    }

    Material _LookupMaterial(string name)
    {
        string lname = name.ToLower();
        if (materialLookup.ContainsKey(lname))
            return materialLookup[lname];
        else
            return null;
    }
	
	int dirtyChecksUpdateCycles = 0;
	bool runTimeCreationMode = false;
	void _RuntimeCreationMode()	
	{
		runTimeCreationMode = true;
		dirtyChecksUpdateCycles = 0;
		dirtyChecks = true;
	}	
	
    
    OTController _Controller(System.Type controllerType, string name)
    {
        name = name.ToLower();
        for (int c = 0; c < controllers.Count; c++)
        {
            if (controllers[c].GetType() == controllerType)
            {
                if (name == "")
                    return controllers[c];
                else
                    if (controllers[c].name == name)
                        return controllers[c];
            }
        }
        return null;
    }
    /// <summary>
	/// Get a Controller from OT with a specific type and with a specific name
    /// </summary>
    /// <param name="controllerType">Type of controller to find</param>
    /// <param name="name">Name of controller to find</param>
    /// <returns>Controller found and null if none was found</returns>
    public static  OTController Controller(System.Type controllerType, string name)
    {
		if (!isValid) return null;
		return instance._Controller(controllerType, name);
	}
    /// <summary>
    /// Get first Controller from OT with a specific type
    /// </summary>
    /// <param name="controllerType">Type of controller to find</param>
    /// <returns>Controller found and null if none was found</returns>
    public static OTController Controller(System.Type controllerType)
    {
		if (!isValid) return null;
        return instance._Controller(controllerType, "");
    }
    /// <summary>
    /// Get a Controller from OT with a specific type and with a specific name
    /// </summary>
    /// <param name="controllerType">Type of controller to find</param>
    /// <param name="name">Name of controller to find</param>
    /// <returns>Controller found and null if none was found</returns>
    public static OTController Controller(string controllerType, string name)
    {
		if (!isValid) return null;
        return instance._Controller(System.Type.GetType(controllerType), name);
    }
    /// <summary>
    /// Get first Controller from OT with a specific type
    /// </summary>
    /// <param name="controllerType">Type of controller to find</param>
    /// <returns>Controller found and null if none was found</returns>
    public OTController Controller(string controllerType)
    {
		if (!isValid) return null;
        return instance._Controller(System.Type.GetType(controllerType), "");
    }
    
    void _AddController(OTController c)
    {
        if (!controllers.Contains(c))
            controllers.Add(c);
    }
	/// <summary>
    /// Adds a controller to OT
    /// </summary>
    /// <param name="c">Controller to add</param>
    public static void AddController(OTController c)
    {
		if (isValid) 
        	instance._AddController(c);
    }
	
	void _RemoveController(OTController c)
    {
        if (controllers.Contains(c))
            controllers.Remove(c);
    }
    /// <summary>
    /// Removes a controller from OT
    /// </summary>
    /// <param name="c">Controller to remove</param>
    public static void RemoveController(OTController c)
    {
		if (isValid) 
        	instance._RemoveController(c);
    }
		
	

}