// ------------------------------------------------------------------------
// Orthello 2D Framework Example Source Code
// (C)opyright 2011 - WyrmTale Games - http://www.wyrmtale.com
// ------------------------------------------------------------------------
// More info http://www.wyrmtale.com/orthello
// ------------------------------------------------------------------------
// Example 7
// Tweening example
// ------------------------------------------------------------------------
// Main Example 7 Demo class
// ------------------------------------------------------------------------
using UnityEngine;
using System.Collections;

/// <exclude />
public class CExample7 : MonoBehaviour
{


    int mode = 0;
    float time = 0;
    int easeIdx = 0;

    OTEase[] easings = new OTEase[] 
    {
        OTEasing.ElasticOut,
        OTEasing.BounceOut,
        OTEasing.StrongOut,
        OTEasing.StrongIn,
        OTEasing.BackOut,
        OTEasing.BackIn,
        OTEasing.SineOut,
        OTEasing.Linear,
    };

	// Update is called once per frame
	void Update () {
        if (time == 0)
            GameObject.Find("text-21").GetComponent<TextMesh>().text = "Next easing will be : " + easings[easeIdx].GetType().ToString();
        time += Time.deltaTime;
        if (time > 3)
        {
            MoveStars();
            time = 0;
        }
	}

    void MoveStars()
    {
        int idx = 1;
        GameObject g = GameObject.Find("stars-"+idx);
        while (g!=null)
        {
            new OTTween(g.transform,1.5f,easings[easeIdx]).TweenAdd("position",new Vector3((mode==0)?400:-400,0,0));
            g = GameObject.Find("stars-" + (++idx));
        }
        mode = 1 - mode;
        if (mode == 0)
        {
            easeIdx++;
            if (easeIdx == easings.Length)
                easeIdx = 0;
        }
    }

}
