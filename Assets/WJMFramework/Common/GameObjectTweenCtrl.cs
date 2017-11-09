using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System;
using System.Collections;
using DG.Tweening;

public class GameObjectTweenCtrl : MonoBehaviour
{


    public void MovePosition(Vector3 toPos)
    {
        transform.DOLocalMove(toPos, 0.5f);

    }

    public void MoveAchorPos(Vector2 v2,float delayTime)
    {
        DOTween.Kill(GetComponent<RectTransform>());

        GetComponent<RectTransform>().DOAnchorPos(v2, 0.5f).SetDelay(delayTime);
    }


    public void MoveAchorPosAndFadeLoop(Vector2 vfrom, Vector2 vTo, float fadeFrom, float fadeTo, float useTimee)
    {
//      DOTween.Kill(GetComponent<RectTransform>());
        GetComponent<RectTransform>().anchoredPosition = vfrom;
        GetComponent<Image>().color = new Color(1, 1, 1, fadeFrom);
        GetComponent<RectTransform>().DOAnchorPos(vTo, useTimee).SetLoops(-1);
        GetComponent<Image>().DOFade(fadeTo, useTimee).SetLoops(-1);
    }



    public void ScaleAndFadeImage2DLoop(float scalefrom,float scaleTo,float fadeFrom,float fadeTo,float useTime)
    {
        GetComponent<RectTransform>().localScale = new Vector3(scalefrom, scalefrom, 1);
        GetComponent<Image>().color = new Color(1,1,1, fadeFrom);

        GetComponent<RectTransform>().DOScale(new Vector3(scaleTo, scaleTo, 1), useTime).SetLoops(-1).SetEase(Ease.InOutBack);
        GetComponent<Image>().DOColor(new Color(1, 1, 1, fadeTo), useTime).SetLoops(-1);
    }






}
