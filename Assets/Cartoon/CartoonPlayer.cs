using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.Events;

public class CartoonPlayer : MonoBehaviour
{
    public int fps = 15;
    public Image cartoonImage;
    public ImageButton cartoonPauseBtn;
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
    public CanveGroupFade triggerCartoonAni;

    int currentSliceID;

    public float averageVolume;
    float maxVolume;

    bool averageVolumeTrriger;
    bool isSpeak;
    bool isSpeakLast;

    void Update()
    {
        //unity开启MIC时执行以下代码
        if (micAudioClip != null)
        {
            text.text =micDeviceName + "\n"; ;
            text.text += Microphone.IsRecording(micDeviceName).ToString()+"\n";

            //当记录音频开始过了0.2秒后
            if(Time.time>micStartTime+0.1f* (currentSliceID+1f))
            {
                currentSliceID++;
                text.text += "currentSliceID" + currentSliceID + "\n";
                text.text += "currentSliceID" + currentSliceID % 10 + "\n";
                text.text += micAudioClip.GetData(micWaveSamples, currentSliceID % 10).ToString()+"\n";

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
                text.text += maxVolume * 0.0005f* 2205 + "\n";
            }
        }


        if (fileAudioClip != null&& audioSource.isPlaying)
        {
            if (audioSource.time < fileAudioClip.length -0.11f)
            {
                fileAudioClip.GetData(fileAudioSamples, (int)(audioSource.time * fileAudioClip.frequency) + fileAudioSamples.Length);

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
/*
            else
            {
                fileAudioClip.GetData(fileAudioSamples, (int)(audioSource.time * fileAudioClip.frequency));
            }
*/
        }

        //由音强来判断是否要播放音乐

//     averageVolume = 20;

        waveLine.rectTransform.localScale = new Vector3(1, averageVolume * 0.005f, 1);

        if (averageVolume * 0.005f > 0.1f)
        {
            averageVolumeTrriger = true;
        }
        else if(averageVolume * 0.005f < 0.05f)
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
                PlayCartoonAni("a0" + UnityEngine.Random.Range(1, 8).ToString(),0);
            }
            else
            {
                PlayCartoonAni("a09",0);
            }
        }
    }


    /// <summary>
    /// 
    /// </summary>
    /// <param name="cartoonType"></param>
    /// <param name="waveTypeUse"></param>
    public void OpenCartoonPeopleUseUnityMic(int cartoonType = 0)
    {
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

        PlayCartoonAni("a09",0);

        cartoonImage.DOFade(1, 0.2f);

    }


    public void OpenCartoonPeopleUseAudioFile(AudioClip a, int cartoonType = 0)
    {
        fileAudioSamples = new float[((int)(a.frequency*0.1f))];

        SetCartoonSex(cartoonType);
        fileAudioClip = a;
        audioSource.clip = fileAudioClip;
        audioSource.loop = false;
//     audioSource.time = 75;
        audioSource.Play();

        PlayCartoonAni("a09", 0);
        cartoonImage.DOFade(1, 0.2f);

    }

    public void Pause()
    {
        if (audioSource.isPlaying)
        {
            audioSource.Pause();
            averageVolume = 0;
            PlayCartoonAni("a09", 0);
        }
        else
        {
            cartoonPauseBtn.SetBtnState(false, 0);
        }

    }

    public void Resume()
    {
        audioSource.Play();

    }



    public void CloseCaratoonPeople()
    {
        cartoonImage.DOFade(0, 0.2f);
        if(audioSource!=null&&audioSource.isPlaying)
        {
            audioSource.Stop();
        }

        if (playCartoonAniCoroutine != null)
        {
            StopCoroutine(playCartoonAniCoroutine);
//            Debug.Log("Stop Coroutine");
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
        if (cartoonAniClip == "a09")
        {
            PlayCartoonAni(cartoonAniClip, delayTime,(string arg) => { Debug.Log(arg + " OnComplete"); PlayCartoonAni("a09", UnityEngine.Random.Range(3f, 6f)); }, cartoonAniClip);
        }
        else
        {
            PlayCartoonAni(cartoonAniClip, 0,(string arg) => { Debug.Log(arg + " OnComplete"); if(isSpeak) PlayCartoonAni("a0" + UnityEngine.Random.Range(1, 8).ToString(), 0); }, cartoonAniClip);
        }

    }

    public void PlayCartoonAni(string cartoonAniClip,float delayTime, OnComplete onComplete,string arg)
    {
        if (playCartoonAniCoroutine != null)
        {
            StopCoroutine(playCartoonAniCoroutine);
            Debug.Log("Stop Coroutine");
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

}

