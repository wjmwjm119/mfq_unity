using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
    
public class MFQTcpClient 
{
    const string SERVERNAME = "hlf.meifangquan.com";
    const int PORT = 7891;          
    const int BUFFERLENGTH = 256;

    public string connectResult;

    public TcpClient tcpClient;
    public NetworkStream bytesStream;

    byte[] readBuffer;
//  byte[] writeBuffer;

    public bool hasInit;
    bool isWriting;
    bool isReading;

    RemoteManger remoteManger;

    public MFQTcpClient(RemoteManger r)
    {
        remoteManger = r;
        tcpClient = new TcpClient();

        tcpClient.SendBufferSize = BUFFERLENGTH;
        tcpClient.ReceiveBufferSize = BUFFERLENGTH;

        try
        {
            tcpClient.BeginConnect(SERVERNAME, PORT, connectCallback, null);
        }
        catch(Exception e)
        {
            Debug.LogError(e.Message);
            GlobalDebug.Addline(e.Message);
        }  
    }

    void connectCallback(IAsyncResult iAsyncResult)
    {
        if (tcpClient.Connected)
        {
            connectResult = "连接成功";
            hasInit = true;
            bytesStream = tcpClient.GetStream();
            bytesStream.WriteTimeout = 1;
            bytesStream.ReadTimeout = 1;
            //tcp连接成功直接进入房间
            remoteManger.EnterRoom();
        }
        else
        {
            connectResult = "连接失败";
            hasInit = false;
        }

        tcpClient.EndConnect(iAsyncResult);
    }

    /// <summary>
    /// 当前的服务器会处理收到的请求,如果请求没有被处理,服务器已有的readbuffer不会被清空.如果Client再多次发送将发生Write错误.这时服务上的readbuffer应该是溢出
    /// </summary>
    /// <param name="needSendString"></param>
    public void WriteBytes(string needSendString)
    {
//        Debug.Log(needSendString);
        byte[] sendBytes = Encoding.UTF8.GetBytes(needSendString);
        WriteBytes(sendBytes);
    }

    public void WriteBytes(byte[] needSendBytes)
    {
        try
        {
            isWriting = true;
            bytesStream.BeginWrite(needSendBytes, 0, needSendBytes.Length, WriteBufferCallBack, null);
        }
        catch(Exception e)
        {
            string log = e.Message;
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
            GlobalDebug.Addline(log);
        }
    }


    void WriteBufferCallBack(IAsyncResult iAsyncResult)
    {
        bytesStream.EndWrite(iAsyncResult);
        isWriting = false;
//        Debug.Log("WriteEnd");
        GlobalDebug.Addline("WriteEnd");
        if (remoteManger.waitingAck)
        {
//            Debug.Log("WaitingAck...");
            GlobalDebug.Addline("WaitingAck...");
        }
    }

    public void ReadBytes()
    {
        isReading = true;
        readBuffer = new byte[BUFFERLENGTH];
        bytesStream.BeginRead(readBuffer, 0, readBuffer.Length, ReadBufferCallBack, null);
    }

    void ReadBufferCallBack(IAsyncResult iAsyncResult)
    {
        int bytesCount=bytesStream.EndRead(iAsyncResult);

        byte[] tempBuffer = new byte[bytesCount];
        for (int i = 0; i < bytesCount; i++)
        {
            tempBuffer[i] = readBuffer[i];
        }
        remoteManger.ReceivedProcess(tempBuffer);
        isReading = false;
    }

    public void Close()
    {
        if (bytesStream !=null)
        bytesStream.Close();
        if(tcpClient!=null)
        tcpClient.Close();
    }




}
