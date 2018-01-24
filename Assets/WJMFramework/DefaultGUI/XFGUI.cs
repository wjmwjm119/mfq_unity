using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class XFGUI : MonoBehaviour
{


    public Material selectMat;
    public Transform selectMeshProxy;
    public SceneInteractiveManger sceneInteractiveManger;

    public CanveGroupFade xfCanveGroupFade;
    int currentLouID;
    int currentUnit;
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

    public ImageZoomInAndZoomOut imageZoomInAndZoomOut;
    public CanveGroupFade triggerOutDoorThumbnail;
    public RenderTexture thumbnail;
    public TouchCtrl thumbnailTouchCtrl;

    SenceInteractiveInfo hxScene;
    SenceInteractiveInfo hxfbScene;
    LouPanManager currentL;

    public bool isThumbnailZoomIn;

    public Dictionary<int, string> louHaoNameDictionary;
    public Dictionary<int, string> unitNameDictionary;


    void Start()
    {
        //选房选中用的材质球及代理模型.设置代理模型的层到30,毛坯相机将看不到30层的物体
        selectMat.SetColor("_Color", new Color(0, 0, 0, 0.0f));
        selectMeshProxy.gameObject.layer = 30;
        thumbnailTouchCtrl.raycastTarget = false;
    }

    public void SetHXSceneAndHXFBScene(SenceInteractiveInfo a, SenceInteractiveInfo b)
    {
        hxScene = a;
        hxfbScene = b;
    }


    public void OpenXFMenu()
    {

 //     hxScene = inHXScene;
 //     hxfbScene = inHXFBScene;
        currentL = hxfbScene.louPanManager;

        if (currentL!=null&&(sceneInteractiveManger.currentActiveSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.loft || sceneInteractiveManger.currentActiveSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.平层 || sceneInteractiveManger.currentActiveSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.独栋|| sceneInteractiveManger.currentActiveSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.叠拼|| sceneInteractiveManger.currentActiveSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.联排))
        {
            xfCanveGroupFade.AlphaPlayForward();

            //从默认主场景中获取楼号map映射

            if (currentL.louHaoNameDictionary!=null)
               louHaoNameDictionary = currentL.louHaoNameDictionary;

            if (currentL.unitNameDictionary!=null)
               unitNameDictionary = currentL.unitNameDictionary;
                
            OpenChooseBuildingMenu();

            btnChooseBuild.SetBtnState(true, 0);
            if (buildScrollMenu.GetFirstScrollItem() != null)
            {
                buildScrollMenu.GetFirstScrollItem().GetComponent<ImageButton>().SetBtnState(true, 0);
            }
        }

    }

    public void CloseXFMenu()
    {
        selectMat.SetColor("_Color", new Color(0, 0, 0, 0.0f));
        //点击默认第一个房间号
        //if (hxInstanceScrollMenu != null)
        //清空之前房号的按钮状态
        hxInstanceScrollMenu.CloseScrollMenu();

        xfCanveGroupFade.AlphaPlayBackward();
        CloseChooseBuildingMenu();
        CloseChooseUnitMenu();
        EndCtrlXFThumbnail();
    }


    public void OpenChooseBuildingMenu()
    {
        if (currentL != null)
        {
            btnChooseUnit.CleanState();

            int[] louHaoIntGorup = currentL.GetDistinctLouHao(currentL.selectHXInstance);
            string[] louHaoStringGorup = new string[louHaoIntGorup.Length];
            string[] louHaoStringGorupDisplay = new string[louHaoIntGorup.Length];

            for (int i = 0; i < louHaoStringGorup.Length; i++)
            {
                louHaoStringGorup[i] = louHaoIntGorup[i].ToString();
                louHaoStringGorupDisplay[i] = louHaoIntGorup[i].ToString() + " #";
                if (louHaoNameDictionary!=null && louHaoNameDictionary.ContainsKey(louHaoIntGorup[i]))
                {
                    louHaoStringGorupDisplay[i] = louHaoNameDictionary[louHaoIntGorup[i]];
                }

            }

            btnChooseUnitBase.transform.DOLocalMoveY(-700, 0.3f);
            buildScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);
            hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, -700), 0.3f);

            buildScrollMenu.CreateItemGroup(louHaoStringGorupDisplay, louHaoStringGorup);
        }
    }




    public void CloseChooseBuildingMenu()
    {
        btnChooseUnitBase.transform.DOLocalMoveY(0, 0.3f);
        buildScrollMenuBase.DOAnchorPos(new Vector2(0, -700), 0.3f);
        hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);
    }

    public void ChooseBuildingNo(string louID)
    {
        Debug.Log(louID);
        SetBuildingLayer(currentLouID, 0);
        buildTextLabel.text = louID;

        currentLouID = int.Parse(louID);

        if (louHaoNameDictionary!=null && louHaoNameDictionary.ContainsKey(currentLouID))
        {
            buildTextLabel.text = louHaoNameDictionary[currentLouID];
        }

        SetBuildingLayer(currentLouID, 30);
        btnChooseBuild.CleanState();

        btnChooseUnit.SetBtnState(true, 0);

        if (unitScrollMenu.GetFirstScrollItem() != null)
        {
            unitScrollMenu.GetFirstScrollItem().GetComponent<ImageButton>().SetBtnState(true, 0);
        }

    }

    void SetBuildingLayer(int inLouID, int layer)
    {
//        int lastBuildID = int.Parse(louID);

        foreach (Building b in currentL.allBuilding)
        {
            if (b.louHaoNo == inLouID)
            {
                foreach (Transform t in b.modelBuildRoot.GetComponentsInChildren<Transform>())
                {
                    t.gameObject.layer = layer;
                }
            }
        }
    }

    public void OpenChooseUnitMenu()
    {

        int[] unitIntGorup = currentL.GetDistinctUnit(currentL.selectHXInstance, currentLouID);
        string[] unitStringGorup = new string[unitIntGorup.Length];
        string[] unitStringGorupDisplay = new string[unitIntGorup.Length];

        for (int i = 0; i < unitIntGorup.Length; i++)
        {
            unitStringGorup[i] = unitIntGorup[i].ToString();
            unitStringGorupDisplay[i] = "unit " + unitIntGorup[i].ToString();

            if (unitNameDictionary != null && unitNameDictionary.ContainsKey(unitIntGorup[i]))
            {
                unitStringGorupDisplay[i] = unitNameDictionary[unitIntGorup[i]];
            }

        }

        btnChooseUnitBase.transform.DOLocalMoveY(0, 0.3f);
        unitScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);
        hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, -700), 0.3f);

        unitScrollMenu.CreateItemGroup(unitStringGorupDisplay, unitStringGorup);


    }

    public void CloseChooseUnitMenu()
    {
        btnChooseUnitBase.transform.DOLocalMoveY(0, 0.3f);
        unitScrollMenuBase.DOAnchorPos(new Vector2(0, -700), 0.3f);
        hxInstanceScrollMenuBase.DOAnchorPos(new Vector2(0, 0), 0.3f);
    }

    public void ChooseUnit(string unitID)
    {
        currentUnit = int.Parse(unitID);
        unitTextLabel.text = unitID;

        if (unitNameDictionary != null && unitNameDictionary.ContainsKey(currentUnit))
        {
            unitTextLabel.text = unitNameDictionary[currentUnit];
        }


        btnChooseUnit.CleanState();
        CreateHXInstanceScrollMenu();

    }

    void CreateHXInstanceScrollMenu()
    {
        string[] fangJianStringGorup = currentL.GetFangJianHao(currentL.selectHXInstance, currentLouID,currentUnit);
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

    public void ChooseFangJian(string fangJianInfo)
    {
        HuXingInstance[] selectHuXingInstance = currentL.SelectSingerInstance(currentL.selectHXInstance, currentLouID, currentUnit, fangJianInfo);
//        Debug.Log(222222222);

        if (selectHuXingInstance.Length > 0)
        {

            if (CameraUniversalCenter.isInMirrorHX != selectHuXingInstance[0].isMirrorHX)
            {
                CameraUniversalCenter.isInMirrorHX = selectHuXingInstance[0].isMirrorHX;
                GlobalDebug.Addline(selectHuXingInstance[0].isMirrorHX.ToString());
//              Debug.Log(CameraUniversalCenter.isInMirrorHX);

                hxScene.cameraUniversalCenter.currentCamera.ResetCameraStateToInitial();
            }

            //设置镜像户型
            if (selectHuXingInstance[0].isMirrorHX)
            {
                hxScene.meshRoot.localScale = new Vector3(-1, 1, 1);
                hxScene.transform.localScale = new Vector3(-1, 1, 1);
            }
            else
            {
                hxScene.meshRoot.localScale = new Vector3(1, 1, 1);
                hxScene.transform.localScale = new Vector3(1, 1, 1);
            }
            RenderXFThumbnail(selectHuXingInstance[0], isThumbnailZoomIn);


        }
    }

    public void ChooseFangJianAndRenderSub(SenceInteractiveInfo outDoor, SenceInteractiveInfo hx, string fangJianInfo)
    {
        HuXingInstance[] selectHuXingInstance = currentL.SelectSingerInstance(currentL.selectHXInstance,currentLouID, currentUnit, fangJianInfo);
        //    Debug.Log(selectHuXingInstance.Length);

        if (selectHuXingInstance.Length > 0)
        {
            RenderXFThumbnail(selectHuXingInstance[0], isThumbnailZoomIn);
        }

    }

    void RenderXFThumbnail(HuXingInstance h,bool isThumbnailZoomIn)
    {
        selectMat.SetColor("_Color", new Color(0, 0, 0, 0.8f));
        selectMeshProxy.position = h.position;
        selectMeshProxy.eulerAngles = h.eulerAngles;
        selectMeshProxy.localScale = h.scale;
    //    hxScene.huXingType.hxMeshRoot.transform.position = h.position;
        hxScene.huXingType.hxMeshRoot.transform.DOMove(h.position, 0.3f);
        hxScene.huXingType.hxMeshRoot.transform.eulerAngles =new Vector3( h.eulerAngles.x,h.eulerAngles.y+h.yRotOffset,h.eulerAngles.z);
        //currentSelectSenceInteractiveInfo.huXingType.hxMeshRoot.transform.localScale = h.scale;

        if (!isThumbnailZoomIn)
        {
            hxfbScene.cameraUniversalCenter.cameras[0].SetCameraPositionAndXYZCountAllArgs(h.position.x.ToString(), h.position.y.ToString(), h.position.z.ToString(), "25", (h.eulerAngles.y + 180).ToString(), "25", 0.0f);
            sceneInteractiveManger.RenderSenceThumbnail(sceneInteractiveManger.thumbnailOutdoor, hxfbScene, hxfbScene.cameraUniversalCenter.cameras[0]);
            selectMat.SetFloat("_alphaSin", 0.0f);
        }
        else
        {
            hxfbScene.cameraUniversalCenter.cameras[0].SetCameraPositionAndXYZCountAllArgs(h.position.x.ToString(), h.position.y.ToString(), h.position.z.ToString(), "25", (h.eulerAngles.y + 180).ToString(), "", 0);
            selectMat.SetFloat("_alphaSin", 0.8f);
        }




    }

    public void EnterHuXing()
    {
        buildScrollMenu.GetComponentInParent<Transform>().DOLocalMoveY(-700, 0.3f);
        unitScrollMenu.GetComponentInParent<Transform>().DOLocalMoveY(-700, 0.3f);
        hxInstanceScrollMenu.GetComponentInParent<Transform>().DOLocalMoveY(-700, 0.3f);
    }

    public void StartCtrlXFThumbnail()
    {
        //先移除SetImageZoomInAndZoomOutBtnFalse这个方法,再添加.保证只添加了一次
        hxScene.touchCtrl.touchDownEvent.RemoveListener(SetImageZoomInAndZoomOutBtnFalse);
        hxScene.touchCtrl.touchDownEvent.AddListener(SetImageZoomInAndZoomOutBtnFalse);

        if (!isThumbnailZoomIn)
        {
            isThumbnailZoomIn = true;

            hxfbScene.cameraUniversalCenter.currentCamera.GetComponent<Camera>().targetTexture = thumbnail;
            hxfbScene.cameraUniversalCenter.currentCamera.EnableCamera();
            //hxScene.touchCtrl.raycastTarget = false;
            thumbnailTouchCtrl.raycastTarget = true;
            thumbnailTouchCtrl.cameraCenter = hxfbScene.cameraUniversalCenter;
            imageZoomInAndZoomOut.ZoomIn();
        }
   
    }

    public void EndCtrlXFThumbnail()
    {


        if (isThumbnailZoomIn)
        {
            isThumbnailZoomIn = false;

            hxfbScene.cameraUniversalCenter.currentCamera.GetComponent<Camera>().targetTexture = null;
            hxfbScene.cameraUniversalCenter.currentCamera.DisableCamera();
          //hxScene.touchCtrl.raycastTarget = true;
            thumbnailTouchCtrl.raycastTarget = false;
            thumbnailTouchCtrl.cameraCenter = null;
            imageZoomInAndZoomOut.ZoomOut();
        }
    }

    public void SetImageZoomInAndZoomOutBtnFalse()
    {
        imageZoomInAndZoomOut.GetComponent<ImageButton>().CleanStateForRemote();  
    }


}
