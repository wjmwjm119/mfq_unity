using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class LoadingScene : Loading
{
    public RawImage bg;
    public RawImage loadingRing;
    public Text text;

    bool oneTimeStart = true;
    bool oneTimePercent = true;



    public override void LoadingAnimation( AsyncOperation inAsy,string info)
    {
        base.LoadingAnimation(inAsy,info);
        text.text = displayInfo;

    }

    public override void StartLoading( AsyncOperation inAsy, string info)
    {
        base.StartLoading(inAsy, info);
    
        if(oneTimeStart)
        {
            oneTimeStart = false;
            canvasGroup.DOFade(1.0f, 0.5f);
        }

        smoothPercent = 0;
        loadingRing.rectTransform.DORotate(new Vector3(0, 0, -7200), 15.0f, RotateMode.LocalAxisAdd);

    }

    
    public override void  SetPercent()
    {
        base.SetPercent();
        smoothPercent = Mathf.Lerp(smoothPercent, asy.isDone ? 1.0f : asy.progress, 0.33f);
        if (!asy.isDone && smoothPercent > 0.001f)
        {
            if (oneTimePercent)
            {
                oneTimePercent = false;
            }
        }


    }

    public override void FinishLoading()
    {
        OnLoadedEvent.Invoke();
        canvasGroup.DOFade(0.0f, 1f).SetDelay(0.3f).OnComplete(()=> { GameObject.Destroy(this.gameObject); });
    }




}
