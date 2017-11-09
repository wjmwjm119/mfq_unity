using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoadPath : MonoBehaviour
{

    public int laneCountOnRight=2;
    public int laneCountOnLeft =2;
    /// <summary>
    /// 中间隔离带宽度
    /// </summary>
    public float baseOffset = 0.5f;
    public float widthPerLane = 3.5f;
//    public float roadMaxSpeed = 90f;


    public Vector3[] dir;
    public Vector3[] binormal;

    public List<Vector3> pathPoint;
    //[HideInInspector]

    
    public LanePath[] rightLanePath;
    public LanePath[] leftLanePath;

    

    [System.Serializable]
    public class LanePath
    {
       public Vector3[] lanePath;
    }

}


