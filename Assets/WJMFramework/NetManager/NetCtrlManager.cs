using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;


//http://www.meifangquan.com/static/data/mfyk/project/unity3d/btnBG.png

public class NetCtrlManager : MonoBehaviour
{
    public int RequestCount;
    public List<RequestHandler> allRequestList;

    void Awake()
    {
 //     DontDestroyOnLoad(this);
        allRequestList = new List<RequestHandler>();
    }

    public void WebRequest(string helpInfo,string inUrl, RequestHandler.WebQuestDelegate p = null, RequestHandler.WebQuestDelegate eFalse = null, RequestHandler.WebQuestDelegateText eText = null, RequestHandler.WebQuestDelegateTexture eTexture = null, RequestHandler.WebQuestDelegateAssetBundle eAssetBundle = null)
    {
        RequestCount++;
        allRequestList.Add(new RequestHandler(helpInfo,this, inUrl,p,eFalse,eText, eTexture,eAssetBundle));
    }

    void Update()
    {
        for (int i = 0; i < allRequestList.Count; i++)
        {
            allRequestList[i].Progress();
        }
    }


    void OnDisable()
    {
        AbortAllRequest();
    }

    public void AbortAllRequest()
    {
        foreach (RequestHandler r in allRequestList)
        {
            if (r != null)
            {
                r.Abort();
            }
        }
    }



    [System.Serializable]
    public class RequestHandler
    {
        //0:text,1:image,2:AssetBundle
        int requestType = -1;
        public string name;
        public string helpInfo;

        NetCtrlManager netCtrlManager;

        public delegate void WebQuestDelegate(RequestHandler r, UnityWebRequestAsyncOperation asy,string info);
        public event WebQuestDelegate onProgress;
        public event WebQuestDelegate onTrue;
        public event WebQuestDelegate onFalse;

        public delegate void WebQuestDelegateText(DownloadHandler handler);
        public event WebQuestDelegateText onEndText;

        public delegate void WebQuestDelegateTexture(DownloadHandlerTexture handler);
        public event WebQuestDelegateTexture onEndTexture;

        public delegate void WebQuestDelegateAssetBundle(DownloadHandlerAssetBundle handler);
        public event WebQuestDelegateAssetBundle onEndAssetBundle;

//      public DownloadHandlerTexture downloadHandlerTexture;
//      public DownloadHandlerAssetBundle downloadHandlerAssetBundle;

        public UnityWebRequest request;
        public UnityWebRequestAsyncOperation asyncOpertion;

        const int timeOut=6;
        int finalTimeOut = 0;
        const int retryCount = 2;
        public int currentRetryCount = 0;
        public string url = "";
        //保存cache的路径
        public string paddingUrl = "";
        string[] paddingUrlSplitGroup;

        //保存Form用的参数
        public string urlWithoutArg;
        string[] paddingArgUrlSplitGroup;

        public float progress;
        public string processInfo;
        public float timing;
        bool isNeedProcess;

        //在LoadAsssetBundle时inUrl链接会拼入用^拆分cache路径,用&拆分连接参数
        public RequestHandler(string inHelpInfo, NetCtrlManager net, string inUrl, WebQuestDelegate p = null, WebQuestDelegate eFalse = null, WebQuestDelegateText eText = null, WebQuestDelegateTexture eTexture = null, WebQuestDelegateAssetBundle eAssetBundle = null)
        {
            if (inUrl == "")
            { 
            Debug.LogError("输入的URL为空！");
            return;
            }
            name = inHelpInfo;
            netCtrlManager = net;
            helpInfo = inHelpInfo;
            Debug.Log("InputURL: <color=blue>" + inUrl+ "</color>");

            timing = 0;
            string[] splitStringGroup = inUrl.Split('^');
            url = splitStringGroup[0];

            //保存所有的参数
            paddingArgUrlSplitGroup = url.Split('?');
            if (paddingArgUrlSplitGroup.Length > 1)
            {
                //除去参数的连接
                urlWithoutArg = paddingArgUrlSplitGroup[0];
                paddingArgUrlSplitGroup = paddingArgUrlSplitGroup[1].Split('&');
            }

//            string[] urlSplit = url.Split('/');
//            name = urlSplit[urlSplit.Length - 1];

            //Cache路径
            if (splitStringGroup.Length > 1)
            { 
                paddingUrl = splitStringGroup[1];
                paddingUrlSplitGroup = paddingUrl.Split(',');
            }

            if (url == "")
            {
                processInfo = "url为空";
                return ;
            }

            isNeedProcess = true;
            onProgress +=GetProgress;

            if (p != null)
            {
                onProgress += p;
                onTrue += p;
                onFalse += p;
            }

            if (eFalse != null)
            onFalse += eFalse;
            
 
            if (eText != null)
            {
                request = UnityWebRequest.Get(url);
                requestType = 0;
                onEndText += eText;
                finalTimeOut = timeOut;
                
            }
            else if (eTexture != null)
            {
                WWWForm form = new WWWForm();

                form.AddField("downloadId", paddingArgUrlSplitGroup[0].Split('=')[1]);
                form.AddField("downloadType", paddingArgUrlSplitGroup[1].Split('=')[1]);
                form.AddField("mediaType", paddingArgUrlSplitGroup[2].Split('=')[1]);
                form.AddField("fileName", paddingArgUrlSplitGroup[3].Split('=')[1]);

/*
                Debug.Log(urlWithoutArg);
                foreach (string s in paddingArgUrlSplitGroup)
                {
                    Debug.Log(s);
                }
*/

                request = UnityWebRequest.Post(urlWithoutArg,form);

                requestType = 1;
                onEndTexture += eTexture;
                request.downloadHandler = new DownloadHandlerTexture();
                finalTimeOut = timeOut*12;
            }
            else if (eAssetBundle!=null)
            {

                request = UnityWebRequest.Get(url);
                requestType = 2;
                onEndAssetBundle += eAssetBundle;
                //name包含了路径Cache
                request.downloadHandler = new DownloadHandlerAssetBundle("", paddingUrlSplitGroup[0],Hash128.Parse(paddingUrlSplitGroup[1]),uint.Parse(paddingUrlSplitGroup[2] ));
                finalTimeOut = timeOut * 360;
            }

            request.timeout = finalTimeOut;

            asyncOpertion = request.SendWebRequest();
//          asyncOpertion.allowSceneActivation = true;

        }

