using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using UnityEngine.Events;
using DG.Tweening;

public class VideoPlayerCtrl : MonoBehaviour
{
    public List<VideoClip> videoClipGroup;
    public RawImage rawImage;
    public VideoPlayer videoPlayer;

    public bool closeWhenEnd;
    public UnityEvent onEnd;

    public void PlayVideoID(int targetID)
    {
        if (targetID > -1 && targetID < videoClipGroup.Count)
        {
            videoPlayer.clip = videoClipGroup[targetID];
            StartCoroutine(PlayVideo_IE());
        }
        else
        {
            Debug.LogError("视频targetID超出范围");
        }
    }

    public void PlayVideoName(string targetName)
    {
        videoPlayer.clip = videoClipGroup.Find(v => v.name == targetName);
        if (videoPlayer.clip != null)
        {
            StartCoroutine(PlayVideo_IE());
        }
        else
        {
            Debug.LogError("未找到"+targetName+"视频");
        }
    }
    

    public void StopVideo()
    {
        rawImage.texture = null;
        StopAllCoroutines();
        videoPlayer.Stop();
        rawImage.DOFade(0, 0.5f);
    }

    IEnumerator PlayVideo_IE()
    {
        videoPlayer.Play();
        while (!videoPlayer.isPrepared)
        {
            yield return new WaitForSeconds(0.1f);
        }
        rawImage.DOFade(1, 0.5f);
        rawImage.texture = videoPlayer.texture;

        while (!(videoPlayer.time + 0.1f > videoPlayer.clip.length))
        {
            yield return new WaitForSeconds(0.1f);
        }

        if(closeWhenEnd)
        StopVideo();

        onEnd.Invoke();
    }


}
