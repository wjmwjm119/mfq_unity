using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;
using UnityEngine.UI;
using UnityEngine.Events;
using System.IO;

public class ServerProjectInfo : MonoBehaviour
{
    public AppBridge appBridge;
    public PathAndURL pathAndURL;
    public NetCtrlManager netCtrlManager;
    public LoadingManager loadingManager;
    public AssetBundleManager assetBundleManager;
    public SceneInteractiveManger sceneInteractiveManger;

    public DefaultGUI defaultGUI;
    public HXGUI hxGUI;
    public ImageCache imageCache;

    public ProjectRootInfo projectRootInfo;
    public TextTable proInfoTextTable;
    public Text warningLabel;
    public Text warningLabel2;

    public string projectInfoJsonFromServer;

    OnServerProjectInfoLoaded onServerProjectInfoLoaded;

    public bool hasLocalCached = false;
    public bool loadSceneFromCache = false;

    [System.Serializable]
    public class OnServerProjectInfoLoaded : UnityEvent<string>
    {

    }

    public void LoadServerProjectInfo_Test(string rojectInfoServerURL, string assetBundleURL, InputField projectid, string sceneLoadMode)
    {
        LoadServerProjectInfo(rojectInfoServerURL, assetBundleURL, projectid.text, sceneLoadMode);
    }

    //  public List<string> needLoadImageNameFromServerl;
    //  public List<string> needLoadImageUrleFromServerl;

    /// <summary>
    /// loadLocalScene表示是否加载本地场景否则从服务器上loadAssetBundle
    /// </summary>
    /// <param name="projectid"></param>
    /// <param name="loadLocalScene"></param>

    public void LoadServerProjectInfo(string rojectInfoServerURL, string assetBundleURL, string projectid, string sceneLoadMode, bool loadLocalScene = false)
    {
//      defaultGUI.DisplayDefaultGUI();

        pathAndURL.assetBundleServerUrl = assetBundleURL;
        pathAndURL.projectInfoServerUrl = rojectInfoServerURL;

        appBridge.appProjectInfo.sceneLoadMode = sceneLoadMode;
        

        onServerProjectInfoLoaded = new OnServerProjectInfoLoaded();

        if (loadLocalScene)
        {
            sceneInteractiveManger.StartLoadLocalScene();
        }
        else
        {
            hasLocalCached = CheckHasLocalCached(projectid);
            onServerProjectInfoLoaded.AddListener(assetBundleManager.LoadProjcetAssetBundles);
            pathAndURL.SetProjectPath(projectid);
            GetProjectInfoFromServer(projectid);
        }

    }

    public void GetProjectInfoFromServer(string projectid)
    {

        Loading loading = loadingManager.AddALoading(0);

        netCtrlManager.WebRequest(
                "获取项目信息",
                pathAndURL.serverProjectInfoFinalUrl,
                  loading.LoadingAnimation,
                 (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a, string info) => 
                 {
                     Debug.Log("Cant Get ServerProjectInfo From Server");
                     //网络未连接且
                     if (hasLocalCached)
                     {
                         loadSceneFromCache = true;

                         projectRootInfo = JsonUtility.FromJson<ProjectRootInfo>(File.ReadAllText(pathAndURL.localProjectInfoPath));
                         projectRootInfo.data.projectid = projectid;

//                       Debug.Log(File.ReadAllText(pathAndURL.localProjectInfoPath));
                         PrcessProjectInfo(projectRootInfo, true);
                         loading.FinishLoading();
                     }
                     else
                     {
       //                loading.displayInfo=""
                     }
                 },
                 (DownloadHandler t) =>
                 {
                     loadSceneFromCache = false;

                     ProcessProjectInfoText(projectid,t.text);
                 },
                  null,
                  null
                );
    }

