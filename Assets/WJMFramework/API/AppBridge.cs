using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.Runtime.InteropServices;
using DG.Tweening;


public class AppBridge : MonoBehaviour
{
    public Unload unload;
    public ServerProjectInfo serverProjectInfo;
    public DefaultGUI defaultGUI;
    public RemoteGUI remoteGUI;
    public RemoteManger remoteManger;
    public HXGUI hxGUI;
    public BackAction backAction;
    public AppProjectInfo appProjectInfo;

    public static bool needSendUnloadMessageToUnity;

    public static bool isInRemoteState;

//  public BackAction backAction;

    public void Exit()
    {
        defaultGUI.Portrait();
        Unity2App("unityProtrait");
        Unity2App("unityExit");
    }

    public void CanceLoad()
    {
        Unity2App("unityCancelLoad");
    }


#if !TEST

#if UNITY_IOS
    [DllImport("__Internal")]
    private static extern void unityReady(); // 告诉外部已经准备好
    [DllImport("__Internal")]
    private static extern void unityProtrait(); // 竖屏时的回调
    [DllImport("__Internal")]
    private static extern void unityLandscape();// 横屏时的回调
    [DllImport("__Internal")]
    private static extern void unityLoadDone(); // 资源加载完毕后的回调
    [DllImport("__Internal")]
    private static extern void unityUnloadDone(); // 卸载已加载项目并清空内存后的回调
    [DllImport("__Internal")]
    private static extern void unityCloseRemote(string state);// 结束讲盘
    [DllImport("__Internal")]
    private static extern void unityShareRoom(string roomID);// 分享户型
    [DllImport("__Internal")]
    private static extern void unityOpenRoomTypeDone();// 进入户型完毕
    [DllImport("__Internal")]
    private static extern void unityBackRoomTypeDone();// 户型返回完毕
    [DllImport("__Internal")]
    private static extern void unitySetMusic(string musicState);//音乐开关
    [DllImport("__Internal")]
    private static extern void unityToPanorama(string url);//切到外部全景图模式
    [DllImport("__Internal")]
    private static extern void unityCancelLoad();//取消加载
    [DllImport("__Internal")]
    private static extern void unityExit();//退出
    

 //[DllImport("__Internal")]
 //private static extern void unityEnterMYInPortraitDone();//竖屏进入户型漫游
 //unityOpenRoomType(string roomID)    进入户型

#endif

#endif

    //Unity调用外部代码
    public void Unity2App(string methodName, params object[] args)
    {

#if UNITY_ANDROID
        try
        {
            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject jo = jc.GetStatic<AndroidJavaObject>("currentActivity");
            jo.Call(methodName, args);
         }
         catch (Exception e)
         {
            Debug.Log(e.ToString());
         }
#endif

#if true

#endif


#if !TEST

#if UNITY_IOS && !UNITY_EDITOR
        switch(methodName) {
                case "unityReady":
                    unityReady();
                    break;
                case "unityProtrait":
                    unityProtrait();
                    break;
                case "unityLandscape":
                    unityLandscape();
                    break;
                case "unityLoadDone":
                    unityLoadDone();
                    break;
                case "unityCloseRemote":
                    unityCloseRemote((string)args[0]);
                    break;
                case "unityShareRoom":
                    unityShareRoom((string)args[0]);
                    break;
                case "unityUnloadDone":
                    unityUnloadDone();
                    break;
                case "unityOpenRoomTypeDone":
                    unityOpenRoomTypeDone();
                    break;
                case "unityBackRoomTypeDone":
                    unityBackRoomTypeDone();
                    break;
//              case "unityEnterMYInPortraitDone":
//                  unityEnterMYInPortraitDone();
//                  break;
                case "unitySetMusic":
                    unitySetMusic((string)args[0]);
                    break;

                case "unityToPanorama":
                    unityToPanorama((string)args[0]);
                    break;

                case "unityCancelLoad":
                    unityCancelLoad();
                    break;

                case "unityExit":
                    unityExit();
                    break;

                default:
                    GlobalDebug.Addline("未定义的方法："+ methodName);
                    break;
            }
#endif
#endif

    }

