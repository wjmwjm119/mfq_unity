using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HuXingInfoLabel : MonoBehaviour
{

    public CanveGroupFade canveGroupFade;
    public Text[] labelGroup;
    public Transform[] baseGroup;


    public void DisplayHuXingInfoLabel(string inStr)
    {
//      Debug.Log(inStr);

        if (inStr != null)
        {
            canveGroupFade.AlphaPlayForward();

            string[] spilitStr = inStr.Split('^');

            for (int i = 0; i < spilitStr.Length; i++)
            {
                labelGroup[i * 2].text = spilitStr[i];
                labelGroup[i * 2 + 1].text = spilitStr[i];
            }

            DOTween.Kill(baseGroup[0]);
            DOTween.Kill(baseGroup[1]);
            DOTween.Kill(baseGroup[2]);
            DOTween.Kill(baseGroup[3]);
            DOTween.Kill(baseGroup[4]);
            DOTween.Kill(baseGroup[5]);
            DOTween.Kill(baseGroup[6]);


            DOTween.Kill(baseGroup[0].GetComponent<CanvasGroup>());
            DOTween.Kill(baseGroup[1].GetComponent<CanvasGroup>());
            DOTween.Kill(baseGroup[2].GetComponent<CanvasGroup>());
            DOTween.Kill(baseGroup[3].GetComponent<CanvasGroup>());
            DOTween.Kill(baseGroup[4].GetComponent<CanvasGroup>());
            DOTween.Kill(baseGroup[5].GetComponent<CanvasGroup>());
            DOTween.Kill(baseGroup[6].GetComponent<CanvasGroup>());

            baseGroup[0].transform.localPosition = new Vector3(0, -300, 0);
            baseGroup[1].transform.localPosition = new Vector3(0, -300, 0);
            baseGroup[2].transform.localPosition = new Vector3(0, -300, 0);
            baseGroup[3].transform.localPosition = new Vector3(0, -300, 0);
            baseGroup[4].transform.localPosition = new Vector3(0, -300, 0);
            baseGroup[5].transform.localPosition = new Vector3(0, -300, 0);
            baseGroup[6].transform.localPosition = new Vector3(0, -300, 0);

            baseGroup[0].GetComponent<CanvasGroup>().alpha = 0;
            baseGroup[1].GetComponent<CanvasGroup>().alpha = 0;
            baseGroup[2].GetComponent<CanvasGroup>().alpha = 0;
            baseGroup[3].GetComponent<CanvasGroup>().alpha = 0;
            baseGroup[4].GetComponent<CanvasGroup>().alpha = 0;
            baseGroup[5].GetComponent<CanvasGroup>().alpha = 0;
            baseGroup[6].GetComponent<CanvasGroup>().alpha = 0;

            baseGroup[0].GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetDelay(0);
            baseGroup[1].GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetDelay(0.1f);
            baseGroup[2].GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetDelay(0.2f);
            baseGroup[3].GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetDelay(0.3f);
            baseGroup[4].GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetDelay(0.4f);
            baseGroup[5].GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetDelay(0.4f);
            baseGroup[6].GetComponent<CanvasGroup>().DOFade(1, 0.5f).SetDelay(0.5f);

            baseGroup[0].transform.DOLocalMoveY(0, 0.5f).SetDelay(0);
            baseGroup[1].transform.DOLocalMoveY(0, 0.5f).SetDelay(0.1f);
            baseGroup[2].transform.DOLocalMoveY(0, 0.5f).SetDelay(0.2f);
            baseGroup[3].transform.DOLocalMoveY(0, 0.5f).SetDelay(0.3f);
            baseGroup[4].transform.DOLocalMoveY(0, 0.5f).SetDelay(0.4f);
            baseGroup[5].transform.DOLocalMoveY(0, 0.5f).SetDelay(0.4f);
            baseGroup[6].transform.DOLocalMoveY(0, 0.5f).SetDelay(0.5f);
        }
    }

    public void HiddenHuXingInfoLabel()
    {

        canveGroupFade.AlphaPlayBackward();

        DOTween.Kill(baseGroup[0]);
        DOTween.Kill(baseGroup[1]);
        DOTween.Kill(baseGroup[2]);
        DOTween.Kill(baseGroup[3]);
        DOTween.Kill(baseGroup[4]);
        DOTween.Kill(baseGroup[5]);
        DOTween.Kill(baseGroup[6]);

        baseGroup[6].GetComponent<CanvasGroup>().DOFade(0, 0.0f);
        baseGroup[5].GetComponent<CanvasGroup>().DOFade(0, 0.1f);
        baseGroup[4].GetComponent<CanvasGroup>().DOFade(0, 0.1f);
        baseGroup[3].GetComponent<CanvasGroup>().DOFade(0, 0.2f);
        baseGroup[2].GetComponent<CanvasGroup>().DOFade(0, 0.3f);
        baseGroup[1].GetComponent<CanvasGroup>().DOFade(0, 0.4f);
        baseGroup[0].GetComponent<CanvasGroup>().DOFade(0, 0.5f);

        baseGroup[6].transform.DOLocalMoveY(0, 0.0f);
        baseGroup[5].transform.DOLocalMoveY(0, 0.1f);
        baseGroup[4].transform.DOLocalMoveY(0, 0.1f);
        baseGroup[3].transform.DOLocalMoveY(0, 0.2f);
        baseGroup[2].transform.DOLocalMoveY(0, 0.3f);
        baseGroup[1].transform.DOLocalMoveY(0, 0.4f);
        baseGroup[0].transform.DOLocalMoveY(0, 0.5f);

    }

}
