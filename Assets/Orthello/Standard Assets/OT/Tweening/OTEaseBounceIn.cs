using UnityEngine;
using System.Collections;

/// <exclude />
public class OTEaseBounceIn : OTEase {
    OTEaseBounceOut easeOut = new OTEaseBounceOut();
    /// <exclude />
    public override float ease(float t, float b, float c, float d)
    {
        return c - easeOut.ease(d - t, 0, c, d) + b;
    }
}
