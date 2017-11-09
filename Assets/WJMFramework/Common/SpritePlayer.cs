using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class SpritePlayer : MonoBehaviour
{
	
    public bool play = false;
    public int waitPerTime = 2;
    public float playSpeed = 1;
    public Image image;



    int currentNo;

    public bool needOrder = false;
    public int numCount = 3;
    public Sprite[] spriteSequence;

	public bool run360;
	public bool reverse;
	public bool useUpAndDown;
    public bool moveLoop=false;
    public float moveSpeed=0.1f;
    int addOffsetNo;
    int finalCount;

	int defaultStartNo=0;




    void Start()
    {
        if(needOrder)
        spriteSequence = OrderTexture.OrderTexture2D(spriteSequence, numCount);

    }

    void Update()
    {
        if (play)
        {
            currentNo = (int)(playSpeed * Time.time * 24) % (spriteSequence.Length + waitPerTime * 24);
            if (currentNo < spriteSequence.Length)
            {
                image.sprite = spriteSequence[currentNo];
            }
        }
    }

    public void AlphaPlayForward()
    {
        image.DOColor(new Color(1, 1, 1, 1), 0.3f);
		currentNo = defaultStartNo;
        play = true;
    }

    public void AlphaPlayBack()
    {
        image.DOColor(new Color(1, 1, 1, 0), 0.3f);
		currentNo = defaultStartNo;
        play = false;
    }


	//序列帧使用
	public void AlphaPlayForward2()
	{
		image.DOColor(new Color(1, 1, 1, 1), 0.3f);
		currentNo = defaultStartNo;
		image.sprite = spriteSequence [currentNo];
		run360 = true;
	}
	//序列帧使用
	public void AlphaPlayBack2()
	{
		image.DOColor(new Color(0, 0, 0, 0), 0.3f);
		currentNo = defaultStartNo;
//		if(spriteSequence.Length>0)
//		image.sprite = spriteSequence [0];
		run360 = false;
	}




    public void ResetPlayer()
    {
		currentNo=defaultStartNo;
        image.sprite = spriteSequence[currentNo];
    }


	public void SetDefaultStartNo(int initlStartNo)
	{
		defaultStartNo = initlStartNo;
	}

	public void SetPlayerNo(int pageID)
	{
		currentNo = pageID;
		image.sprite = spriteSequence[pageID];
	}

    public void TouchDown()
    {
		if (!run360)
			return;
//		Debug.Log (111);
    }

    public void TouchUp()
    {
		if (!run360)
			return;

	        currentNo=finalCount;

	        addOffsetNo=0;
	//		Debug.Log (222);

    }

    public void TouchMove(Vector2 moveOffset )
    {
		if (!run360)
			return;


		if (useUpAndDown)
		{
			addOffsetNo = (int)(-moveOffset.x * moveSpeed*0.25f);
		} 
		else
		{	if (reverse)
			{
				addOffsetNo = (int)(-moveOffset.y * moveSpeed);
			} 
			else 
			{
			addOffsetNo = (int)(moveOffset.y * moveSpeed);
				
			}
			
		}
			

			finalCount = currentNo + addOffsetNo;

			if (moveLoop) {
				finalCount = finalCount % (spriteSequence.Length - 1);
				if (finalCount < 0) {
					finalCount += (spriteSequence.Length - 1);
				}
			} else 
		{
				finalCount = Mathf.Clamp (finalCount, 0, spriteSequence.Length - 1);
			}



			image.sprite = spriteSequence [finalCount];


    }

	public void SetUpAndDown()
	{
		useUpAndDown = true;
		moveLoop = false;
	}
	public void CleanUpAndDown()
	{
		useUpAndDown = false;
		moveLoop = true;
	}

}
