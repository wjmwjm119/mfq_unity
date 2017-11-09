using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;



public class ScaleInAndOut : MonoBehaviour
{
	
	public float moveSpeed=1;
	
	public float scaleSpeed=1;
	public float scaleMin=0.5f;
	public float scaleMax=2;


	Vector3 orginPosition;
	Vector3 orginScale;

	Vector3 lastUpPosition;

    //	public bool enabled;


    void Reset()
    {
        transform.localPosition = orginPosition;
        transform.localPosition = lastUpPosition;
        transform.localScale = orginScale;
    }



	void Start()
	{
		orginPosition = transform.localPosition;
		lastUpPosition = transform.localPosition;
		orginScale = transform.localScale;
	}

	public void EnableScaleAndMove()
	{		
		enabled = true;
//		lastUpPosition =orginPosition;
//		transform.DOScale(orginScale,1f);
//		transform.DOLocalMove(orginPosition, 1f).OnComplete (EnableEnd);
	}

	void EnableEnd()
	{
		enabled = true;
	}


	public void DisableScaleAndMove()
	{
		
//		transform.DOScale(orginScale,1f).SetEase(Ease.InOutCubic);
//		transform.DOLocalMove(orginPosition,1f);
		enabled = false;

	}

	public void OnDrag(Vector2 inOffset)
	{

		if (enabled) 
		{
			
			transform.localPosition=lastUpPosition+moveSpeed*new Vector3(inOffset.y,-inOffset.x,0);

		}

	}

	public void OnDown()
	{
		if (enabled)
		{
			
			lastUpPosition = transform.localPosition;

		}
	}


	public void OnUp()
	{
		if (enabled) 
		{		
			
			lastUpPosition = transform.localPosition;

		}
	}


	public void BeforeOnScale()
	{
		
		orginScale = transform.localScale;

	}

    
	public void OnScale(float inScale)
	{
		
		float scaleFactor=Mathf.Clamp(orginScale.x+0.001f*inScale,scaleMin,scaleMax);

		if (enabled)
		{
			transform.localScale = new Vector3 (scaleFactor, scaleFactor, orginScale.z);
		}
	}


}
