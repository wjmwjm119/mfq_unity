using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ImageZoomInAndZoomOut : MonoBehaviour
{

    public RectTransform rectTransform;

    public Vector2 sizeSmall = new Vector2(200, 200);
    public Vector2 sizeBig = new Vector2(768, 768);

    public Vector2 posSmall = new Vector2(0, 0);
    public Vector2 posBig = new Vector2(512,512);

    public float fadeUseTime=1;


    public void ZoomIn()
    {

        DOTween.Kill(rectTransform);
        rectTransform.DOAnchorPos(posBig, fadeUseTime);
        rectTransform.DOSizeDelta(sizeBig, fadeUseTime);

    }


    public void ZoomOut()
    {

        DOTween.Kill(rectTransform);
        rectTransform.DOAnchorPos(posSmall, fadeUseTime);
        rectTransform.DOSizeDelta(sizeSmall, fadeUseTime);

    }




}
