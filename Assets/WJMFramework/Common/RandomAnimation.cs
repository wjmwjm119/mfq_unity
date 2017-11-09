using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class RandomAnimation : MonoBehaviour
{

	public bool autoPlay=false;

	public bool randomPos=true;
	public float posDistance=10;
	public float posUseTime=1f;
	public float posDelayRangeMax=0f;


	public bool randomRot=false;
	public float rotRotateMax=360;
	public float rotUseTime=1f;
	public float rotDelayRangeMax=0f;

	public bool randomScale=false;
	public float scaleMin=0.5f;
	public float scaleMax=2f;
	public float  scaleUseTime=1f;
	public float  scaleDelayRangMax=0f;




	void Start()
	{
		if (autoPlay)
		{
			if (randomPos)
				Pos ();
			if (randomRot)
				Rot ();
			if (randomScale)
				Scale ();	
		}
	}


	public void Pos()
	{
		float randomTime=Random.Range(0, posDelayRangeMax);
		Vector3 randomPos=Random.insideUnitCircle * 30;
		GetComponent<RectTransform>().DOAnchorPos(new Vector2(randomPos.x,randomPos.y),posUseTime).SetEase(Ease.InOutSine).OnComplete(Pos).SetDelay(randomTime);


	}



    public void Rot()
    {
		float random = Random.Range(0, rotRotateMax);
		GetComponent<RectTransform>().DORotate(new Vector3(0, 0, random),rotUseTime).OnComplete(Rot);
    }


	public void Scale()
	{
		float random = Random.Range(scaleMin, scaleMax);
		GetComponent<RectTransform>().DOScale(new Vector3(random,random,1),scaleUseTime).SetEase(Ease.InOutSine).OnComplete(Scale).SetDelay(scaleDelayRangMax);
	}


	public void StopPos()
	{

		GetComponent<RectTransform>().DOAnchorPos(new Vector2(0,0),posUseTime).SetEase(Ease.InOutSine);

	}

	public void StopRot()
	{


	}

	public void StopScale()
	{


	}


	public void PlayAll()
	{
		if (randomPos)
		Pos ();


		if (randomRot)
		Rot ();


		if (randomScale)
		Scale ();

	}

	public void StopAll()
	{
		DOTween.Kill(GetComponent<RectTransform>());
		StartCoroutine (StopAllIE());


	}

	IEnumerator StopAllIE()
	{
		yield return new	WaitForSeconds (0.5f);
		if (randomPos)
			StopPos ();
		if (randomRot)
			StopRot ();
		if (randomScale)
			StopScale ();

	}




//    IEnumerator Rot()
//    {
 //       yield 
//
 //   }

	
}
