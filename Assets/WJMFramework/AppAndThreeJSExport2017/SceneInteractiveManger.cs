using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneInteractiveManger : MonoBehaviour
{
    public AppBridge appBridge;
//  public ShaderLib shaderLib;
//  public ARManager arManager;
    public AssetBundleManager assetBundleManager;
    public LoadingManager loadingManager;
    public RemoteManger remoteManger;
    public CameraUniversalCenter globalCameraCenter;
    public ZBZ zbz;
    public DefaultGUI defaultGUI;
    public HXGUI hxGUI;
    public ImageCache imageCache;

 //   public CanveGroupFade cartoonPlayerFade;
 //   public CartoonPlayer cartoonPlayer;

    public RenderTexture thumbnailOutdoor;
    public RenderTexture thumbnailHX;
    public SenceInteractiveInfo mainSenceInteractiveInfo;
    public SenceInteractiveInfo currentActiveSenceInteractiveInfo;
    public List<SenceInteractiveInfo> senceInteractiveInfoGroup;

    public CartoonPlayer cartoonPlayer;

    public bool finishLoadAssetBundle;
    bool lastFinishLoadAssetBundle;

    public enum LoadAtState
    {
        normal = 0,
        onlineTalk = 1,
        ar = 2
    }

//  public static LoadAtState loadAtState = LoadAtState.normal;

    public string addSceneName;

    int currentAddSceneID;

    public CameraUniversal currentThumbnailCamera;

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

        defaultGUI.triggerCancelBtn.AlphaPlayBackward();

        switch (appBridge.appProjectInfo.sceneLoadMode)
        {
            
            case "0":
                //加载主场景
                Loading loadingScene = loadingManager.AddALoading(5);
                loadingScene.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[0], LoadSceneMode.Additive), "正在加载");
                loadingScene.OnLoadedEvent.AddListener(() => {StartCoroutine(LoadSenceInteractiveIE()); });
                break;

            case "1":
                //不加载主场景,只加载户型,跳过第一个主场景
//             defaultGUI.triggerMusic.AlphaPlayBackward();
                currentAddSceneID = 0;
                LoopAdditiveScene(true);
                //arManager.OpenARCamrea();
                break;

            case "2":
                //加载主场景，并且激活
                Loading loadingScene3 = loadingManager.AddALoading(5);
                loadingScene3.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[0], LoadSceneMode.Additive), "正在加载");
                loadingScene3.OnLoadedEvent.AddListener(() => { StartCoroutine(LoadSenceInteractiveIE()); });
                break;

            case "8":
                //不加载主场景,只加载户型,以供AR扫描,跳过第一个主场景
                currentAddSceneID = 0;
                LoopAdditiveScene(true);

                //arManager.OpenARCamrea();
                break;

            case "9":

                //加载主场景且打开在线讲
                Loading loadingScene2 = loadingManager.AddALoading(5);
                loadingScene2.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[0], LoadSceneMode.Additive), "正在加载");
                loadingScene2.OnLoadedEvent.AddListener(() => { StartCoroutine(LoadSenceInteractiveIE(true)); });
                appBridge.serverProjectInfo.Trigger_ToPanoramaBtn.AlphaPlayBackward();


                break;

        }
    }


    IEnumerator LoadSenceInteractiveIE(bool isOnlineTalk = false)
    {
        yield return new WaitForSeconds(0.3f);

        if (senceInteractiveInfoGroup[0]!=null)
        {
            ChangeInteractiveScene(senceInteractiveInfoGroup[0], true);
        }
        else
        {
            Debug.LogError("大场景未含SenceInteractiveInfo");
        }

        if (isOnlineTalk)
            remoteManger.StartOnlineTalk();
        LoopAdditiveScene();

    }

    void LoopAdditiveScene(bool loadImageInEnd=true)
    {
        currentAddSceneID++;

        if (currentAddSceneID < assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath.Length)
        {
            /*
            Loading loadingScene = loadingManager.AddALoading(4);
            loadingScene.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[currentAddSceneID], LoadSceneMode.Additive), "正在加载");
            loadingScene.OnLoadedEvent.AddListener(() => { LoopAdditiveScene(); });
            */
                        if (assetBundleManager.serverProjectAssetBundlesInfo.sceneTypeSet != null && assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath.Length == assetBundleManager.serverProjectAssetBundlesInfo.sceneTypeSet.Length)
                        {
                            if (assetBundleManager.serverProjectAssetBundlesInfo.sceneTypeSet[currentAddSceneID] == 8)
                            {
                                Debug.Log("跳过360场景 "+ assetBundleManager.serverProjectAssetBundlesInfo.sceneAssetBundle[currentAddSceneID]);
                                //跳过360场景,开始加载下个场景
                                LoopAdditiveScene();
                            }
                            else
                            {
                                Loading loadingScene = loadingManager.AddALoading(4);
                                loadingScene.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[currentAddSceneID], LoadSceneMode.Additive), "正在加载");
                                loadingScene.OnLoadedEvent.AddListener(() => { LoopAdditiveScene(); });
                            }
                        }
                        else
                        {
                            Loading loadingScene = loadingManager.AddALoading(4);
                            loadingScene.LoadingAnimation(SceneManager.LoadSceneAsync(assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath[currentAddSceneID], LoadSceneMode.Additive), "正在加载");
                            loadingScene.OnLoadedEvent.AddListener(() => { LoopAdditiveScene(); });
                        }
            

        }
        else if(currentAddSceneID == assetBundleManager.serverProjectAssetBundlesInfo.needExportScenePath.Length)
        {
            GlobalDebug.Addline("All Additive Scene Loaded!");
            Debug.Log("All Additive Scene Loaded!");

            if(loadImageInEnd)
            imageCache.LoopLoadAndCacheImageFromServer();

            if (appBridge.appProjectInfo.sceneLoadMode =="2")
            {
                defaultGUI.mainBtnGroup.GetComponent<ButtonGroup>().imageButtonGroup[4].SetBtnState(true, 0);
            }

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
        if(l==LoadSceneMode.Additive)
        assetBundleManager.hasAddedSceneName.Add(addSceneName);
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

//      Debug.Log(1111);

        if (s.meshRoot != null)
        {
            //查找websky，因为由些老的项目websky没有正确设置
            if (s.websky == null)
            {
                Transform t = s.meshRoot.Find("websky");
                if (t != null)
                    s.websky = t;
            }

            RecoverMatShader(s.meshRoot);
        }

        // Debug.Log(2222);

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
                RecoverMatShader(c.camBase.transform);
            }
        
