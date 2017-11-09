using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using DG.Tweening;


public class SenceLoadingAnimation : MonoBehaviour
{

    public Text percentText;
    public Image upBlock;
    public Image downBlock;
    public SpritePlayer spritePlayer;
    public bool isPlaying;

    float loadPercent;
    float targetPercent;

//   float height = 0;


    void Start()
    {


    }


    public void LoadingAnimation(float hasLoadPercent, float deltaTime)
    {
        targetPercent = hasLoadPercent;
        if (hasLoadPercent < 1 && !isPlaying)
        {
            isPlaying = true;
            upBlock.rectTransform.DOSizeDelta(new Vector2(1920f, 541f), 0.3f);
            downBlock.rectTransform.DOSizeDelta(new Vector2(1920f, 541f), 0.3f);
            spritePlayer.AlphaPlayForward();
        }
        else if (hasLoadPercent == 1)
        {
   

        }

    }


    void Update()
    {




        if (isPlaying)
        {

            loadPercent = Mathf.Lerp(loadPercent, targetPercent, 0.2f);
            percentText.text = ((int)(loadPercent * 100 + 1f)).ToString() + "%";

            if (loadPercent > 0.99f)
            {
                percentText.text = "100%";

                isPlaying = false;
                loadPercent = 0;
                targetPercent = 0;
                upBlock.rectTransform.DOSizeDelta(new Vector2(1920f, 0f), 0.3f);
                downBlock.rectTransform.DOSizeDelta(new Vector2(1920f, 0f), 0.3f);
                spritePlayer.AlphaPlayBack();

            }


        }

    }





}

