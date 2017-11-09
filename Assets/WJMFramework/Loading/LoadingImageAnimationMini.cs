using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class LoadingImageAnimationMini : Loading
{

    public Text text;

    public Slider slider;

    bool oneTimeStart = true;
    bool oneTimePercent = true;

    public override void LoadingAnimation(NetCtrlManager.RequestHandler r, AsyncOperation inAsy,string info)
    {
        base.LoadingAnimation(r,inAsy,info);
        text.text = displayInfo;
    }

    public override void LoadingAnimation(AsyncOperation inAsy, string info)
    {
        base.LoadingAnimation(inAsy, info);
        text.text = displayInfo;
    }

    public override void StartLoading(NetCtrlManager.RequestHandler r, AsyncOperation inAsy, string info)
    {
        base.StartLoading(r,inAsy, info);

        if(oneTimeStart)
        {
            oneTimeStart = false;
            canvasGroup.DOFade(1.0f, 0.5f);
        }
        smoothPercent = 0;

    }

    public override void StartLoading( AsyncOperation inAsy, string info)
    {

        base.StartLoading(inAsy, info);

        if (oneTimeStart)
        {
            oneTimeStart = false;
            canvasGroup.DOFade(1.0f, 0.5f);
        }
        smoothPercent = 0;

    }


    public override void DisplayRetry()
    {
        base.DisplayRetry();

    }

    public override void HiddenRetry()
    {
        base.HiddenRetry();
    }

    public override void  SetPercent()
    {
        base.SetPercent();
        smoothPercent = Mathf.Lerp(smoothPercent, asy.isDone ? 1.0f : asy.progress, 0.33f);
//        smoothPercent = asy.progress;
        if (!asy.isDone && smoothPercent > 0.001f)
        {
            if (oneTimePercent)
            {
                oneTimePercent = false;
                slider.transform.localScale = new Vector3(1, 1, 1);
            }


            if (request!=null)
            text.text =request.helpInfo;

            slider.value = smoothPercent*1.05f;
        }

    }

    public override void FinishLoading()
    {
        base.FinishLoading();
    }



}