        void Retry()
        {
            Debug.Log("Retry");
            request.Dispose();
            request = UnityWebRequest.Get(url);
            request.timeout = finalTimeOut;

            
            switch (requestType)
            {
                case 0:
                    break;
                case 1:
                    request.downloadHandler = new DownloadHandlerTexture();
                    break;
                case 2:
                    request.downloadHandler = new DownloadHandlerAssetBundle("", paddingUrlSplitGroup[0], Hash128.Parse(paddingUrlSplitGroup[1]), uint.Parse(paddingUrlSplitGroup[2]));
                    break;
                default:
                    break;
            }
            
            asyncOpertion = request.SendWebRequest();

        }

        public void ManualRetry()
        {
            Debug.Log("ManualRetry");
            currentRetryCount = 0;
            isNeedProcess = true;
            Retry();
        }

        void GetProgress(NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a,string info)
        {
            progress = a.progress;
//          Debug.Log(request.downloadProgress);
        }

        public void Progress()
        {
            if (isNeedProcess)
            {
                timing += Time.deltaTime;
                onProgress(this,asyncOpertion,currentRetryCount.ToString());

                processInfo = "Retry:" + currentRetryCount;
                processInfo += ",Timing: " + timing;

                if (request.isHttpError)
                {
                    processInfo += ",HttpErrorCode:" + request.responseCode.ToString();
                    processInfo += ",Erorr:" + request.error;
                    FalseEvent(request.error);
                }

                if (request.isNetworkError)
                {
                    //Debug.Log(request.error);
                    //1.没这服务器,连接超时
                    //2.有这服务器,但是没有web服务
                    //3,request.error如果中途断网是提示Failed to receive data
                    processInfo += ",CannotConnectServer!";
                    processInfo += ",Erorr:" + request.error;

                    if (currentRetryCount < retryCount)
                    {
                        currentRetryCount++;
                        Retry();
                    }
                    else
                    {
                        FalseEvent(request.error);
                    }
                }

                if (request.isDone && !request.isNetworkError && !request.isHttpError && progress == 1)
                {
                    processInfo += ",State:WebRequestSccuess!";
                    TrueEvent();
                }
            }
        }

        void TrueEvent()
        {
//          Debug.Log("TrueEvent");
            isNeedProcess = false;

            if(onEndText!=null)
            onEndText(request.downloadHandler);
            if (onEndTexture != null)
            onEndTexture((DownloadHandlerTexture)request.downloadHandler);
            if (onEndAssetBundle != null)
            onEndAssetBundle((DownloadHandlerAssetBundle)request.downloadHandler);

            if (onTrue != null)
            onTrue(this,asyncOpertion,"ok");

            Dispose();

        }

        void FalseEvent(string errorInfo)
        {

            isNeedProcess = false;

            if (onFalse!=null)
            onFalse(this,asyncOpertion, errorInfo);
        }

        public void Abort()
        {
            if (request != null)
            {
                request.Abort();
                request.Dispose();
            }
        }


        void Dispose()
        {
            netCtrlManager.allRequestList.Remove(this);
            request.Dispose();
        }

    }


/*
    IEnumerator GetTXT_IE(string url)
    {
        UnityWebRequest request = UnityWebRequest.Get(url);
        request.timeout = 3;
            Debug.Log(url);
            allRequestList.Add(request);
            yield return request.SendWebRequest();
            Debug.Log(allRequestList[0].isDone);
            Debug.Log(request.responseCode);

            if (request.isNetworkError || request.isHttpError)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
//              byte[] results = request.downloadHandler.data;
            }
        Debug.Log(allRequestList.Count);
        Debug.Log(allRequestList[0].timeout);

        Debug.Log(allRequestList[0].isDone);
        UnityWebRequest u = allRequestList[0];

        allRequestList.Remove(u);
        u.Dispose();
//        Debug.Log(allRequestList[0]);
    }
*/

}
