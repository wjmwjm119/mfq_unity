using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCtrl : MonoBehaviour
{

    public AudioSource audioSource;

    //关闭音乐，由App控制
    void Start()
    {
        MuteMusic();
    }

    public void MuteMusic()
    {
        audioSource.mute = true;
    }

    public void PlayMusic()
    {
        audioSource.mute = false;
    }


}
