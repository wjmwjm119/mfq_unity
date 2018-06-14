using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;
using UnityEngine.UI;

//要能使用AR
//户型的AssetBundleName必须和SenceInteractiveInfo.HuXingType.hxName一致，不然会出现AssetBundle 和服务器上的hxInfo不一致

public class HXInfoCache : MonoBehaviour
{
    public PathAndURL pathAndURL;
    public ServerProjectInfo serverProjectInfo;
    public AssetBundleManager assetBundleManager;
    public NetCtrlManager netCtrlManager;
    public LoadingManager loadingManager;
    public HXGUI hXGUI;

    int currentLoadID;
    bool isLoopLoading;

    ProjectAssetBundlesInfo projectAssetBundlesInfo;

    public List<HXInfo> hasLoadedHXinfo;

     void Start()
    {
        hasLoadedHXinfo = new List<HXInfo>();
    }

    public void LoadHXinARmode(string projectID,string hxAssetBundleName)
    {
        projectAssetBundlesInfo = null;
        pathAndURL.SetProjectPath(projectID);
        LoadProjectAssetBundlesInfo(projectID,hxAssetBundleName);    
    }

    void LoadProjectAssetBundlesInfo(string projectID,string hxAssetBundleName)
    {   
        assetBundleManager.LoadProjcetAssetBundlesCache(projectID, OnLoadProjectAssetBundlesInfo,hxAssetBundleName);
    }

    void OnLoadHXinfoFromServer(ProjectRootInfo p,HXInfo hXInfo)
    {
        if (p != null)
        {
            for (int i = 0; i < p.data.hxInfo.Length; i++)
            {
                if (p.data.hxInfo[i].modeName == hXInfo.modeName)
                {
                    p.data.hxInfo[i].modeName = hXInfo.modeName;
                    p.data.hxInfo[i].hxAssetBundleName = hXInfo.hxAssetBundleName;
                    p.data.hxInfo[i].hxSenePath = hXInfo.hxSenePath;
                    p.data.hxInfo[i].hash = hXInfo.hash;
                    p.data.hxInfo[i].crc = hXInfo.crc;
                    p.data.hxInfo[i].projectID = hXInfo.projectID;
                    p.data.hxInfo[i].senceInteractiveInfo = hXInfo.senceInteractiveInfo;
                    hXInfo = p.data.hxInfo[i];

                    hXInfo.senceInteractiveInfo.huXingType.introduction = hXInfo.discr;
                    hXInfo.senceInteractiveInfo.huXingType.normalArea =float.Parse( hXInfo.area);
                    hXInfo.senceInteractiveInfo.huXingType.fangXing = hXInfo.modeFormat;

                    foreach (HuXingType.floor f in hXInfo.senceInteractiveInfo.huXingType.allFloor)
                    {
                        if (f.fczMesh != null)
                            f.fczMesh.gameObject.SetActive(false);
                    }

                    hasLoadedHXinfo.Add(hXInfo);

//                    assetBundleManager.sceneInteractiveManger.arManager.CheckCurrentState();

                    return;
                }
            }
        }
        Debug.LogError("未找到项目"+ p.data.projectid+"得户型"+ hXInfo.modeName);
    }



    void OnLoadProjectAssetBundlesInfo(ProjectAssetBundlesInfo a,string projectID,string hxAssetBundleName)
    {
        projectAssetBundlesInfo = a;

        for (int i = 0; i < projectAssetBundlesInfo.sceneAssetBundle.Length; i++)
        {
            if (projectAssetBundlesInfo.sceneAssetBundle[i] == hxAssetBundleName)
            {
                HXInfo newHXInfo = new HXInfo();
                newHXInfo.hxSenePath = projectAssetBundlesInfo.needExportScenePath[i];
                newHXInfo.hash = projectAssetBundlesInfo.sceneAssetBundleHash[i];
                newHXInfo.crc = projectAssetBundlesInfo.sceneAssetBundleCRC[i];
                newHXInfo.hxAssetBundleName = hxAssetBundleName;
                newHXInfo.projectID = projectID;

                assetBundleManager.LoadAssetBundleAndAddToSceneInARMode(projectID, newHXInfo);
            }
        }

    }

    public void GetHXInfoFromServer(string projectid, HXInfo hXInfo)
    {
        ProjectRootInfo projectRootInfo = null;
        Loading loading = loadingManager.AddALoading(0);
        netCtrlManager.WebRequest(
                "获取项目信息",
                  pathAndURL.serverProjectInfoFinalUrl,
                  loading.LoadingAnimation,
                 (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a, string info) =>
                 {
                     //网络未连接且
                     string log = "未能连接服务器";
                     GlobalDebug.Addline(log);
                     Debug.Log(log);
                 },
                 (DownloadHandler t) =>
                 {
                     string log4 = "已连上服务器";
                     GlobalDebug.Addline(log4);
                     Debug.Log(log4);

                     projectRootInfo = JsonUtility.FromJson<ProjectRootInfo>(t.text);
                     projectRootInfo.data.projectid = projectid;

                     OnLoadHXinfoFromServer(projectRootInfo, hXInfo);

                 },
                  null,
                  null
                );
    }



}
