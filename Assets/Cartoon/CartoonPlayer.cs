﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class CartoonPlayer : MonoBehaviour
{
    public int fps = 15;
    public Image cartoonImage;
//  public ImageButton cartoonPauseBtn;
    public Text text;
    public Image waveLine;
    
    public AudioClip micAudioClip;

    float[] fileAudioSamples;
    public AudioClip fileAudioClip;
    public AudioSource audioSource;

    public CartoonAniClip[] cartoonAniClipGroup;
    public CartoonAniClip[] cartoonAniClipGroup2;

    CartoonAniClip[] currentClipGroup;

    public delegate void OnComplete(string str);

    string micDeviceName;
    float micStartTime;
    float[] micWaveSamples;

    Coroutine playCartoonAniCoroutine;
// public CanveGroupFade triggerCartoonAni;

    int currentSliceID;

    public float averageVolume;
    float maxVolume;
    int currentStartPos;

    bool averageVolumeTrriger;
    bool isSpeak;
    bool isSpeakLast;

    /// <summary>
    /// 
    /// </summary>

    public UnityEvent hasStopWhenPause;

    public UnityEvent OnOpen;
    public UnityEvent OnClose;

    public UnityEvent OnSpeak;
    public UnityEvent OnStopSpeak;

    public static bool hasInit;
    bool isStop;
    bool lastAudioState;

    void Update()
    {
        //unity开启MIC时执行以下代码
        if (micAudioClip != null)
        {
            text.text = micDeviceName + "\n"; ;
            text.text += Microphone.IsRecording(micDeviceName).ToString() + "\n";

            //当记录音频开始过了0.2秒后
            if (Time.time > micStartTime + 0.1f * (currentSliceID + 1f))
            {
                currentSliceID++;
                text.text += "currentSliceID" + currentSliceID + "\n";
                text.text += "currentSliceID" + currentSliceID % 10 + "\n";
                text.text += micAudioClip.GetData(micWaveSamples, currentSliceID % 10).ToString() + "\n";

                averageVolume = 0;
                maxVolume = 0;

                for (int i = 0; i < 2205; i++)
                {
                    float inValue = Math.Abs(micWaveSamples[i]);
                    averageVolume += inValue;
                    if (inValue > maxVolume)
                    {
                        maxVolume = inValue;
                    }
                }
                text.text += averageVolume * 0.0005f + "\n";
                text.text += maxVolume * 0.0005f * 2205 + "\n";
            }
        }

        if (fileAudioClip != null && audioSource.isPlaying)
        {

            if (audioSource.time < fileAudioClip.length - 0.11f)
            {
                currentStartPos = (int)(audioSource.time * fileAudioClip.frequency);
                fileAudioClip.GetData(fileAudioSamples, currentStartPos);

                averageVolume = 0;
                maxVolume = 0;

                for (int i = 0; i < fileAudioSamples.Length; i++)
                {
                    float inValue = Math.Abs(fileAudioSamples[i]);
                    averageVolume += inValue;
                    if (inValue > maxVolume)
                    {
                        maxVolume = inValue;
                    }
                }
            }

            //  由音强来判断是否要播放音乐
            //  averageVolume = 20;

            waveLine.rectTransform.localScale = new Vector3(1, averageVolume * 0.005f, 1);

            if (averageVolume * 0.005f > 0.1f)
            {
                averageVolumeTrriger = true;
            }
            else if (averageVolume * 0.005f < 0.05f)
            {
                averageVolumeTrriger = false;
            }

            if (isSpeak)
            {
                if (!averageVolumeTrriger)
                {
                    isSpeak = false;
                }
            }
            else
            {
                if (averageVolumeTrriger)
                {
                    isSpeak = true;
                }
            }

            if (isSpeakLast != isSpeak)
            {
                isSpeakLast = isSpeak;

                if (isSpeak)
                {
                    PlayCartoonAni("a0" + UnityEngine.Random.Range(1, 9).ToString(), 0);
                }
                else
                {
                    PlayCartoonAni("n01", 0);

                }
            }
        }

        if (audioSource != null && lastAudioState != audioSource.isPlaying)
        {
            if (audioSource.isPlaying)
            {
                Debug.Log("OnSpeak");
                OnSpeak.Invoke();
            }
            else
            {
                Debug.Log("OnStopSpeak");
                OnStopSpeak.Invoke();
            }

            lastAudioState = audioSource.isPlaying;
        }

    }

    /// <summary>
    /// 打开卡通角色，使用本机麦克风
    /// </summary>
    /// <param name="cartoonType">卡通角色类型</param>
    public void OpenCartoonPeopleUseUnityMic(int cartoonType = 0)
    {
        OnOpen.Invoke();

        SetCartoonSex(cartoonType);

        text.text = "Find\n";

        for (int i = 0; i < Microphone.devices.Length; i++)
        {
            text.text += Microphone.devices[i] + "\n";
            micDeviceName = Microphone.devices[i];
            //新建一个1秒长的AudioClip
            micAudioClip = Microphone.Start(Microphone.devices[i], true, 1, 22050);
            micStartTime = Time.time;

            //截取10分之一长度，即0.1秒的buffer
            micWaveSamples = new float[2205];
            currentSliceID = 0;
        }

        PlayCartoonAni("n01",0);

        cartoonImage.color = new Color(1f, 1f, 1f, 1f);

    }


    public void OpenCartoonPeopleUseAudioFile(AudioClip a)
    {
        if(a!=null)
        OpenCartoonPeopleUseAudioFile(a, 0);
    }
    /// <summary>
    /// 打开卡通角色，使用已有的音频文件
    /// </summary>
    /// <param name="a">所要使用的音频文件</param>
    /// <param name="cartoonType">卡通角色类型</param>
    public void OpenCartoonPeopleUseAudioFile(AudioClip a, int cartoonType = 0)
    {
        OnOpen.Invoke();

        fileAudioSamples = new float[((int)(a.frequency*0.1f))];

        SetCartoonSex(cartoonType);
        fileAudioClip = a;
        audioSource.clip = fileAudioClip;
        audioSource.loop = false;
 //    audioSource.time = 53f;
        audioSource.Play();

        PlayCartoonAni("n01", 0);
        cartoonImage.color = new Color(1f, 1f, 1f, 1f);
    }
    /// <summary>
    /// 音频文件播放暂停
    /// </summary>
    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            averageVolume = 0;
            PlayCartoonAni("n01", 0);
        }
        else
        {
            hasStopWhenPause.Invoke();   
        }
    }

    /// <summary>
    /// 音频文件播放恢复
    /// </summary>
    public void Resume()
    {
        audioSource.Play();
    }


    /// <summary>
    /// 关闭卡通人物
    /// </summary>
    public void CloseCaratoonPeople()
    {
        OnClose.Invoke();

        cartoonImage.color = new Color(1f, 1f, 1f,0f);
        if (audioSource!=null&&audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (playCartoonAniCoroutine != null)
        {
            StopCoroutine(playCartoonAniCoroutine);
//         Debug.Log("Stop Coroutine");
        }
    }



    /// <summary>
    /// 
    /// </summary>
    /// <param name="sex">0:女,1:男</param>
    void SetCartoonSex(int sex)
    {
        if (sex == 0)
        {
            currentClipGroup = cartoonAniClipGroup;
            cartoonImage.sprite = cartoonAniClipGroup[0].spriteGroup[0];
        }
        else
        {
            currentClipGroup = cartoonAniClipGroup2;
            cartoonImage.sprite = cartoonAniClipGroup2[0].spriteGroup[0];
        }

    }

     public void PlayCartoonAni(string cartoonAniClip,float delayTime)
    {
        //默认眨眼动作
        if (cartoonAniClip.Substring(0,1) =="n")
        {
            PlayCartoonAni(cartoonAniClip, delayTime,(string arg) => { Debug.Log(arg + " OnComplete"); PlayCartoonAni("n0" + UnityEngine.Random.Range(1, 3).ToString(), UnityEngine.Random.Range(3f, 6f)); }, cartoonAniClip);
        }
        else
        {
            PlayCartoonAni(cartoonAniClip, 0,(string arg) => { Debug.Log(arg + " OnComplete"); if (isSpeak) { PlayCartoonAni("a0" + UnityEngine.Random.Range(1, 9).ToString(), 0); } }, cartoonAniClip);
        }
        
    }

    void PlayCartoonAni(string cartoonAniClip,float delayTime, OnComplete onComplete,string arg)
    {
        if (playCartoonAniCoroutine != null)
        {
            StopCoroutine(playCartoonAniCoroutine);
//            Debug.Log("Stop Coroutine");
        }

//     currentPlayAniClip = null;
        if (currentClipGroup != null)
        {
            for (int i = 0; i < currentClipGroup.Length; i++)
            {
                if (currentClipGroup[i].name == cartoonAniClip)
                {
                    playCartoonAniCoroutine = StartCoroutine(PlayCartoonAniIE(currentClipGroup[i], delayTime, onComplete, arg ));
                    break;
                }
            }
        }

    }

    IEnumerator PlayCartoonAniIE(CartoonAniClip c,float delayTime,OnComplete onComplete, string arg)
    {
        yield return new WaitForSeconds(delayTime);

        int currentID = 0;

        yield return new WaitForEndOfFrame();

        while (currentID < c.spriteGroup.Length - 1)
        {
            yield return new WaitForSeconds(1.0f / fps);
            currentID++;
            cartoonImage.sprite = c.spriteGroup[currentID];
        }
    
        if (onComplete != null)
             onComplete.Invoke(arg);
    }

/*
    public void PlayTTSAudio(TTS_ActionSequence t)
    {
        OpenCartoonPeopleUseAudioFile(t.voiceAudioClipGroup[2]);
    }
*/

}

