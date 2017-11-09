using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using UnityEngine.Events;

public class TouchCtrl:Graphic, IPointerDownHandler,IPointerUpHandler,IDragHandler
{
	public bool globalDebug;

    public CameraUniversalCenter cameraCenter;

    bool inTouch = false;
	bool inZoom=false;
    Vector2 firstPosition = new Vector2(0, 0);
    float disVec2 = 0;
    float disVec2_first = 0;
    float zCountStart = 0;

    float xConutOffset = 0;
    float yConutOffset = 0;
    float zConutOffset = 0;

    //双击使用
    bool flip;
    float mTimeA;
    float mTimeB = -1000;
    Vector2 aClickPosition;
    Vector2 bClickPosition;

    int count;
    int count2;

    float dpiScaleFactor = 1.0f;

    //用来关闭缩略图用的
    public UnityEvent touchDownEvent;

//  [System.Serializable]
    public class DoubleClickEvent : UnityEvent<PointerEventData>
    {

    }

    public DoubleClickEvent doubleClickEvent;

    override protected void  Start()
	{
        dpiScaleFactor = 100.0f / Screen.dpi;
        touchDownEvent = new UnityEvent();
        doubleClickEvent = new DoubleClickEvent();
    }

	public  void OnPointerDown(PointerEventData eventData)
	{
  //      doubleClickEvent.is
        touchDownEvent.Invoke();

        if (cameraCenter != null)
        {
            if (Input.touches.Length == 2)
            {
                zConutOffset = 0;
                disVec2 = new Vector2(Input.touches[0].position.x - Input.touches[1].position.x, Input.touches[0].position.y - Input.touches[1].position.y).magnitude;
                disVec2_first = disVec2;
                zCountStart = cameraCenter.currentCamera.Zcount;

                inZoom = true;
                inTouch = false;
            }
            else if (Input.touches.Length == 1)
            {
                xConutOffset = 0;
                yConutOffset = 0;
                firstPosition = Input.touches[0].position;

                inZoom = false;
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
                doubleClickEvent.Invoke(eventData);
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
            inTouch = false;

            cameraCenter.currentCamera.TouchUp();
        }
    }

	public  void OnDrag(PointerEventData eventData)
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

}









