using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


public class RemoteManger : MonoBehaviour
{
    public SceneInteractiveManger sceneInteractiveManger;
    public RemoteGUI remoteGUI;
//  public ImageButton iBtn;
//  public RemoteGather.RemoteMessage mess;

    public enum RunAtType
    {
        //未启用远程控制
        None = 0,
        //控制者
        Master = 1,
        //被控制者
        Slave = 2
    }

    public RunAtType runAtType;
    public string remoteID = "0000200001";
    public string userID = "0000200001";
    string roomID;

    public bool isEnterRoom;
    public bool isOtherSideOnline;
    public bool lastIsOtherSideOnline;
    //距离上次收到消息已过去的时间
    public float lastMessageReceiveFromOtherSidePastTime;
    public bool waitingAck;
    public float waitingAckPastTime;

    const float maxTimeWaitAck = 3;
    const int maxRetryCount = 3;
    int currentRetryCount = 0;

    MFQTcpClient mfqTcpClient;

    public static bool isUseRemoteGather=false;

    float[] cameraStateWhenSendMessage;
    float[] currentCameraState;

    float[] scaleImageWhenSendMessage;
    float[] currentScaleImageState;



    void Start()
    {
        isEnterRoom = false;
        isOtherSideOnline = false;
        lastIsOtherSideOnline = false;

//      Debug.Log(isEnterRoom);
//      Debug.Log(isOtherSideOnline);
//      Debug.Log(lastIsOtherSideOnline);
    }

    public void StartOnlineTalk()
    {
        StartMFQTcpClient(runAtType, remoteID, userID);
    }

    void OnDisable()
    {
        DisConnect();
    }

