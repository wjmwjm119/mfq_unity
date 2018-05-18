using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TTS_Test : MonoBehaviour
{
    public string xmlString;
    public TTS_ActionSequence tts_ActionSequence;
    public TTS_Transform tts_Transform;

	public void StartTransform ()
    {
        //由xml文本生成动作
        if (tts_ActionSequence.GenSequence(xmlString))
        {
            tts_Transform.onTransformEnd.RemoveAllListeners();
            tts_Transform.onTransformEnd.AddListener(tts_ActionSequence.SaveSerializeCache);
            tts_Transform.GenTTSmp3Files(tts_ActionSequence.sceneName, tts_ActionSequence.sequence);
        }
        else
        {
            Debug.LogError("转换XML错误");
        }
	}

    public void ProcessCMD(string cmdString)
    {
        Debug.Log("处理CMD: "+cmdString);
    }

    public void LJXProcessCMD(string cmdString)
    {

    }

}
