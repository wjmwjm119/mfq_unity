using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;


[System.Serializable]
public class InteractiveAction
{
    public bool disableDefaultAction;
    public string replaceDefaultBtnName;
    //----------为了兼容H5页面
    public CameraUniversal cameraUniversal;
    public string cameraStates = "";
    public Transform needDisplayRoot;
    public Transform pzMesh;
    //----------

    public UnityEvent trueEvent;
    public UnityEvent falseEvent;
}
