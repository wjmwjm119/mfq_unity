using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.Events;
using System.IO;
using System;
using System.Text.RegularExpressions;
using System.Text;

public class AssetBundleManager : MonoBehaviour
{
//  public bool globalDebug;
    public CommonAssetBundleInfo[] commonAssetBundlesIOS;
    public CommonAssetBundleInfo[] commonAssetBundlesAndriod;
    CommonAssetBundleInfo[] commonAssetBundlesInfo;

//  public ImageCache imageCache;
    public PathAndURL pathAndURL;
    public NetCtrlManager netCtrlManager;
    public LoadingManager loadingManager;
    public LoadingAssetBundleTotalInfo loadingAssetBundleTotalInfo;

    public ProjectAssetBundlesInfo localProjectAssetBundlesInfo;
    public ProjectAssetBundlesInfo serverProjectAssetBundlesInfo;

    public List<AssetBundle> currentLoadedCommonAssetBundles;
    public List<AssetBundle> currentLoadedSceneAssetBundles;
    public List<string> hasAddedSceneName;
    public SceneInteractiveManger sceneInteractiveManger;
    public DefaultGUI defaultGUI;


    public List<string> point360SceneNameGroup;


    int commonAssetBundlesInfoHasLoadedCount;
    int sceneAssetBundlesInfoTotalCount;
    int sceneAssetBundlesInfoHasLoadedCount;

    //AssetBundle加载总数的提示圆点数
    int circlePointCount;
    int circlePointCurrent;

    //默认场景要加载的数
    public int countSceneOnDefault;
 // bool hasRecordCountSceneOnDefault;

    public Texture2D imageGet;

    public   UnityEvent OnSceneRemoved;
    public   UnityEvent OnSceneAdded;


    //test
    //    public InputField projectIDField;
    //    public InputField sceneField;
    //    public InputField asserBundleField;
    //public InputField inputField;



    void Awake()
    {
//      DontDestroyOnLoad(this);
//      Caching.ClearCache();
        Caching.compressionEnabled = false;

        currentLoadedCommonAssetBundles = new List<AssetBundle>();
        currentLoadedSceneAssetBundles = new List<AssetBundle>();
        hasAddedSceneName = new List<string>();
        commonAssetBundlesInfoHasLoadedCount = 0;
        sceneAssetBundlesInfoHasLoadedCount = 0;
        circlePointCount=0;
        circlePointCurrent=0;
        OnSceneRemoved = new UnityEvent();
        //        OnSceneAdded = new UnityEvent();
        point360SceneNameGroup = new List<string>();
    }


