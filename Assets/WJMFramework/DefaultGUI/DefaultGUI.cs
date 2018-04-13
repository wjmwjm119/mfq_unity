using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DefaultGUI : MonoBehaviour
{
    public AppBridge appBridege;
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
    public ImageButton musicBtn;

    public CanveGroupFade leftWarning;
    public CanveGroupFade rightWarning;

    bool oneTime = false;

    public static bool isLandscape;
    public static bool lastIsLandscape;

    AudioCtrl audioCtrl;

    [HideInInspector]
    public  string currentMusicState = "1";

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

    //全屏
    public void Landscape(string musicState,bool useOrientation)
    {
        isLandscape = true;
        CartoonPlayer.hasInit = true;
        sceneInteractiveManger.PlayCartoonAni();

        triggerMusic.AlphaPlayForward();
        leftWarning.AlphaPlayBackward();
        rightWarning.AlphaPlayForward();

        defaultGUIRoot.AlphaPlayForward();
        mainBtnGroup.AlphaPlayForward();
        triggerHXList.AlphaPlayForward();
        triggerXFGroup.AlphaPlayForward();
        triggerBackBtn.AlphaPlayForward();
        triggerExit.AlphaPlayForward();
        triggerVRBtn.AlphaPlayForward();
        triggerShare.AlphaPlayForward();
        triggerEnterFangJianPortrait.AlphaPlayBackward();

        currentMusicState = musicState;

        if(useOrientation)
        Screen.orientation = ScreenOrientation.LandscapeLeft;

        appBridege.Unity2App("unityLandscape");
        Debug.Log("unityLandscape");
        GlobalDebug.Addline("unityLandscape");

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

        if (appBridege.appProjectInfo.sceneLoadMode == "9")
        {
            triggerExit.AlphaPlayBackward();
            triggerVRBtn.AlphaPlayBackward();
            triggerShare.AlphaPlayBackward();
            AppBridge.isInRemoteState = true;
        }

            

    }


    //半屏
    public void Portrait(bool useOrientation = true)
    {
        isLandscape = false;
        CartoonPlayer.hasInit = false;
        sceneInteractiveManger.CloseCartoonAni();

        leftWarning.AlphaPlayForward();
        rightWarning.AlphaPlayBackward();
        defaultGUIRoot.AlphaPlayForward();
        mainBtnGroup.AlphaPlayBackward();
        triggerHXList.AlphaPlayBackward();
        triggerXFGroup.AlphaPlayBackward();
        triggerBackBtn.AlphaPlayBackward();
        triggerExit.AlphaPlayBackward();
        triggerVRBtn.AlphaPlayBackward();
        triggerShare.AlphaPlayBackward();
        triggerEnterFangJianPortrait.AlphaPlayForward();
        triggerMusic.AlphaPlayBackward();

        if(useOrientation)
        Screen.orientation = ScreenOrientation.Portrait;

        appBridege.Unity2App("unityProtrait");
        Debug.Log("unityProtrait");
        GlobalDebug.Addline("unityProtrait");

    }



    public void OpenMusic()
    {
        /*
        FindBGMusic();

        if (audioCtrl != null)
        {
            audioCtrl.PlayMusic();
        }
        */
        appBridege.Unity2App("unitySetMusic","1");
    }

    public void CloseMusic()
    {
        /*
        FindBGMusic();

        if (audioCtrl != null)
        {
            audioCtrl.MuteMusic();
        }
        */

        appBridege.Unity2App("unitySetMusic", "0");
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
        appBridege.Unity2App("unityToPanorama", appBridege.serverProjectInfo.projectRootInfo.data.panoramaSwitch.link);
    }
}
