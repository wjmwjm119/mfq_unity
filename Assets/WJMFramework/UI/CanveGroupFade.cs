using UnityEngine;
using System.Collections;

public class CanveGroupFade : MonoBehaviour
{

	public bool defaultState=false;

	public CanvasGroup canvasGroup;
	public float fadeUseTime=1;

	void Awake()
	{
	    canvasGroup.blocksRaycasts = defaultState;
		canvasGroup.alpha = 0;
		if(defaultState)
		canvasGroup.alpha = 1;	
	}

	public void AlphaPlayForward()
	{		
		this.StopAllCoroutines();
		canvasGroup.blocksRaycasts = true;

        if (fadeUseTime != 0)
        {
            StartCoroutine(AlphaPlayForwardIE());
        }
        else
        {
            canvasGroup.alpha = 1;
        }
		
	}
	
	public void AlphaPlayBackward()
	{
		this.StopAllCoroutines();
		canvasGroup.blocksRaycasts = false;

        if (fadeUseTime != 0)
        {
            StartCoroutine(AlphaPlayBackwardIE());
        }
        else
        {
            canvasGroup.alpha = 0;
        }
	}
	
	
	IEnumerator AlphaPlayForwardIE()
	{
		
		while(canvasGroup.alpha<1)
		{		
			yield return new WaitForEndOfFrame ();
			canvasGroup.alpha+=Time.deltaTime/fadeUseTime;
		}
		canvasGroup.alpha = 1;
		
	}
	
	IEnumerator AlphaPlayBackwardIE()
	{
		
		while(canvasGroup.alpha>0)
		{
			yield return new WaitForEndOfFrame ();
			canvasGroup.alpha-=Time.deltaTime/fadeUseTime;
		}

		canvasGroup.alpha = 0;
		
		
	}


}