    public void LoadProjcetAssetBundles(string inProjectID)
    {
//      Debug.Log("PersistentDataPath: "+Application.persistentDataPath);
//      SetProjectPath(inProjectID);

        Loading loading = loadingManager.AddALoading(0);

        bool checkFile = false;

        if (checkFile=File.Exists(pathAndURL.localProjectAssetBundlesInfoPath))
        {
            string log = "有本地ProjectAssetBundlesInfo.txt";
            GlobalDebug.Addline(log);
            Debug.Log(log);

            string jsonStr = File.ReadAllText(pathAndURL.localProjectAssetBundlesInfoPath);
            localProjectAssetBundlesInfo = JsonUtility.FromJson<ProjectAssetBundlesInfo>(jsonStr);

			Debug.Log (pathAndURL.serverProjectAssetBundlesInfoPath);
			GlobalDebug.Addline(pathAndURL.serverProjectAssetBundlesInfoPath);

			//"?"+DateTime.Now.ToString() 添加时间防止ios读取http缓存
            netCtrlManager.WebRequest(
                "同步AssetBundle服务器",
				pathAndURL.serverProjectAssetBundlesInfoPath+"?"+DateTime.Now.ToString(),
                  loading.LoadingAnimation,
                 (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a,string info) => { Debug.Log("ServerProjectAssetBundlesInfo Load Failed!"); },
                 (DownloadHandler t) =>
                 {

                     serverProjectAssetBundlesInfo = JsonUtility.FromJson<ProjectAssetBundlesInfo>(t.text);

//                     if (globalDebug)
                     GlobalDebug.ReplaceLine(t.text,15);
					 Debug.Log(t.text);
                     
                     string log2 ="ServerTime:"+serverProjectAssetBundlesInfo.buildTime+" LocalTime:"+ localProjectAssetBundlesInfo.buildTime;
                     GlobalDebug.Addline(log2);
                     Debug.Log(log2);

                     //判断两个资源的生成时间,如果时间不一样表示资源以过期,要删去旧的资源以便重新下载
                     if (serverProjectAssetBundlesInfo.buildTime != localProjectAssetBundlesInfo.buildTime)
                     {
                         string log3 = "删除老资源，替换新资源";
                         GlobalDebug.Addline(log3);
                         Debug.Log(log3);

                         File.WriteAllText(pathAndURL.localProjectAssetBundlesInfoPath, t.text);
                         for (int i = 0; i < serverProjectAssetBundlesInfo.sceneAssetBundle.Length; i++)
                         {
                             Caching.ClearOtherCachedVersions(pathAndURL.projectPath + serverProjectAssetBundlesInfo.sceneAssetBundle[i], Hash128.Parse(serverProjectAssetBundlesInfo.sceneAssetBundleHash[i]));
                         }
                     }

                     StartLoadAssetBundle(serverProjectAssetBundlesInfo);
                 },
                  null,
                  null
                );
        }
        else
        {
            string log = "无本地ProjectAssetBundlesInfo.txt";
            GlobalDebug.Addline(log);
            Debug.Log(log);

            netCtrlManager.WebRequest(
                "同步AssetBundle服务器",
                pathAndURL.serverProjectAssetBundlesInfoPath,
                 loading.LoadingAnimation,
                 (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a,string info) => { Debug.Log("ServerProjectAssetBundlesInfo Load Failed!"); },
                 (DownloadHandler t) =>
                 {
                     string log2 = "获取到Server的ProjectAssetBundlesInfo.txt";
                     GlobalDebug.Addline(log2);
                     Debug.Log(log2);

                     GlobalDebug.ReplaceLine(t.text, 15);

                     serverProjectAssetBundlesInfo = JsonUtility.FromJson<ProjectAssetBundlesInfo>(t.text);
                     File.WriteAllText(pathAndURL.localProjectAssetBundlesInfoPath, t.text);

                     StartLoadAssetBundle(serverProjectAssetBundlesInfo);
                 },
                  null,
                  null
                );
        }

//  if(globalDebug)
      GlobalDebug.ReplaceLine("HasLoaclInfo.txt:" + checkFile.ToString(), 16);

    }

    public void StartLoadAssetBundle(ProjectAssetBundlesInfo p)
    {


#if UNITY_IPHONE || UNITY_IOS
        commonAssetBundlesInfo=commonAssetBundlesIOS;
#elif UNITY_ANDROID
        commonAssetBundlesInfo = commonAssetBundlesAndriod;
#endif
        countSceneOnDefault = 0;
        //由sceneType获取常规场景个数
        if (p.sceneTypeSet != null)
        {
            foreach (int i in p.sceneTypeSet)
            {
                if (i != 8)
                    countSceneOnDefault++;
            }
        }


        sceneAssetBundlesInfoTotalCount = p.needExportScenePath.Length;

        LoopLoadCommonAssetBundle(0);
        //加载总数显示,point的个数
        circlePointCount = sceneAssetBundlesInfoTotalCount + commonAssetBundlesInfo.Length;
        loadingAssetBundleTotalInfo.OpenTotalInfo(circlePointCount);

    }

