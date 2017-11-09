using UnityEngine;
using System.Collections;
using DG.Tweening;
using UnityEngine.UI;

public class ImageFade : MonoBehaviour
{
    public Image image;

    public void  AlphaPlayForward()
    {
        image.DOColor(new Color(1, 1, 1, 1), 0.3f).OnComplete(EnableCast);
//		image.DOFade(1,0.3f).OnComplete(EnableCast);
    }

    public void AlphaPlayBack()
    {
		image.DOColor(new Color(1, 1, 1, 0), 0.3f).OnComplete(DisableCast);
//		image.DOFade(0,0.3f).OnComplete(DisableCast);
    }

    void EnableCast()
    {
        image.raycastTarget = true;
    }

    void DisableCast()
    {
        image.raycastTarget = false;
    }

	public void TempBolckTouch(float BlockTime=1.0f)
	{
        DOTween.Kill (image);
		image.raycastTarget = true;
		image.DOFade (0, BlockTime).OnComplete (DisableCast);
	}

	public void SetCastState(bool willState)
	{	
		image.raycastTarget = willState;
	}

}
