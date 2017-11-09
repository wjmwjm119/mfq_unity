using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using DG.Tweening;
using UnityEngine.Events;

public class Loading : MonoBehaviour
{
    public CanvasGroup canvasGroup;
    public bool isloading;
    
    [HideInInspector]
    public AsyncOperation asy;
    [HideInInspector]
    public string displayInfo = "";
    [HideInInspector]
    public string receiveInfo = "";
    [HideInInspector]
    public float smoothPercent;

    public UnityEvent OnLoadedEvent;

    protected NetCtrlManager.RequestHandler request;

    public virtual void LoadingAnimation(NetCtrlManager.RequestHandler r, AsyncOperation inAsy, string info)
    {
        OnLoadedEvent = new UnityEvent();

        asy = inAsy;

        if (!isloading)
        {
            isloading = true;
            StartLoading(r,inAsy,info);
        }

        if (receiveInfo != info)
        {
            receiveInfo = info;
//          Debug.Log(info);

            switch (info)
            {
                case "ok":
                    displayInfo = "";
                    break;
                case "0":
                    displayInfo = "正在连接服务";
                    break;
                case "1":
                    displayInfo = "尝试重新连接";
                    break;
                case "2":
                    displayInfo = "尝试修复连接";
                    break;
                case "3":
                    displayInfo = "第4次连接";
                    break;
                case "4":
                    displayInfo = "第5次连接";
                    break;
                case "Request timeout":
                    displayInfo = "请求超时,请检查网络";
                    DisplayRetry();
                    break;
                case "Cannot connect to destination host":
                    displayInfo = "无法连接目标主机";
                    DisplayRetry();
                    break;
                default:
                    displayInfo = info;
                    DisplayRetry();
                    break;
            }
        }
    }

    public virtual void LoadingAnimation(AsyncOperation inAsy, string info)
    {
        OnLoadedEvent = new UnityEvent();

        asy = inAsy;

        if (!isloading)
        {
            isloading = true;
            StartLoading(inAsy, info);
        }

        if (receiveInfo != info)
        {
            receiveInfo = info;

            switch (info)
            {
                case "ok":
                    displayInfo = "";
                    break;
                default:
                    displayInfo = info;
                    break;
            }
        }
    }

    public virtual void StartLoading(NetCtrlManager.RequestHandler r, AsyncOperation inAsy, string info)
    {
        request = r;
        Debug.Log(r.name+" StartLoading "+info);
    }
    public virtual void StartLoading(AsyncOperation inAsy, string info)
    {
        Debug.Log("StartLoading "+info);
    }

    public virtual void DisplayRetry()
    {
 //       Hash128 hash128;
        
    }

    public virtual void HiddenRetry()
    {

    }

    public virtual void FinishLoading()
    {
        OnLoadedEvent.Invoke();
        GameObject.Destroy(this.gameObject);
    }

    public virtual void SetPercent()
    {

    }

    void Update()
    {
        if (isloading && asy != null)
        {
            SetPercent();
            if (asy.isDone)
            {
                if (receiveInfo == "ok"|| receiveInfo == "正在加载")
                {
                    SetPercent();
                    FinishLoading();
                }
                isloading = false;
            }
        }
    }

}