    void LoopLoadCommonAssetBundle(int currentID)
    {
        if (currentLoadedCommonAssetBundles.FindIndex(x => x.name == commonAssetBundlesInfo[currentID].name) == -1)
        {
            //不清空common资源的版本
            //Caching.ClearOtherCachedVersions(pathAndURL.commonPath + commonAssetBundlesInfo[currentID].name, Hash128.Parse(commonAssetBundlesInfo[currentID].hash));

            Loading loading = loadingManager.AddALoading(2);

            netCtrlManager.WebRequest(
                "下载基础资源包 "+(currentID+1) ,
               pathAndURL.serverCommonAssetBundlePath + commonAssetBundlesInfo[currentID].name + "^" + pathAndURL.commonPath + commonAssetBundlesInfo[currentID].name + "," + commonAssetBundlesInfo[currentID].hash + "," + commonAssetBundlesInfo[currentID].CRC,
                loading.LoadingAnimation,
               (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a, string info) => { Debug.Log("ServerProjectAssetBundlesInfo Load Failed!"); },
               null,
               null,
               (DownloadHandlerAssetBundle t) =>
               {
                   circlePointCurrent++;
                   loadingAssetBundleTotalInfo.SetCirclePointOkColor(circlePointCurrent);
          
                   commonAssetBundlesInfoHasLoadedCount++;

                   currentLoadedCommonAssetBundles.Add(t.assetBundle);

                   if (commonAssetBundlesInfoHasLoadedCount < commonAssetBundlesInfo.Length)
                   {
                        LoopLoadCommonAssetBundle(commonAssetBundlesInfoHasLoadedCount);
                   }
                   else if (commonAssetBundlesInfoHasLoadedCount == commonAssetBundlesInfo.Length)
                   {
                       Debug.Log("AllCommonAssetbundleLoaded!");
                       GlobalDebug.Addline("AllCommonAssetbundleLoaded!");
                       LoopLoadSceneAssetBundle(0,2);
                   }
               }
              );
        }
    }


    /*
        void LoadCommonAssetBundles()
        {
            //commonPath + commonAssetBundlesInfo[i].name 这个是Caching的路径名
            for (int i = 0; i < commonAssetBundlesInfo.Length; i++)
            {
                //检测currentLoadedAssetBundles中是否已含有将要装载的AssetBundle
                if (currentLoadedAssetBundles.FindIndex(x => x.name == commonAssetBundlesInfo[i].name) == -1)
                {
                    Caching.ClearOtherCachedVersions(pathAndURL.commonPath + commonAssetBundlesInfo[i].name, Hash128.Parse(commonAssetBundlesInfo[i].hash));

                    Loading loading = loadingManager.AddALoading(2);

                    netCtrlManager.WebRequest(
                        "下载基础资源包",
                       pathAndURL.serverCommonAssetBundlePath + commonAssetBundlesInfo[i].name + "?" + pathAndURL.commonPath + commonAssetBundlesInfo[i].name + "," + commonAssetBundlesInfo[i].hash + "," + commonAssetBundlesInfo[i].CRC,
                        loading.LoadingAnimation,
                       (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a,string info) => { Debug.Log("ServerProjectAssetBundlesInfo Load Failed!"); },
                       null,
                       null,
                       (DownloadHandlerAssetBundle t) =>
                       {
                           commonAssetBundlesInfoHasLoadedCount++;
                           currentLoadedAssetBundles.Add(t.assetBundle);
                           AllCommonAssetBundleLoaded();
                       }
                      );
                }

            }
        }

        void AllCommonAssetBundleLoaded()
        {
            if (commonAssetBundlesInfoHasLoadedCount == commonAssetBundlesInfo.Length)
            {
                Debug.Log("AllCommonAssetbundleLoaded!");
                LoadFirstSceneAssetBundle(true);
            }
        }
        */

    public void LoopLoadSceneAssetBundleDefault()
    {
        loadingAssetBundleTotalInfo.OpenTotalInfo(sceneAssetBundlesInfoTotalCount);
        circlePointCurrent =0;
        LoopLoadSceneAssetBundle(0,5);
    }


