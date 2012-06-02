// ------------------------------------------------------------------------
// Orthello 2D Framework Example 
// (C)opyright 2011 - WyrmTale Games - http://www.wyrmtale.com
// ------------------------------------------------------------------------
// More info http://www.wyrmtale.com/orthello
// ------------------------------------------------------------------------
// Because Orthello is created as a C# framework the C# classes 
// will only be available as you place them in the /Standard Assets folder.
//
// If you would like to test the JS examples or use the framework in combination
// with Javascript coding, you will have to move the /Orthello/Standard Assets folder
// to the / (root).
//
// This code was commented to prevent compiling errors when project is
// downloaded and imported using a package.
// ------------------------------------------------------------------------
// Example 2 - Block behaviour class
// ------------------------------------------------------------------------

/*

private var sprite:OTSprite;            // This block's sprite class
private var colorFade:boolean = false;  // color fade notifier
private var fadeTime:Number;            // fade time counter
private var fadeSpeed:Number = 0.5;     // fade speed

private var startColor:Color =          // block color
    new Color(0.4f, 0.4f, 0.4f);
private var hitColor:Color =            // hit block color
    new Color(1f, 1f, 0.9f);

// Use this for initialization
function Start () {
    // Get this block's sprite
	sprite = GetComponent("OTSprite");
	// Because Javascript does not support C# delegate we have to notify our sprite class that 
	// we want to receive notification callbacks.
	// If we have initialized for callbacks our (!IMPORTANT->) 'public' declared call back 
	// functions will be asutomaticly called when an event takes place.
	// HINT : This technique can be used within a C# class as well.
	sprite.InitCallBacks(this);
	// Set block's color
    sprite.tintColor = startColor;
    // Register this material with Orthello so we can re-use it later
    OT.RegisterMaterial("Block-start", new Material(sprite.material));	
	// Generate Highlight materials and store them
	for (var i:int=0; i<10; i++)
	{
		var m:Material = new Material(sprite.material);
		m.SetColor("_EmisColor", Color.Lerp(Color.white,startColor,0.1f * i));
		OT.RegisterMaterial("Block-tint"+i, m);            
	}
	
}

// Update is called once per frame
function Update () {
	if (colorFade)
	{
		var fadeIndex:int = Mathf.Floor((fadeTime / fadeSpeed) * 10);
		sprite.material = OT.LookupMaterial("Block-tint" + fadeIndex);
		// Incement fade time
		fadeTime += Time.deltaTime;
		if (fadeTime >= fadeSpeed)
		{
			// We have faded long enough
			colorFade = false;
			// Set block's color to start color
			sprite.material = OT.LookupMaterial("Block-start");
		}
	}
}

// This function will be called when this block is hit.
public function OnCollision(owner:OTObject)
{
    // Set color fading indicator
    colorFade = true;
    // Determine random hit color 
    hitColor = new Color(0.65f + Random.value * 0.35f, 0.65f + Random.value * 0.35f, 0.65f + Random.value * 0.35f);
    // Reset fade time
    fadeTime = 0;
}

*/