    public void Load_Test()
    {
        Load(JsonUtility.ToJson(appProjectInfo));   
//      Load(serverProjectInfo.projectInfoJsonFromServer);
    }

    static string EncodeDateKEY(int year, int month, int day)
    {
        int modiferMouth = 0;
        string valueKey = "";
        char[] addCodeChar = new char[] { 't', 'k', '9', '1', 'z', 'y', 'u', '1' };

        if (month == 10)
        {
            modiferMouth = 7;
        }
        else
        {
            modiferMouth = month;
        }

        valueKey = (modiferMouth * day).ToString();//生成一个由月和日相乘的数，取末尾一位数，加到日前面
        valueKey = valueKey.Substring(valueKey.Length - 1);//在实际日期前多补一位     如   52013,5,13

        valueKey += year.ToString() + ":" + month.ToString() + ":" + day.ToString();

        char[] valueKeyChars = valueKey.ToCharArray();

        int firtCharAscii = (int)valueKeyChars[0];

        for (int i = 0; i < valueKeyChars.Length; i++)
        {
            if (i == 0)
            {
                valueKeyChars[i] = (char)((int)valueKeyChars[i] + 30);
            }
            else
            {
                if (i < addCodeChar.Length)
                {
                    valueKeyChars[i] = (char)((int)valueKeyChars[i] + 17 + (((7 + firtCharAscii) * i) + (int)addCodeChar[i]) % 15);
                }
                else
                {
                    valueKeyChars[i] = (char)((int)valueKeyChars[i] + 17 + ((7 + firtCharAscii) * i) % 15);
                }
            }
        }
        return new string(valueKeyChars);
    }

    //添加验证环节（IOS版SDK使用）
    void LoadUnity(string info)
    {
        appProjectInfo = JsonUtility.FromJson<AppProjectInfo>(info);

        if (EncodeDateKEY(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day) == appProjectInfo.checkKey)
        {
            Load(info);
        }
        else
        {
            Debug.Log("CheckKey 校验不一致!");
        }
    }

    //以下函数为,外部调用Unity函数
    void Load(string info)
    {
        GlobalDebug.Addline("APP2Unity ReceiveInfo:" + info);
        Debug.Log("APP2Unity ReceiveInfo:" + info);

        appProjectInfo = JsonUtility.FromJson<AppProjectInfo>(info);

        string[] ipS = appProjectInfo.remoteServer.Split('.');
        if (ipS.Length == 4)
        {
            byte[] ipBytes = new byte[] { (byte)int.Parse(ipS[0]), (byte)int.Parse(ipS[1]), (byte)int.Parse(ipS[2]), (byte)int.Parse(ipS[3]) };
            MFQTcpClient.remoteIPAddress = new System.Net.IPAddress(ipBytes);
        }
        else
        {
            Debug.LogError("RemoteServerIPAdressError!");
        }

        int remoteUserIDLength = appProjectInfo.remoteUserID.Length;
        int userIDLenght = appProjectInfo.userID.Length;

        //必须使用10位长度，才能正确建立房间
        if (remoteUserIDLength > 10)
        {
            appProjectInfo.remoteUserID = appProjectInfo.remoteUserID.Substring(remoteUserIDLength - 10, 10);
        }

        if (userIDLenght > 10)
        {
            appProjectInfo.userID = appProjectInfo.userID.Substring(userIDLenght - 10, 10);
        }

        if (remoteUserIDLength < 10)
        {
//            Debug.Log(remoteUserIDLength);
            appProjectInfo.remoteUserID = appProjectInfo.remoteUserID.PadLeft(10,'0');
        }

        if (userIDLenght < 10)
        {
//            Debug.Log(remoteUserIDLength);
            appProjectInfo.userID = appProjectInfo.userID.PadLeft(10,'0');
        }

        appProjectInfo.userID=appProjectInfo.userID.Remove(0, 1).PadLeft(10,'0');

        if (appProjectInfo.userType == "1")
        {
            remoteManger.runAtType = RemoteManger.RunAtType.Master;
            appProjectInfo.remoteUserID = appProjectInfo.userID.Substring(4, 6).PadLeft(10, '0');
        }
        else if (appProjectInfo.userType == "2")
        {
            remoteManger.runAtType = RemoteManger.RunAtType.Slave;
            appProjectInfo.remoteUserID = appProjectInfo.remoteUserID.Substring(4, 6).PadLeft(10, '0');
        }
        remoteManger.userID = appProjectInfo.userID;
        remoteManger.remoteID = appProjectInfo.remoteUserID;


        serverProjectInfo.LoadServerProjectInfo(appProjectInfo.dataServer, appProjectInfo.dataServer,appProjectInfo.projectID, appProjectInfo.sceneLoadMode);
    }

