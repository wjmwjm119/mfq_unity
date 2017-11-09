using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioCtrl : MonoBehaviour
{

    public AudioSource audioSource;

    public void MuteMusic()
    {
        audioSource.mute = true;
    }

    public void PlayMusic()
    {
        audioSource.mute = false;
    }


}
