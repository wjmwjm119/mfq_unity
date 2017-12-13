using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Events;


public class TouchCtrl:Graphic, IPointerDownHandler,IPointerUpHandler,IDragHandler,IPointerClickHandler
{
	public bool globalDebug;

    public CameraUniversalCenter cameraCenter;

    bool inTouch = false;
	bool inZoom=false;
    Vector2 firstPosition = new Vector2(0, 0);
    Vector2 secondPostion = new Vector2(0,0);
    float disVec2 = 0;
    float disVec2_first = 0;
    float zCountStart = 0;

    float xConutOffset = 0;
    float yConutOffset = 0;
    float zConutOffset = 0;

    float xConutOffsetSecond = 0;
    float yConutOffsetSecond = 0;

    //双击使用
    bool flip;
    float mTimeA;
    float mTimeB = -1000;
    Vector2 aClickPosition;
    Vector2 bClickPosition;

    Ray ray;
    RaycastHit hit;

    int count;
    int count2;

    float dpiScaleFactor = 1.0f;

    float touchDownStartTime;
//    float touchClickEndTime;

    //用来关闭缩略图用的
    public UnityEvent touchDownEvent;

//  [System.Serializable]
    public class ColliderTriggerEvent : UnityEvent<PointerEventData>
    {

    }

    public ColliderTriggerEvent hxfbColliderTriggerEvent;

    override protected void Awake()
    {
        hxfbColliderTriggerEvent = new ColliderTriggerEvent();
    }


    override protected void Start()
	{
        dpiScaleFactor = 100.0f / Screen.dpi;
        touchDownEvent = new UnityEvent();
    }

