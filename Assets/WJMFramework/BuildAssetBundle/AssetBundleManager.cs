using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.IO;


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

    public List<AssetBundle> currentLoadedAssetBundles;
    public SceneInteractiveManger sceneInteractiveManger;

    int commonAssetBundlesInfoHasLoadedCount;
    int sceneAssetBundlesInfoTotalCount;
    int sceneAssetBundlesInfoHasLoadedCount;

    //加载总数的提示圆点数
    int circlePointCount;
    int circlePointCurrent;

	//test
    public InputField projectIDField;
    public InputField sceneField;
    public InputField asserBundleField;
  //public InputField inputField;

    public Texture2D imageGet;

    void Awake()
    {
//      DontDestroyOnLoad(this);
//      Caching.ClearCache();
        Caching.compressionEnabled = false;
        currentLoadedAssetBundles = new List<AssetBundle>();
        commonAssetBundlesInfoHasLoadedCount = 0;
        sceneAssetBundlesInfoHasLoadedCount = 0;
        circlePointCount=0;
        circlePointCurrent=0;
    }


    public void LoadProjcetAssetBundles(string inProjectID)
    {
//      Debug.Log("PersistentDataPath: "+Application.persistentDataPath);
//      SetProjectPath(inProjectID);

        Loading loading = loadingManager.AddALoading(0);

        bool checkFile = false;

        if (checkFile=File.Exists(pathAndURL.localProjectAssetBundlesInfoPath))
        {
            string jsonStr = File.ReadAllText(pathAndURL.localProjectAssetBundlesInfoPath);
            localProjectAssetBundlesInfo = JsonUtility.FromJson<ProjectAssetBundlesInfo>(jsonStr);

            netCtrlManager.WebRequest(
                "同步服务器",
                pathAndURL.serverProjectAssetBundlesInfoPath,
                  loading.LoadingAnimation,
                 (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a,string info) => { Debug.Log("ServerProjectAssetBundlesInfo Load Failed!"); },
                 (DownloadHandler t) =>
                 {
                     serverProjectAssetBundlesInfo = JsonUtility.FromJson<ProjectAssetBundlesInfo>(t.text);

//                     if (globalDebug)
                     GlobalDebug.ReplaceLine(t.text,15);

                     //判断两个资源的生成时间,如果时间不一样表示资源以过期,要删去旧的资源以便重新下载
                     if (serverProjectAssetBundlesInfo.buildTime != localProjectAssetBundlesInfo.buildTime)
                     {
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
            netCtrlManager.WebRequest(
                "同步服务器",
                pathAndURL.serverProjectAssetBundlesInfoPath,
                 loading.LoadingAnimation,
                 (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a,string info) => { Debug.Log("ServerProjectAssetBundlesInfo Load Failed!"); },
                 (DownloadHandler t) =>
                 {
//                   if (globalDebug)
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

        sceneAssetBundlesInfoTotalCount = p.needExportScenePath.Length;

        LoopLoadCommonAssetBundle(0);
        //加载总数显示,point的个数
        circlePointCount = sceneAssetBundlesInfoTotalCount + commonAssetBundlesInfo.Length;
        loadingAssetBundleTotalInfo.OpenTotalInfo(circlePointCount);

    }

    void LoopLoadCommonAssetBundle(int currentID)
    {

        if (currentLoadedAssetBundles.FindIndex(x => x.name == commonAssetBundlesInfo[currentID].name) == -1)
        {
            //不清空common资源的版本
//            Caching.ClearOtherCachedVersions(pathAndURL.commonPath + commonAssetBundlesInfo[currentID].name, Hash128.Parse(commonAssetBundlesInfo[currentID].hash));

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
                   loadingAssetBundleTotalInfo.SetCirclePointOkColor(commonAssetBundlesInfoHasLoadedCount);

                   commonAssetBundlesInfoHasLoadedCount++;
                   currentLoadedAssetBundles.Add(t.assetBundle);

                   if (commonAssetBundlesInfoHasLoadedCount < commonAssetBundlesInfo.Length)
                   {
                       LoopLoadCommonAssetBundle(commonAssetBundlesInfoHasLoadedCount);
                   }
                   else if (commonAssetBundlesInfoHasLoadedCount == commonAssetBundlesInfo.Length)
                   {
                       Debug.Log("AllCommonAssetbundleLoaded!");
                       GlobalDebug.Addline("AllCommonAssetbundleLoaded!");
                       LoopLoadSceneAssetBundle(0);
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

    void LoopLoadSceneAssetBundle(int currentID)
    {
        Loading loading = loadingManager.AddALoading(2);

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
                 loadingAssetBundleTotalInfo.SetCirclePointOkColor(commonAssetBundlesInfo.Length + sceneAssetBundlesInfoHasLoadedCount);

                 Debug.Log(t.assetBundle.name);
                 sceneAssetBundlesInfoHasLoadedCount++;
                 currentLoadedAssetBundles.Add(t.assetBundle);

                 if (sceneAssetBundlesInfoHasLoadedCount < sceneAssetBundlesInfoTotalCount)
                 {
                     LoopLoadSceneAssetBundle(sceneAssetBundlesInfoHasLoadedCount);
                 }
                 else if (sceneAssetBundlesInfoHasLoadedCount == sceneAssetBundlesInfoTotalCount)
                 {
//                     Debug.Log("AllSceneAssetbundleLoaded!");
                     loadingAssetBundleTotalInfo.CloseTotalInfo();
                     //sceneInteractiveManger.finishLoadAssetBundle = true;
                     sceneInteractiveManger.OnAllAssetBundleLoaded();
                 }


             }
            );
    }

    

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
        AssetBundle[] AB = currentLoadedAssetBundles.ToArray();
        for (int i = 0; i < AB.Length; i++)
        {
            AB[i].Unload(true);
        }

        Debug.Log("UnLoadAssetBundle");
    }


    void OnDisable()
    {
        foreach (AssetBundle a in currentLoadedAssetBundles)
        {
            if (a != null)
            {
                Debug.Log(a);
                a.Unload(true);
            }
        }
        currentLoadedAssetBundles.Clear();
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


}



