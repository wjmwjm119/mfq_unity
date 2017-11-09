using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RemoteGUI : MonoBehaviour
{
    public AppBridge appBridge;
    public RemoteManger remoteManger;
    public CanveGroupFade touchBlock;
    public CanveGroupFade bg_Group;
    public CanveGroupFade exitOnlineTalk;
    public CanveGroupFade avatar;
    public Image avatarICO;

    float currentConnectPastTime;
    int currentTimeInt;
    int hour;
    int min;
    int second;

    int tick;
    string tickString;
    string helpInfo;

    public Text[] infoLabelGroup;

    bool lastIsOtherSideOnline;


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

        if (lastIsOtherSideOnline != remoteManger.isOtherSideOnline)
        {
            lastIsOtherSideOnline = remoteManger.isOtherSideOnline;
            if (lastIsOtherSideOnline)
            {

                SucessConnect();
            }
            else
            {
                LostConnect();
            }

        }



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

    public void SucessConnect()
    {
        if (remoteManger.runAtType == RemoteManger.RunAtType.Slave)
            touchBlock.AlphaPlayForward();

        SetHelpInfoString("成功连线!开始讲盘");
        infoLabelGroup[4].text ="已连接";
        infoLabelGroup[5].text ="已连接";

        bg_Group.AlphaPlayBackward();
    }

    public void LostConnect()
    {
        SetHelpInfoString("对方已掉线,等待对方重新连接");
        infoLabelGroup[2].text = "--:--:--";
        infoLabelGroup[3].text = infoLabelGroup[2].text;
        infoLabelGroup[4].text = "已掉线";
        infoLabelGroup[5].text = "已掉线";

        bg_Group.AlphaPlayForward();



    }

    


    public void OKCloseOnLineTalk()
    {
        exitOnlineTalk.AlphaPlayBackward();
        GetComponent<CanveGroupFade>().AlphaPlayBackward();

        remoteManger.DisConnect();

        appBridge.Unity2App("unityCloseRemote");
        Debug.Log("unityCloseRemote");
        GlobalDebug.Addline("unityCloseRemote");
    }

    //由app控制的关闭远程讲盘
    public void AppCloseOnLineTalk()
    {
        exitOnlineTalk.AlphaPlayBackward();
        GetComponent<CanveGroupFade>().AlphaPlayBackward();

        remoteManger.DisConnect();


    }


}
