using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using DG.Tweening;

public class ImagePlayer2 : MonoBehaviour
{

    public List<NetTexture2D> netTexture2DGroup;

//  public Texture2D[] pageGroup;

    public ScaleImage scaleImage;

    public CanveGroupFade nextBtn;
    public CanveGroupFade preBtn;

    public int currentPageNo;
    int lastPageNo;

    public void OpenImagePlayer()
    {
        if (netTexture2DGroup.Count > 0)
        {
            ToImage(0);
            if (netTexture2DGroup.Count > 1)
            {
                nextBtn.AlphaPlayForward();
                preBtn.AlphaPlayBackward();
            }
        }
        else
        {
            scaleImage.FullScreen();
        }
    }


    public void ToImage(int to)
    {
        lastPageNo = currentPageNo;
        netTexture2DGroup[lastPageNo].scaleImage = null;

        currentPageNo = to;
        scaleImage.SetImage(netTexture2DGroup[currentPageNo].LoadTexture2D(scaleImage));

    }

    public void NextPage()
    {
        lastPageNo = currentPageNo;
        netTexture2DGroup[lastPageNo].scaleImage = null;

        if (currentPageNo < netTexture2DGroup.Count - 1)
        {
            currentPageNo++;
            scaleImage.SetImage(netTexture2DGroup[currentPageNo].LoadTexture2D(scaleImage));
        }
        
        if(currentPageNo== netTexture2DGroup.Count - 1)
        nextBtn.AlphaPlayBackward();
            
        
        preBtn.AlphaPlayForward();
    }




    public void CloseImagePlayer()
    {
        currentPageNo = 0;
        lastPageNo = 0;
        nextBtn.AlphaPlayBackward();
        preBtn.AlphaPlayBackward();

        foreach (NetTexture2D n in netTexture2DGroup)
        {
            n.UnloadTexture();
        }

    }



    public void PrePage()
    {
        lastPageNo = currentPageNo;
        netTexture2DGroup[lastPageNo].scaleImage = null;


        if (currentPageNo > 0)
        {
            currentPageNo--;
            scaleImage.SetImage(netTexture2DGroup[currentPageNo].LoadTexture2D(scaleImage));
        }

        if (currentPageNo == 0)
            preBtn.AlphaPlayBackward();
        

        nextBtn.AlphaPlayForward();
    }

    



}
