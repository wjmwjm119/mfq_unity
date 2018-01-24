using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class PathAndURL : MonoBehaviour
{

    //"http://www.meifangquan.com/"
    //"http://192.168.1.114:8080/mfyk_app/"
    //http://www.meifangquan.com/static/data/mfyk/project/unity3d/


    /// <summary>
    /// 服务器地址
    /// </summary>
    public string assetBundleServerUrl = @"http://www.meifangquan.com/";
    public string projectInfoServerUrl = @"http://www.meifangquan.com/";

    public string assetBundleAddUrl= "static/data/mfyk/project/unity3d/";
    public string projectInfoAddUrl = "mfyk_app/unity/projectInfo?projectId=";
    public string imageAddUrl = "mfyk_app/file/download?";

    public string serverProjectInfoFinalUrl = "";
    public string imageFinalUrl = "";
    /// <summary>
    /// 工程名
    /// </summary>
    public string projectID = "projectid";


    /// <summary>
    /// 
    /// </summary>
//    string androidPath = "";
//   string iosPath = "";

    /// <summary>
    /// unity3d 管理的Cache的路径,放置通用资源
    /// </summary>
    public string commonPath = "";
    /// <summary>
    /// unity3d 管理的Cache的路径,放置普通资源
    /// </summary>
    public string projectPath = "";

    public string serverAssetBundlePath = "";
    public string serverCommonAssetBundlePath = "";

    public string localProjectInfoPath = "";
    public string localProjectAssetBundlesInfoPath = "";
    public string serverProjectAssetBundlesInfoPath = "";
    public string localImageCachePath = "";


    void Awake()
    {
//     DontDestroyOnLoad(this);
//     SetProjectPath(projectID);
    }


    public void SetProjectPath(string inProjectID)
    {
        projectID = inProjectID;

#if UNITY_IPHONE || UNITY_IOS
        projectPath= projectID+"/IOS/";
        commonPath="common2/IOS/";
        serverProjectInfoFinalUrl = projectInfoServerUrl + projectInfoAddUrl+ projectID;
        imageFinalUrl = projectInfoServerUrl + imageAddUrl;   
        
        serverAssetBundlePath = assetBundleServerUrl+ assetBundleAddUrl + projectPath;
        serverCommonAssetBundlePath= assetBundleServerUrl + assetBundleAddUrl + commonPath;
#elif UNITY_ANDROID
        projectPath = projectID + "/Andriod/";
        commonPath = "common2/Andriod/";
        serverProjectInfoFinalUrl = projectInfoServerUrl + projectInfoAddUrl+ projectID;
        imageFinalUrl = projectInfoServerUrl + imageAddUrl;

        serverAssetBundlePath = assetBundleServerUrl + assetBundleAddUrl +  projectPath;
        serverCommonAssetBundlePath = assetBundleServerUrl + assetBundleAddUrl + commonPath;
#endif
        if (!Directory.Exists(Application.persistentDataPath + "/" + projectID.ToString()))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + projectID.ToString());
        }
        if (!Directory.Exists(Application.persistentDataPath + "/" + projectID.ToString() + "/imageCache"))
        {
            Directory.CreateDirectory(Application.persistentDataPath + "/" + projectID.ToString() + "/imageCache");
        }

        localProjectInfoPath=Application.persistentDataPath+"/"+ projectID.ToString() + "/ProjectInfo.txt";
        localProjectAssetBundlesInfoPath = Application.persistentDataPath + "/" + projectID.ToString() + "/ProjectAssetBundlesInfo.txt";
        localImageCachePath = Application.persistentDataPath + "/" + projectID.ToString() + "/imageCache";
        serverProjectAssetBundlesInfoPath = serverAssetBundlePath + "ProjectAssetBundlesInfo.txt";



        GlobalDebug.ReplaceLine("Loacl PersistentDataPath: " + Application.persistentDataPath, 8);
        GlobalDebug.ReplaceLine("CacheCount:" + Caching.cacheCount.ToString(), 9);
        GlobalDebug.ReplaceLine(Caching.GetCacheAt(0).path, 10);
        GlobalDebug.ReplaceLine("Loacl "+localProjectAssetBundlesInfoPath, 11);
        GlobalDebug.ReplaceLine("Servrl "+serverProjectAssetBundlesInfoPath, 12);

    }



}
