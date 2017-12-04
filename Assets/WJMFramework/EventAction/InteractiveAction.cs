using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InteractiveAction
{
    //----------为了兼容H5页面
    public CameraUniversal cameraUniversal;
    public string cameraStates = ",,,,,";
    public Transform needDisplayRoot;
    public Transform pzMesh;
    //----------

}
