using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;

public static class RemoteGather 
{
    //One:SendMessageWaitAck->One:WaitAckMessage
    //Two:WaitReceiveMessage->Two:ProcessMessage->Two:SendAckMessage
    //One:SendMessage->Two:ReceiveMessage

    public static Dictionary<string,ImageButton> allImageButton;
    public static Dictionary<string,ScaleImage> allScaleImage;

    public static ScaleImage currentCtrlScaleImage;
    //上次发送的缩放图片大小posX,posY,Scale
    public static float[] lastSendScaleImageState=new float[3];

    public static CameraUniversal currentCameraUniversal;
    public static float[] lastSendCameraCameraState = new float[6];

    public static List<RemoteMessage> needSendAckMessages;
    public static List<RemoteMessage> needSendBtnCtrlMessages;

    public static List<RemoteMessage> needProcessMessages;

    public static bool hasInit = false;

    public static void SetupRemoteGather()
    {
        if (!hasInit)
        {
            allImageButton = new Dictionary<string, ImageButton>();
            allScaleImage = new Dictionary<string, ScaleImage>();
            needSendAckMessages = new List<RemoteMessage>();
            needSendBtnCtrlMessages = new List<RemoteMessage>();
            needProcessMessages = new List<RemoteMessage>();
            currentCtrlScaleImage = null;
            currentCameraUniversal = null;
            RemoteMessage.globalID = 0;
            hasInit = true;
            Debug.Log("RemoteGatherInit");
            GlobalDebug.Addline("RemoteGatherInit");
        }
    }

    public static void AddImageToGroup(ImageButton iBtn,bool replace=false)
    {
        if (iBtn.btnNameForRemote != ""&& iBtn.btnNameForRemote.Length<33)
        {
            if (!allImageButton.ContainsValue(iBtn))
            {
                if (replace)
                {
                    if (allImageButton.ContainsKey(iBtn.btnNameForRemote))
                    {
                        allImageButton.Remove(iBtn.btnNameForRemote);
                    }
                }

                allImageButton.Add(iBtn.btnNameForRemote, iBtn);
//                string log = "GatherImageBtnCount:" + allImageButton.Count;
//                Debug.Log(log);
//                GlobalDebug.Addline(log);
            }
            
        }
    }

    public static void AddSacleImageToGroup(ScaleImage iBtn)
    {
        if (iBtn.btnNameForRemote != "" && iBtn.btnNameForRemote.Length < 33)
        {
            if (!allScaleImage.ContainsKey(iBtn.btnNameForRemote))
            {
                allScaleImage.Add(iBtn.btnNameForRemote, iBtn);
//              string log = "GatherScaleImageCount:" + allScaleImage.Count;
//              Debug.Log(log);
//              GlobalDebug.Addline(log);
            }
        }
    }

    public static void AddBtnCtrlMessages(string iBtnName,bool btnState)
    {
        if (iBtnName != "" && iBtnName.Length < 33)
        {
            if(needSendBtnCtrlMessages!=null)
            needSendBtnCtrlMessages.Add(new RemoteMessage(50, iBtnName, btnState));
        }
    }

    public static void ProcessRemoteMessage(string inBtnName,bool btnState)
    {
        if (allImageButton.ContainsKey(inBtnName))
        {
            allImageButton[inBtnName].SetBtnState(btnState, 0);
        }
        else
        {
            string log = "找不到" +inBtnName+"按钮";
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
            GlobalDebug.Addline(log);
        }

    }


    [System.Serializable]
    public class RemoteMessage
    {
        //Ascii码
        //对方退出/106

        //CameraStateMessage:/49
        //BtnCtrlMessage:/50
        //AreYouReady:/51 对方是否在
        //ScaleImage:/52

        //ackMessage:72,CameraStateMessage ack//相机
        //ackMessage:73,BtnCtrlMessage ack //按钮操作完成回执
        //ackMessage:74,otherOnline ack//对方在线回执
        //ackMessage:75,scaleImage ack//

        public byte messageType;

        //定长4byte
        public int id = 0;

        //定长32byte
        public string btnName;

        //定长1byte 0/64 false 1/65 true
        public byte btnState; 

        //定长4byte *6   posX,Y,Z countX,Y,Z
        public float[] cameraStates;
        //定长4byte*3
        public float[] scaleImageStates;

        List<byte> byteData;
        public static int globalID = 0;


        public RemoteMessage()
        {
            //空信息
            messageType = 200;
        }



        public RemoteMessage(byte[] receiveMessage)
        {

            byteData = new List<byte>();
            byteData.AddRange(receiveMessage);
            messageType = byteData[0];
            id = BitConverter.ToInt32(receiveMessage, 1);
            btnName = Encoding.UTF8.GetString(receiveMessage, 5, 32).Replace("^","");
            btnState= receiveMessage[37];
            cameraStates = new float[6];
            for (int i = 0; i < cameraStates.Length; i++)
            {
                cameraStates[i] = BitConverter.ToSingle(receiveMessage, 38 + 4 * i);
            }

            scaleImageStates = new float[3];
            for (int i = 0; i < scaleImageStates.Length; i++)
            {
                scaleImageStates[i] = BitConverter.ToSingle(receiveMessage, 62 + 4 * i);
            }

        }

        public RemoteMessage(int inMessageType,string inCtrlBtnName="",bool inBtnState=false,CameraUniversal c=null,ScaleImage s=null)
        {

            if (inCtrlBtnName.Length > 32)
            {
                string log = "按钮名字超过32个字符"+ inCtrlBtnName;
                GlobalDebug.Addline(log);
                Debug.Log(log);
                Debug.LogWarning(log);
                Debug.LogError(log);
            }

            id = globalID;

            messageType =(byte)inMessageType;
            btnName = inCtrlBtnName.PadLeft(32, '^');


            if (inBtnState)
            {
                btnState = 65;
            }
            else
            {
                btnState = 64;
            }


            if (c == null)
            {
                cameraStates = new float[] { 0, 0, 0, 0, 0, 0 };
            }
            else
            {
                cameraStates = c.GetCameraState();
            }

            if (s == null)
            {
                scaleImageStates = new float[] { 0, 0, 0 };
            }
            else
            {
                scaleImageStates = s.GetState();
            }


            GenBytes();

            globalID++;
        }

        void GenBytes()
        {
            byteData = new List<byte>();
            byteData.Add(messageType);
            byteData.AddRange(BitConverter.GetBytes(id));
            byteData.AddRange(Encoding.UTF8.GetBytes(btnName));
            byteData.Add(btnState);

            for (int i = 0; i < cameraStates.Length; i++)
            {
                byteData.AddRange(BitConverter.GetBytes(cameraStates[i]));
            }

            for (int i = 0; i < scaleImageStates.Length; i++)
            {
                byteData.AddRange(BitConverter.GetBytes(scaleImageStates[i]));
            }

        }

        public byte[] GetBytesData()
        {
            return byteData.ToArray();
        }


    }

}