    void Update()
    {

        if (mfqTcpClient != null && mfqTcpClient.hasInit)
        {

            GlobalDebug.ReplaceLine("NeedProcessEvents: " + RemoteGather.needProcessMessages.Count.ToString(), 15);
            GlobalDebug.ReplaceLine("NeedSendEvents: " + RemoteGather.needSendBtnCtrlMessages.Count.ToString(), 16);

            GlobalDebug.ReplaceLine("IsEnterRoom: " + isEnterRoom.ToString(), 17);
            GlobalDebug.ReplaceLine("IsOtherSideOnline: " + isOtherSideOnline.ToString(), 18);
            GlobalDebug.ReplaceLine("waitingAck: " + waitingAck.ToString(), 19);

            if (sceneInteractiveManger.currentActiveSenceInteractiveInfo != null)
            {
                RemoteGather.currentCameraUniversal = sceneInteractiveManger.currentActiveSenceInteractiveInfo.cameraUniversalCenter.currentCamera;
            }

            //等待Ack
            if (waitingAck)
            {
                waitingAckPastTime += Time.deltaTime;

                if (waitingAckPastTime > maxTimeWaitAck)
                {
                    if (!isEnterRoom)
                    {
                        if (currentRetryCount < maxRetryCount)
                        {
                            string log = "重试进入房间次数:" + currentRetryCount;
                            GlobalDebug.Addline(log);
                            Debug.Log(log);

                            waitingAckPastTime = 0;
                            waitingAck = false;
                            currentRetryCount = 0;

                            DisConnect();
                            StartMFQTcpClient(runAtType, remoteID, userID);
                            EnterRoom();
                        }
                        else
                        {
                            string log = "重试进入房间达到最大次数,请检查网络连接!";
                            GlobalDebug.Addline(log);
                            Debug.Log(log);
                            Debug.LogWarning(log);
                            Debug.LogError(log);

                            waitingAckPastTime = 0;
                            waitingAck = false;
                            currentRetryCount++;

                            //网络连接断开,这是由于网络造成的被动断开
                            remoteGUI.FinishOnLineTalk("网络连接失败");

                        }

                    }//以下是操作传输重试
                    else 
                    {
                        if (currentRetryCount < maxRetryCount)
                        {
                            waitingAckPastTime = 0;
                            waitingAck = false;
                            currentRetryCount++;

                            string log = "重试发送信息次数:" + currentRetryCount;
                            GlobalDebug.Addline(log);
                            Debug.Log(log);
                        }
                        else
                        {
                            string log = "重试发送信息达到最大次数,准备重新连接讲盘网络!";
//                          string log = "重试发送信息达到最大次数,结束讲盘!";
                            GlobalDebug.Addline(log);
                            Debug.Log(log);
                            Debug.LogWarning(log);
                            Debug.LogError(log);

                            waitingAck = false;
                            waitingAckPastTime = 0;
                            currentRetryCount = 0;

                            //重试达到最大数还没成功就断开房间，重新连接
                            ReEnterRoom();


                        }
                    }
                }
            }
            else if (isEnterRoom)
            {
                if (isOtherSideOnline)
                {
                    lastMessageReceiveFromOtherSidePastTime += Time.deltaTime;

                    //判断5秒内有没有收到对方消息,发送StillOnline信息确定对方是否在线，由控制方发送,
                    if (runAtType==RunAtType.Master&& lastMessageReceiveFromOtherSidePastTime > 5)
                    {
                        //如果没有收到对方的应答，会以每3秒重试3次,9+5=14秒。超过14秒将会执行断开。
                        SendCtrlMessage(new RemoteGather.RemoteMessage(98).GetBytesData());
                    }

                    //判断14秒有没有收到Master消息,如果没有收到消息,判定掉线,执行断开。
                    if (runAtType == RunAtType.Slave && lastMessageReceiveFromOtherSidePastTime > 14)
                    {
                        ReEnterRoom();
//                      string logStr = "超时 Close InvokeFrom: 被操作者";
                    }
                        
                }


                if (RemoteGather.needProcessMessages.Count > 0)
                {
                    ProcessMessage(RemoteGather.needProcessMessages[0]);

                } //判断对方是否在线,由slave每2秒进行发送验证消息,Master接收后确认Slave在线.Master开始发送当前状态.
                else if (!isOtherSideOnline)
                {
                    if (runAtType == RunAtType.Slave)
                    {

                        SendCtrlMessage(new RemoteGather.RemoteMessage(51).GetBytesData());
                      //设置成无限尝试
                        currentRetryCount = 0;
                      //起始时间设置为3秒,这样两秒后就能重试
                        waitingAckPastTime = 1;

                    }

                }//下面的if暂时没有使用上，needSendAckMessages未使用
                else if (isOtherSideOnline && RemoteGather.needSendAckMessages.Count > 0)
                {
                    SendCtrlMessage(RemoteGather.needSendAckMessages[0].GetBytesData(), false);
                    RemoteGather.needSendAckMessages.RemoveAt(0);
                }
                else if (isOtherSideOnline &&runAtType==RunAtType.Master&&RemoteGather.needSendBtnCtrlMessages.Count > 0)
                {
                    SendCtrlMessage(RemoteGather.needSendBtnCtrlMessages[0].GetBytesData());
                }
                else if (isOtherSideOnline && NeedSendCameraState())
                {
                    //目前的问题:在等待CameraState Ack的时候会收到对方的其它类型的信息,以至于没有收到CameraState Ack,一直处于等待Ack的状态.
                    //本想实现,master和slave相互之间的操作,有问题,先实现Master控制Slave
                    //
                    cameraStateWhenSendMessage = RemoteGather.currentCameraUniversal.GetCameraState();
                    SendCtrlMessage(new RemoteGather.RemoteMessage(49, "", false, RemoteGather.currentCameraUniversal).GetBytesData());
                }
                else if (isOtherSideOnline && NeedSendScaleImageState())
                {
                    scaleImageWhenSendMessage = RemoteGather.currentCtrlScaleImage.GetState();
                    SendCtrlMessage(new RemoteGather.RemoteMessage(52, RemoteGather.currentCtrlScaleImage.btnNameForRemote, false, null, RemoteGather.currentCtrlScaleImage).GetBytesData());
                }
            }
          }
    }

    bool NeedSendCameraState()
    {
     
        if (RemoteGather.currentCameraUniversal == null)
        {
            return false;
        }

        currentCameraState = RemoteGather.currentCameraUniversal.GetCameraState();

        if (RemoteGather.currentCameraUniversal.cameraEnableState&&runAtType == RunAtType.Master)
        {
            for (int i = 0; i < 6; i++)
            {
                if (Mathf.Abs(currentCameraState[i] - RemoteGather.lastSendCameraCameraState[i]) > 0.02f)
                {
//                  Debug.Log(currentCameraState[i] + "<>" + RemoteGather.lastSendCameraCameraState[i]);
                    return true;
                }
            }
        }
        return false;
    }