    public void LoopLoadSceneAssetBundle(int currentID,int loadingType)
    {
        Loading loading = loadingManager.AddALoading(loadingType);
        ProjectAssetBundlesInfo p = serverProjectAssetBundlesInfo;

        string sceneName = p.needExportScenePath[currentID].Split('.')[0];
        string[] splitStr = sceneName.Split('/');
        sceneName= splitStr[splitStr.Length - 1];
        sceneName =sceneName.ToUpper();

        if (sceneName == "OUTDOOR")
        {
            sceneName = "大场景";
        }
        else
        {
            sceneName +="户型";
        }

        //index的
        netCtrlManager.WebRequest(
            "下载 "+(sceneName),
            pathAndURL.serverAssetBundlePath + p.sceneAssetBundle[currentID] + "^" + pathAndURL.projectPath + p.sceneAssetBundle[currentID] + "," + p.sceneAssetBundleHash[currentID] + "," + p.sceneAssetBundleCRC[currentID],
               loading.LoadingAnimation,
              (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a, string info) => { Debug.Log("ServerProjectAssetBundlesInfo Load Failed!"); },
              null,
              null,
             (DownloadHandlerAssetBundle t) =>
             {
                 //改变加载场景数圆点的颜色
                 circlePointCurrent++;
                 loadingAssetBundleTotalInfo.SetCirclePointOkColor(circlePointCurrent);
                 
//               if (p.sceneTypeSet != null && p.sceneTypeSet.Length == p.needExportScenePath.Length && p.sceneTypeSet[sceneAssetBundlesInfoHasLoadedCount] == 8)
//               {
//定点360不加载
//               }
//               else
//               {
                 currentLoadedSceneAssetBundles.Add(t.assetBundle);
//               }

                 if (p.sceneTypeSet != null && p.sceneTypeSet.Length == p.needExportScenePath.Length && p.sceneTypeSet[sceneAssetBundlesInfoHasLoadedCount] == 8)
                 {

                     string[] spiltStrGroup = p.needExportScenePath[currentID].Split('/');
                     string[] spiltStrGroup2 = spiltStrGroup[spiltStrGroup.Length - 1].Split('.');
                     string point360SceneName = spiltStrGroup2[0];

                     if (!point360SceneNameGroup.Contains(point360SceneName))
                     {
                         point360SceneNameGroup.Add(point360SceneName);
                         //如果有Point360,就显示360按钮
                         defaultGUI.point360PointButton_Trigger.AlphaPlayForward();
                     }
                 }

                 sceneAssetBundlesInfoHasLoadedCount++;

                 if (sceneAssetBundlesInfoHasLoadedCount < sceneAssetBundlesInfoTotalCount)
                 {
                     LoopLoadSceneAssetBundle(sceneAssetBundlesInfoHasLoadedCount, loadingType);
                 }
                 else if (sceneAssetBundlesInfoHasLoadedCount == sceneAssetBundlesInfoTotalCount)
                 {
//                   Debug.Log("AllSceneAssetbundleLoaded!");
                     loadingAssetBundleTotalInfo.CloseTotalInfo();
                     //sceneInteractiveManger.finishLoadAssetBundle = true;
                     sceneInteractiveManger.OnAllAssetBundleLoaded();
                     sceneAssetBundlesInfoHasLoadedCount = 0;
                 }
             }
            );
    }

    public void LoopRemoveAddedScene(int startID)
    {

        if (startID == 0)
        {
//         OnSceneRemoved.AddListener(()=> { Debug.LogError("gggggggggg"); });
            sceneInteractiveManger.globalCameraCenter.ChangeCamera(sceneInteractiveManger.globalCameraCenter.cameras[0]);
        }

        if (hasAddedSceneName.Count > startID)
        {
            StartCoroutine(LoopRemoveAddedScene_IE(startID));
        }
        else
        {
            hasAddedSceneName.Clear();

            //卸载场景AssetBundle
            foreach (AssetBundle a in currentLoadedSceneAssetBundles)
            {
                if (a != null)
                {
                    Debug.Log("卸载资源:" + a.name);
                    a.Unload(true);
                }
            }

            currentLoadedSceneAssetBundles.Clear();

            //整理设置默认全局相机
            CameraUniversal[] cG = sceneInteractiveManger.globalCameraCenter.cameras.ToArray();
            for (int i = 0; i < cG.Length; i++)
            {
                if (cG[i] == null)
                    sceneInteractiveManger.globalCameraCenter.cameras.Remove(cG[i]);
            }

            //清空整理senceInteractiveInfoGroup
            sceneInteractiveManger.senceInteractiveInfoGroup.Clear();
            sceneInteractiveManger.currentActiveSenceInteractiveInfo = null;
            sceneInteractiveManger.mainSenceInteractiveInfo = null;

            //执行挂载到 OnSceneRemoved 的事件并且清空
            OnSceneRemoved.Invoke();
            OnSceneRemoved.RemoveAllListeners();

            //OnSceneRemoved.Invoke("");
            //OnSceneRemoved = new UnityAction<string>("");
        }
    }

