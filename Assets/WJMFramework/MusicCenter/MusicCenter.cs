//2013.04.2


using UnityEngine;
using System.Collections;

public class MusicCenter : BaseEventCenter 
{

    public AudioSource main;
    public AudioSource singer;
    public int playTime;
    public bool isPlaying;

    public bool initialAutoPlay=true;
    public int initailTargetClip;

	public bool loopPlay=true;
		
	public AudioClip[] audioGroup;

	public float fadeInSecond=1f;
	public float fadeOutSecond=1f;

	public bool audioChanging;
	public bool audioPlaying;
    public int currentPlayClip;

    public void PlayMainAudioFirstTime()
    {
        if (playTime > 0)
        {
            playTime--;
            PlayMainAudio();
        }  
    }

    public void PlayMainAudio()
    {
        if (!isPlaying)
        {
            main.mute = true;
            isPlaying = true;
            main.Pause();
            singer.Play();
            StartCoroutine(WaitSomeTime());
        }

    }

    IEnumerator WaitSomeTime()
    {
        yield return new WaitForSeconds(singer.clip.length);
        main.mute = false;
        main.Play();
        singer.Stop();
        isPlaying = false;
    }


	void Awake () 
	{	
		DontDestroyOnLoad (this);
		GetComponent<AudioSource>().volume=0f;
		if(loopPlay)
		{
			GetComponent<AudioSource>().loop=true;
		}
	}

    void Start()
    {
        if (initialAutoPlay)
        {
            StartCoroutine(PlayAudio(initailTargetClip));
        }
    }

	
	IEnumerator PlayAudio(int targetClip)
	{			
		Debug.Log("AudioStartPlay!");
		if(!audioChanging)
		{
			audioChanging=true;
            audioPlaying = true;

			GetComponent<AudioSource>().clip=audioGroup[targetClip];
			GetComponent<AudioSource>().Play();
			
			while(GetComponent<AudioSource>().volume<0.95f)
			{
				GetComponent<AudioSource>().volume+=0.3f;
				yield return new WaitForSeconds(0.01f/fadeInSecond);
			}
		
			audioChanging=false;
			Debug.Log("AudioPlaying!");
		}
		
	}
	
	IEnumerator StopAudioIE()
	{	

        audioPlaying = false;
		if(!audioChanging)
		{
			audioChanging=true;
			
			while(GetComponent<AudioSource>().volume>0f)
			{
				GetComponent<AudioSource>().volume-=0.3f;
				yield return new WaitForSeconds(0.01f/fadeOutSecond);
			}
			GetComponent<AudioSource>().Stop();
		
			audioChanging=false;
			Debug.Log("AudioEndStop!");
		}
	}
		
	IEnumerator ChangeAudio(int targetClip)
	{	
		if(!audioChanging&&targetClip!=currentPlayClip||!audioPlaying)
		{
			Debug.Log("AudioStartChange!");
			audioChanging=true;
			audioPlaying = true;

            currentPlayClip = targetClip;	
						
			while(GetComponent<AudioSource>().volume>0f)
			{
				GetComponent<AudioSource>().volume-=0.3f;
				yield return new WaitForSeconds(1f/fadeOutSecond);
			}
			
			GetComponent<AudioSource>().Stop();
						
			GetComponent<AudioSource>().clip=audioGroup[targetClip];
			
			GetComponent<AudioSource>().Play();	
			
			while(GetComponent<AudioSource>().volume<0.95f)
			{
				GetComponent<AudioSource>().volume+=0.3f;
				yield return new WaitForSeconds(1f/fadeInSecond);
			}
			
			audioChanging=false;
			
			Debug.Log("AudioEndStop!");
		}
		
	}


    /*
    public override void ExecuteMGMouseDown(BaseEventArgs args)
    {
        MusicCenterArgs cc = (MusicCenterArgs)args;

        if (cc.playAudio)
        {
            StartCoroutine(ChangeAudio(cc.targetClip));
        }

        if (cc.stopAudio)
        {
            StartCoroutine(StopAudio());
        }

    }
*/


    public void ChangeMusic(int target)
    {
        StartCoroutine(ChangeAudio(target));
    }

    public void StopAudio()
    {
        StartCoroutine(StopAudioIE());
    }





}
