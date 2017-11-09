


using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class GlobalManager : MonoBehaviour
{

    public Material hxfbMat;
    public Material selectMat;
    public Transform selectMeshProxy;
    public ZBZ zbz;

    public CanveGroupFade defaultGUIRoot;
//  public CanveGroupFade mainGUI;
//  public CanveGroupFade HuXingScrollMenu;

    public HuXingInfoLabel huXingInfoLabel;

    public CanveGroupFade triggerEnterHX;
    public CanveGroupFade triggerShare;
    public CanveGroupFade triggerEnterFangJian;
    public CanveGroupFade triggerHuXingThumbnail;
    public CanveGroupFade triggerOutDoorThumbnail;

    public ScrollMenu huXingScrollMenu;
    public ScrollMenu huXingFloorScrollMenu;

    public Transform huXingCameraBG;

    public RenderTexture thumbnail;

    public CanveGroupFade xfCanveGroupFade;
    public Text buildTextLabel;
    public Text unitTextLabel;
    public Text fangJianTextLabel;

    public ImageButton btnChooseBuild;
    public ImageButton btnChooseUnit;
    public Transform btnChooseBuildBase;
    public Transform btnChooseUnitBase;

    public RectTransform buildScrollMenuBase;
    public RectTransform unitScrollMenuBase;
    public RectTransform hxInstanceScrollMenuBase;

    public ScrollMenu buildScrollMenu;
    public ScrollMenu unitScrollMenu;
    public ScrollMenu hxInstanceScrollMenu;
    
    public HuXingType[] allHuXingTypeFromWeb;
    public HuXingType currentSelectHuXingType;

    public List<SenceInteractiveInfo> senceInteractiveInfoGroup;
    public SenceInteractiveInfo currentSelectSenceInteractiveInfo;

    bool oneTime = false;
    LouPanManager louPanManager;

    string hxfbCameraArgs;

	void Start ()
    {
        if (!oneTime )
        {
            oneTime = true;
            DontDestroyOnLoad(this);
        }
        DisplayDefaultGUI();

        //选房选中用的材质球及代理模型.设置代理模型的层到31,毛坯相机将看不到31层的物体
        selectMat.SetColor("_Color", new Color(0, 0, 0, 0.0f));
        selectMeshProxy.gameObject.layer = 30;

    }

    //简介,配套,交通
    public void MainBtnAction(int toInt)
    {
        if (senceInteractiveInfoGroup != null && senceInteractiveInfoGroup[0] != null && senceInteractiveInfoGroup[0].sceneType == SenceInteractiveInfo.SceneType.大场景)
        {
            senceInteractiveInfoGroup[0].ProcessMainBtnAction(toInt);
        }
    }

    public string SelectHuXingType(string hxName)
    {
        foreach (HuXingType h in allHuXingTypeFromWeb)
        {
            if (h.hxName == hxName)
            {
                currentSelectHuXingType = h;
                return h.GetHuXingTypeInfo();
            }
        }
        return "";
    }

    public void ChooseHuXingType(string hxName)
    {

        //显示户型信息，获取到当前选择的户型

        string info = SelectHuXingType(hxName);

        huXingInfoLabel.DisplayHuXingInfoLabel(info);

        //判断是否有Mesh
        if (currentSelectHuXingType.hxMeshRoot != null)
        {
            triggerEnterHX.AlphaPlayForward();
            triggerHuXingThumbnail.AlphaPlayForward();
            LoadHuXingRenderThumbnail(thumbnail, hxName);
        }
        else
        {
            triggerHuXingThumbnail.AlphaPlayBackward();
            triggerEnterHX.AlphaPlayBackward();
        }

/*
        //判断是否有H5分享
        if (currentSelectHuXingType.hasShare)
        {
            triggerShare.AlphaPlayForward();
        }
        else
        {
            triggerShare.AlphaPlayBackward();
        }
*/        
        //显示户型分布块
        if (senceInteractiveInfoGroup[0].ProcessHXFBAction(hxName))
        {
            hxfbMat.SetColor("_Color", new Color(0, 0, 0, 0.8f));
            hxfbCameraArgs = senceInteractiveInfoGroup[0].cameraUniversalCenter.currentCamera.GetCameraStateJson();
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
        senceInteractiveInfoGroup[0].ProcessHXFBAction("");
    }

    void DisplayDefaultGUI()
    {
        defaultGUIRoot.AlphaPlayForward();
//        mainGUI.AlphaPlayForward();
    }

    public void AddSenceInteractiveInfo(SenceInteractiveInfo s)
    {
        if (senceInteractiveInfoGroup == null)
        {
            senceInteractiveInfoGroup = new List<SenceInteractiveInfo>();
        }

        if (!senceInteractiveInfoGroup.Contains(s))
        {
            if (s.sceneType == SenceInteractiveInfo.SceneType.大场景)
            {
                currentSelectSenceInteractiveInfo = s;
                zbz.cameraUniversalCenter = s.cameraUniversalCenter;

                //设置大场景相机的layer
                foreach (CameraUniversal c in s.cameraUniversalCenter.cameras)
                {
                    //0101 1111 1111 1111 1111 1111 1111 1111
                    //屏蔽29层
                    c.GetComponent<Camera>().cullingMask &= 0x5FFFFFFF;
                }

            }
            else
            {
                //从复制单个场景中的info到allHuXingTypeFromWeb
                foreach (HuXingType h in allHuXingTypeFromWeb)
                {
                    if (s.huXingType.hxName == h.hxName)
                    {

                        if (h.displayName == "")
                        {
                            if (s.huXingType.displayName == "")
                                s.huXingType.displayName = s.huXingType.hxName;

                            h.displayName = s.huXingType.displayName;
                        }
                            
                        if (h.leiXing == "")
                            h.leiXing=s.huXingType.leiXing;
                        if (h.introduction == "")
                            h.introduction=s.huXingType.introduction;
                        h.PMT=s.huXingType.PMT;
                        h.proxyRoot=s.huXingType.proxyRoot;
                        h.hxMeshRoot=s.huXingType.hxMeshRoot;
                        h.rotOffset=s.huXingType.rotOffset;
                        h.nkCameraPosAndXYZcount=s.huXingType.nkCameraPosAndXYZcount;
                        h.allFloor=s.huXingType.allFloor;
                    }
                }

            }

                HiddenAddScene(s);

                senceInteractiveInfoGroup.Add(s);          
        }
    }

    public void HiddenAddScene(SenceInteractiveInfo s)
    {

        //隐藏模型，隐藏ui，隐藏event，关闭默认相机
        if (s.sceneType != SenceInteractiveInfo.SceneType.大场景)
        {

            foreach (Transform t in s.huXingType.hxMeshRoot.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = 29;
            }

            s.huXingType.hxMeshRoot.gameObject.SetActive(false);

            foreach (Canvas c in s.GetComponentsInChildren<Canvas>(true))
            {
                c.gameObject.SetActive(false);
            }

            foreach (EventSystem e in s.GetComponentsInChildren<EventSystem>(true))
            {
                e.gameObject.SetActive(false);
            }

            //设置毛坯相机的layer
            foreach (CameraUniversal c in s.cameraUniversalCenter.cameras)
            {
                //0011 1111 1111 1111 1111 1111 1111 1111
                //设置不渲染30层
                c.GetComponent<Camera>().cullingMask &= 0x3FFFFFFF;
            }
   
            s.cameraUniversalCenter.currentCamera.DisableCamera();

        }

    }

    public void CreateHuXingScrollMenu()
    {
        string[] hxNameG = new string[allHuXingTypeFromWeb.Length];
        string[] displayNameG=new string[allHuXingTypeFromWeb.Length];

        for (int i=0;i<allHuXingTypeFromWeb.Length;i++)
        {
            if (allHuXingTypeFromWeb[i].displayName == "")
                allHuXingTypeFromWeb[i].displayName = allHuXingTypeFromWeb[i].hxName;

            hxNameG[i] = allHuXingTypeFromWeb[i].hxName;
            displayNameG[i] = allHuXingTypeFromWeb[i].displayName;
        }

        huXingScrollMenu.CreateItemGroup(displayNameG, hxNameG);
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

    void LoadHuXingRenderThumbnail(RenderTexture renderTex, string hxName)
    {

        foreach (SenceInteractiveInfo s in senceInteractiveInfoGroup)
        {
            if (s.huXingType.hxName == currentSelectHuXingType.hxName)
            {
                if (s != null && senceInteractiveInfoGroup[0] != null)
                {
//                  Debug.Log("444");

                    currentSelectSenceInteractiveInfo = s;

                    s.cameraUniversalCenter.currentCamera.GetComponent<Camera>().targetTexture = renderTex;
                    s.cameraUniversalCenter.currentCamera.EnableCamera();

                    currentSelectSenceInteractiveInfo.transform.parent = s.huXingType.hxMeshRoot;
                    currentSelectSenceInteractiveInfo.transform.localPosition = Vector3.zero;
                    currentSelectSenceInteractiveInfo.transform.localRotation = new Quaternion();

                    s.huXingType.hxMeshRoot.position = s.huXingType. hxNKWorldPos;
                    s.huXingType.hxMeshRoot.eulerAngles = new Vector3(0, s.huXingType.rotOffset, 0);
                    s.huXingType.hxMeshRoot.gameObject.SetActive(true);

                    s.cameraUniversalCenter.currentCamera.GetComponent<Camera>().Render();
                    s.cameraUniversalCenter.currentCamera.GetComponent<Camera>().targetTexture = null;
                    s.cameraUniversalCenter.currentCamera.DisableCamera();
                    s.huXingType.hxMeshRoot.gameObject.SetActive(false);

                }
            }
        }

    }

    public void EnterHuXing()
    {
        foreach (SenceInteractiveInfo s in senceInteractiveInfoGroup)
        {
            if (s.huXingType.hxName == currentSelectHuXingType.hxName)
            {
                if (s != null && senceInteractiveInfoGroup[0] != null)
                {
                    currentSelectSenceInteractiveInfo = s;
                    s.huXingType.hxMeshRoot.position = s.huXingType.hxNKWorldPos;
                    s.huXingType.hxMeshRoot.eulerAngles = new Vector3(0, s.huXingType.rotOffset, 0);
                    s.huXingType.hxMeshRoot.gameObject.SetActive(true);

                    //选房操作初始化预设
                    if (senceInteractiveInfoGroup[0].sceneType == SenceInteractiveInfo.SceneType.大场景 && senceInteractiveInfoGroup[0].louPanManager != null)
                    {
                        louPanManager = senceInteractiveInfoGroup[0].louPanManager;
                        louPanManager.GetSelectHuXinginstance(s.huXingType.hxName);

                        buildScrollMenu.GetComponentInParent<Transform>().DOLocalMoveY(-700, 0.3f);
                        unitScrollMenu.GetComponentInParent<Transform>().DOLocalMoveY(-700, 0.3f);
                        hxInstanceScrollMenu.GetComponentInParent<Transform>().DOLocalMoveY(-700, 0.3f);

                    }
                }
            }
        }

            triggerHuXingThumbnail.AlphaPlayBackward();
            triggerOutDoorThumbnail.AlphaPlayForward();

            huXingCameraBG.transform.parent = currentSelectSenceInteractiveInfo.cameraUniversalCenter.cameras[0].transform;
            huXingCameraBG.transform.localPosition = new Vector3(0, 0, 90);
            huXingCameraBG.transform.rotation = new Quaternion();
            huXingCameraBG.transform.localScale = new Vector3(250, 140, 1);

            senceInteractiveInfoGroup[0].cameraUniversalCenter.currentCamera.GetComponent<Camera>().targetTexture = thumbnail;
            senceInteractiveInfoGroup[0].cameraUniversalCenter.currentCamera.GetComponent<Camera>().Render();
            senceInteractiveInfoGroup[0].cameraUniversalCenter.currentCamera.GetComponent<Camera>().targetTexture = null;

            senceInteractiveInfoGroup[0].cameraUniversalCenter.ChangeCamera(currentSelectSenceInteractiveInfo.cameraUniversalCenter.cameras[0], 0.0f);

            zbz.cameraUniversalCenter = currentSelectSenceInteractiveInfo.cameraUniversalCenter;

            currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot.gameObject.SetActive(true);

            foreach (Canvas c in senceInteractiveInfoGroup[0].GetComponentsInChildren<Canvas>(true))
            {
                c.gameObject.SetActive(false);
            }

            foreach (Canvas c in currentSelectSenceInteractiveInfo.GetComponentsInChildren<Canvas>(true))
            {
                c.gameObject.SetActive(true);
            }

            DisplayHuXingFloorScrollMenu();

    }

    public void ExitHuXing()
    {
        if (currentSelectSenceInteractiveInfo != null && senceInteractiveInfoGroup[0] != null&& currentSelectSenceInteractiveInfo!= senceInteractiveInfoGroup[0])
        {

            huXingCameraBG.transform.parent =this.transform;
            huXingCameraBG.transform.localPosition = new Vector3(0, 0, 90);
            huXingCameraBG.transform.rotation = new Quaternion();
            huXingCameraBG.transform.localScale = new Vector3(0, 0, 1);

            if (currentSelectSenceInteractiveInfo.sceneType!=SenceInteractiveInfo.SceneType.大场景)
            currentSelectSenceInteractiveInfo.huXingType.DisplayAllFloorMesh();

            currentSelectSenceInteractiveInfo.cameraUniversalCenter.currentCamera.GetComponent<Camera>().targetTexture = thumbnail;
            currentSelectSenceInteractiveInfo.cameraUniversalCenter.currentCamera.GetComponent<Camera>().Render();
            currentSelectSenceInteractiveInfo.cameraUniversalCenter.currentCamera.GetComponent<Camera>().targetTexture = null;

            senceInteractiveInfoGroup[0].cameraUniversalCenter.ChangeCamera(senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0], 1);

            currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot.gameObject.SetActive(false);

            //判断是否有Mesh
            if (currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot != null)
            {
                triggerHuXingThumbnail.AlphaPlayForward();
                triggerOutDoorThumbnail.AlphaPlayBackward();
            }
            else
            {
                triggerHuXingThumbnail.AlphaPlayBackward();
            }

            foreach (Canvas c in currentSelectSenceInteractiveInfo.GetComponentsInChildren<Canvas>(true))
            {
                c.gameObject.SetActive(false);
            }

            foreach (Canvas c in senceInteractiveInfoGroup[0].GetComponentsInChildren<Canvas>(true))
            {
                c.gameObject.SetActive(true);
            }
            currentSelectSenceInteractiveInfo = senceInteractiveInfoGroup[0];
            zbz.cameraUniversalCenter = currentSelectSenceInteractiveInfo.cameraUniversalCenter;

        }
    }

    public void EnterHuXingFloor()
    {
        Debug.Log("EnterHuXingFloor");
        huXingFloorScrollMenu.SetNonStandFloorBtnVisblity(false);
        currentSelectHuXingType.EnterHuXingFloor(currentSelectSenceInteractiveInfo.cameraUniversalCenter, "1F");

        if (currentSelectSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.loft || currentSelectSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.平层 || currentSelectSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.独栋)
        {
            xfCanveGroupFade.AlphaPlayForward();
            hxfbMat.SetColor("_Color", new Color(0, 0, 0, 0.0f));
        }

        btnChooseBuild.SetBtnState(true, 0);
        if (buildScrollMenu.GetFirstScrollItem() != null)
        {
            buildScrollMenu.GetFirstScrollItem().GetComponent<ImageButton>().SetBtnState(true, 0);
        }

    }

    public void ExitHuXingFloor()
    {
        huXingFloorScrollMenu.SetNonStandFloorBtnVisblity(true);

        currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot.position = currentSelectSenceInteractiveInfo.huXingType.hxNKWorldPos;
        currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot.eulerAngles = new Vector3(0, currentSelectSenceInteractiveInfo.huXingType.rotOffset, 0);

        //点击默认第一个房间号
        //if (hxInstanceScrollMenu != null)
        //清空之前房号的按钮状态
        hxInstanceScrollMenu.CloseScrollMenu();

        currentSelectHuXingType.ExitHuXingFloor(currentSelectSenceInteractiveInfo.cameraUniversalCenter);

        xfCanveGroupFade.AlphaPlayBackward();
        CloseChooseBuildingMenu();
        CloseChooseUnitMenu();

        hxfbMat.SetColor("_Color", new Color(0, 0, 0, 0.8f));
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].GetComponent<Camera>().targetTexture = thumbnail;
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].EnableCamera();
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].SetCameraPositionAndXYZCount(hxfbCameraArgs,0);
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].GetComponent<Camera>().Render();
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].DisableCamera();
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].GetComponent<Camera>().targetTexture = null;

    }

    public void HuXingChooseFloor(string floorName)
    {
        currentSelectHuXingType.HuXingChooseFloor(currentSelectSenceInteractiveInfo.cameraUniversalCenter,floorName);
    }

    public void OpenChooseBuildingMenu()
    {
        if (louPanManager != null)
        {
            btnChooseUnit.CleanState();

            int[] louHaoIntGorup= louPanManager.GetDistinctLouHao(louPanManager.selectHXInstance);
            string[] louHaoStringGorup = new string[louHaoIntGorup.Length];
            string[] louHaoStringGorupDisplay = new string[louHaoIntGorup.Length];

            for (int i = 0; i < louHaoStringGorup.Length; i++        )
            {
                louHaoStringGorup[i] = louHaoIntGorup[i].ToString();
                louHaoStringGorupDisplay[i] = louHaoIntGorup[i].ToString() +" #";
            }
       
            btnChooseUnitBase.transform.DOLocalMoveY(-700, 0.3f);
            buildScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);
            hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, -700), 0.3f);

            buildScrollMenu.CreateItemGroup(louHaoStringGorupDisplay, louHaoStringGorup);
         }
    }

    public void CloseChooseBuildingMenu()
    {
        if (louPanManager != null)
        {
            btnChooseUnitBase.transform.DOLocalMoveY(0,0.3f);
            buildScrollMenuBase.DOAnchorPos(new Vector2(0, -700), 0.3f);
            hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);
        }
    }

    public void ChooseBuildingNo(string louID)
    {
//      Debug.Log("dddddddddd");
        SetBuildingLayer(buildTextLabel.text, 0);
        buildTextLabel.text = louID;
        SetBuildingLayer(louID,30);
        btnChooseBuild.CleanState();

        btnChooseUnit.SetBtnState(true, 0);
        if (unitScrollMenu.GetFirstScrollItem() != null)
        {
            unitScrollMenu.GetFirstScrollItem().GetComponent<ImageButton>().SetBtnState(true, 0);
        }

    }

    void SetBuildingLayer(string louID,int layer)
    {
        int lastBuildID = int.Parse(louID);

        foreach (Building b in louPanManager.allBuilding)
        {
            if (b.louHaoNo == lastBuildID)
            {
                foreach (Transform t in b.modelBuildRoot.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer=layer;
                }
            }
        }
    }

    public void OpenChooseUnitMenu()
    {

        if (louPanManager != null)
        {
            int[] unitIntGorup = louPanManager.GetDistinctUnit(louPanManager.selectHXInstance,int.Parse(buildTextLabel.text));
            string[] unitStringGorup = new string[unitIntGorup.Length];
            string[] unitStringGorupDisplay = new string[unitIntGorup.Length];

            for (int i = 0; i < unitIntGorup.Length; i++)
            {
                unitStringGorup[i] = unitIntGorup[i].ToString();
                unitStringGorupDisplay[i] = "unit "+unitIntGorup[i].ToString();
            }

            btnChooseUnitBase.transform.DOLocalMoveY(0, 0.3f);
            unitScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);
            hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, -700), 0.3f);

            unitScrollMenu.CreateItemGroup(unitStringGorupDisplay, unitStringGorup);

        }

    }

    public void CloseChooseUnitMenu()
    {
        if (louPanManager != null)
        {
            btnChooseUnitBase.transform.DOLocalMoveY(0, 0.3f);
            unitScrollMenuBase.DOAnchorPos(new Vector2(0, -700), 0.3f);
            hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);
        }
    }

    public void ChooseUnit(string unitID)
    {
        unitTextLabel.text = unitID;
        btnChooseUnit.CleanState();
        CreateHXInstanceScrollMenu();

     }

    void CreateHXInstanceScrollMenu( )
    {

        if (louPanManager != null)
        {
            string[] fangJianStringGorup = louPanManager.GetFangJianHao(louPanManager.selectHXInstance, int.Parse(buildTextLabel.text), int.Parse(unitTextLabel.text));
            string[] fangJianStringGorupDisplay = new string[fangJianStringGorup.Length];

            for (int i = 0; i < fangJianStringGorup.Length; i++)
            {
                fangJianStringGorupDisplay[i] = "·" + fangJianStringGorup[i].ToString() + "·";
            }

            hxInstanceScrollMenu.scrollRect.content.position = new Vector3(hxInstanceScrollMenu.scrollRect.content.position.x, 0, hxInstanceScrollMenu.scrollRect.content.position.z);

            hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);

            hxInstanceScrollMenu.CreateItemGroup(fangJianStringGorupDisplay, fangJianStringGorup);

            if (hxInstanceScrollMenu.GetFirstScrollItem() != null)
            {
                hxInstanceScrollMenu.GetFirstScrollItem().GetComponent<ImageButton>().SetBtnState(true, 0);
            }
        }
    }

    public void ChooseFangJian(string fangJianInfo)
    {
      HuXingInstance[] selectHuXingInstance=louPanManager.SelectSingerInstance(louPanManager.selectHXInstance, int.Parse(buildTextLabel.text), int.Parse(unitTextLabel.text), fangJianInfo);
//    Debug.Log(selectHuXingInstance.Length);

      if (selectHuXingInstance.Length > 0)
      {
         RenderXFThumbnail(selectHuXingInstance[0]);
      }
    }

    void RenderXFThumbnail(HuXingInstance h)
    {

        selectMat.SetColor("_Color", new Color(0, 0, 0, 0.8f));

        selectMeshProxy.position = h.position;
        selectMeshProxy.eulerAngles = h.eulerAngles;
        selectMeshProxy.localScale = h.scale;

        currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot.transform.position = h.position;
        currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot.transform.eulerAngles = h.eulerAngles;
//      currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot.transform.localScale = h.scale;

        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].GetComponent<Camera>().targetTexture = thumbnail;
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].EnableCamera();
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].SetCameraPositionAndXYZCountAllArgs(h.position.x.ToString(), h.position.y.ToString(), h.position.z.ToString(), "25", (h.eulerAngles.y+180).ToString(), "60", 0);
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].GetComponent<Camera>().Render();
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].DisableCamera();
        senceInteractiveInfoGroup[0].cameraUniversalCenter.cameras[0].GetComponent<Camera>().targetTexture = null;
    }



}