//          Debug.Log(3333);
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
            else if (s.sceneType == SenceInteractiveInfo.SceneType.Point360)
            {

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
//                        hFinal.hxAudioClip = s.huXingType.hxAudioClip;
//                        hFinal.cartoonType = s.huXingType.cartoonType;

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

            Debug.Log(s.sceneName + " SenceType: " + s.sceneType);
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
        if (s.sceneType == SenceInteractiveInfo.SceneType.loft||
            s.sceneType == SenceInteractiveInfo.SceneType.叠拼||
            s.sceneType == SenceInteractiveInfo.SceneType.平层||
            s.sceneType == SenceInteractiveInfo.SceneType.独栋||
            s.sceneType == SenceInteractiveInfo.SceneType.联排
            )
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

    public void RenderSenceThumbnail(RenderTexture renderTexture, SenceInteractiveInfo needRenderScene,CameraUniversal cameraThumbnail, string cameraArgs="")
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

        cameraThumbnail.GetComponent<Camera>().targetTexture = renderTexture;
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

//     Debug.Log(toInteractiveScene.name);
//     Debug.Log(currentActiveSenceInteractiveInfo.name);

        if (toInteractiveScene == currentActiveSenceInteractiveInfo)
        return;

        if (currentActiveSenceInteractiveInfo != null&&disableCurrentSceneMesh)
        {
            if (currentActiveSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.大场景)
            {
                if (currentActiveSenceInteractiveInfo.meshRoot != null)
                    currentActiveSenceInteractiveInfo.meshRoot.gameObject.SetActive(false);

            }
            else if (currentActiveSenceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.Point360)
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

        if (toInteractiveScene.sceneType == SenceInteractiveInfo.SceneType.Point360)
        {
            globalCameraCenter.ChangeCamera(toInteractiveScene.cameraUniversalCenter.cameras[1], 0.0f);
        }
        else
        {
            globalCameraCenter.ChangeCamera(toInteractiveScene.cameraUniversalCenter.cameras[0], 0.0f);
        }

        if (toInteractiveScene.sceneType == SenceInteractiveInfo.SceneType.大场景)
        {
            if (toInteractiveScene.meshRoot != null)
                toInteractiveScene.meshRoot.gameObject.SetActive(true);
            mainSenceInteractiveInfo = toInteractiveScene;
            //            defaultGUI.DisplayDefaultGUI();
        }
        else if (toInteractiveScene.sceneType == SenceInteractiveInfo.SceneType.Point360)
        {
            if (toInteractiveScene.meshRoot != null)
                toInteractiveScene.meshRoot.gameObject.SetActive(true);
            mainSenceInteractiveInfo = toInteractiveScene;
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

 //    loadingManager.load
        if(CartoonPlayer.hasInit)
        PlayCartoonAni();

    }


    public void PlayCartoonAni()
    {
        if (currentActiveSenceInteractiveInfo != null)
        {
            CartoonPlayArgs cartoonArgs = currentActiveSenceInteractiveInfo.GetComponent<CartoonPlayArgs>();
            if (DefaultGUI.isLandscape && cartoonArgs != null && cartoonArgs.hxAudioClip != null)
            {
                //如果已经播放，默认是不播放的
                if (!cartoonArgs.hasPlayed)
                {
                    cartoonPlayer.OpenCartoonPeopleUseAudioFile(cartoonArgs.hxAudioClip, cartoonArgs.cartoonType);
                    cartoonArgs.hasPlayed = true;
                }
                else
                {
                    cartoonPlayer.OpenCartoonPeopleUseAudioFile(cartoonArgs.hxAudioClip, cartoonArgs.cartoonType);
                    cartoonPlayer.Pause();
                }
            }
            else
            {
                cartoonPlayer.CloseCaratoonPeople();
            }
        }
    }

    public void CloseCartoonAni()
    {
        if(cartoonPlayer!=null)
        cartoonPlayer.CloseCaratoonPeople();
    }


    /// <summary>
    /// Editor下替换Shader使得场景在Editor下可以正常显示
    /// </summary>
    /// <param name="root"></param>
    static public void RecoverMatShader(Transform root)
    {

//        Debug.Log(root.name);
            
        if (ShaderLib.lib == null)
            return;

        MeshRenderer[] allChildMeshRenderer = root.GetComponentsInChildren<MeshRenderer>(true);
        for (int i = 0; i < allChildMeshRenderer.Length; i++)
        {
//            Material[] tempMatGroup = new Material[allChildMeshRenderer[i].sharedMaterials.Length];
            for (int j = 0; j < allChildMeshRenderer[i].sharedMaterials.Length; j++)
            {
//             Debug.Log(allChildMeshRenderer[i].name);
                if (allChildMeshRenderer[i].sharedMaterials[j] != null)
                {
                    // allChildMeshRenderer[i].sharedMaterials[j].shader = Shader.Find(allChildMeshRenderer[i].sharedMaterials[j].shader.name);
                    if (ShaderLib.lib.ContainsKey(allChildMeshRenderer[i].sharedMaterials[j].shader.name))
                    {
                        allChildMeshRenderer[i].sharedMaterials[j].shader = ShaderLib.lib[allChildMeshRenderer[i].sharedMaterials[j].shader.name];
                    }
//                    else
//                    {
//                        Debug.LogError("Shader设置错误："+ allChildMeshRenderer[i].sharedMaterials[j].name+"  "+ allChildMeshRenderer[i].sharedMaterials[j].shader.name);
//                    }

                }
                else
                {
                    Debug.LogError(allChildMeshRenderer[i].name+"丢失材质球");
                }
            }
        }
    }

    static public void RecoverMatShaderSkinMesh(Transform root)
    {
        if (ShaderLib.lib == null)
            return;

        SkinnedMeshRenderer[] allChildMeshRenderer = root.GetComponentsInChildren<SkinnedMeshRenderer>(true);
        for (int i = 0; i < allChildMeshRenderer.Length; i++)
        {
//            Material[] tempMatGroup = new Material[allChildMeshRenderer[i].sharedMaterials.Length];
            for (int j = 0; j < allChildMeshRenderer[i].sharedMaterials.Length; j++)
            {
                allChildMeshRenderer[i].sharedMaterials[j].shader = ShaderLib.lib[allChildMeshRenderer[i].sharedMaterials[j].shader.name];
            }
        }
    }

}


