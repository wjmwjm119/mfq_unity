using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class P360GUI : MonoBehaviour
{
    public AppBridge appBridge;

    public CanveGroupFade exitLandscape;
    public CanveGroupFade exitPoint360;
    public CanveGroupFade backActionBtn;
    public BackAction backAction;
    public ImageButton mainGUIPoint360Btn;
    public AssetBundleManager assetBundleManager;
    public SceneInteractiveManger sceneInteractiveManger;
    public HuXingInfoLabel huXingInfoLabel;
    public ScrollMenu huXingScrollMenu;
    public HXGUI hxGUI;

    public string needLoadSceneName;
    public bool hasEnterPoint360;

    public void CreatePoint360HXScrollMenu(SceneInteractiveManger s)
    {
        string[] point360HXNameG = s.assetBundleManager.point360SceneNameGroup.ToArray();
        string[] displayNameG = new string[point360HXNameG.Length];

        for (int i = 0; i < point360HXNameG.Length; i++)
        {
            displayNameG[i] = point360HXNameG[i].Replace("_360","");
        }
        huXingScrollMenu.CreateItemGroup(displayNameG, point360HXNameG);
    }

    public void ChoosePoint360Scene(string inSceneName)
    {

        EnterPoint360();

        needLoadSceneName = inSceneName;
        inSceneName = inSceneName.Replace("_360", "");

        bool isHX = false;
        //如果inSceneName是户型，显示户型信息
        foreach (HuXingType h in hxGUI.hxSceneHuXingTypeFinal)
        {
            if (h.hxName == inSceneName)
            {
                huXingInfoLabel.DisplayHuXingInfoLabel(h.GetHuXingTypeInfo());
                isHX = true;
            }
        }

        if(!isHX)
        huXingInfoLabel.HiddenHuXingInfoLabel();

        assetBundleManager.LoopRemoveAddedScene(0);
        assetBundleManager.OnSceneRemoved.AddListener(LoadPoint360Scene);
    }

    public void LoadPoint360Scene()
    {
        assetBundleManager.LoadAddSingerScene(needLoadSceneName);
    }

    public void ReChooseHuXingType(string hxName)
    {
        huXingInfoLabel.HiddenHuXingInfoLabel();
    }

    public void EnterPoint360()
    {
        if (!hasEnterPoint360)
        {
            hasEnterPoint360 = true;
            backAction.RemoveAllBackEvent();

            exitLandscape.AlphaPlayBackward();
            exitPoint360.AlphaPlayForward();
            backActionBtn.AlphaPlayBackward();
        }

    }

    public void ExitPoint360()
    {
        if (hasEnterPoint360)
        {
            hasEnterPoint360 = false;
            backAction.RemoveAllBackEvent();

            exitPoint360.AlphaPlayBackward();
            exitLandscape.AlphaPlayForward();
            backActionBtn.AlphaPlayBackward();

            huXingInfoLabel.HiddenHuXingInfoLabel();

            assetBundleManager.LoopRemoveAddedScene(0);
            assetBundleManager.OnSceneRemoved.AddListener(assetBundleManager.LoopLoadSceneAssetBundleDefault);
            mainGUIPoint360Btn.SetBtnState(false, 3);
            huXingScrollMenu.GetComponent<CanveGroupFade>().AlphaPlayBackward();
        }
    }


}
