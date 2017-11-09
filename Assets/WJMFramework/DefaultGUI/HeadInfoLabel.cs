using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HeadInfoLabel : MonoBehaviour
{
    public CanveGroupFade canveGroupFade;
    public Text leftText;
    public Text leftShadow;
    public Text rightText;
    public Text rightShadow;

    public string[] displayString;



    public void DisplayHeadInfoLabel(int targetID)
    {
        canveGroupFade.AlphaPlayForward();
        if (displayString.Length>0)
        {
            if (targetID < displayString.Length)
            {
                string[] splitStr = displayString[targetID].Split(',');
                if (splitStr.Length == 2)
                {

                    leftText.text = splitStr[0];
                    leftShadow.text = splitStr[0];
                    rightText.text = splitStr[1];
                    rightShadow.text = splitStr[1];


                    leftShadow.rectTransform.anchoredPosition = new Vector2(-150, 0);
                    rightShadow.rectTransform.anchoredPosition = new Vector2(200, 0);

                    leftText.color = new Color(1, 1, 1, 0);
                    rightText.color = new Color(1, 1, 1, 0);

                    leftShadow.color = new Color(0, 0, 0, 0);
                    rightShadow.color = new Color(0, 0, 0, 0);


                    DOTween.Kill(leftText);
                    DOTween.Kill(rightText);
                    DOTween.Kill(rightText);
                    DOTween.Kill(rightShadow);

                    DOTween.Kill(rightText.rectTransform);
                    DOTween.Kill(rightShadow.rectTransform);


//                    leftText.rectTransform.DOAnchorPosX(150, 2);
                    leftShadow.rectTransform.DOAnchorPosX(150, 2);
//                    rightText.rectTransform.DOAnchorPosX(-50, 2);
                    rightShadow.rectTransform.DOAnchorPosX(-50, 2);


                    leftText.DOColor(new Color(1,1,1,1),2);
                    rightText.DOColor(new Color(1,1,1,1),2);
                    leftShadow.DOColor(new Color(0, 0, 0, 1), 2);
                    rightShadow.DOColor(new Color(0, 0, 0, 1), 2);



                }
            }
        }
    }

    public void HiddenHeadInfoLabel()
    {

        canveGroupFade.AlphaPlayBackward();
        DOTween.Kill(leftText);
        DOTween.Kill(rightText);
        DOTween.Kill(leftShadow);
        DOTween.Kill(rightText);
        DOTween.Kill(rightText.rectTransform);
        DOTween.Kill(rightText.rectTransform);

        leftShadow.rectTransform.DOAnchorPosX(-150, 1);
        rightShadow.rectTransform.DOAnchorPosX(200, 1);

        leftText.DOColor(new Color(1, 1, 1, 0), 1);
        rightText.DOColor(new Color(1, 1, 1, 0), 1);
        leftShadow.color = new Color(0, 0, 0, 0);
        rightShadow.color = new Color(0, 0, 0, 0);


    }

}
