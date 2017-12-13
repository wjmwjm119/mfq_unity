using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using DG.Tweening;

public class MiniMapUI : Graphic,IPointerClickHandler
{
    public Camera miniMapCamera;
    bool state;
    public float minScale=0.8f;
    public float maxScale = 2f;

    Vector2 screenPosInMiniMapCamera;
    Vector2 minimapUIRightUpPos;
    Vector2 minimapUILeftDownPos;
    Vector2 minimapUIActualWdithAndHeight;
    Vector2 scaleXY;

    Ray ray;
    RaycastHit hit;

    public void ScaleInAndOut()
    {
        state = !state;
        if (state)
        {
            GetComponent<RectTransform>().DOScale(maxScale, 0.5f);
        }
        else
        {
            GetComponent<RectTransform>().DOScale(minScale, 0.5f);
        }
    }

    public void OnPointerClick(PointerEventData p)
    {
        minimapUIActualWdithAndHeight = new Vector2(rectTransform.sizeDelta.x * rectTransform.localScale.x, rectTransform.sizeDelta.y * rectTransform.localScale.y);
        minimapUIRightUpPos = new Vector2(rectTransform.position.x, rectTransform.position.y);

        scaleXY = new Vector2(rectTransform.position.x/(rectTransform.localPosition.x*2), rectTransform.position.y / (rectTransform.localPosition.y*2));

        screenPosInMiniMapCamera = new Vector2(1.0f-(rectTransform.position.x - p.position.x) / (minimapUIActualWdithAndHeight.x * scaleXY.x),1.0f- (rectTransform.position.y - p.position.y) / (minimapUIActualWdithAndHeight.y * scaleXY.y));

//        Debug.Log("右上角实际位置：" + rectTransform.position);
//        Debug.Log("缩放值："+scaleXY);
//        Debug.Log("鼠标点击位置："+p.position);
//        Debug.Log("左下角实际位置：" + new Vector2(rectTransform.position.x-minimapUIActualWdithAndHeight.x*scaleXY.x, rectTransform.position.y - minimapUIActualWdithAndHeight.y * scaleXY.y));
        Debug.Log("screenPosInMiniMapCamera:" + screenPosInMiniMapCamera);

        ray =miniMapCamera.GetComponent<Camera>().ViewportPointToRay(new Vector3(screenPosInMiniMapCamera.x,screenPosInMiniMapCamera.y,0.0f));

        Debug.DrawLine(ray.origin,ray.origin+ray.direction*1000);

        if (Physics.Raycast(ray, out hit, 1000))
        {
            Debug.Log(hit.collider.name);

            ColliderTriggerButton c = hit.transform.GetComponent<ColliderTriggerButton>();
            if (c != null)
            {
                c.ExeTriggerEvent();
            }
        }


    }



}
