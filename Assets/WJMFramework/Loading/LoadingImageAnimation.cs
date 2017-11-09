using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class LoadingImageAnimation : Loading
{
    public RawImage bg;
    public RawImage loadingRing;
    public Text text;
    public Text percentText;
    public Button reTryBtn;
    Vector2 reTryBtnOrginPos;
    public Slider slider;

    bool oneTimeStart = true;
    bool oneTimePercent = true;

    public override void LoadingAnimation(NetCtrlManager.RequestHandler r, AsyncOperation inAsy,string info)
    {
        base.LoadingAnimation(r,inAsy,info);
        text.text = displayInfo;
    }

    public override void StartLoading(NetCtrlManager.RequestHandler r, AsyncOperation inAsy, string info)
    {
        base.StartLoading(r,inAsy, info);

        if(oneTimeStart)
        {
            oneTimeStart = false;
            reTryBtn.onClick.AddListener(r.ManualRetry);
            reTryBtn.onClick.AddListener(HiddenRetry);
            reTryBtnOrginPos = reTryBtn.GetComponent<RectTransform>().localPosition;
            reTryBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(5000, 0);
            canvasGroup.DOFade(1.0f, 0.5f);

        }

        smoothPercent = 0;
        loadingRing.rectTransform.DORotate(new Vector3(0, 0, -7200), 15.0f, RotateMode.LocalAxisAdd);

    }

    
    public override void DisplayRetry()
    {
        base.DisplayRetry();
        reTryBtn.GetComponent<RectTransform>().anchoredPosition = reTryBtnOrginPos;
        reTryBtn.GetComponent<RectTransform>().DOScale(1.0f, 0.5f);
    }

    public override void HiddenRetry()
    {
        base.HiddenRetry();
        reTryBtn.GetComponent<RectTransform>().DOScale(0.001f, 0.2f);
        reTryBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(5000, 0);
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
                loadingRing.rectTransform.anchoredPosition = new Vector2(5000, 0);
                slider.transform.localScale = new Vector3(1, 1, 1);
            }


            text.text =request.helpInfo;
            percentText.text = " "+Mathf.Clamp((int)(smoothPercent * 105),0,100).ToString() + " %";
            slider.value = smoothPercent*1.05f;
        }

    }

    public override void FinishLoading()
    {
        base.FinishLoading();

    }




}