    void Unload()
    {
        GlobalDebug.Addline("APP2Unity Unload");
        Debug.Log("APP2Unity Unload");
        unload.LoadUnloadScene();
    }

    void VedioButton(string state)
    {
        GlobalDebug.Addline("APP2Unity VedioButton "+state);
        Debug.Log("APP2Unity VedioButton "+state);

        if (state == "0")
        {
            remoteGUI.vedio_TrrigerFromApp.AlphaPlayBackward();
        }
        else if (state=="1")
        {
            remoteGUI.vedio_TrrigerFromApp.AlphaPlayForward();
        }

    }

    void Landscape(string musicState)
    {
        GlobalDebug.Addline("APP2Unity Landscape");
        Debug.Log("APP2Unity Landscape");
        defaultGUI.Landscape(musicState,true);

    }

    void Portrait()
    {
        GlobalDebug.Addline("APP2Unity Portrait");
        Debug.Log("APP2Unity Portrait");
        defaultGUI.Portrait();
    }

    void ChangeUIOrientation(string orientation)
    {

        if (orientation == "0")
        {
            defaultGUI.Portrait(false);
        }
        else if (orientation == "1")
        {
            defaultGUI.Landscape(defaultGUI.currentMusicState, false);
        }

        GlobalDebug.Addline("APP2Unity ChangeUI");
        Debug.Log("APP2Unity ChangeUI");
    }

    void CloseRemote(string state)
    {
//        GlobalDebug.Addline("APP2Unity CloseRemote ,state:"+state);
//        Debug.Log("APP2Unity CloseRemote ,state:" + state);
//        remoteGUI.AppCloseOnLineTalk();
    }

    void Home()
    {
        GlobalDebug.Addline("APP2Unity Home");
        Debug.Log("APP2Unity Home");

        defaultGUI.mainBtnGroup.GetComponent<ButtonGroup>().ResetAllButton();
    }

    void Intro()
    {
        GlobalDebug.Addline("APP2Unity Intro");
        Debug.Log("APP2Unity Intro");
        defaultGUI.mainBtnGroup.GetComponent<ButtonGroup>().imageButtonGroup[1].SetBtnState(true,0);
    }

    void Supports()
    {
        GlobalDebug.Addline("APP2Unity Supports");
        Debug.Log("APP2Unity Supports");
        defaultGUI.mainBtnGroup.GetComponent<ButtonGroup>().imageButtonGroup[2].SetBtnState(true, 0);
    }

    void Roads()
    {
        GlobalDebug.Addline("APP2Unity Roads");
        Debug.Log("APP2Unity Roads");
        defaultGUI.mainBtnGroup.GetComponent<ButtonGroup>().imageButtonGroup[3].SetBtnState(true, 0);
    }

    /// <summary>
    /// 户型分布
    /// </summary>
    /// <param name="roomName"></param>
    void RoomType(string roomName)
    {
        GlobalDebug.Addline("APP2Unity RoomType "+roomName);
        Debug.Log("APP2Unity RoomType " + roomName);
        if (roomName == "")
        {
            defaultGUI.mainBtnGroup.GetComponent<ButtonGroup>().imageButtonGroup[4].SetBtnState(true, 0);
        }
        else
        {
            hxGUI.ChooseHuXingType(roomName);
        }

    }

