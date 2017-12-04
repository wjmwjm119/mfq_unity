using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneInteractiveManger : MonoBehaviour
{
    public AppBridge appBridge;

//    public ARManager arManager;
    public AssetBundleManager assetBundleManager;
    public LoadingManager loadingManager;
    public RemoteManger remoteManger;
    public CameraUniversalCenter globalCameraCenter;
    public ZBZ zbz;
    public DefaultGUI defaultGUI;
    public HXGUI hxGUI;
    public ImageCache imageCache;

    public RenderTexture thumbnail;
    public SenceInteractiveInfo mainSenceInteractiveInfo;
    public SenceInteractiveInfo currentActiveSenceInteractiveInfo;
    public List<SenceInteractiveInfo> senceInteractiveInfoGroup;

    public bool finishLoadAssetBundle;
    bool lastFinishLoadAssetBundle;

    public enum LoadAtState
    {
        normal = 0,
        onlineTalk = 1,
        ar = 2
    }

//    public static LoadAtState loadAtState = LoadAtState.normal;

    public string addSceneName;

    int currentAddSceneID;

    void Start()
    {
        finishLoadAssetBundle = false;
        lastFinishLoadAssetBundle = false;
        SceneManager.sceneLoaded += GetAddSceneName;
    }

    //制作人员测试做好的场景
    public void StartLoadLocalScene()
    {
        GlobalDebug.Addline("StartLoadLocalScene");
        Debug.Log("StartLoadLocalScene");

        Debug.Log(SceneManager.sceneCountInBuildSettings);

        switch (appBridge.appProjectInfo.sceneLoadMode)
        {

            case "0":

                //加载主场景
                for (int i = 2; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    SceneManager.LoadScene(i, LoadSceneMode.Additive);       
                }
                StartCoroutine(LocalLoadSenceInteractiveIE());

                break;


            case "8":
                //不加载主场景,只加载户型,以供AR扫描
                break;

            case "9":

                for (int i = 2; i < SceneManager.sceneCountInBuildSettings; i++)
                {
                    SceneManager.LoadScene(i, LoadSceneMode.Additive);
                }

                StartCoroutine(LocalLoadSenceInteractiveIE(true));

                break;

        }
    }

    IEnumerator LocalLoadSenceInteractiveIE(bool isOnlineTalk = false)
    {
        yield return new WaitForSeconds(0.3f);
        ChangeInteractiveScene(senceInteractiveInfoGroup[0], true);
        if (isOnlineTalk)
            remoteManger.StartOnlineTalk();
        imageCache.LoopLoadAndCacheImageFromServer();
    }


    public void OnAllAssetBundleLoaded()
    {
        currentAddSceneID = 0;
        GlobalDebug.Addline("OnAllAssetBundleLoaded");
        Debug.Log("OnAllAssetBundleLoaded");

        switch (appBridge.appProjectInfo.sceneLoadMode)
        {

            case "0":
                //加载主场景
                Loading loadingScene = loadingManager.AddALoading(3);
                loadingScene.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[0], LoadSceneMode.Additive), "正在加载");
                loadingScene.OnLoadedEvent.AddListener(() => { StartCoroutine(LoadSenceInteractiveIE()); });
                break;

            case "1":
                //不加载主场景,只加载户型,跳过第一个主场景
//                defaultGUI.triggerMusic.AlphaPlayBackward();
                currentAddSceneID = 0;
                LoopAdditiveScene(true);
                //arManager.OpenARCamrea();
                break;

            case "8":
                //不加载主场景,只加载户型,以供AR扫描,跳过第一个主场景
                currentAddSceneID = 0;
                LoopAdditiveScene(true);
                //              arManager.OpenARCamrea();
                break;

            case "9":

                //加载主场景且打开在线讲
                Loading loadingScene2 = loadingManager.AddALoading(3);
                loadingScene2.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[0], LoadSceneMode.Additive), "正在加载");
                loadingScene2.OnLoadedEvent.AddListener(() => { StartCoroutine(LoadSenceInteractiveIE(true)); });

                break;

        }
    }


    IEnumerator LoadSenceInteractiveIE(bool isOnlineTalk = false)
    {
        yield return new WaitForSeconds(0.3f);
        ChangeInteractiveScene(senceInteractiveInfoGroup[0], true);
        if (isOnlineTalk)
            remoteManger.StartOnlineTalk();
        LoopAdditiveScene();

    }

    void LoopAdditiveScene(bool loadImageInEnd=true)
    {
        currentAddSceneID++;

        if (currentAddSceneID < assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath.Length)
        {
            Loading loadingScene = loadingManager.AddALoading(4);
            loadingScene.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[currentAddSceneID], LoadSceneMode.Additive), "正在加载");
            loadingScene.OnLoadedEvent.AddListener(() => { LoopAdditiveScene(); });
        }
        else if(currentAddSceneID == assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath.Length)
        {
            GlobalDebug.Addline("All Additive Scene Loaded!");
            Debug.Log("All Additive Scene Loaded!");

            if(loadImageInEnd)
            imageCache.LoopLoadAndCacheImageFromServer();

            appBridge.Unity2App("unityLoadDone");
            appBridge.Unity2App("unityReady");

            Debug.Log("unityLoadDone");
            GlobalDebug.Addline("unityLoadDone");
        }
    }

    /*
    public void AdditiveScene(int id)
    {
        if (id == 0)
        {
            Loading loadingScene = loadingManager.AddALoading(3);
            loadingScene.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[id], LoadSceneMode.Additive), "正在加载");
            loadingScene.OnLoadedEvent.AddListener(() => { StartCoroutine(LoadSenceInteractiveIE()); });
        }
        else
        {
            SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[id], LoadSceneMode.Additive);
        }
    }
    */


    //场景加载完时的回调,获取场景名加到InteractiveInfo;
    void GetAddSceneName(Scene s,LoadSceneMode l)
    {
        addSceneName = s.name;
    }

    //简介,配套,交通
    public void MainBtnAction(int toInt)
    {
        if (senceInteractiveInfoGroup != null && senceInteractiveInfoGroup[0] != null && senceInteractiveInfoGroup[0].sceneType == SenceInteractiveInfo.SceneType.大场景)
        {
            senceInteractiveInfoGroup[0].ProcessMainBtnAction(toInt);
        }
    }

    public SenceInteractiveInfo GetHuXingTypeInteractiveInfo(string hxName)
    {
        foreach (SenceInteractiveInfo s in senceInteractiveInfoGroup)
        {
            if (s.huXingType.hxName == hxName)
            {
                return s;
            }
        }

        return null;
    }

    public void AddSenceInteractiveInfo(SenceInteractiveInfo s)
    {

#if UNITY_EDITOR

        if (s.meshRoot != null)
        {
            RecoverMatShader(s.meshRoot);
        }
        
#endif


        //        Debug.Log(2222);
        s.sceneName = addSceneName;

        if (senceInteractiveInfoGroup == null)
        {
            senceInteractiveInfoGroup = new List<SenceInteractiveInfo>();
        }

        if (!senceInteractiveInfoGroup.Contains(s))
        {   
            foreach (CameraUniversal c in s.cameraUniversalCenter.cameras)
            {
                c.InitlCameraInRuntime();
            }

            if (s.sceneType == SenceInteractiveInfo.SceneType.大场景)
            {
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
                //将户型单场景中的info复制到allHuXingTypeFromFinal
                foreach (HuXingType hFinal in hxGUI.hxSceneHuXingTypeFinal)
                {
                    if (s.huXingType.hxName == hFinal.hxName)
                    {

                        if (hFinal.displayName == "")
                        {
                            if (s.huXingType.displayName == "")
                            {
                                s.huXingType.displayName = s.huXingType.hxName;
                            }
                                
                            hFinal.displayName = s.huXingType.displayName;
                        }

                        if (hFinal.leiXing == "")
                            hFinal.leiXing = s.huXingType.leiXing;
                        if (hFinal.introduction == "")
                            hFinal.introduction = s.huXingType.introduction;

                        hFinal.PMT = s.huXingType.PMT;
                        hFinal.proxyRoot = s.huXingType.proxyRoot;
                        hFinal.hxMeshRoot = s.huXingType.hxMeshRoot;
                        hFinal.rotOffset = s.huXingType.rotOffset;
                        hFinal.nkCameraPosAndXYZcount = s.huXingType.nkCameraPosAndXYZcount;
                        hFinal.defaultMYFloorName = s.huXingType.defaultMYFloorName;

/*
                        if (s.huXingType.allFloor.Length != hFinal.allFloor.Length)
                        {
                            GlobalDebug.Addline("服务器的户型楼层数与模型设置的楼层不一致,请检查楼层设置是否正确且统一");
                            Debug.LogError("服务器的户型楼层数与模型设置的楼层不一致,请检查楼层设置是否正确且统一");
                            return;
                        }
*/
                        for (int i = 0; i < s.huXingType.allFloor.Length; i++)
                        {
                            for (int j = 0; j < hFinal.allFloor.Length; j++)
                            {
                                if (s.huXingType.allFloor[i].floorName == hFinal.allFloor[j].floorName + "F")
                                {
                                    //将必要的网络数据复制至到单个户型的info中
                                    if (hFinal.allFloor[j].displayName != "")
                                    {
                                        s.huXingType.allFloor[i].displayName = hFinal.allFloor[j].displayName;
                                    }
                                    else
                                    {
                                        if (s.huXingType.allFloor[i].displayName == "")
                                        {
                                            s.huXingType.allFloor[i].displayName = s.huXingType.allFloor[i].floorName;
                                        }
                                    }

                                    s.huXingType.allFloor[i].pmtUrl = hFinal.allFloor[j].pmtUrl;
                                    s.huXingType.allFloor[i].pmtName = hFinal.allFloor[j].pmtName;
                                }
                            }

                            //默认关闭非承重墙
                            if (s.huXingType.allFloor[i].fczMesh != null)
                            {
                                s.huXingType.allFloor[i].fczMesh.gameObject.SetActive(false);
                            }

                        }

                        //把每楼层信息的复制到FinalHX中的楼层
                        hFinal.allFloor = s.huXingType.allFloor;
                    }
                }
            }

            if(s.huXingType.hxMeshRoot!=null)
            s.transform.position = s.huXingType.hxMeshRoot.position;

            HiddenScene(s);

            senceInteractiveInfoGroup.Add(s);
        }
    }

    public void RemoveSenceInteractiveInfo(SenceInteractiveInfo s)
    {
        if (senceInteractiveInfoGroup.Contains(s))
        {
            senceInteractiveInfoGroup.Remove(s);
        }
    }

    public void HiddenScene(SenceInteractiveInfo s)
    {
        //隐藏模型，隐藏ui，隐藏event，关闭默认相机
        if (s.sceneType != SenceInteractiveInfo.SceneType.大场景)
        {
            foreach (Transform t in s.huXingType.hxMeshRoot.GetComponentsInChildren<Transform>())
            {
                t.gameObject.layer = 29;
            }
            if(s.huXingType.hxMeshRoot!=null)
            s.huXingType.hxMeshRoot.gameObject.SetActive(false);

            //设置毛坯相机的layer
            foreach (CameraUniversal c in s.cameraUniversalCenter.cameras)
            {
                //0011 1111 1111 1111 1111 1111 1111 1111
                //设置不渲染30层
                c.GetComponent<Camera>().cullingMask &= 0x3FFFFFFF;
                //关闭所有相机
                c.DisableCamera();
            }



        }

        if (s.meshRoot != null)
            s.meshRoot.gameObject.SetActive(false);

            foreach (Canvas c in s.GetComponentsInChildren<Canvas>(true))
            {
                c.gameObject.SetActive(false);
            }

            foreach (EventSystem e in s.GetComponentsInChildren<EventSystem>(true))
            {
                e.gameObject.SetActive(false);
            }

//        s.cameraUniversalCenter.currentCamera.DisableCamera();

    }

    public void RenderSenceThumbnail(SenceInteractiveInfo needRenderScene,CameraUniversal cameraThumbnail, string cameraArgs="")
    {

        bool orginDisplayState = false;

        if (needRenderScene.sceneType == SenceInteractiveInfo.SceneType.大场景)
        {
            orginDisplayState = needRenderScene.meshRoot.gameObject.activeInHierarchy;
            needRenderScene.meshRoot.gameObject.SetActive(true);
        }
        else
        {
            if (needRenderScene.huXingType.hxMeshRoot != null)
            {
               orginDisplayState = needRenderScene.huXingType.hxMeshRoot.gameObject.activeInHierarchy;
               needRenderScene.huXingType.hxMeshRoot.gameObject.SetActive(true);
            }
        }

        cameraThumbnail.GetComponent<Camera>().targetTexture = thumbnail;
        cameraThumbnail.EnableCamera();
        if(cameraArgs!="")
        cameraThumbnail.SetCameraPositionAndXYZCount(cameraArgs, 0);
        cameraThumbnail.GetComponent<Camera>().Render();
        cameraThumbnail.DisableCamera();
        cameraThumbnail.GetComponent<Camera>().targetTexture = null;

        if (needRenderScene.sceneType == SenceInteractiveInfo.SceneType.大场景)
        {
            needRenderScene.meshRoot.gameObject.SetActive(orginDisplayState);
        }
        else
        {
            if (needRenderScene.huXingType.hxMeshRoot != null)
                needRenderScene.huXingType.hxMeshRoot.gameObject.SetActive(orginDisplayState);
        }

    }

    public void ChangeInteractiveScene(SenceInteractiveInfo toInteractiveScene,bool disableCurrentSceneMesh)
    {

        if (toInteractiveScene == currentActiveSenceInteractiveInfo)
        return;

        if (currentActiveSenceInteractiveInfo != null&&disableCurrentSceneMesh)
        {
            if (currentActiveSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.大场景)
            {
                if (currentActiveSenceInteractiveInfo.meshRoot != null)
                    currentActiveSenceInteractiveInfo.meshRoot.gameObject.SetActive(false);
            }
            else
            {
                if (currentActiveSenceInteractiveInfo.huXingType.hxMeshRoot != null)
                    currentActiveSenceInteractiveInfo.huXingType.hxMeshRoot.gameObject.SetActive(false);
            }
        }

        globalCameraCenter.ChangeCamera(toInteractiveScene.cameraUniversalCenter.cameras[0], 0.0f);


        if (toInteractiveScene.sceneType == SenceInteractiveInfo.SceneType.大场景)
        {
            if (toInteractiveScene.meshRoot != null)
                toInteractiveScene.meshRoot.gameObject.SetActive(true);
            mainSenceInteractiveInfo = toInteractiveScene;

//            defaultGUI.DisplayDefaultGUI();

        }
        else
        {
            if (toInteractiveScene.huXingType.hxMeshRoot != null)
                toInteractiveScene.huXingType.hxMeshRoot.gameObject.SetActive(true);
        }

        if (currentActiveSenceInteractiveInfo != null)
        {
            foreach (Canvas c in currentActiveSenceInteractiveInfo.GetComponentsInChildren<Canvas>(true))
            {
                c.gameObject.SetActive(false);
            }
        }

        foreach (Canvas c in toInteractiveScene.GetComponentsInChildren<Canvas>(true))
        {
            c.gameObject.SetActive(true);
        }

        currentActiveSenceInteractiveInfo = toInteractiveScene;
        globalCameraCenter.zbzOffset = toInteractiveScene.cameraUniversalCenter.zbzOffset;
        zbz.cameraUniversalCenter = toInteractiveScene.cameraUniversalCenter;

    }


    /// <summary>
    /// Editor下替换Shader使得场景在Editor下可以正常显示
    /// </summary>
    /// <param name="root"></param>
    static public void RecoverMatShader(Transform root)
    {
        MeshRenderer[] allChildMeshRenderer = root.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < allChildMeshRenderer.Length; i++)
        {
//            Material[] tempMatGroup = new Material[allChildMeshRenderer[i].sharedMaterials.Length];
            for (int j = 0; j < allChildMeshRenderer[i].sharedMaterials.Length; j++)
            {
                allChildMeshRenderer[i].sharedMaterials[j].shader = Shader.Find(allChildMeshRenderer[i].sharedMaterials[j].shader.name);
            }
        }
    }

    static public void RecoverMatShaderSkinMesh(Transform root)
    {
        SkinnedMeshRenderer[] allChildMeshRenderer = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < allChildMeshRenderer.Length; i++)
        {
//            Material[] tempMatGroup = new Material[allChildMeshRenderer[i].sharedMaterials.Length];
            for (int j = 0; j < allChildMeshRenderer[i].sharedMaterials.Length; j++)
            {
                allChildMeshRenderer[i].sharedMaterials[j].shader = Shader.Find(allChildMeshRenderer[i].sharedMaterials[j].shader.name);
            }
        }
    }

}