	public void OnPointerDown(PointerEventData eventData)
	{
        if(touchDownEvent!=null)
        touchDownEvent.Invoke();

        touchDownStartTime = Time.time;

        if (cameraCenter != null)
        {
            if (Input.touches.Length == 2)
            {
                zConutOffset = 0;
                disVec2 = new Vector2(Input.touches[0].position.x - Input.touches[1].position.x, Input.touches[0].position.y - Input.touches[1].position.y).magnitude;
                disVec2_first = disVec2;
                zCountStart = cameraCenter.currentCamera.Zcount;

                firstPosition = Input.touches[0].position;
                secondPostion = Input.touches[1].position;

                inZoom = true;
                inTouch = false;
                cameraCenter.currentCamera.inZoomState = true;
            }
            else if (Input.touches.Length == 1)
            {
                xConutOffset = 0;
                yConutOffset = 0;
                firstPosition = Input.touches[0].position;

                inZoom = false;
                cameraCenter.currentCamera.inZoomState = false;
                inTouch = true;
            }

            flip = !flip;

            if (flip)
            {
                aClickPosition = eventData.position;
                mTimeA = Time.time;
            }
            else
            {
                bClickPosition = eventData.position;
                mTimeB = Time.time;
            }

            if (Mathf.Abs(mTimeA - mTimeB) < 0.3f && Vector2.Distance(aClickPosition, bClickPosition) < 30)
            {
                hxfbColliderTriggerEvent.Invoke(eventData);
                cameraCenter.currentCamera.DoubleClick(eventData.position);

            }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

            xConutOffset = 0;
            yConutOffset = 0;
            firstPosition = eventData.position;

#endif

            cameraCenter.currentCamera.TouchDown(firstPosition);

        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (cameraCenter != null)
        { 
            inZoom = false;
            cameraCenter.currentCamera.inZoomState = false;
            inTouch = false;

            cameraCenter.currentCamera.TouchUp();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
/*
#if UNITY_EDITOR || UNITY_STANDALONE_WIN
        ClickPoint360Collider(eventData);
#endif
*/
        if(Time.time-touchDownStartTime<0.3f)
        {
            ClickPoint360Collider(eventData);
        }

    }


    public void OnDrag(PointerEventData eventData)
	{

        if (cameraCenter != null)
        {
            if (inTouch)
            {
                xConutOffset = Input.touches[0].position.y - firstPosition.y;
                yConutOffset = Input.touches[0].position.x - firstPosition.x;

                cameraCenter.currentCamera.TouchMove(firstPosition, new Vector2(-xConutOffset * 5 * dpiScaleFactor, yConutOffset * 5 * dpiScaleFactor));
            }
            else if (inZoom)
            {
                disVec2 = new Vector2(Input.touches[0].position.x - Input.touches[1].position.x, Input.touches[0].position.y - Input.touches[1].position.y).magnitude;
                zConutOffset = disVec2_first - disVec2;
                //zCountStart
                cameraCenter.currentCamera.SetCameraZoom(zCountStart + zCountStart * 0.01f * dpiScaleFactor * zConutOffset);

                //漫游上下抬头
                xConutOffset = Input.touches[0].position.y - firstPosition.y;
                yConutOffset = Input.touches[0].position.x - firstPosition.x;

                xConutOffsetSecond = Input.touches[1].position.y - secondPostion.y;
                yConutOffsetSecond = Input.touches[1].position.x - secondPostion.x;

                cameraCenter.currentCamera.TouchMoveForLookupAndLookDown(0.5f*(firstPosition+ secondPostion), new Vector2(-(xConutOffset+xConutOffsetSecond) * 5 * dpiScaleFactor, (yConutOffset+ yConutOffsetSecond) * 2.5f * dpiScaleFactor));
            }

#if UNITY_EDITOR || UNITY_STANDALONE_WIN

            xConutOffset = eventData.position.y - firstPosition.y;
            yConutOffset = eventData.position.x - firstPosition.x;
            cameraCenter.currentCamera.TouchMove(firstPosition, new Vector2(-xConutOffset * dpiScaleFactor, yConutOffset * dpiScaleFactor));

#endif

        }
    }

	public void ChangeSacleContentObject(ScaleInAndOut inContent)
	{
//		scaleInAndOutArea = inContent;
	}

	public void ChangeSpritePlayer(SpritePlayer sp)
	{
//		spritePlayer = sp;
	}

	void Update()
	{

        if (globalDebug)
        {
            GlobalDebug.ReplaceLine("inTouch:" + inTouch.ToString(),0);
            GlobalDebug.ReplaceLine("inZoom:" + inZoom.ToString(), 1);
            GlobalDebug.ReplaceLine("firstPosition:" + firstPosition.ToString(), 2);
            GlobalDebug.ReplaceLine("disVec2_first:" + disVec2_first.ToString(), 3);
            GlobalDebug.ReplaceLine("disVec2:" + disVec2.ToString(), 4);
            GlobalDebug.ReplaceLine("dpi:" + Screen.dpi.ToString(), 5);
        }


#if UNITY_EDITOR || UNITY_STANDALONE_WIN

        if (cameraCenter!=null&&cameraCenter.currentCamera != null)
        {
            cameraCenter.currentCamera.MouseScroll(-Input.mouseScrollDelta.y);
        }
#endif


    }

    public void ZoomCamera(float inZoomOffset)
	{
        //		float scaleFactor=Mathf.Clamp(orginView+inZoomOffset * 0.05f,minMaxCameraView.x,minMaxCameraView.y);
        //		needScaleCamera.fieldOfView = scaleFactor;
    }


    public void ClickPoint360Collider(PointerEventData p)
    {
        if (cameraCenter != null)
        {
            ray = cameraCenter.currentCamera.GetComponent<Camera>().ScreenPointToRay(p.position);

            if (Physics.Raycast(ray, out hit, 1000))
            {
                ColliderTriggerButton c = hit.transform.GetComponent<ColliderTriggerButton>();
                if (c!= null)
                {
                    c.ExeTriggerEvent();
                }       
            }
        }
    }
}