    void OpenRoomType(string roomName)
    {
        GlobalDebug.Addline("APP2Unity OpenRoomType " + roomName);
        Debug.Log("APP2Unity OpenRoomType " + roomName);

        hxGUI.OnlyEnterHXNK(roomName);
    }

    void CloseRoomType()
    {
        GlobalDebug.Addline("APP2Unity CloseRoomType");
        Debug.Log("APP2Unity CloseRoomType");

        hxGUI.ExitHXNK();
    }

    void ExitMYInPortrait()
    {
        GlobalDebug.Addline("APP2Unity ExitMYInPortrait");
        Debug.Log("APP2Unity ExitMYInPortrait");
        //执行back操作,
        backAction.Back();
    }

    void SetSatatusBar(string state)
    {

#if UNITY_ANDROID

        if (state == "1")
        {
            ApplicationChrome.statusBarState = ApplicationChrome.States.Tran;
            ApplicationChrome.statusBarColor = 0xff000000;
            ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;
        }
        else if(state=="0")
        {
            ApplicationChrome.statusBarState = ApplicationChrome.States.Hidden;
            ApplicationChrome.statusBarColor = 0xff000000;
            ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        }

        Debug.Log("SetSatatusBar:"+ state);
        GlobalDebug.Addline("SetSatatusBar:"+ state);

#endif

    }

    void SetNavigationBar(string state)
    {

#if UNITY_ANDROID

        if (state == "1")
        {
            ApplicationChrome.navigationBarState = ApplicationChrome.States.Visible;

        }
        else if (state == "0")
        {
            ApplicationChrome.navigationBarState = ApplicationChrome.States.Hidden;
        }

#endif

    }

    void DisplayCancelBtn()
    {
        defaultGUI.triggerCancelBtn.AlphaPlayForward();
    }

    public void SetSatatusBar2(string state)
    {
        SetSatatusBar(state);
    }

    public void SetNavigationBar2(string state)
    {
        SetNavigationBar(state);
    }

    public void EnterHX_Test(InputField projectid)
    {
        OpenRoomType(projectid.text);
    }

    public void ExitHX_Test()
    {
        CloseRoomType();
    }

    [System.Serializable]
    public class AppProjectInfo
    {
        public string projectID;
        public string sceneLoadMode;
        public string projectName;
        public string dataServer;
//      public string mobileNetworkDownload;
        public string userID;
        public string remoteUserID;
        public string remoteUserHeadUrl;
        public string userType;
//      ublic string isRemote;
        public string remoteServer;
        //        public string portraitWidth;
        //        public string portraitHeight;
        //        public string debug;
        public string checkKey;
    }

    //App2Unity
    //Load(string info)
    //Unload()    卸载当前Unity资源，并释放资源
    //Landscape() 横屏
    //Portrait()  竖屏
    //CloseRemote(string state)   结束讲盘	"state参数：0->正常退出,1->本方掉线，2->远端掉线，3->电话异常主动退出，4->电话异常被动退出，5->正常被动退出"
    //Home()  主页状态
    //Intro() 简介
    //Supports()  配套
    //Roads() 交通
    //RoomType(string roomName)   户型分布 当名称为空时只激活按钮
    //OpenRoomType(string roomName)   进入3D户型
    //CloseRoomType（）	退出户型

    //Unity2App
    //unityCloseRemote(string state)  结束讲盘 state参数：0->正常退出,1->本方掉线，2->远端掉线，3->电话异常主动退出，4->电话异常被动退出，5->正常被动退出
    //unityShareRoom(string roomID)   分享户型(任一层的modeid, 使用第一层的id)
    //unityOpenRoomType(string roomID)    进入户型
    //unityOpenRoomTypeDone() 打开户型完毕
    //unityBackRoomTypeDone() 户型返回完毕(户型到大场景)
    //unityReady()    unity主场景已准备完毕
    //unityProtrait() 竖屏回调,确认ok
    //unityLandscape()    横屏时的回调,确认ok
    //unityUnloadDone()   卸载已加载项目并清空内存后的回调,清除当前scene后
    //unityLoadDone() 加载项目完毕后的回调,Load(string info)的OK
    //unityToPanorama(string url)切换到外部全景图模式

}
