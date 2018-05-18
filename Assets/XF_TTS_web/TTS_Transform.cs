using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using System.Text;
using System.Security.Cryptography;
using UnityEngine.Networking;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.Events;

public class TTS_Transform : MonoBehaviour
{
    public string ttsServerUrl = "http://api.xfyun.cn/v1/service/v1/tts";
    public string appID;
    public string apiKEY;

    //音频比特率：audio/L16;rate=8000  or   audio/L16;rate=16000
    public string auf = "audio/L16;rate=16000";
    //文件格式：raw,lame(mp3)
    public string aue = "lame";
    //发音人名:
    public string voice_name="xiaoyan";
    //发音语速0~100
    public string speed = "50";
    //音量0~100
    public string volume="50";
    //音高0~100
    public string pitch = "50";
    //语音引擎aisound（普通效果），intp65（中文），intp65_en（英文），mtts（小语种，需配合小语种发音人使用），x（优化效果），默认为inpt65
    public string engine_type = "intp65";
    //文本格式类型
    public string text_type = "text";

    public bool isTransforming;
    Coroutine ttsCoroutine;
    string outputPath;
    float hasPastTime;
    MD5 md5;
    string paramJson;
    string paramBase64;

    [Serializable]
    public class OnTransformEnd : UnityEvent{};
    public OnTransformEnd onTransformEnd;

    public void GenTTSmp3Files(string sceneName, List<TTSAction> actions)
    {
        paramJson = "{";
        paramJson += "\"aue\":\"" + aue + "\",";
        paramJson += "\"auf\":\"" + auf + "\",";
        paramJson += "\"voice_name\":\"" + voice_name + "\",";    
        paramJson += "\"speed\":\"" + speed + "\",";
        paramJson += "\"volume\":\"" + volume + "\",";
        paramJson += "\"pitch\":\"" + pitch + "\",";
        paramJson += "\"engine_type\":\"" + engine_type + "\",";
        paramJson += "\"text_type\":\"" + text_type + "\"}";
        
//     Debug.Log(paramJson);

        paramBase64 = Convert.ToBase64String(Encoding.ASCII.GetBytes(paramJson));
//     Debug.Log(paramBase64);

        md5 = MD5.Create();

#if UNITY_EDITOR
            outputPath = "Assets/StreamingAssets/TTSmp3Audio/" + sceneName;
#else
            outputPath = Directory.GetCurrentDirectory() + "/StreamingAssets/TTSmp3Audio/" +sceneName ;
#endif

        if (Directory.Exists(outputPath))
        {   
           //删除已有的文件
           Directory.Delete(outputPath,true);
        }
           Directory.CreateDirectory(outputPath);
        
        if (!isTransforming)
        {
            isTransforming = true;
            ttsCoroutine = StartCoroutine(GenTTSmp3FilesIE(actions));
        }
        else
        {
            Debug.Log("正在转换,请等待完成");
        }
    }

    IEnumerator GenTTSmp3FilesIE(List<TTSAction> actions)
    {
        yield return new WaitForEndOfFrame();
        
        string curTime = ((long)((DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds)).ToString();
//     Debug.Log(curTime);
//     Debug.Log(apiKEY + curTime + paramBase64);
        byte[] md5HashBytes =md5.ComputeHash(Encoding.ASCII.GetBytes(apiKEY + curTime+ paramBase64));
        string md5Hash="";
        foreach (byte b in md5HashBytes)
        {
            md5Hash += b.ToString("x2");
        }
//     Debug.Log(md5Hash);

         for (int i = 0; i < actions.Count; i++)
         {
              if (i<100&&actions[i].actionType == TTSAction.ActionType.VOICE)
              {
                    WWWForm form = new WWWForm();
                    form.AddField("text", actions[i].text);
                    Debug.Log(actions[i].text);

                    UnityWebRequest unityWebRequest = UnityWebRequest.Post(ttsServerUrl, form);
                    unityWebRequest.SetRequestHeader("X-Appid", appID);
                    unityWebRequest.SetRequestHeader("X-CurTime", curTime);
                    unityWebRequest.SetRequestHeader("X-Param", paramBase64);
                    unityWebRequest.SetRequestHeader("X-CheckSum", md5Hash);
                    unityWebRequest.SetRequestHeader("Content-Type", "application/x-www-form-urlencoded; charset=utf-8");

                    DownloadHandlerAudioClip downloadHandlerAudioClip = new DownloadHandlerAudioClip(outputPath+"/"+ actions[i].order+".wav",AudioType.WAV);
                    unityWebRequest.downloadHandler = downloadHandlerAudioClip;
                    unityWebRequest.SendWebRequest();

                    hasPastTime = 0;
                    while (!unityWebRequest.isDone)
                    {
                        yield return new WaitForSeconds(0.2f);
                        hasPastTime += 0.2f;
                        Debug.Log("下载转置音频已过 " + hasPastTime + " 秒！");
                    }

                    string responseContentType = unityWebRequest.GetResponseHeader("Content-Type");
                    if (responseContentType == "audio/mpeg")
                    {
                        File.WriteAllBytes(outputPath + "/" + actions[i].order + ".wav", downloadHandlerAudioClip.data);
                        actions[i].voiceWavBin = new float[downloadHandlerAudioClip.audioClip.samples];

                        if (downloadHandlerAudioClip.audioClip.GetData(actions[i].voiceWavBin, 0))
                        {
                            Debug.Log(actions[i].order + "----SamplesCount" + actions[i].voiceWavBin.Length);
                        }
                        else
                        {
                            Debug.LogError("audioClip.GetData获取Buffer错误");
                        }
                    }
                    else if (responseContentType == "text/plain")
                    {
                        Debug.LogError(Encoding.ASCII.GetString(downloadHandlerAudioClip.data));
                        break;
                    }
                    Debug.Log("已完成转置音频");
              }
         }
        Debug.Log("完成所有音频转置");

        onTransformEnd.Invoke();
        isTransforming = false;
    }

}