    bool NeedSendScaleImageState()
    {
        if (RemoteGather.currentCtrlScaleImage == null)
            return false;

        currentScaleImageState = RemoteGather.currentCtrlScaleImage.GetState();

        if (runAtType == RunAtType.Master)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Mathf.Abs(currentScaleImageState[i] - RemoteGather.lastSendScaleImageState[i]) > 0.02f)
                    return true;
            }
        }
        return false;
    }

    IEnumerator WaitingReceiveIE()
    {
        while (true)
        {
            if ( mfqTcpClient != null && mfqTcpClient.hasInit&& (waitingAck || isEnterRoom) && mfqTcpClient.bytesStream.DataAvailable)
            {
//              Debug.Log("Receive");
//              GlobalDebug.Addline("Receive");
                mfqTcpClient.ReadBytes();
            }
            //每秒40帧的接收
            yield return new WaitForSeconds(0.025f);

            if (mfqTcpClient != null&&!mfqTcpClient.hasInit&&!mfqTcpClient.isConnecting)
            {
                mfqTcpClient.StartConnection();
            }

        }
    }

    public void StartMFQTcpClient(RunAtType runAtType, string remoteID, string inUserID)
    {
        string log = "开始连接讲盘服务器,请等待";
        GlobalDebug.Addline(log);
        Debug.Log(log);
        remoteGUI.DisplayHelpInfo();
        remoteGUI.SetHelpInfoString(log);

        if (runAtType == RunAtType.None)
        {
            return;
        }
        else if (runAtType == RunAtType.Master)
        {
            roomID = remoteID;
        }
        else if (runAtType == RunAtType.Slave)
        {
            roomID = remoteID;
        }

        userID = inUserID;

//      sendOnlinePastTime = 0;

        StartCoroutine(WaitingReceiveIE());
        RemoteGather.SetupRemoteGather();
        mfqTcpClient = new MFQTcpClient(this);

    }

    //断开网络连接,主动断开
    public void DisConnect()
    {
        StopAllCoroutines();

        isEnterRoom = false;
        isOtherSideOnline = false;
//      lastIsOtherSideOnline = false;
//      isUseRemoteGather = false;

//        if(RemoteGather.needSendBtnCtrlMessages!=null)
//        RemoteGather.needSendBtnCtrlMessages.Clear();

        if (mfqTcpClient != null)
        {
                mfqTcpClient.Close();
                mfqTcpClient = null;

                string log = "断开TCP连接";
                GlobalDebug.Addline(log);
                Debug.Log(log);
                Debug.LogWarning(log);
        }

    }


    public void ReEnterRoom()
    {
        remoteGUI.ReConnectGUI();
        DisConnect();
        StartCoroutine(ReEnterRoom_IE());
    }

    IEnumerator ReEnterRoom_IE()
    {
        yield return new WaitForSeconds(0.5f);
        StartMFQTcpClient(runAtType, remoteID, userID);
        EnterRoom();
    }


    public void EnterRoom()
    {
        if (!waitingAck && !isEnterRoom && mfqTcpClient != null && mfqTcpClient.hasInit)
        {
            Debug.Log("Start EnterRoom");
            waitingAck = true;
            waitingAckPastTime = 0;
            mfqTcpClient.WriteBytes("0000241002" + roomID + userID);
        }
    }

    public void ExitRoomMessageToServer()
    {
        if (mfqTcpClient != null && mfqTcpClient.hasInit)
        {
            string log = "向服务器发送退出："+ "0000241003" + roomID + userID;
            GlobalDebug.Addline(log);
            Debug.Log(log);
            Debug.LogWarning(log);      
            mfqTcpClient.WriteBytes("0000241003" + roomID + userID);
        }    
    }

    public void SendCtrlMessage(byte[] byteData,bool needWaitAck=true)
    {
        if (!waitingAck&&isEnterRoom && mfqTcpClient != null && mfqTcpClient.hasInit)
        {
            waitingAck = needWaitAck;
            waitingAckPastTime = 0;

            List<byte> message = new List<byte>();
            message.AddRange(Encoding.UTF8.GetBytes("0000981004" + roomID + userID));
            message.AddRange(byteData);

            string log="";
            switch (byteData[0])
            {
                case 49:
                    log = "CameraStateMessage";
                    break;
                case 50:
                    log = "BtnCtrlMessage";
                    break;
                case 51:
                    log = "AreYouReady";
                    break;
                case 52:
                    log = "ScaleImage";
                    break;


                case 72:
                    log = "CameraStateMessage Ack";
                    break;
                case 73:
                    log = "BtnCtrlMessage Ack";
                    break;
                case 74:
                    log = "AreYouReady Ack";
                    break;
                case 75:
                    log = "ScaleImage Ack";
                    break;

                case 98:
                    log = "Still Online";
                    break;
                case 99:
                    log = "Still Online Ack";
                    break;
				 
			    case 252:
				    log = "Exit OnlineTalk";
				break;

				case 253:
				    log = "Exit OnlineTalk Ack";
				break;
				
                case 255:
                    log = "Null Head Info Ack";
                    break;
                default:
                    break;
            }

            //CameraStateMessage:/49
            //BtnCtrlMessage:/50
            //AreYouReady:/51 对方是否在

            //ackMessage:72,CameraStateMessage ack//相机
            //ackMessage:73,BtnCtrlMessage ack //按钮操作完成回执
            //ackMessage:74,otherOnline ack//对方在线回执

            Debug.Log("SendMessageType:"+ log);

            mfqTcpClient.WriteBytes(message.ToArray());
        }
    }

    public void ReceivedProcess(byte[] receivedBytes)
    {
        string log = "receivedBytes的长度"+ receivedBytes.Length;
        GlobalDebug.Addline(log);
        Debug.Log(log);

        byte[] headInfo = new byte[0];
        byte[] ctrlData=new byte[0];

        List<byte> bytesList = new List<byte>();
        bytesList.AddRange(receivedBytes);

        headInfo = bytesList.GetRange(6, 8).ToArray();

//        Debug.Log(Encoding.UTF8.GetString(receivedBytes,0,receivedBytes.Length));

        if (receivedBytes.Length < 14)
        {
            log = "receivedBytes的长度不足,检查是否丢包";
            GlobalDebug.Addline(log);
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
            waitingAck = false;

            return;
        }
        else if(receivedBytes.Length == 14)
        {
            foreach (byte b in receivedBytes)
            {
//                Debug.Log(b);
            }
        }
        else if (receivedBytes.Length == 104)
        {
            ctrlData = bytesList.GetRange(30, 74).ToArray();
//          Debug.Log("length92");
        }
        else
        {

        }

        CopyMessage(headInfo, ctrlData);
       
    }

    void CopyMessage(byte[] inHead,byte[] ctrlData)
    {
      
        string front = Encoding.UTF8.GetString(inHead, 0, 4);
        string back = Encoding.UTF8.GetString(inHead, 4, 4);
        Debug.Log("Receive Head:"+front + ":" + back);
        GlobalDebug.Addline("Receive Head:" + front + ":" + back);
        //处理成功
        if (front == "9000")
        {
            switch (back)
            {
                case "1002":
                    isEnterRoom = true;
                    Debug.Log("成功连接服务器,等待对方上线");
                    GlobalDebug.Addline("成功连接服务器,等待对方上线");
                    remoteGUI.SetHelpInfoString("成功连接服务器,等待对方上线");

                    isUseRemoteGather = true;
                    waitingAck = false;
                    currentRetryCount = 0;
                    break;
                default:
                    break;
            }

        }//处理失败
        else if (front == "9001")
        {
            switch (back)
            {
                case "1002":
                    isEnterRoom = true;
                    Debug.Log("进入房间失败");
                    GlobalDebug.Addline("进入房间失败");
                    waitingAck = false;
                    currentRetryCount = 0;
                    break;
                default:
                    break;
            }

        }//收到信息
        else if (front == "1004")
        {
            lastMessageReceiveFromOtherSidePastTime = 0;

            RemoteGather.RemoteMessage rMessage = new RemoteGather.RemoteMessage();
            if (ctrlData.Length == 74)
            {
                rMessage = new RemoteGather.RemoteMessage(ctrlData);
            }
            else
            {

            }

            switch (back)
            {
                
                case "0000":
                    string log;

                    if (rMessage != null)
                    {
//                        Debug.Log(rMessage.messageType);

                        switch (rMessage.messageType)
                        {

                            //不能在这里处理，因为该程序不在主线程下运行,将消息传递至下方处理
                            case 49:
                            case 50:
                            case 51:
                            case 52:
                            case 98:
							case 252:
                            case 253:
                                RemoteGather.needProcessMessages.Add(rMessage);
                                break;

                            //Receive CameraStateMessage Ack
                            case 72:

                                log = "Receive CameraStateMessage Ack";
                                Debug.Log(log);
                                GlobalDebug.Addline(log,true);

                                RemoteGather.lastSendCameraCameraState = cameraStateWhenSendMessage;

                                waitingAck = false;
                                currentRetryCount = 0;
                                break;

                            //Receive BtnCtrlMessage Ack
                            case 73:

                                log = "Receive BtnCtrlMessage Ack";
                                Debug.Log(log);
                                GlobalDebug.Addline(log, true);

                                RemoteGather.needSendBtnCtrlMessages.RemoveAt(0);
                                waitingAck = false;
                                currentRetryCount = 0;
                                break;

                            //Receive AreYouReady Ack
                            case 74:
                                RemoteGather.needProcessMessages.Add(rMessage);
                                waitingAck = false;

                                break;
                            //Receive ScaleImage Ack
                            case 75:

                                log = "Receive ScaleImage Ack";
                                Debug.Log(log);
                                GlobalDebug.Addline(log, true);

                                RemoteGather.lastSendScaleImageState = scaleImageWhenSendMessage;

                                waitingAck = false;
                                currentRetryCount = 0;
                                break;

                            //Receive Still Onlinr Ack
                            case 99:

                                log = "Receive Still Online Ack";
                                Debug.Log(log);
                                GlobalDebug.Addline(log, true);

                                waitingAck = false;
                                currentRetryCount = 0;
                                break;


                            case 200:

                                log = "Receive 200 OK Ack";
                                Debug.Log(log);
                                GlobalDebug.Addline(log);

                                //接到空消息
                                waitingAck = false;
                                currentRetryCount = 0;

                                break;

                            //收到 未知报头 Ack
                            case 255:
                                log = "Receive 未知报头 Ack";
                                Debug.Log(log);
                                GlobalDebug.Addline(log);

                                //接到空消息
                                waitingAck = false;
                                currentRetryCount = 0;

                                break;

                        }

                    }
                    else
                    {
                        //接到空消息
                        waitingAck = false;
                    }

                    break;

                default:
                break;
            }
		}//服务器断开连接,需要重新进入房间
        else if (front == "1006")
        {
            string log = "Receive 1006 Head 服务器断开连接";
            Debug.Log(log);
            GlobalDebug.Addline(log);
            RemoteGather.needProcessMessages.Add(new RemoteGather.RemoteMessage(106));
        }
        else
        {   
            string log = "front 未知报头:" + front;
            GlobalDebug.Addline(log);
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
            RemoteGather.needProcessMessages.Add(new RemoteGather.RemoteMessage(254));

        }


    }

    void ProcessMessage(RemoteGather.RemoteMessage p)
    {
        switch (p.messageType)
        {
            //CameraStateMessage
            case 49:

                string log = "Process CameraStateMessage";
                Debug.Log(log);
                GlobalDebug.Addline(log,true);
                /*
                foreach (float f in p.cameraStates)
                {
                    Debug.Log(f);
                }
                */
                RemoteGather.currentCameraUniversal.SetCameraPositionAndXYZCount(p.cameraStates);
                RemoteGather.needProcessMessages.Remove(p);
                //Send Ack
                SendCtrlMessage(new RemoteGather.RemoteMessage(72).GetBytesData(), false);
//              RemoteGather.needSendAckMessages.Add(new RemoteGather.RemoteMessage(72));

                break;

            //BtnCtrlMessage
            case 50:
                string log2 = "Process BtnCtrlMessage";
                Debug.Log(log2);
                GlobalDebug.Addline(log2,true);

                RemoteGather.ProcessRemoteMessage(p.btnName, p.btnState == 65);
                RemoteGather.needProcessMessages.Remove(p);

                log2 = "Btn事件还有:" + RemoteGather.needSendBtnCtrlMessages.Count;
                Debug.Log(log2);
                GlobalDebug.Addline(log2);

                //Send Ack
                SendCtrlMessage(new RemoteGather.RemoteMessage(73).GetBytesData(), false);
//              RemoteGather.needSendAckMessages.Add(new RemoteGather.RemoteMessage(73));

                break;

            //AreYouReadyMessage
            case 51:
                string log3 = "Process AreYouReady Message";
                Debug.Log(log3);
                GlobalDebug.Addline(log3, true);

                RemoteGather.needProcessMessages.Remove(p);

                //Send Ack
                SendCtrlMessage(new RemoteGather.RemoteMessage(74).GetBytesData(), false);
                // RemoteGather.needSendAckMessages.Add(new RemoteGather.RemoteMessage(74));

                remoteGUI.SucessConnectGUI();

                break;

            //ScaleImageMessage
            case 52:
                string log4 = "Process ScaleImage Message";
                Debug.Log(log4);
                GlobalDebug.Addline(log4, true);

//              Debug.Log(p.btnName);

                RemoteGather.allScaleImage[p.btnName].SetState(p.scaleImageStates);
                RemoteGather.needProcessMessages.Remove(p);

                //Send Ack
                SendCtrlMessage(new RemoteGather.RemoteMessage(75).GetBytesData(), false);
                //                RemoteGather.needSendAckMessages.Add(new RemoteGather.RemoteMessage(74));
                break;

            case 74:
                string log74 = "Process AreYouReady Ack";
                Debug.Log(log74);
                GlobalDebug.Addline(log74, true);
                RemoteGather.needProcessMessages.Remove(p);


                currentRetryCount = 0;
                remoteGUI.SucessConnectGUI();
                break;

            //StillOnlineMessage
            case 98:

                string log5 = "Process StillOnlineMessage";
                Debug.Log(log5);
                GlobalDebug.Addline(log5, true);
                RemoteGather.needProcessMessages.Remove(p);

                //Send Ack
                SendCtrlMessage(new RemoteGather.RemoteMessage(99).GetBytesData(), false);
                break;


			//服务器断线    
            case 106:
                string log6 = "Receive 服务器断线,尝试重进房间";
                Debug.Log(log6);
                GlobalDebug.Addline(log6);
                RemoteGather.needProcessMessages.Remove(p);
                //服务器断线得重新再进房间
                ReEnterRoom();

                //remoteGUI.OKCloseOnLineTalk("服务器断线");

                break;

			//退出讲盘    
			case 252:
			string log7 = "Receive 退出讲盘";
			Debug.Log(log7);
			GlobalDebug.Addline(log7);
			RemoteGather.needProcessMessages.Remove(p);

            SendCtrlMessage(new RemoteGather.RemoteMessage(253).GetBytesData(),true);
            remoteGUI.FinishOnLineTalk("退出讲盘");

            break;

            //退出讲盘Ack
            case 253:
                string log253 = "Receive 退出讲盘 Ack";
                Debug.Log(log253);
                GlobalDebug.Addline(log253);
                RemoteGather.needProcessMessages.Remove(p);

//              SendCtrlMessage(new RemoteGather.RemoteMessage(253).GetBytesData(), false);
                ExitRoomMessageToServer();
                remoteGUI.FinishOnLineTalk("退出讲盘 Ack");

            //ExitRoom();
                break;

            //收到未知报头,给对方发送收到未知报头
            case 254:
                string log8 = "Receive 收到未知报头";
                Debug.Log(log8);
                GlobalDebug.Addline(log8, true);
                RemoteGather.needProcessMessages.Remove(p);
                
                //收到一个未知报头，也得给对方返回有一个消息，表示己方已收到消息但是不知道是什么消息，请重新发送最后一次消息 未知报头Ack
                SendCtrlMessage(new RemoteGather.RemoteMessage(255).GetBytesData(), false);

                break;


            default:
            break;
        }

    }

}
