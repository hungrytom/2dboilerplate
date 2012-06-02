using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <exclude />
public class OTTweenController : OTController
{

    List<OTTween> tweens = new List<OTTween>();

    /// <exclude />
    public OTTweenController(string name)
        : base(null, name)
    {
    }

    /// <exclude />
    public OTTweenController()
        : base()
    {
    }

    /// <exclude />
    public void Add(OTTween tween)
    {
      tweens.Add(tween);
    }


    /// <exclude />
    protected override void Update()
    {
        base.Update();

       	int t = 0;
		while (t<tweens.Count)
		{
			if (tweens[t].Update(deltaTime))
				tweens.Remove(tweens[t]);
			else
				t++;
		}
    }

}