    IEnumerator LoopRemoveAddedScene_IE(int currentID)
    {

        Debug.Log(hasAddedSceneName[currentID]);

        AsyncOperation removeAsync = SceneManager.UnloadSceneAsync(hasAddedSceneName[currentID]);

        while (!removeAsync.isDone)
        {
            yield return new WaitForSeconds(0.0333f);
        }
        LoopRemoveAddedScene(++currentID);  
    }

    public void LoadAddSingerScene(string sceneName)
    {
        Loading loading = loadingManager.AddALoading(5);

        loadingAssetBundleTotalInfo.OpenTotalInfo(1);
        circlePointCurrent = 0;

        ProjectAssetBundlesInfo p = serverProjectAssetBundlesInfo;
        int targetID = -1;

        for (int i = 0; i < p.sceneAssetBundle.Length; i++)
        {
            if (GetUTF16(sceneName).ToLower() == p.sceneAssetBundle[i])
            {
                
                targetID = i;
            }
        }

        netCtrlManager.WebRequest(
            "下载 " + (sceneName),
            pathAndURL.serverAssetBundlePath + p.sceneAssetBundle[targetID] + "^" + pathAndURL.projectPath + p.sceneAssetBundle[targetID] + "," + p.sceneAssetBundleHash[targetID] + "," + p.sceneAssetBundleCRC[targetID],
               loading.LoadingAnimation,
              (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a, string info) => { Debug.Log("LoadAddSingerScene Failed!"); },
              null,
              null,
             (DownloadHandlerAssetBundle t) =>
             {
                 currentLoadedSceneAssetBundles.Add(t.assetBundle);

                 Loading loadingScene2 = loadingManager.AddALoading(5);

                 foreach (Transform tr in loadingScene2.GetComponentsInChildren<Transform>())
                 {
                     if (tr.name == "BG")
                     {
                         tr.gameObject.SetActive(false);
                     }
                 }
                 loadingAssetBundleTotalInfo.CloseTotalInfo();
                 loadingScene2.LoadingAnimation(SceneManager.LoadSceneAsync(serverProjectAssetBundlesInfo.needExportScenePath[targetID], LoadSceneMode.Additive), "正在加载");
                 loadingScene2.OnLoadedEvent.AddListener(() => { sceneInteractiveManger.ChangeInteractiveScene(sceneInteractiveManger.senceInteractiveInfoGroup[0], true); });
             }
            );



    }



//    public void LoopAddedScene(int startID)
//    {
        
//    }
/*
    IEnumerator LoopAddedScene_IE(int currentID)
    {
        Debug.Log(hasAddedSceneName[currentID]);

    //    AsyncOperation removeAsync = SceneManager.LoadSceneAsync(proj;

        while (!removeAsync.isDone)
        {
            yield return new WaitForSeconds(0.0333f);
        }
        LoopRemoveAddedScene(++currentID);
    }
*/

/*
    AsyncOperation LoopRemoveAddedScene_Async()
    {

    }
*/
/*
    void LoadFirstSceneAssetBundle(bool autoLoadSceneWhenDownload)
    {
        Loading loading = loadingManager.AddALoading(2);

        ProjectAssetBundlesInfo p = serverProjectAssetBundlesInfo;
        //index的
        netCtrlManager.WebRequest(
            "下载主场景",
            pathAndURL.serverAssetBundlePath + p.sceneAssetBundle[0] + "?" + pathAndURL.projectPath + p.sceneAssetBundle[0] + "," + p.sceneAssetBundleHash[0] + "," + p.sceneAssetBundleCRC[0],
               loading.LoadingAnimation,
              (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a,string info) => { Debug.Log("ServerProjectAssetBundlesInfo Load Failed!"); },
              null,
              null,
             (DownloadHandlerAssetBundle t) =>
             {
                 Debug.Log(t.assetBundle.name);
                 if (autoLoadSceneWhenDownload)
                 {
                     Loading loadingScene = loadingManager.AddALoading(3);
                     loadingScene.LoadingAnimation(SceneManager.LoadSceneAsync(p.needExportScenePath[0]), "正在加载");
                 }
                 currentLoadedAssetBundles.Add(t.assetBundle);
             }
            );
    }
*/
/*
    public void LoadScene()
    {
  //      StartCoroutine(LoadAssetBundle2());
    }
*/

