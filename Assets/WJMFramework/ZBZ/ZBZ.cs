using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ZBZ : MonoBehaviour
{

    public Image zbz;
    public CanvasGroup canvasGroup;
    public CameraUniversalCenter cameraUniversalCenter;

    void Update()
    {
        if (zbz != null&& cameraUniversalCenter!=null)
        {
            zbz.rectTransform.localPosition = new Vector3(-2.84444f * cameraUniversalCenter.rotForZBZ, zbz.rectTransform.localPosition.y, 0);
        }
    }





}
