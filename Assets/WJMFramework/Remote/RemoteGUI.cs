using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

public class RemoteGUI : MonoBehaviour
{
    public NetCtrlManager netCtrlManager;
    public LoadingManager loadingManager;
    public AppBridge appBridge;
    public RemoteManger remoteManger;
    public CanveGroupFade touchBlock;
    public CanveGroupFade bg_Group;
    public CanveGroupFade exitOnlineTalk;
    public CanveGroupFade avatar;
    public CanveGroupFade vedio_Trriger;
    public CanveGroupFade vedio_TrrigerFromApp;
    public RawImage avatarICO;

    public CanveGroupFade bg_Group_Trriger;
    public 



    float currentConnectPastTime;
    int currentTimeInt;
    int hour;
    int min;
    int second;

    int tick;
    string tickString;
    string helpInfo;

    bool hasFinishExitOnlineTalk;
    bool hasHeadIcoLoaded;

    public Text[] infoLabelGroup;


    void Update()
    {
        tick++;
        tickString = "".PadLeft(tick/5%4+1, '.');

        infoLabelGroup[0].text = helpInfo + tickString;
        infoLabelGroup[1].text = helpInfo + tickString;

        if (remoteManger.isOtherSideOnline)
        {
            currentConnectPastTime += Time.deltaTime;
            currentTimeInt = (int)currentConnectPastTime;
            hour = currentTimeInt / 3600;
            min = currentTimeInt % 3600 / 60;
            second = currentTimeInt % 3600 % 60;

            infoLabelGroup[2].text = hour.ToString().PadLeft(2, '0') + ":" + min.ToString().PadLeft(2, '0') + ":" + second.ToString().PadLeft(2, '0');
            infoLabelGroup[3].text = infoLabelGroup[2].text;
        }

		/*
        if (remoteManger.isOtherSideOnline != remoteManger.lastIsOtherSideOnline)
        {
            remoteManger.lastIsOtherSideOnline = remoteManger.isOtherSideOnline;

            if (remoteManger.isOtherSideOnline)
            {
                currentConnectPastTime = 0;
                SucessConnect();
            }
            else
            {
                //各种原因结束
                FinishOnLineTalk();
            }
        }
		*/

    }

	public void CheckConnectState(bool isOtherOnline)
	{



	}
		

    public void LoadAvatarICO()
    {


    }

    public void DisplayHelpInfo()
    {
        GetComponent<CanveGroupFade>().AlphaPlayForward();
        bg_Group.AlphaPlayForward();
    }

    public void SetHelpInfoString(string info)
    {
        helpInfo = info;
    }

    public void SucessConnectGUI()
    {
        remoteManger.isOtherSideOnline = true;
        remoteManger.lastIsOtherSideOnline = true;

        if (remoteManger.runAtType == RemoteManger.RunAtType.Slave)
            touchBlock.AlphaPlayForward();

        SetHelpInfoString("成功连线!开始讲盘");
        infoLabelGroup[4].text ="已连接";
        infoLabelGroup[5].text ="已连接";
        GlobalDebug.Addline("成功连线!开始讲盘");
        Debug.Log("成功连线!开始讲盘");

        bg_Group.AlphaPlayBackward();

        //只要连接上后，全屏的连接提示就不使用；
        bg_Group_Trriger.AlphaPlayBackward();


        vedio_Trriger.AlphaPlayForward();

        if (!hasHeadIcoLoaded)
        {
            Loading loading = loadingManager.AddALoading(4);
            netCtrlManager.WebRequest("Loading:" + "HeadImage", appBridge.appProjectInfo.remoteUserHeadUrl, loading.LoadingAnimation,
            (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a, string info) => { Debug.LogError("头像下载失败！"); },
             null,
             (DownloadHandlerTexture t) =>
             {
                 avatarICO.texture = t.texture;
                 hasHeadIcoLoaded = true;
             },
             null
             );
        }//这是重新连接后的状况
        else
        {

        }


    }


    public void ReConnectGUI()
    {
        SetHelpInfoString("已掉线,开始重新连接");
        infoLabelGroup[2].text = "--:--:--";
        infoLabelGroup[3].text = infoLabelGroup[2].text;
        infoLabelGroup[4].text = "连线中";
        infoLabelGroup[5].text = "连线中";
        bg_Group.AlphaPlayForward();
        vedio_Trriger.AlphaPlayBackward();

    }



    public void OKCloseOnLineTalk(string invokeFrom)
    {
        if (!remoteManger.lastIsOtherSideOnline)
        {
            FinishOnLineTalk(invokeFrom);
        }
        else
        {
            //向对方发送退出命令
            remoteManger.SendCtrlMessage(new RemoteGather.RemoteMessage(252).GetBytesData(), false);
            //执行一个延迟3秒的强制退出
            StartCoroutine(DelayExitOnLineTalk_IE());
        }
    }

    IEnumerator DelayExitOnLineTalk_IE()
    {
        yield return new WaitForSeconds(4f);

        if (!hasFinishExitOnlineTalk)
        {
            FinishOnLineTalk("4秒强制退出");
        }
    }

    public void FinishOnLineTalk(string invokeFrom)
    {
        string logStr = "结束讲盘 InvokeFrom:" + invokeFrom;
        Debug.Log(logStr);
        GlobalDebug.Addline(logStr);

        touchBlock.AlphaPlayBackward();
        exitOnlineTalk.AlphaPlayBackward();
        GetComponent<CanveGroupFade>().AlphaPlayBackward();
        StartCoroutine(FinishOnLineTalk_IE());
    }

    IEnumerator FinishOnLineTalk_IE()
    {
        yield return new WaitForSeconds(0.3f);
        remoteManger.DisConnect();
        yield return new WaitForSeconds(0.2f);

        if (!hasFinishExitOnlineTalk)
        {
            appBridge.Unity2App("unityCloseRemote", "0");
            string logStr = "unityCloseRemote";
            Debug.Log(logStr);
            GlobalDebug.Addline(logStr);
            hasFinishExitOnlineTalk = true;
        }

    }

    public void GotoVedioTalk()
    {
        string logStr = "进入视频通话";
        Debug.Log(logStr);
        GlobalDebug.Addline(logStr);
        //向对方发送退出讲盘进入视频通话
        remoteManger.SendCtrlMessage(new RemoteGather.RemoteMessage(250).GetBytesData(), false);
    }

    public void FinishGotoVediaoTalk()
    {
        StartCoroutine(FinishGotoVediaoTalk_IE());
    }

    IEnumerator FinishGotoVediaoTalk_IE()
    {
        yield return new WaitForSeconds(0.3f);
        remoteManger.DisConnect();
        yield return new WaitForSeconds(0.2f);

        avatar.AlphaPlayBackward();
        vedio_Trriger.AlphaPlayBackward();


        appBridge.Unity2App("unityCloseRemote", "6");
        string logStr = "unityCloseRemote 6";
        Debug.Log(logStr);
        GlobalDebug.Addline(logStr);

    }


    //由app控制的关闭远程讲盘
    public void AppCloseOnLineTalk()
    {
//        exitOnlineTalk.AlphaPlayBackward();
//        GetComponent<CanveGroupFade>().AlphaPlayBackward();
//        remoteManger.DisConnect();
    }


}
