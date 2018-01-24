using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class LoadingAssetBundleTotalInfo : MonoBehaviour
{
    public CanveGroupFade canveGroupFade;
    public RectTransform circlePointPerfab;
    public RectTransform holdRoot;
    public Color defaultColor;
    public Color okColor;

    public List<RectTransform> allCirclePoint;

    float eachSpace=50;
    bool flip;
    int sortID;
    float scale;
    int animationID;

    public void OpenTotalInfo(int assetBundleCount)
    {
        canveGroupFade.AlphaPlayForward();
        CreatTotalCirclePoint(assetBundleCount);
    }

    public void CloseTotalInfo()
    {
        canveGroupFade.AlphaPlayBackward();
        holdRoot.DOScaleX(1, 1).OnComplete(DestroyAllPoint);      
//      Destroy(allCirclePoint);
    }

    void DestroyAllPoint()
    {
        for (int i = 0; i < allCirclePoint.Count; i++)
        {
            Destroy(allCirclePoint[i].gameObject);
        }
        allCirclePoint.Clear();
    }

    void CreatTotalCirclePoint(int totalCirclePoint)
    {
        float totalLenght =Mathf.Max(0, (totalCirclePoint - 1) * 50f);
        float firstStartPos = -totalLenght * 0.5f;

        allCirclePoint = new List<RectTransform>();

        for (int i = 0; i < totalCirclePoint; i++)
        {
            allCirclePoint.Add(   Instantiate(circlePointPerfab, Vector3.zero, new Quaternion(), holdRoot));
            allCirclePoint[i].gameObject.name = "CirclePoint" + i;
            allCirclePoint[i].anchoredPosition = new Vector2(firstStartPos + 50f * i, 0);
        }
        animationID = 0;
        AnimationCircle();
        if (totalCirclePoint>0)
        {
            SetCirclePointOkColor(0);
        }
    }

    void AnimationCircle()
    {
        animationID++;

        sortID = animationID % allCirclePoint.Count;
        if (sortID == 0)
            flip = !flip;

        allCirclePoint[sortID].DOScale(flip?0.75f:1.25f, 0.3f).SetEase(Ease.InOutSine).OnComplete(AnimationCircle);
    }

    public void SetCirclePointOkColor(int id)
    {
//        Debug.Log(id);
        if(id< allCirclePoint.Count)
        allCirclePoint[id].GetComponent<Image>().color = okColor;
    }

}
