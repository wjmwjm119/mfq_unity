using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CartoonPlayArgs : MonoBehaviour
{
    public AudioClip hxAudioClip;
    public int cartoonType;
    /// <summary>
    /// 是否已经播放过音频了，如果已播放过就不自动播放
    /// </summary>
    public bool hasPlayed;

    public void OnWake()
    {
        Debug.Log("OnWaking");
    }

}
