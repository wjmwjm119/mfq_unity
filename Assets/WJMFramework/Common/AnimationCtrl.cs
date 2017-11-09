using UnityEngine;
using System.Collections;

public class AnimationCtrl : MonoBehaviour
{


	public Animation ani;

	public BaseButton[] onAnimationEndEventGroup;

	public int onCompleteEventID;

	public bool isReverse=false;
	public bool startPlaying = false;
//	public bool eventTrigger=false;


	public void PlayDefaultAnimation()
	{
		//ani.name是当前物体的名
		PlayAnimation (ani.name,0);
	}

	public void ReverseDefaultAnimation()
	{
		ReverseAnimation (ani.name,0);
	}

	//-1不执行OnCompleteEvent
	public void PlayAnimation(string aniName,int onCompEventID=-1)
	{
		
		onCompleteEventID = onCompEventID;

		isReverse = false;
		startPlaying = true;

		Debug.Log (aniName);

		ani[aniName].normalizedTime = 0.0f;
		ani[aniName].speed = 1;
		ani.CrossFade (aniName);
		ani.Play (aniName);
	}

	public void ReverseAnimation(string aniName,int onCompEventID=-1)
	{
		onCompleteEventID = onCompEventID;
		isReverse = true;
		startPlaying = true;
		ani[aniName].normalizedTime = 1.0f;
		ani[aniName].speed = -1;
		ani.CrossFade (aniName);
		ani.Play(aniName);

	}

	void Update()
	{
		if (startPlaying&&ani!=null )
		{

			if (!ani.isPlaying)
			{
				startPlaying = false;
				Debug.Log (onCompleteEventID);
				if(onCompleteEventID>-1)
				onAnimationEndEventGroup[onCompleteEventID].ProcessEvent (!isReverse);
			}

//			eventTrigger = true;



		}


	}



}
