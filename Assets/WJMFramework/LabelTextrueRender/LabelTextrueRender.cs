using System.Collections;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class LabelTextrueRender : MonoBehaviour
{
    [HideInInspector]
    public Camera renderUseCamera;
    [HideInInspector]
    public RenderTexture renderTextrue;
    [HideInInspector]
    public Transform labelGroup;
    [HideInInspector]
    public Image headICO;
    [HideInInspector]
    public Text labelText;
    [HideInInspector]
    public Text labelText2;
    [HideInInspector]
    public Image labelTextBG;
    [HideInInspector]
    public Transform louHaoGroup;
    [HideInInspector]
    public Text louHaoText;

    public Sprite[] headICOLib;
    [HideInInspector]
    public Label[] labelRenderList;

    public Mesh mesh;

    public Transform needRenderLabelRoot;


}
