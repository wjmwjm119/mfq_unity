using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogDistanceSet : MonoBehaviour
{
    public Color fogColor;
    public float start=800f;
    public float end=1000f;
	// Use this for initialization
	void Start ()
    {
        RenderSettings.fogStartDistance = start;
        RenderSettings.fogEndDistance= end;
        RenderSettings.fogColor = fogColor;

    }
	

}
