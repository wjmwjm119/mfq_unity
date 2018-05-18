using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;


public class LoadingAssetBundleAnimation : Loading
{
    public RawImage bg;
    public RawImage loadingRing;
    public Text text;
    public Text percentText;
    public Text tipsText;
    public Button reTryBtn;
    Vector2 reTryBtnOrginPos;
    public SpritePlayer spritePlayer;
    public Slider slider;

    bool oneTimeStart = true;
    bool oneTimePercent = true;
    /*
        string[] tipsGorup =
        {
            "发起讲盘前，音量开大更清晰",
            "点击\"全屏\"更多VR体验哦",
            "户型可以通过微信,分享给亲朋好友哦",
            "双指同时上下滑动,可以抬头低头看",
            "户型漫游时,摁住屏幕的下面1/5处,可以倒退",
            "在VR体验时,准星代替您的手指",
            "在线讲盘是不消耗流量的",
            "每名用户都有一个专属顾问",
            "浏览楼盘评论吧，人民的眼睛是雪亮的！",
            "双指张开并拢，可以缩放楼盘户型。",
            "户型漫游时,摁住屏幕的上半部分前进"
        };
    */

    string[] tipsGorup =
    {
        "正在升级系统，请稍后"
    };



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
            DisplayTips();
            reTryBtn.onClick.AddListener(r.ManualRetry);
            reTryBtn.onClick.AddListener(HiddenRetry);
            reTryBtnOrginPos = reTryBtn.GetComponent<RectTransform>().localPosition;
            reTryBtn.GetComponent<RectTransform>().anchoredPosition = new Vector2(5000, 0);
            canvasGroup.DOFade(1.0f, 0.5f).SetDelay(0.5f);
//            canvasGroup.DOFade(1.0f, 0.0f);
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
                spritePlayer.AlphaPlayForward();
            }


            text.text =request.helpInfo;
            percentText.text = " "+Mathf.Clamp((int)(smoothPercent * 122.22222),0,100).ToString() + " %";
            slider.value = smoothPercent*1.2222f;
        }

    }

    public override void FinishLoading()
    {
        base.FinishLoading();
    }

    void DisplayTips()
    {
        tipsText.text = tipsGorup[Random.Range(0, tipsGorup.Length - 1)];
        StartCoroutine(DisplayTipsIE());
    }

    IEnumerator  DisplayTipsIE()
    {
        yield return new WaitForSeconds(4.0f);
        DisplayTips();
    }



}
