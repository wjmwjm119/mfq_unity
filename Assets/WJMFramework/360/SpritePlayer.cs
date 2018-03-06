using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class SpritePlayer : Image, IPointerDownHandler, IPointerUpHandler, IDragHandler, IPointerClickHandler
{

    public int numLength = 3;

    public Sprite nullSprite;
    public bool reverse;
    public Sprite[] spriteSequence;

    public bool play = false;
    public int waitPerTime = 2;
    public float playSpeed = 1;
//  public Image image;

    int currentNo;

	public bool run360;

	public bool useUpAndDown;
    public bool moveLoop=false;
    public float moveSpeed=0.1f;
    int addOffsetNo;
    int finalCount;

	public int defaultStartNo=0;
    Vector2 firstPosition = new Vector2(0, 0);
    Vector2 secondPostion = new Vector2(0, 0);

    Vector2 moveOffset;

    float playTime;

    public void OrderSprite(int inNumLength)
    {
        numLength = inNumLength;
        spriteSequence = OrderTexture.OrderTexture2D(spriteSequence, numLength);
    }

    void Update()
    {
        if (play)
        {
            playTime += Time.deltaTime;

            currentNo = (int)(playSpeed * playTime * 24) % (spriteSequence.Length + waitPerTime * 24);

            if (currentNo < spriteSequence.Length)
            {
                sprite = spriteSequence[currentNo];
            }
            else if (currentNo >= spriteSequence.Length)
            {
                playTime = 0.0f;
            }
        }
    }

    public void AlphaPlayForward()
    {
        this.DOColor(new Color(1, 1, 1, 1), 0.3f);
		currentNo = defaultStartNo;
        this.sprite = spriteSequence[currentNo];
        play = true;
    }

    public void AlphaPlayBack()
    {
        this.DOColor(new Color(1, 1, 1, 0), 0.3f);
        this.sprite = nullSprite;
        play = false;
    }


	//序列帧使用
	public void AlphaPlayForward360()
	{
        raycastTarget = true;

        currentNo = defaultStartNo;
        this.sprite = spriteSequence[currentNo];
        this.DOColor(new Color(1, 1, 1, 1), 0.3f);
		run360 = true;

	}
	//序列帧使用
	public void AlphaPlayBack360()
	{
        raycastTarget = false;

        this.sprite = nullSprite;
        this.DOColor(new Color(0, 0, 0, 0), 0.3f);
		run360 = false;

	}




    public void ResetPlayer()
    {
		currentNo=defaultStartNo;
        this.sprite = spriteSequence[currentNo];
    }


	public void SetDefaultStartNo(int initlStartNo)
	{
		defaultStartNo = initlStartNo;
	}

	public void SetPlayerNo(int pageID)
	{
		currentNo = pageID;
		this.sprite = spriteSequence[pageID];
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


        public void OnPointerClick(PointerEventData eventData)
        {

        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!run360)
                return;

            if (eventData.pointerId == 0 || eventData.pointerId == -1)
            {
                firstPosition = eventData.position;
            }
            else if (eventData.pointerId == 1)
            {
                secondPostion = eventData.position;
            }

        }

        public void OnPointerUp(PointerEventData eventData)
        {
        if (!run360)
            return;

        currentNo = finalCount;

        addOffsetNo = 0;
    }

        public void OnDrag(PointerEventData eventData)
        {

        if (!run360)
            return;

        if (eventData.pointerId == 0 || eventData.pointerId == -1)
        {
            moveOffset = new Vector2(eventData.position.x - firstPosition.x, eventData.position.y - firstPosition.y);
        }



        if (useUpAndDown)
        {
            addOffsetNo = (int)(-moveOffset.y * moveSpeed * 0.25f);
        }
        else
        {
            if (reverse)
            {
                addOffsetNo = (int)(-moveOffset.x * moveSpeed);
            }
            else
            {
                addOffsetNo = (int)(moveOffset.x * moveSpeed);

            }

        }


        finalCount = currentNo + addOffsetNo;

        if (moveLoop)
        {
            finalCount = finalCount % (spriteSequence.Length - 1);
            if (finalCount < 0)
            {
                finalCount += (spriteSequence.Length - 1);
            }
        }
        else
        {
            finalCount = Mathf.Clamp(finalCount, 0, spriteSequence.Length - 1);
        }



        this.sprite = spriteSequence[finalCount];

    }


}