    /*
        IEnumerator LoadAssetBundle2()
        {
            WWW www = new WWW(assetBundleLoadPath);
            yield return www;

            oneAsset = www.assetBundle;

            Debug.Log(oneAsset.isStreamedSceneAssetBundle);

    //        Debug.Log(oneAsset);

            string[] allName = oneAsset.GetAllAssetNames();

            foreach (string s in allName)
            {
    //            Debug.Log(s);
            }


            WWW www2 = new WWW(assetBundleLoadPath);
            yield return www2;

            twoAsset = www2.assetBundle;

            Debug.Log(twoAsset.isStreamedSceneAssetBundle);
            Debug.Log(twoAsset);

            string[] allName2 = twoAsset.GetAllAssetNames();
            string[] allPath2 = twoAsset.GetAllScenePaths();

            foreach (string s in allPath2)
            {
                Debug.Log(s);
            }

            SceneManager.LoadScene(allPath2[0]);
    //        SceneManager.loa("Assets\\A_OutdoorTest\\outDoorTest.unity");

    //      Instantiate(twoAsset.LoadAsset(allName2[0]));
        }
    */

    public void UnLoadAssetBundle()
    {
        AssetBundle[] AB = currentLoadedSceneAssetBundles.ToArray();
        for (int i = 0; i < AB.Length; i++)
        {
            AB[i].Unload(true);
        }

        AB = currentLoadedCommonAssetBundles.ToArray();
        for (int i = 0; i < AB.Length; i++)
        {
            AB[i].Unload(true);
        }

        Debug.Log("UnLoadAssetBundle");
    }




//    public void Unload


    void OnDisable()
    {
        foreach (AssetBundle a in currentLoadedSceneAssetBundles)
        {
            if (a != null)
            {
//                Debug.Log(a);
                a.Unload(true);
            }
        }
        currentLoadedSceneAssetBundles.Clear();

        foreach (AssetBundle a in currentLoadedCommonAssetBundles)
        {
            if (a != null)
            {
//              Debug.Log(a);
                a.Unload(true);
            }
        }
        currentLoadedCommonAssetBundles.Clear();
    }

/*
        IEnumerator LoadAssetBundle()
        {
            WWW www = new WWW(sourcePath);
            yield return www;
            Debug.Log(www.assetBundle);
            Debug.Log(Time.time);
            Debug.Log(www.assetBundle.isStreamedSceneAssetBundle);
            string[] allName = www.assetBundle.GetAllAssetNames();
            string[] allPath = www.assetBundle.GetAllScenePaths();
//          www.assetBundle.

            SceneManager.LoadScene(allPath[0]);
            foreach(string s in allName)
            {
                Debug.Log(s);
            }
            foreach (string s in allPath)
            {
                Debug.Log(s);
            }
//          yield return new  WaitForSeconds(2);
//          yield return new WaitForSeconds(2);
            Debug.Log("dddd");
            www.assetBundle.Unload(true);
            Debug.Log(www.assetBundle);
    //      Instantiate(www.assetBundle.mainAsset);
        }
*/

    [System.Serializable]
    public struct CommonAssetBundleInfo
    {
        public string name;
        public string hash;
        public uint CRC;
    }

    public static string GetUTF16(string inStr)
    {


        string outStr = "";

        char[] inStrAllChar = inStr.ToCharArray();

        //      Debug.Log(inStrAllChar.Length);

        for (int i = 0; i < inStrAllChar.Length; i++)
        {
            //            Debug.Log(inStrAllChar[i]);

            if (Regex.IsMatch(inStrAllChar[i].ToString(), @"[\u4e00-\u9fa5]"))
            {
                byte[] labelTextBytes = Encoding.BigEndianUnicode.GetBytes(inStrAllChar, i, 1);

                //                Debug.Log(labelTextBytes.Length);

                foreach (byte b in labelTextBytes)
                {
                    outStr += b.ToString("X2");
                }
            }
            else
            {
                outStr += inStrAllChar[i];
            }
        }
        return outStr;
    }

}



