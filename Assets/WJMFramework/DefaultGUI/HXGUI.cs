using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HXGUI : MonoBehaviour
{
    public AppBridge appBridge;
    public XFGUI xfGUI;
    public ImagePlayer2 pmtImagePlayer;
    public SceneInteractiveManger sceneInteractiveManger;

    public Material hxfbMat;

    public HuXingInfoLabel huXingInfoLabel;

    public ImageButton enterHXBtn;
//  public ImageButton enterMYBtnInPortrait;
    public CanveGroupFade triggerEnterHX;
    public CanveGroupFade triggerShare;
    public CanveGroupFade triggerEnterFangJian;
    public CanveGroupFade triggerHuXingThumbnail;
    public CanveGroupFade triggerVR;
    public CanveGroupFade triggerFCZ;
    public CanveGroupFade triggerMusicBtn;

    public ScrollMenu huXingScrollMenu;
    public ScrollMenu huXingFloorScrollMenu;

    public Transform huXingCameraBG;

    public HuXingType currentSelectHuXingType;

    //所有的户型最终信息都会整合到这
    public HuXingType[] hxSceneHuXingTypeFinal;

    Ray ray;
    RaycastHit hit;
//  public Collider colliderMeshObject;

//  string hxNameChoose;
    SenceInteractiveInfo hxScene;
    SenceInteractiveInfo hxfbScene;
    string hxfbCameraArgs;

    public void EnterAR_HX(SenceInteractiveInfo s)
    {

        GlobalDebug.Addline("EnterAR_HX");

        hxScene = s;

        for (int i = 0; i < hxSceneHuXingTypeFinal.Length; i++)
        {
            if (hxSceneHuXingTypeFinal[i].hxName == s.huXingType.hxName)
            {
                currentSelectHuXingType = hxSceneHuXingTypeFinal[i];
            }
        }

        triggerEnterHX.AlphaPlayBackward();
        triggerEnterFangJian.AlphaPlayBackward();
//      triggerShare.AlphaPlayBackward();
        triggerVR.AlphaPlayBackward();
        triggerHuXingThumbnail.AlphaPlayBackward();

        huXingInfoLabel.DisplayHuXingInfoLabel(currentSelectHuXingType.GetHuXingTypeInfo());
        pmtImagePlayer.netTexture2DGroup = currentSelectHuXingType.netTexture2DGroup;


        DisplayHuXingFloorScrollMenu();

    }

    public void ExitAR_HX()
    {
        GlobalDebug.Addline("ExitAR_HX");
        if (hxScene != null)
            hxScene.huXingType.DisplayAllFloorMesh();

        HiddenHuXingFloorScrollMenu();
        huXingInfoLabel.HiddenHuXingInfoLabel();
    }


    /// <summary>
    /// 竖屏进入户型,只有鸟瞰,没有漫游
    /// </summary>
    /// <param name="inName"></param>
    public void OnlyEnterHXNK(string inName)
    {

        GlobalDebug.Addline("Portrait EnterHXNK");

        foreach (SenceInteractiveInfo s in sceneInteractiveManger.senceInteractiveInfoGroup)
        {
            if (inName == s.huXingType.hxName)
            {
                if (s.huXingType.hxMeshRoot != null)
                {

                    appBridge.Unity2App("unityOpenRoomType", currentSelectHuXingType.huXingID);
                    Debug.Log("unityOpenRoomType:" + currentSelectHuXingType.huXingID);
                    GlobalDebug.Addline("unityOpenRoomType:" + currentSelectHuXingType.huXingID);

                    hxScene = s;

                    for (int i = 0; i < hxSceneHuXingTypeFinal.Length; i++)
                    {
                        if (hxSceneHuXingTypeFinal[i].hxName == s.huXingType.hxName)
                        {
                            currentSelectHuXingType = hxSceneHuXingTypeFinal[i];
                        }
                    }

                    sceneInteractiveManger.ChangeInteractiveScene(hxScene, true);

                    //                  triggerVR.AlphaPlayBackward();
                    //                  triggerEnterFangJian.AlphaPlayBackward();
//                    triggerFCZ.transform.localPosition = new Vector3(0, 100, 0);
                    triggerFCZ.AlphaPlayForward();
//                  triggerEnterFangJian.transform.localPosition = new Vector3(-500, 0, 0);
                    triggerEnterHX.AlphaPlayBackward();
                    triggerHuXingThumbnail.AlphaPlayBackward();
                    xfGUI.triggerOutDoorThumbnail.AlphaPlayBackward();

                    huXingInfoLabel.DisplayHuXingInfoLabel(currentSelectHuXingType.GetHuXingTypeInfo());
                    pmtImagePlayer.netTexture2DGroup = currentSelectHuXingType.netTexture2DGroup;

                    DisplayHuXingFloorScrollMenu();

                    huXingCameraBG.transform.parent = hxScene.cameraUniversalCenter.cameras[0].transform;
                    huXingCameraBG.transform.localPosition = new Vector3(0, 0, 90);
                    huXingCameraBG.transform.rotation = new Quaternion();
                    huXingCameraBG.transform.localScale = new Vector3(250, 140, 1);

                    appBridge.Unity2App("unityOpenRoomTypeDone");
                    Debug.Log("unityOpenRoomTypeDone");
                    GlobalDebug.Addline("unityOpenRoomTypeDone");

                }
            }
        }
    }

    /// <summary>
    /// 退出竖屏户型鸟瞰
    /// </summary>
    public void ExitHXNK()
    {

        triggerEnterHX.AlphaPlayForward();
//      triggerEnterFangJian.transform.localPosition = new Vector3(0, 0, 0);
        //triggerFCZ.transform.localPosition = Vector3.zero;
        triggerFCZ.AlphaPlayBackward();

        huXingCameraBG.transform.parent = this.transform;
        huXingCameraBG.transform.localPosition = new Vector3(0, 0, 90);
        huXingCameraBG.transform.rotation = new Quaternion();
        huXingCameraBG.transform.localScale = new Vector3(0, 0, 1);

        if (hxScene != null && hxScene.huXingType.hxMeshRoot!=null)
        {
            hxScene.huXingType.hxMeshRoot.gameObject.SetActive(false);
        }

        HiddenHuXingFloorScrollMenu();

        if (sceneInteractiveManger.mainSenceInteractiveInfo != null)
        {
            sceneInteractiveManger.ChangeInteractiveScene(sceneInteractiveManger.mainSenceInteractiveInfo, true);
        }
        else
        {
            sceneInteractiveManger.globalCameraCenter.ChangeCamera2(sceneInteractiveManger.globalCameraCenter.cameras[0], 0);
            sceneInteractiveManger.currentActiveSenceInteractiveInfo = null;
        }


        huXingInfoLabel.HiddenHuXingInfoLabel();

        appBridge.Unity2App("unityBackRoomTypeDone");
        Debug.Log("unityBackRoomTypeDone");
        GlobalDebug.Addline("unityBackRoomTypeDone");

    }


    public void EnterHuXing()
    {



        appBridge.Unity2App("unityOpenRoomType", currentSelectHuXingType.huXingID);
        Debug.Log("unityOpenRoomType:" + currentSelectHuXingType.huXingID);
        GlobalDebug.Addline("unityOpenRoomType:" + currentSelectHuXingType.huXingID);

        sceneInteractiveManger.RenderSenceThumbnail(hxfbScene, hxfbScene.cameraUniversalCenter.currentCamera, hxfbCameraArgs);

        hxScene = sceneInteractiveManger.GetHuXingTypeInteractiveInfo(currentSelectHuXingType.hxName);

        hxScene.huXingType.hxMeshRoot.position = hxScene.huXingType.hxNKWorldPos;
        hxScene.huXingType.hxMeshRoot.eulerAngles = new Vector3(0, hxScene.huXingType.rotOffset, 0);
        hxScene.huXingType.hxMeshRoot.gameObject.SetActive(true);

        //选房操作初始化预设
        if (sceneInteractiveManger.mainSenceInteractiveInfo!=null&& sceneInteractiveManger.mainSenceInteractiveInfo.louPanManager!=null)
        {
             sceneInteractiveManger.mainSenceInteractiveInfo.louPanManager.GetSelectHuXinginstance(hxScene.huXingType.hxName);
             xfGUI.EnterHuXing();
        }

        triggerMusicBtn.AlphaPlayBackward();
        triggerFCZ.AlphaPlayForward();
        triggerShare.AlphaPlayForward();
        triggerHuXingThumbnail.AlphaPlayBackward();
        xfGUI.triggerOutDoorThumbnail.AlphaPlayForward();

        huXingCameraBG.transform.parent = hxScene.cameraUniversalCenter.cameras[0].transform;
        huXingCameraBG.transform.localPosition = new Vector3(0, 0, 90);
        huXingCameraBG.transform.rotation = new Quaternion();
        huXingCameraBG.transform.localScale = new Vector3(250, 140, 1);

        sceneInteractiveManger.ChangeInteractiveScene(hxScene, false);

        DisplayHuXingFloorScrollMenu();

        xfGUI.SetHXSceneAndHXFBScene(hxScene, hxfbScene);

        appBridge.Unity2App("unityOpenRoomTypeDone");
        Debug.Log("unityOpenRoomTypeDone");
        GlobalDebug.Addline("unityOpenRoomTypeDone");

    }

    public void ExitHuXing()
    {


        CameraUniversalCenter.isInMirrorHX = false;

        huXingCameraBG.transform.parent = this.transform;
        huXingCameraBG.transform.localPosition = new Vector3(0, 0, 90);
        huXingCameraBG.transform.rotation = new Quaternion();
        huXingCameraBG.transform.localScale = new Vector3(0, 0, 1);

            if (hxScene.sceneType != SenceInteractiveInfo.SceneType.大场景)
            hxScene.huXingType.DisplayAllFloorMesh();
      
            sceneInteractiveManger.RenderSenceThumbnail(hxScene, hxScene.cameraUniversalCenter.cameras[0]);
            hxScene.huXingType.hxMeshRoot.gameObject.SetActive(false);
            hxfbScene.meshRoot.gameObject.SetActive(true);

            sceneInteractiveManger.ChangeInteractiveScene(hxfbScene,true);
            

            //判断是否有Mesh
            if (hxScene.huXingType.hxMeshRoot != null)
            {
                triggerHuXingThumbnail.AlphaPlayForward();
                xfGUI.triggerOutDoorThumbnail.AlphaPlayBackward();
            }
            else
            {
                triggerHuXingThumbnail.AlphaPlayBackward();
            }

        triggerShare.AlphaPlayBackward();
        triggerFCZ.AlphaPlayBackward();
        triggerMusicBtn.AlphaPlayForward();

        HiddenHuXingFloorScrollMenu();

        DisplayHXFBBox(currentSelectHuXingType.hxName);

        appBridge.Unity2App("unityBackRoomTypeDone");
        Debug.Log("unityBackRoomTypeDone");
        GlobalDebug.Addline("unityBackRoomTypeDone");

    }

    void DisplayHuXingFloorScrollMenu()
    {
        if (currentSelectHuXingType.allFloor.Length > 1)
        {
            List<string> dName = new List<string>();
            dName.AddRange(currentSelectHuXingType.GetHuXingAllDisplayName());

            List<string> pName = new List<string>();
            pName.AddRange(currentSelectHuXingType.GetHuXingAllFloorName());
            dName.Reverse();
            pName.Reverse();

            huXingFloorScrollMenu.CreateItemGroup(dName.ToArray(), pName.ToArray());

            huXingFloorScrollMenu.GetComponent<CanveGroupFade>().AlphaPlayForward();
            huXingFloorScrollMenu.GetComponent<GameObjectTweenCtrl>().MoveAchorPos(new Vector2(-65, -1075), 0.0f);

        }
    }

    public void HiddenHuXingFloorScrollMenu()
    {
        if (currentSelectHuXingType.allFloor.Length > 1)
        {
            huXingFloorScrollMenu.GetComponent<CanveGroupFade>().AlphaPlayBackward();
            huXingFloorScrollMenu.GetComponent<GameObjectTweenCtrl>().MoveAchorPos(new Vector2(150, -1075), 0.0f);
        }
    }

    public void HuXingChooseFloor(string floorName)
    {
        currentSelectHuXingType.HuXingChooseFloor(hxScene.cameraUniversalCenter, floorName);
    }



    public void EnterHuXingFloor(bool inPortrait)
    {
        Debug.Log("EnterHuXingFloor");

        if(hxScene)
        triggerFCZ.AlphaPlayBackward();

        if (!inPortrait)
        {
            //得先执行OpenXFMenu,要判断是否镜像户型
            xfGUI.OpenXFMenu();
//            Debug.Log("2333");
            hxfbMat.SetColor("_Color", new Color(0, 0, 0, 0.0f));
        }
        else
        {
            if (hxScene!=null&& hxScene.websky != null)
            {
                hxScene.websky.gameObject.SetActive(true);
            }

//           appBridge.Unity2App("unityEnterMYInPortraitDone");

        }

        huXingFloorScrollMenu.SetNonStandFloorBtnVisblity(false);
        currentSelectHuXingType.EnterHuXingFloor(sceneInteractiveManger.currentActiveSenceInteractiveInfo.cameraUniversalCenter, currentSelectHuXingType.GetDefaultMYFloorName());

    }

    public void ExitHuXingFloor(bool inPortrait)
    {

        triggerFCZ.AlphaPlayForward();

        huXingFloorScrollMenu.SetNonStandFloorBtnVisblity(true);

        if (!inPortrait)
        {
            hxScene.huXingType.hxMeshRoot.position = hxScene.huXingType.hxNKWorldPos;
            hxScene.huXingType.hxMeshRoot.eulerAngles = new Vector3(0, hxScene.huXingType.rotOffset, 0);
            hxfbMat.SetColor("_Color", new Color(0, 0, 0, 0.8f));
            sceneInteractiveManger.RenderSenceThumbnail(hxfbScene, hxfbScene.cameraUniversalCenter.currentCamera, hxfbCameraArgs);
            xfGUI.CloseXFMenu();
        }
        else
        {
            if (hxScene != null && hxScene.websky != null)
            {
                hxScene.websky.gameObject.SetActive(false);
            }
        }


        currentSelectHuXingType.ExitHuXingFloor(hxScene.cameraUniversalCenter);



    }



    public void CreateHuXingScrollMenu(SceneInteractiveManger s)
    {
        hxfbScene = s.mainSenceInteractiveInfo;

        //先移除SetImageZoomInAndZoomOutBtnFalse这个方法,再添加.保证只添加了一次
        hxfbScene.touchCtrl.doubleClickEvent.RemoveListener(DoubleClickHXFBBox);
        hxfbScene.touchCtrl.doubleClickEvent.AddListener(DoubleClickHXFBBox);

        string[] hxNameG = new string[hxSceneHuXingTypeFinal.Length];
        string[] displayNameG = new string[hxSceneHuXingTypeFinal.Length];

        for (int i = 0; i < hxSceneHuXingTypeFinal.Length; i++)
        {
            if (hxSceneHuXingTypeFinal[i].displayName == "")
                hxSceneHuXingTypeFinal[i].displayName = hxSceneHuXingTypeFinal[i].hxName;

            hxNameG[i] = hxSceneHuXingTypeFinal[i].hxName;
            displayNameG[i] = hxSceneHuXingTypeFinal[i].displayName;
        }

        huXingScrollMenu.CreateItemGroup(displayNameG, hxNameG);
    }

    public void DoubleClickHXFBBox(PointerEventData p)
    {
        ray=hxfbScene.cameraUniversalCenter.currentCamera.GetComponent<Camera>().ScreenPointToRay(p.position);

        if (Physics.Raycast(ray, out hit, 800))
        {            
            foreach (HuXingType h in hxSceneHuXingTypeFinal)
            {
                if (h.hxName == hit.transform.gameObject.name)
                {
                    Debug.Log(hit.transform.gameObject.name+" HX Collider");
                    if(h.hxMeshRoot!=null&&enterHXBtn.buttonState==false)
                    enterHXBtn.SetBtnStateForRemote(true, 0);
                }
            }
            //Debug.DrawLine(ray.origin, hit.point);
        }

    }

    public void ChooseHuXingTypeInputText(InputField inputText)
    {
        huXingScrollMenu.ChooseScroolItemHXName(inputText.text);
    }

    public void ChooseHuXingType(string hxName)
    {

//        triggerShare.AlphaPlayBackward();

//        Debug.Log(hxfbScene);

        //显示户型信息，获取到当前选择的户型
        currentSelectHuXingType = new HuXingType();
        bool hasFind=false;

        foreach (HuXingType h in hxSceneHuXingTypeFinal)
        {
            if (h.hxName == hxName)
            {
                currentSelectHuXingType = h;
                hasFind = true;
            }
        }

        if (!hasFind)
        {
            string log = hxName + "户型未找到！";
            GlobalDebug.Addline(log);
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
            return;
        }

        huXingInfoLabel.DisplayHuXingInfoLabel(currentSelectHuXingType.GetHuXingTypeInfo());

        //判断是否有Mesh
        if (currentSelectHuXingType.hxMeshRoot != null)
        {
            triggerEnterHX.AlphaPlayForward();
            triggerHuXingThumbnail.AlphaPlayForward();
            RenderHuXingThumbnail(hxName);
        }
        else
        {
            triggerHuXingThumbnail.AlphaPlayBackward();
            triggerEnterHX.AlphaPlayBackward();
        }

        /*
                //判断是否有H5分享
                if (selectH.hasShare)
                {
                    triggerShare.AlphaPlayForward();
                }
                else
                {
                    triggerShare.AlphaPlayBackward();
                }
        */
        DisplayHXFBBox(hxName);

        pmtImagePlayer.netTexture2DGroup = currentSelectHuXingType.netTexture2DGroup;

    }

    void DisplayHXFBBox(string hxName)
    {
//        Debug.Log(hxfbScene);
        //显示户型分布块
        if (hxfbScene != null && hxfbScene.ProcessHXFBAction(hxName))
        {

            foreach (InteractiveAction a in hxfbScene.f3d_HXFBGroup)
            {
                if (a.needDisplayRoot != null)
                {
                    foreach (MeshRenderer m in a.needDisplayRoot.GetComponentsInChildren<MeshRenderer>())
                    {
                        if (m.material != null)
                        {
                            m.material = hxfbMat;
                        }
                    }
                }
            }

            hxfbMat.SetColor("_Color", new Color(0, 0, 0, 0.8f));
            hxfbCameraArgs = hxfbScene.cameraUniversalCenter.currentCamera.GetCameraStateJson();
        }
        else
        {
            string log = hxName + "的户型分布块未找到！";
            GlobalDebug.Addline(log);
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
            GlobalDebug.Addline(log);
        }

    }


    public void ReChooseHuXingType(string hxName)
    {
        huXingInfoLabel.HiddenHuXingInfoLabel();
        //为空
        hxfbScene.ProcessHXFBAction("");
    }

    void RenderHuXingThumbnail( string hxName)
    {

        SenceInteractiveInfo s= sceneInteractiveManger.GetHuXingTypeInteractiveInfo(hxName);
//      s.transform.localPosition = s.huXingType.hxMeshRoot.localPosition;
//      s.transform.localRotation = s.huXingType.hxMeshRoot.localRotation;
        s.transform.parent = s.huXingType.hxMeshRoot;
        s.transform.localPosition = Vector3.zero;
        s.transform.localRotation = new Quaternion();

       s.huXingType.hxMeshRoot.position = s.huXingType.hxNKWorldPos;
       s.huXingType.hxMeshRoot.eulerAngles = new Vector3(0, s.huXingType.rotOffset, 0);

       sceneInteractiveManger.RenderSenceThumbnail(s, s.cameraUniversalCenter.cameras[0]);

    }

    public void ShareRoom()
    {
        appBridge.Unity2App("unityShareRoom",currentSelectHuXingType.huXingID.ToString());
        Debug.Log("unityShareRoom");
        GlobalDebug.Addline("unityShareRoom");
    }

    public void DisplayFCZ()
    {
        if (hxScene != null)
        {
            for (int i = 0; i < hxScene.huXingType.allFloor.Length; i++)
            {
                if (hxScene.huXingType.allFloor[i].fczMesh != null)
                {
                    hxScene.huXingType.allFloor[i].fczMesh.gameObject.SetActive(true);
                }
            }
        }
    }

    public void HiddenFCZ()
    {
        if (hxScene != null)
        {
            for (int i = 0; i < hxScene.huXingType.allFloor.Length; i++)
            {
                if (hxScene.huXingType.allFloor[i].fczMesh != null)
                {
                    hxScene.huXingType.allFloor[i].fczMesh.gameObject.SetActive(false);
                }
            }
        }
    }

}
