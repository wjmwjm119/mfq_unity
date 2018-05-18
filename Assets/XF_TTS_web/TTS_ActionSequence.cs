using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Xml;
using System.IO;
using System;
using UnityEngine.Networking;
using UnityEngine.Events;

public class TTS_ActionSequence : MonoBehaviour
{
    //用来保存结果时的文件名
    public string sceneName = "sceneName";
    public CartoonPlayer cartoonPlayer;
    public List<TTSAction> sequence;
    public AudioClip[] voiceAudioClipGroup;
    string outputPath;

    [Serializable]
    public class CtrlCMDProcess : UnityEvent<string> { };
    public CtrlCMDProcess ctrlCMDProcess;

    Coroutine sequenceCoroutine;

    void Start()
    {
        #if UNITY_EDITOR
        outputPath = "Assets/StreamingAssets/TTSmp3Audio/" + sceneName;
        #else
        outputPath = Directory.GetCurrentDirectory() + "/StreamingAssets/TTSmp3Audio/" +sceneName ;
        #endif
        LoadSerializeCache();
    }

    public bool GenSequence(string xmlString)
    {
        sequence = new List<TTSAction>();
        //添加根目录
        string modiferXmlString = "<document>" + xmlString + "</document>";
        XmlDocument xmlDoc = new XmlDocument();
        try
        {
            //由string转xml
            xmlDoc.LoadXml(modiferXmlString);
            //获取所有子节点
            XmlNode allChildNodes = xmlDoc.SelectSingleNode("document");

            for (int i = 0; i < allChildNodes.ChildNodes.Count; i++)
            {
                sequence.Add(new TTSAction(allChildNodes.ChildNodes[i].Name, allChildNodes.ChildNodes[i].InnerText, i));
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }

        return false;
    }

    public void LoadSerializeCache()
    {
        if (File.Exists(outputPath + "/SerializeCache.bin"))
        {
            Debug.Log("调用已有缓存");
            byte[] serializeCacheBuffer;
            serializeCacheBuffer = File.ReadAllBytes(outputPath + "/SerializeCache.bin");
            sequence = (List<TTSAction>)SerializeTool.DeserializeObject(serializeCacheBuffer);
            GenVoiceAudioClipGroup();
        }
    }

    public void SaveSerializeCache()
    {
        GenVoiceAudioClipGroup();
        File.WriteAllBytes(outputPath + "/SerializeCache.bin", SerializeTool.SerializeObject(sequence));
        Debug.Log("SaveSerializeCache");
    }

    public void GenVoiceAudioClipGroup()
    {
        voiceAudioClipGroup = new AudioClip[sequence.Count];

        for (int i = 0; i < sequence.Count; i++)
        {
            if (sequence[i].actionType == TTSAction.ActionType.VOICE&& sequence[i].voiceWavBin!=null)
            {
               voiceAudioClipGroup[i] = sequence[i].GenAudioClip();
            }
        }
    }

    public void PlayTTSSequence()
    {
        if(sequenceCoroutine!=null)
        StopCoroutine(sequenceCoroutine);

        sequenceCoroutine=StartCoroutine(PlayTTSSequenceIE());
    }

    IEnumerator PlayTTSSequenceIE()
    {
        for (int i = 0; i < sequence.Count; i++)
        {
            if (sequence[i].actionType == TTSAction.ActionType.VOICE)
            {
                cartoonPlayer.OpenCartoonPeopleUseAudioFile(sequence[i].GenAudioClip());
                
                while (cartoonPlayer.audioSource.isPlaying)
                {
                    yield return new WaitForSeconds(0.1f);
                }
            }
            else if (sequence[i].actionType == TTSAction.ActionType.CMD)
            {
                ctrlCMDProcess.Invoke(sequence[i].text);
                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void StopTTSSequence()
    {
        if (sequenceCoroutine != null)
            StopCoroutine(sequenceCoroutine);
        cartoonPlayer.CloseCaratoonPeople();
    }

}

[System.Serializable]
public class TTSAction
{
    public enum ActionType
    {
        NULL=0,
        VOICE = 1,
        CMD = 2,
    }
    public int order;
    public ActionType actionType;
    public string text;
    public float[] voiceWavBin;


    public TTSAction(string inType,string inText,int inOrder)
    {
        order = inOrder;
        text = inText;

        switch (inType)
        {
            case "VOICE":
                actionType = ActionType.VOICE;
                break;
            case "CMD":
                actionType = ActionType.CMD;
                break;
            default:
                Debug.LogError(inType+" 是未定义标签");
                break;
        }
    }

    public AudioClip GenAudioClip()
    {
        AudioClip a;
        if (voiceWavBin!=null&& voiceWavBin.Length > 0)
        {
            //加3200sample也就是0.2秒的空白音频，因为自动生成的音频文件里没有空白区域
            a = AudioClip.Create(order.ToString(),voiceWavBin.Length+3200,1,16000, false);
            a.SetData(voiceWavBin,0);

            return a;
        }
        return null;
    }

    public void PlayAudio()
    {

    }

}

