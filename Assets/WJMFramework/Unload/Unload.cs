using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;


public class Unload : MonoBehaviour
{
    public ServerProjectInfo serverProjectInfo;
    public AssetBundleManager assetBundleManager;
    public AppBridge appBridge;

    public bool autoUnloadAndReset;


	void Start ()
    {
        //在Unload场景会自动执行清空资源操作,之后再加载Start场景
        if (autoUnloadAndReset)
        {
            AppBridge.needSendUnloadMessageToUnity = true;

            StartCoroutine(UnloadUnusedAssetsIE());
        }
        else if (AppBridge.needSendUnloadMessageToUnity)
        {
            AppBridge.needSendUnloadMessageToUnity = false;

            appBridge.Unity2App("unityUnloadDone");
            Debug.Log("unityUnloadDone");
            GlobalDebug.Addline("unityUnloadDone");

			GlobalDebug.Clear ();

            if (serverProjectInfo != null)
            {
                serverProjectInfo.LoadServerProjectInfo("http://mfq.meifangquan.com/", "http://mfq.meifangquan.com/", "201700000259", "0");
            }

        }

	}

    IEnumerator UnloadUnusedAssetsIE()
    {
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();
        yield return new WaitForEndOfFrame();

        AsyncOperation a= Resources.UnloadUnusedAssets();

        while (!a.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        RemoteGather.hasInit = false;

        yield return new WaitForEndOfFrame();
        SceneManager.LoadSceneAsync(0);
    }


    //在start场景调用,转到Unload场景,进行资源清空
    public void LoadUnloadScene()
    {
        SceneInteractiveManger.needBreakLoad = false;
        SceneInteractiveManger.isLoopingAddSource = false;

        if (assetBundleManager != null)
            assetBundleManager.UnLoadAssetBundle();

        SceneManager.LoadSceneAsync(1);

    }




}
