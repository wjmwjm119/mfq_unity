using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultGUI : MonoBehaviour
{
    public AppBridge appBridege;
    public CanveGroupFade defaultGUIRoot;
    public CanveGroupFade mainBtnGroup;
    public ImagePlayer2 quweiImagePlayer;
    public CanveGroupFade triggerHXList;
    public CanveGroupFade triggerXFGroup;
    public CanveGroupFade triggerBackBtn;
    public CanveGroupFade triggerVRBtn;
    public CanveGroupFade triggerShare;
    public CanveGroupFade triggerMusic;
    public CanveGroupFade triggerEnterFangJianPortrait;

    public CanveGroupFade leftWarning;
    public CanveGroupFade rightWarning;


    bool oneTime = false;

    public static bool isLandscape;
    public static bool lastIsLandscape;


    AudioCtrl audioCtrl;

    void Start()
    {
        if (!oneTime)
        {
            oneTime = true;
            isLandscape = false;
            lastIsLandscape = true;
        }
//        DisplayDefaultGUI();
    }

    public void DisplayDefaultGUI()
    {

        if (lastIsLandscape != isLandscape)
        {
            lastIsLandscape = isLandscape;

            if (isLandscape)
            {
                Landscape();
            }
            else
            {
                Portrait();
            }

         }
    }

    //全屏
    public void Landscape()
    {
        leftWarning.AlphaPlayBackward();
        rightWarning.AlphaPlayForward();

        Screen.orientation= ScreenOrientation.LandscapeLeft;
        defaultGUIRoot.AlphaPlayForward();
        mainBtnGroup.AlphaPlayForward();
        triggerHXList.AlphaPlayForward();
        triggerXFGroup.AlphaPlayForward();
        triggerBackBtn.AlphaPlayForward();
        triggerVRBtn.AlphaPlayForward();
        triggerShare.AlphaPlayForward();
        triggerEnterFangJianPortrait.AlphaPlayBackward();

        appBridege.Unity2App("unityLandscape");
        Debug.Log("unityLandscape");
        GlobalDebug.Addline("unityLandscape");
    }

    //半屏
    public void Portrait()
    {
        leftWarning.AlphaPlayForward();
        rightWarning.AlphaPlayBackward();

        Screen.orientation = ScreenOrientation.Portrait;

        defaultGUIRoot.AlphaPlayForward();
        mainBtnGroup.AlphaPlayBackward();
        triggerHXList.AlphaPlayBackward();
        triggerXFGroup.AlphaPlayBackward();
        triggerBackBtn.AlphaPlayBackward();
        triggerVRBtn.AlphaPlayBackward();
        triggerShare.AlphaPlayBackward();
        triggerEnterFangJianPortrait.AlphaPlayForward();


        appBridege.Unity2App("unityProtrait");
        Debug.Log("unityProtrait");
        GlobalDebug.Addline("unityProtrait");

    }

    public void OpenMusic()
    {
        FindBGMusic();

        if (audioCtrl != null)
        {
            audioCtrl.PlayMusic();
        }
    }

    public void CloseMusic()
    {
        FindBGMusic();

        if (audioCtrl != null)
        {
            audioCtrl.MuteMusic();
        }

    }

    void FindBGMusic()
    {
        if (audioCtrl == null)
        {
            GameObject g = GameObject.Find("BGMusic");
            if (g != null)
            {
                if (g.GetComponent<AudioCtrl>() != null)
                {
                    audioCtrl = g.GetComponent<AudioCtrl>();
                }
            }
        }

    }


}