    void ProcessProjectInfoText(string inProjectid, string infoText)
    {
        //GlobalDebug.Clear();
        //GlobalDebug.Addline(t.text);

        projectInfoJsonFromServer = infoText;
        projectRootInfo = JsonUtility.FromJson<ProjectRootInfo>(infoText);
        projectRootInfo.data.projectid = inProjectid;
//        Debug.Log(infoText);

        if (projectRootInfo.data.proName == null)
        {
            string debugStr = "服务器返回的项目数据为空!";
            Debug.Log(debugStr);
            Debug.LogWarning(debugStr);
            Debug.LogError(debugStr);
            GlobalDebug.Addline(debugStr);
            return;
        }


        PrcessProjectInfo(projectRootInfo);

    }

    public void SaveProjectInfoToLocal()
    {
        if (!loadSceneFromCache&&projectInfoJsonFromServer != "")
        {
            File.WriteAllText(pathAndURL.localProjectInfoPath, projectInfoJsonFromServer);
            Debug.Log("缓存当前项目");
            GlobalDebug.Addline("缓存当前项目");
        }

    }

    void PrcessProjectInfo(ProjectRootInfo inProjectRootInfo,bool isLoadFromCache=false)
    {
        //声明信息
        if (inProjectRootInfo.data.declareSwitch == "1")
        {
            string[] infoGroup = inProjectRootInfo.data.declareContent.Split('#');
            if (infoGroup.Length == 2)
            {
                string infoFinal = infoGroup[0] + "\n" + infoGroup[1];
                warningLabel.text = infoFinal;
                warningLabel2.text = infoFinal;
            }
        }
        else
        {
            warningLabel2.text = "";
        }

        proInfoTextTable.SetString(inProjectRootInfo.data.proName, inProjectRootInfo.data.proDiscr);



        imageCache.allNetTextrue2D = new List<NetTexture2D>();

        defaultGUI.quweiImagePlayer.netTexture2DGroup = new List<NetTexture2D>();

        for (int i = 0; i < inProjectRootInfo.data.qwInfo.Length; i++)
        {
            QwImage q = inProjectRootInfo.data.qwInfo[i];

            string modiferTime = q.modifyTime.Replace(" ", "").Replace("-", "").Replace(":", "");
            string qwImageUrl = "downloadId=" + inProjectRootInfo.data.projectid + "&downloadType=10&mediaType=2&fileName=" + q.quweiImg;
            string qwImageName = modiferTime + q.quweiImg;

            List<NetTexture2D> tempFindGroup = imageCache.allNetTextrue2D.Where(n => n.texName == qwImageName).Select(n => n).ToList();

            if (tempFindGroup.Count == 0)
            {
                NetTexture2D temp = new NetTexture2D(qwImageName, qwImageUrl, imageCache);
                defaultGUI.quweiImagePlayer.netTexture2DGroup.Add(temp);
                imageCache.allNetTextrue2D.Add(temp);
            }
            else if (tempFindGroup.Count > 0)
            {
                defaultGUI.quweiImagePlayer.netTexture2DGroup.Add(tempFindGroup[0]);
            }
        }

        HXInfo[] hx = inProjectRootInfo.data.hxInfo;
        hxGUI.hxSceneHuXingTypeFinal = new HuXingType[hx.Length];

        for (int i = 0; i < hx.Length; i++)
        {
            hxGUI.hxSceneHuXingTypeFinal[i] = new HuXingType();
            hxGUI.hxSceneHuXingTypeFinal[i].hxName = hx[i].modeName;
            
            hxGUI.hxSceneHuXingTypeFinal[i].displayName = hx[i].display;
          //hxGUI.hxSceneHuXingTypeFinal[i].huXingID = hx[i].id;
            hxGUI.hxSceneHuXingTypeFinal[i].normalPrice =-1;
            hxGUI.hxSceneHuXingTypeFinal[i].normalArea = float.Parse(hx[i].area);
            hxGUI.hxSceneHuXingTypeFinal[i].fangXing = hx[i].modeFormat;
            hxGUI.hxSceneHuXingTypeFinal[i].leiXing = "";
            hxGUI.hxSceneHuXingTypeFinal[i].introduction = hx[i].discr;
            //hxGUI.hxSceneHuXingTypeFinal[i].pmtUrl = "";

            hxGUI.hxSceneHuXingTypeFinal[i].allFloor = new HuXingType.floor[hx[i].floorData.Length];
            hxGUI.hxSceneHuXingTypeFinal[i].netTexture2DGroup = new List<NetTexture2D>();
            for (int j = 0; j < hxGUI.hxSceneHuXingTypeFinal[i].allFloor.Length; j++)
            {
                hxGUI.hxSceneHuXingTypeFinal[i].huXingID = hx[i].modeId;


//                Debug.Log(hx[i].floorData[j].modeFloor);
                //服务器上的modeFloor表示的意思是Unity中floorName的意思
                hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].floorName = hx[i].floorData[j].modeFloor;
                hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].displayName = hx[i].floorData[j].floorName;

                string modiferTime = hx[i].floorData[j].modifyTime.Replace(" ","").Replace("-","").Replace(":","");
                hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].pmtUrl="downloadId=" + hx[i].floorData[j].modeId + "&downloadType=3&mediaType=2&fileName="+ hx[i].floorData[j].fileName;
                hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].pmtName=modiferTime+hx[i].floorData[j].fileName;

                if (hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].pmtName != "")
                {

//                  hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].pmtName = "A" + (j+1) + ".jpg";
                    List<NetTexture2D> tempFindGroup = imageCache.allNetTextrue2D.Where(n => n.texName == hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].pmtName).Select(n=>n).ToList();

                    if (tempFindGroup.Count==0)
                    {
                        //测试用
                        //NetTexture2D temp = new NetTexture2D("A" + (j + 1) + ".jpg", "A" + (j + 1) + ".jpg", imageCache);
                        //服务器用
                        NetTexture2D temp =new NetTexture2D(hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].pmtName, hxGUI.hxSceneHuXingTypeFinal[i].allFloor[j].pmtUrl,imageCache);

                        hxGUI.hxSceneHuXingTypeFinal[i].netTexture2DGroup.Add(temp);
                        imageCache.allNetTextrue2D.Add(temp);
                    }
                    else if(tempFindGroup.Count>0)
                    {
                        hxGUI.hxSceneHuXingTypeFinal[i].netTexture2DGroup.Add(tempFindGroup[0]);
                    }
                }
           }
        }

        if (isLoadFromCache)
        {
            assetBundleManager.serverProjectAssetBundlesInfo = JsonUtility.FromJson<ProjectAssetBundlesInfo>(File.ReadAllText(pathAndURL.localProjectAssetBundlesInfoPath));
            assetBundleManager.StartLoadAssetBundle(assetBundleManager.serverProjectAssetBundlesInfo);
        }
        else
        {
            onServerProjectInfoLoaded.Invoke(inProjectRootInfo.data.projectid);
        }


    }

    public bool CheckHasLocalCached(string inProjectid)
    {
        pathAndURL.SetProjectPath(inProjectid);
        //检查ProjectAssetBundlesInfo.txt和ProjectInfo.txt是否本地已有
        if (File.Exists(pathAndURL.localProjectInfoPath) && File.Exists(pathAndURL.localProjectAssetBundlesInfoPath))
        {
            GlobalDebug.Addline("已有本地缓存");
            return true;
        }
        else
        {
            GlobalDebug.Addline("未含本地缓存");
            return false;
        }
    }

}



[System.Serializable]
public class ProjectRootInfo
{
   public ProjectInfo data;
   public string errorCode;
   public string operFlag;

}

[System.Serializable]
public class ProjectInfo
{
    public string projectid;
    public string proName;
    public string proDiscr;
    public string declareSwitch;
    public string declareContent;
    public QwImage[] qwInfo;
    public HXInfo[] hxInfo;

}

[System.Serializable]
public class QwImage
{
    public string quweiImg;
    public string modifyTime;
}

[System.Serializable]
public class HXInfo
{
    public string modeId;
    public string modeName;
    public string display;
    public string modeFormat;
    public string area;
    public string discr;
    public FloorData[] floorData;
}


[System.Serializable]
public class FloorData
{
  public string modeFloor;
  public string modeId;
  public string floorName;
  public string fileName;
  public string modifyTime;      
}


