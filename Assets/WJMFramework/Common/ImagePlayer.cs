using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class ImagePlayer : MonoBehaviour
{
    public bool orderTextureGorup;

    public Text tiShiInfo;

    public Sprite[] pageGroup;
    public Image topImage;
    public Image buttomImage;

    public int currentPageNo;




	void Start ()
    {
        if (orderTextureGorup)
        {
            pageGroup = OrderTexture.OrderTexture2D(pageGroup, 2);
        }

    }


    public void ToImage(int to)
    {
        currentPageNo = to;
        //        topImage.sprite = pageGroup[to];
        //        buttomImage.sprite = pageGroup[to];
 //       Debug.Log("dddd");
        topImage.DOFade(0, 0.3f);
        buttomImage.sprite = pageGroup[currentPageNo];
        buttomImage.DOFade(1, 0.3f).OnComplete(SetTopImage);
    }

    public void NextPage()
    {

        if (currentPageNo < pageGroup.Length - 1)
        {
            currentPageNo++;
            topImage.DOFade(0, 0.3f);
            buttomImage.sprite = pageGroup[currentPageNo];
            buttomImage.DOFade(1, 0.3f).OnComplete(SetTopImage);
        }
        else
        {
            tiShiInfo.text = "已到最后一页";
            tiShiInfo.DOFade(1, 0.3f).OnComplete(InfoFadeOut);
        }


    }







    public void Reset()
    {
        currentPageNo = 0;
        topImage.sprite = pageGroup[0];
        buttomImage.sprite = pageGroup[0];
    }



    public void LastPage()
    {

        if (currentPageNo > 0)
        {
            currentPageNo--;
            topImage.DOFade(0, 0.3f);
            buttomImage.sprite = pageGroup[currentPageNo];
            buttomImage.DOFade(1, 0.3f).OnComplete(SetTopImage);
        }
        else
        {
            tiShiInfo.text = "已到第一页";
            tiShiInfo.DOFade(1, 0.3f).OnComplete(InfoFadeOut);
        }

       

    }

     void   SetTopImage()
    {
        topImage.color = new Color(1, 1, 1, 1);
        topImage.sprite = pageGroup[currentPageNo];
    }

    void SetButtomImage()
    {
        buttomImage.sprite = pageGroup[currentPageNo];
    }


    void InfoFadeOut()
    {
        tiShiInfo.DOFade(0, 0.3f).SetDelay(2);
    }




}
