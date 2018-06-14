using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using DG.Tweening;

public class ScaleImage : RawImage, IPointerDownHandler,IPointerUpHandler, IDragHandler,IPointerClickHandler
{

//  public bool gf55;
//  public bool globalDebug;

    public ImageButton closeImageBtn;
    public RectTransform canvaRect;

    bool inTouch = false;
    bool inZoom = false;
    Vector2 firstPosition = new Vector2(0, 0);
    float disVec2 = 0;
    float disVec2_first = 0;
//  float zCountStart = 0;

    float xConutOffset = 0;
    float yConutOffset = 0;
    float zConutOffset = 0;
    float dpiScaleFactor = 1.0f;

    //双击使用
    bool flip;
    float mTimeA;
    float mTimeB = -1000;
    Vector2 aClickPosition;
    Vector2 bClickPosition;

    Vector2 orginPosition;

    float orginScale;

    Vector2 lastPosition;

    float lastScale;

    Vector2 scaleMinMax;

    public string btnNameForRemote;

    float onDownTime;
//    float onClickTime;


    public void SetImage(Texture2D sp)
    { 
        //缩放默认值
        scaleMinMax = new Vector2(1f, 8);
        //计算scaleMinMax,缩放最小到屏幕的二分之一
        float screenMin = Mathf.Min(Screen.width, Screen.height);
        float imageMin = Mathf.Min(sp.width, sp.height);
        float minScale = 0.5f * screenMin / imageMin;
        float maxScale = Mathf.Max(minScale * 4, scaleMinMax.y);

        scaleMinMax = new Vector2(minScale, maxScale);
        
            //        if (sp != null)
            //        {
            texture = sp;
            rectTransform.sizeDelta = new Vector2(sp.width, sp.height);
            FullScreen();
//        }
    }

    public void ChangeScaleMinMax(Vector2 minAndMax)
    {
        scaleMinMax = minAndMax;
    }

    void RecordOrginState()
    {
        orginPosition = rectTransform.anchoredPosition;
        orginScale = rectTransform.localScale.x;
    }

    void RecordLastState()
    {
        lastPosition = rectTransform.anchoredPosition;
        lastScale = rectTransform.localScale.x;
    }


    override protected void Start()
    {
        dpiScaleFactor = 100.0f / Screen.dpi;
        RecordOrginState();
        RecordLastState();

        RemoteGather.SetupRemoteGather();
        RemoteGather.AddSacleImageToGroup(this);
    }



    public void FullScreen()
    {

        bool landscape = Screen.width > Screen.height ? true : false;
//      Debug.Log(Screen.width);
//      Debug.Log(Screen.height);
//      Debug.Log(sprite.texture.height);
//      Debug.Log(sprite.texture.height);

        float needScale;

        if (landscape)
        {
            needScale = Screen.width / (texture.width * canvaRect.localScale.x);
            if (needScale * texture.height > Screen.height)
            {
                needScale = Screen.height / (texture.height * canvaRect.localScale.x);
            }
        }
        else
        {
            needScale = Screen.height / (texture.height * canvaRect.localScale.x);
            if (needScale * texture.width > Screen.width)
            {
                needScale = Screen.width / (texture.width * canvaRect.localScale.x);
            }
        }

        rectTransform.DOScale(new Vector3(needScale, needScale, 1), 0.5f);
        rectTransform.DOAnchorPos(orginPosition, 0.5f).OnComplete(RecordLastState);

    }


    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - onDownTime < 0.1f)
        {
            if(closeImageBtn!=null)
            closeImageBtn.SetBtnStateForRemote(true, 0);
//          GlobalDebug.Addline((Time.time - onDownTime).ToString());
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {

        onDownTime = Time.time;

        RemoteGather.currentCtrlScaleImage = this;


        if (Input.touches.Length == 2)
        {
            zConutOffset = 0;
            disVec2 = new Vector2(Input.touches[0].position.x - Input.touches[1].position.x, Input.touches[0].position.y - Input.touches[1].position.y).magnitude;
            disVec2_first = disVec2;
//          zCountStart = cameraCenter.currentCamera.GetComponent<CameraBase>().Zconut;


            inZoom = true;
            inTouch = false;
        }
        else if (Input.touches.Length == 1)
        {
            xConutOffset = 0;
            yConutOffset = 0;
            firstPosition = Input.touches[0].position;

            GlobalDebug.Addline("TouchIn!");

            inZoom = false;
            inTouch = true;
        }


        #if UNITY_EDITOR || UNITY_STANDALONE_WIN

                xConutOffset = 0;
                yConutOffset = 0;
                firstPosition = eventData.position;


        this.DOColor(new Color(0.8f, 0.8f, 0.8f), 0.2f).OnComplete(NormalColor);

        #endif


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
            GlobalDebug.Addline("DoubleClick!");
            FullScreen();
        }


    }

    public void OnPointerUp(PointerEventData eventData)
    {
        inZoom = false;
        inTouch = false;
        RecordLastState();

    }

    public void OnDrag(PointerEventData eventData)
    {

        if (inTouch)
        {
            xConutOffset = Input.touches[0].position.y - firstPosition.y;
            yConutOffset = Input.touches[0].position.x - firstPosition.x;

            Vector2 offsetPosition2 = new Vector2(yConutOffset, xConutOffset);
            rectTransform.DOAnchorPos(offsetPosition2 + lastPosition, 0.2f);

        }
        else if (inZoom)
        {
            disVec2 = new Vector2(Input.touches[0].position.x - Input.touches[1].position.x, Input.touches[0].position.y - Input.touches[1].position.y).magnitude;
            zConutOffset = disVec2- disVec2_first ;

            float scaleFade =Mathf.Clamp(lastScale+ 0.05f *dpiScaleFactor * zConutOffset,scaleMinMax.x,scaleMinMax.y);
            rectTransform.DOScale( new Vector3(scaleFade, scaleFade, 1), 0.2f);

        }


        #if UNITY_EDITOR || UNITY_STANDALONE_WIN

                xConutOffset = eventData.position.y - firstPosition.y;
                yConutOffset = eventData.position.x - firstPosition.x;

                Vector2 offsetPosition = new Vector2(yConutOffset, xConutOffset);
                rectTransform.DOAnchorPos(offsetPosition + lastPosition, 0.2f);

        #endif


    }



    void NormalColor()
    {
        this.DOColor(new Color(1.0f, 1.0f, 1.0f), 0.2f);
    }

    public void ResetToInit()
    {
        rectTransform.anchoredPosition = orginPosition;
        rectTransform.localScale =new Vector3( orginScale,orginScale,orginScale);
    }

    public float[] GetState()
    {
        return new float[3] { rectTransform.anchoredPosition.x, rectTransform.anchoredPosition.y, rectTransform.localScale.x };
    }

    public void SetState(float[] inState)
    {
        rectTransform.DOAnchorPos(new Vector2(inState[0], inState[1]), 0.5f);
//      rectTransform.anchoredPosition = new Vector2(inState[0], inState[1]);
        rectTransform.localScale = new Vector3(inState[2], inState[2], 1);
    }

}

	







