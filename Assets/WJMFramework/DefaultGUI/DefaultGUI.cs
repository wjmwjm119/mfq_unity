using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultGUI : MonoBehaviour
{
    public AppBridge appBridge;
    public SceneInteractiveManger sceneInteractiveManger;
    public CanveGroupFade defaultGUIRoot;
    public CanveGroupFade mainBtnGroup;
    public ImagePlayer2 quweiImagePlayer;
    public CanveGroupFade triggerExit;
    public CanveGroupFade triggerHXList;
    public CanveGroupFade triggerXFGroup;
    public CanveGroupFade triggerBackBtn;
    public CanveGroupFade triggerVRBtn;
    public CanveGroupFade triggerShare;
    public CanveGroupFade triggerMusic;
    public CanveGroupFade triggerEnterFangJianPortrait;
    public CanveGroupFade point360PointButton_Trigger;
    public CanveGroupFade triggerCancelBtn;
    public ImageButton musicBtn;

    public CanveGroupFade leftWarning;
    public CanveGroupFade rightWarning;

    bool oneTime = false;

    public static bool isPortraitUI;
    //    public static bool lastIsPortraitUI;
    bool isChangeOrientation;

    AudioCtrl audioCtrl;

    [HideInInspector]
    public  string currentMusicState = "1";

    void Start()
    {
        if (!oneTime)
        {
            oneTime = true;
            isPortraitUI = true;
//         lastIsPortraitUI = false;
        }
//        DisplayDefaultGUI();
    }

    /*
        public void DisplayDefaultGUI()
        {
            Debug.Log("DisplayDefaultGUI");

            if (lastIsLandscape != isLandscape)
            {
                lastIsLandscape = isLandscape;

                if (isLandscape)
                {
                    Landscape(currentMusicState);
                }
                else
                {
                    SetPortraitGUIState();
                }
            }
        }
    */


/*
    public  void MusicButton(string musicState)
    {
        currentMusicState = musicState;
        if (musicState == "0" && !musicBtn.buttonState)
        {
            musicBtn.buttonState = true;
            musicBtn.GetComponent<Image>().sprite = musicBtn.downSprite;
        }
        else if (musicState == "1" && musicBtn.buttonState)
        {
            musicBtn.buttonState = false;
            musicBtn.GetComponent<Image>().sprite = musicBtn.normalSprite;
        }
        else if (musicState == "2")
        {
            triggerMusic.AlphaPlayBackward();
        }
    }
*/
    
    public void ChangeOrientation(bool isPortrait)
    {
        if (isChangeOrientation)
        {
            return;
        }
        isChangeOrientation = true;

        if (isPortrait)
        {
            Screen.orientation = ScreenOrientation.Portrait;
            StartCoroutine(IEWaitChangeOrientationDone(ScreenOrientation.Portrait));
        }
        else
        {
            Screen.orientation = ScreenOrientation.LandscapeLeft;
            StartCoroutine(IEWaitChangeOrientationDone(ScreenOrientation.LandscapeLeft));
        }
    }

    IEnumerator IEWaitChangeOrientationDone(ScreenOrientation toOrientation)
    {
        yield return new WaitForSeconds(0.5f);

        if (toOrientation == ScreenOrientation.Portrait)
        {
            appBridge.Unity2App("unityChangeOrientationDone", "0");
            Debug.Log("unityChangeOrientationDone Portrait");
            GlobalDebug.Addline("unityChangeOrientationDone Portrait");
        }
        else if (toOrientation == ScreenOrientation.LandscapeLeft)
        {
            appBridge.Unity2App("unityChangeOrientationDone", "1");
            Debug.Log("unityChangeOrientationDone Landscape");
            GlobalDebug.Addline("unityChangeOrientationDone Landscape");
        }
        isChangeOrientation = false;
    }

    public void DisplayUI(string state)
    {
        switch (state)
        {
            case "0":
                defaultGUIRoot.AlphaPlayBackward();
                break;
            case "1":
                defaultGUIRoot.AlphaPlayForward();
                isPortraitUI = false;

//                triggerMusic.AlphaPlayForward();
                leftWarning.AlphaPlayBackward();
                rightWarning.AlphaPlayForward();
                mainBtnGroup.AlphaPlayForward();
                triggerHXList.AlphaPlayForward();
                triggerXFGroup.AlphaPlayForward();
                triggerBackBtn.AlphaPlayForward();
                triggerExit.AlphaPlayForward();
                triggerVRBtn.AlphaPlayForward();
                triggerShare.AlphaPlayForward();
                triggerEnterFangJianPortrait.AlphaPlayBackward();          
                //设置卡通角色
                CartoonPlayer.hasInit = true;
                sceneInteractiveManger.PlayCartoonAni();

                //在线讲盘设置,在线讲盘
                if (appBridge.appProjectInfo.sceneLoadMode == "9")
                {
                    triggerExit.AlphaPlayBackward();
                    triggerVRBtn.AlphaPlayBackward();
                    triggerShare.AlphaPlayBackward();
                    AppBridge.isInRemoteState = true;
                }

                break;
            case "2":
                defaultGUIRoot.AlphaPlayForward();
                isPortraitUI = true;

                leftWarning.AlphaPlayForward();
                rightWarning.AlphaPlayBackward();
                mainBtnGroup.AlphaPlayBackward();
                triggerHXList.AlphaPlayBackward();
                triggerXFGroup.AlphaPlayBackward();
                triggerBackBtn.AlphaPlayBackward();
                triggerExit.AlphaPlayBackward();
                triggerVRBtn.AlphaPlayBackward();
                triggerShare.AlphaPlayBackward();
                triggerEnterFangJianPortrait.AlphaPlayForward();
//                triggerMusic.AlphaPlayBackward();

                //设置卡通角色
                CartoonPlayer.hasInit = false;
                sceneInteractiveManger.CloseCartoonAni();

                break;
        }
//     appBridege.Unity2App("DisplayUI");
        Debug.Log("DisplayUI "+state);
        GlobalDebug.Addline("DisplayUI "+state);

    }

    public void OpenMusic()
    {
        appBridge.Unity2App("unitySetMusic","1");
    }

    public void CloseMusic()
    {
        appBridge.Unity2App("unitySetMusic", "0");
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

    public void ToPanorama()
    {
        appBridge.Unity2App("unityToPanorama", appBridge.serverProjectInfo.projectRootInfo.data.panoramaSwitch.link);
    }
}
