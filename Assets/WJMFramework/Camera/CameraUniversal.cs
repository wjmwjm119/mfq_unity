using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;


public class CameraUniversal : MonoBehaviour
{


    public bool cameraEnableState;
    public bool useCamreaDepth;

    public bool resetXcountWhenTouchup = false;
    public bool resetCameraStateWhenDisable=true;

    public bool XAxes = true;
    public bool YAxes = true;
    public bool Zpush = true;

    public float Xcount = 0f;//可用于初始化
    public float Ycount = 0f;//可用于初始化
    public float Zcount = 0f;//可用于初始化

    public float smoothSpeed = 5F;

    public float minimumX = 5f;
    public float maximumX = 80f;

    public float minimumZ = 5f;
    public float maximumZ = 60f;

    public float sensitivityX = 10f;
    public float sensitivityY = 10f;
    public float sensitivityZ = 15f;

    public float zhiBeiZhenCorrect;//调整指北针的角度

    public AnimationClip autoPlayClip;
    Animation myAnimation;

    public Transform camBase;
    public Transform universalY;
    public Transform universalX;

    public bool uselookAtSphere = true;

    protected float Xsmooth = 0.0f;
    protected float Ysmooth = 0.0f;
    protected float Zsmooth = 0.0f;

    /// <summary>
    /// 相机一开始的默认位置
    /// </summary>
    protected float defaultXcount;
    protected float defaultYcount;
    protected float defaultZcount;
    protected Vector3 defaultPosition;

    protected float countXBegin = 0;
    protected float countYBegin = 0;
    protected float countZBegin = 0;
    protected float offsetZcount = 0;

    protected bool pressed;
    protected Vector2 currentTouchPos;

    public bool spaceLookAtCanMove;//是否能在一个面上滑
    public Collider colliderMeshObject;
    public float flySpeedPerSecond = 5;

    Ray ray;
    RaycastHit hit;
    bool hasInit = false;

    public TweenCallback onEnd;

    public float firstPersonHorizontal;
    public float firstPersonVertical;
    float smoothFirstPersonVertical;

    public string currentAtRoom;
    public Text roomInfo;

    public string sleepPosAndXYZcountStr=",,,,,";
    public float maxTimeToSleep=120;
    public float sleepTime;
    public bool hasSleep;

    public bool playAnimationPath;

    public bool changingCamera;

    string defaultCameraPositionAndXYZCount;
    public string camName;

    int orginCullMask;
    CameraClearFlags orginCameraCleraFlags;

    //如果是在镜像户型下用来设置初始位置用
    Vector3 hxMirrorOffset;

    [HideInInspector]
    public bool vrMoveForward = false;

    public bool isARCamera = false;


    public void SetYSmooth(float inF)
    {
        Ysmooth = inF ;
    }


public  void TouchDown(Vector2 pos)
    {

        if (cameraEnableState)
        {
            SleepWake();

            pressed = true;
            currentTouchPos = pos;
            countXBegin = Xcount;
            countYBegin = Ycount;
        //	countZBegin = Zcount;
        }

    }

    public  void TouchUp()
    {  
        if (cameraEnableState)
        {
            SleepWake();
            pressed = false;
            if (resetXcountWhenTouchup)
            {
                Xcount = 0;
            }
        }
    }

    public  void TouchMove(Vector2 pos, Vector2 offset)
    {
        

        if (cameraEnableState&& pressed)
        {
            SleepWake();
            currentTouchPos = pos;
            Xcount = countXBegin + 0.02f * offset.x * sensitivityX;
            Ycount = countYBegin + 0.02f * offset.y * sensitivityY;
        }
    }

    public  void MouseScroll(float scroll)
    {
        if (cameraEnableState)
        {
            Zcount += 0.2f * scroll * sensitivityZ;
        }
    }

    public  void DoubleClick(Vector2 pos)
    {
        if (cameraEnableState & spaceLookAtCanMove)
        {
            ray = GetComponent<Camera>().ScreenPointToRay(pos);

            if (colliderMeshObject.Raycast(ray, out hit, 800))
            {

                if (hit.transform.gameObject.name == "ColliderPlan")
                {
                    Debug.DrawLine(ray.origin, hit.point);
                    Vector3 toPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
//                    Debug.Log("MoveCameraPos");
                    MoveCameraPos(toPos);
                }
            }
        }
    }

    void Start()
    {
        InitlCamera();
    }

    void InitlCamera()
    {
        if (!hasInit)
        {

            if (camBase != null && camBase.GetComponent<CharacterController>() != null)
            {
                camBase.GetComponent<CharacterController>().height = 0.02f;
                camBase.GetComponent<CharacterController>().radius = 0.02f;
                camBase.GetComponent<CharacterController>().stepOffset = 0.01f;
                camBase.GetComponent<CharacterController>().center = new Vector3(0, -1000, 0);
            }
                

            hasInit = true;

            camName = transform.name;
            if (useCamreaDepth)
                GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;

            RecodeInitialCameraState();
            ResetCameraStateToInitial();
            onEnd = EnableCameraCtrl;

        }
    }

    public void InitlCameraInEditor()
    {
        InitlCamera();
        hasInit = false;
    }

    public void InitlCameraInRuntime()
    {
        InitlCamera();
    }

    void SleepWake()
    {
        sleepTime = 0;
        if (hasSleep)
        {
            hasSleep = false;
            Debug.Log("wakeUP");
        }
    }

    void OnSleep()
    {
        hasSleep = true;
        Debug.Log("sleep");

        SetCameraPositionAndXYZCount(sleepPosAndXYZcountStr);
    }

    void Update()
    {
        if(!playAnimationPath&&maximumZ>1)
        sleepTime += Time.deltaTime;

        if (sleepTime > maxTimeToSleep)
        {
            if(!hasSleep)
            OnSleep();

            Ycount += 0.1f;

        }
        
        if(!isARCamera)
        UpdatePosition();

        if (pressed&& maximumZ < 0.1f)
        {
            if (currentTouchPos.y < 0.3f * Screen.height)
            {
                firstPersonVertical = -1;
            }
            else
            {
                firstPersonVertical = 1;
            }
        }
        else
        {
            firstPersonVertical = 0;
        }

        if (vrMoveForward)
        {
            firstPersonVertical = 0.8f;
        }


        smoothFirstPersonVertical = Mathf.Lerp(smoothFirstPersonVertical, firstPersonVertical, 0.1f);

        if(camBase.GetComponent<CharacterMotor>()!=null)
        camBase.GetComponent<CharacterMotor>().firstPersonVertical = smoothFirstPersonVertical;

        if (uselookAtSphere&&!playAnimationPath)
        {
            universalY.position = camBase.position - Zsmooth * 0.3f * transform.forward;
        }
        else
        {
            universalY.position = camBase.position;
        }

        if (Input.GetKey(KeyCode.A))
        {
            Ycount += 0.5f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            Ycount -= 0.5f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            Xcount += 0.5f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            Xcount -= 0.5f;
        }
    }

    /// <summary>
    /// 双击移动用
    /// </summary>
    /// <param name="toPos"></param>
    /// <param name="endEnableCtrl"></param>
    void MoveCameraPos(Vector3 toPos,float customFlyTime=-1, bool endEnableCtrl = true)
    {
        DisableCameraCtrl();
        float flyUseTime = GetFlyTime(Vector3.Magnitude(camBase.position - toPos));
        if (customFlyTime > -1)
            flyUseTime =customFlyTime;
        flyUseTime = Mathf.Max(0.5f, flyUseTime);
        //        Debug.Log("FlyUseTime:" +flyUseTime);
        //        Debug.Log("toPos:" + toPos);

        DOTween.Kill(camBase.transform);
        if (endEnableCtrl)
        {
            camBase.transform.DOLocalMove(toPos, flyUseTime).SetEase(Ease.OutQuad).OnComplete(onEnd);
        }
        else
        {
            camBase.transform.DOLocalMove(toPos, flyUseTime).SetEase(Ease.OutQuad);
        }
    }

    float ModiferYCount(float currentYcount,float toYcount)
    {

//      Debug.Log(currentYcount);
//      Debug.Log(toYcount);

        float outYcount;
        float yLess360 = toYcount % 360;
        float currentLess360 = currentYcount % 360;

        if (yLess360 < 0)
        {
            yLess360 += 360;
        }

        if (currentLess360 < 0)
        {
            currentLess360 += 360;
        }

        if (currentLess360 > yLess360)
        {
            if (currentLess360 - yLess360 > 180)
            {
                yLess360 = yLess360 + 360;
            }
            outYcount = currentYcount + yLess360 - currentLess360;

        }
        else
        {
            if (currentLess360 - yLess360 < -180)
            {
                currentLess360 = currentLess360 + 360;
            }
            outYcount = currentYcount - (currentLess360 - yLess360);
        }
//        Debug.Log(outYcount);

        return outYcount;
    }

    public void CameraPathAutoPlay()
    {
        SleepWake();
        if (autoPlayClip != null)
        {

            universalX.localRotation = Quaternion.Euler(0, 0, 0);
            camBase.localRotation = Quaternion.Euler(0, 0, 0);
            transform.localPosition = new Vector3(0, 0, -0.001f);


            if (myAnimation == null)
            {
                myAnimation = camBase.gameObject.AddComponent<Animation>() as Animation;
            }

            myAnimation.wrapMode = WrapMode.Loop;
            myAnimation.AddClip(autoPlayClip, "autoPlay");
            myAnimation.Play("autoPlay");
            playAnimationPath = true;
        }
    }

    public void CameraPathStopPlay()
    {
        SleepWake();
        Debug.Log("StopCamera");
        if (myAnimation != null)
        {
            myAnimation.Stop("autoPlay");
        }
        playAnimationPath = false;

        transform.localPosition = Vector3.zero;
        transform.localEulerAngles = Vector3.zero;

        Ycount = camBase.localRotation.eulerAngles.y;
        Ysmooth = Ycount;

    }

    public  void DisableCameraCtrl()
    {
        cameraEnableState = false;
    }

    public  void EnableCameraCtrl()
    {
        TouchUp();
        cameraEnableState = true;
    }

    /// <summary>
    /// 23,34,43,15,16,17
    /// </summary>
    /// <param name="pos"></param>
    public void SetCameraPositionAndXYZCount(string posAndXYZcount, float customFlyTime = -1)
    {
//        Debug.Log(posAndXYZcount);

        string[] s = posAndXYZcount.Split(',');

        if (s.Length == 6)
        {
            SetCameraPositionAndXYZCountAllArgs(s[0], s[1], s[2], s[3], s[4], s[5],customFlyTime);
        }
    }

    public void SetCameraPositionAndXYZCountAllArgs( string xPos,string yPos,string zPos,string xCount,string yCount,string zCount, float customFlyTime = -1)
    {

        float xP, yP, zP;
        float xC, yC, zC;

        bool b1, b2, b3, b4, b5, b6;

        b1 = float.TryParse(xPos, out xP);
        b2 = float.TryParse(yPos, out yP);
        b3 = float.TryParse(zPos, out zP);
        b4 = float.TryParse(xCount, out xC);
        b5 = float.TryParse(yCount, out yC);
        b6 = float.TryParse(zCount, out zC);

        if (b5)
        {
//            Debug.Log("ddd");
            yC = ModiferYCount(Ysmooth, yC);
        }
            
        Vector3 toPos = new Vector3(b1 ? xP : camBase.localPosition.x, b2 ? yP : camBase.localPosition.y, b3 ? zP : camBase.localPosition.z);

        if (customFlyTime==0)
        {
            camBase.localPosition = toPos;
            if (b4)
            {
                Xcount = xC;
                Xsmooth = Xcount;
            }

            if (b5)
            {
                Ycount = yC;
                Ysmooth = Ycount;
            }

            if (b6)
            {
                Zcount = zC;
                Zsmooth = Zcount;
            }
            camBase.localEulerAngles = new Vector3(0, Ysmooth, 0);
            universalX.localEulerAngles = new Vector3(Xsmooth, 0, 0);
            transform.localPosition = new Vector3(0, 0, -Zsmooth);

            if (uselookAtSphere)
            {
                universalY.position = camBase.position - Zcount * 0.3f * transform.forward;
            }
            else
            {
                universalY.position = camBase.position;
            }
            EnableCameraCtrl();
        }
        else
        {
            MoveCameraPos(toPos,customFlyTime);
            if (b4)
            {             
                Xcount = xC;
            }
            if (b5)
            {
                Ycount = yC;
            }
            if (b6)
            {
                Zcount = zC;
            }
        }
    }

    public void SetCameraPositionAndXYZCount(float[] inState)
    {
        DOTween.Kill(camBase.transform);
        camBase.DOLocalMove(new Vector3(inState[0], inState[1], inState[2]), 0.5f);
//      camBase.localPosition =

        Xcount = inState[3];
        Ycount = inState[4];
        Zcount = inState[5];
    }


    public void SetCameraZoom(float z)
    {
        Zcount = z;
    }

    public void GetRoomName(string inName)
    {
        currentAtRoom = inName;
    }

    public float GetZsmooth()
    {
        return Zsmooth;
    }

    public float GetXsmooth()
    {
        return Xsmooth;
    }

    public float GetYsmooth()
    {
        return Ysmooth;
    }

    public float[] GetCameraState()
    {
        float[] cameraStates = new float[6];
        cameraStates[0] = camBase.localPosition.x;
        cameraStates[1] = camBase.localPosition.y;
        cameraStates[2] = camBase.localPosition.z;
        cameraStates[3] = Xcount;
        cameraStates[4] = Ycount;
        cameraStates[5] = Zcount;

        return cameraStates;
    }

    public string GetCameraStateJson()
    {
        string cameraStateStringJson = camBase.localPosition.x + "," + camBase.localPosition.y + "," + camBase.localPosition.z + "," + Xcount + "," + Ycount + "," + Zcount;
//      if (CameraUniversalCenter.isInMirrorHX)
//      cameraStateStringJson = -camBase.localPosition.x + "," + camBase.localPosition.y + "," + camBase.localPosition.z + "," + Xcount + "," + Ycount + "," + Zcount;

        return cameraStateStringJson;
    }

    string GetCameraSmoothStateJson()
    {
        string cameraStateStringJson = camBase.localPosition.x + "," + camBase.localPosition.y + "," + camBase.localPosition.z + "," + Xsmooth + "," + Ysmooth + "," + Zsmooth;
//      if(CameraUniversalCenter.isInMirrorHX)
//      cameraStateStringJson = -camBase.localPosition.x + "," + camBase.localPosition.y + "," + camBase.localPosition.z + "," + Xsmooth + "," + Ysmooth + "," + Zsmooth;       
        return cameraStateStringJson;
    }

    float GetFlyTime(float distance)
    {
        return distance / flySpeedPerSecond;
    }

    void UpdatePosition()
    {
        if (XAxes)
        {
            Xcount = Mathf.Clamp(Xcount, minimumX, maximumX);
            Xsmooth = Mathf.Lerp(Xsmooth, Xcount, 0.0166f * smoothSpeed);
        }

        if (YAxes)
        {
            Ysmooth = Mathf.Lerp(Ysmooth, Ycount, 0.0166f * smoothSpeed);
        }

        if (Zpush)
        {
            Zcount = Mathf.Clamp(Zcount, minimumZ, maximumZ);
            Zsmooth = Mathf.Lerp(Zsmooth, Zcount, 0.0166f * smoothSpeed);
//          if(changingCamera)
//          Zsmooth = Mathf.Lerp(Zsmooth, Zcount, 0.566f * smoothSpeed);
        }


        if (!playAnimationPath)
        {
            universalX.localRotation = Quaternion.Euler(Xsmooth, 0, 0);
            camBase.localRotation = Quaternion.Euler(0, Ysmooth, 0);

            if(!changingCamera)
            transform.localPosition = new Vector3(0, 0, -Zsmooth);
            //          universalY.localEulerAngles = new Vector3(0, Ysmooth, 0);
            //          universalX.localEulerAngles = new Vector3(Xsmooth,0,0);
        }
        
    }

    /// <summary>
    /// 朝向蓝色轴
    /// </summary>
    public void LookAtTransform(Transform t, bool redTrueBlurFalse, bool endEnableCtrl = true)
    {
       LookAtPoint(t.position,redTrueBlurFalse?t.right:t.forward, endEnableCtrl);
    }

    public void LookAtPoint(Vector3 lookAtPosition, Vector3 lookForward,bool endEnableCtrl=true)//一个相机目标点的位置，和目标点朝前的向量
    {
        Debug.Log("lookAtPosition");
        MoveCameraPos(lookAtPosition);
        lookForward = Vector3.Normalize(lookForward);

        float mYconut =180+Mathf.Rad2Deg * Mathf.Atan2(lookForward.x, lookForward.z);
        Ycount = ModiferYCount(Ycount, mYconut);
    }

    void RecodeInitialCameraState()
    {

//      GlobalDebug.Addline("RecodeInitialCameraState");
//      Debug.Log(this.name+"_RecodeInitialCameraState");

        Xcount = Mathf.Clamp(Xcount, minimumX, maximumX);
        Zcount = Mathf.Clamp(Zcount, minimumZ, maximumZ);

        defaultXcount = Xcount;
        defaultYcount = Ycount;
        defaultZcount = Zcount;
        defaultPosition = camBase.localPosition;

        defaultCameraPositionAndXYZCount = GetCameraStateJson();
    }

    public void ResetCameraStateToInitial()
    {
        Xcount = defaultXcount;
        Ycount = defaultYcount;
        Zcount = defaultZcount;

        Xsmooth = defaultXcount;
        Ysmooth = defaultYcount;
        Zsmooth = defaultZcount;

        camBase.localPosition = defaultPosition;

//      GlobalDebug.Addline(this.gameObject.name+"_ResetCamera");
        if (CameraUniversalCenter.isInMirrorHX)
        {
            camBase.localPosition = new Vector3(-defaultPosition.x, defaultPosition.y, defaultPosition.z);
            Ycount = -defaultYcount;
            Ysmooth = -defaultYcount;
        }

        camBase.localEulerAngles = new Vector3(0, Ysmooth, 0);
        universalX.localEulerAngles = new Vector3(Xsmooth, 0, 0);
        transform.localPosition = new Vector3(0, 0, -Zsmooth);

        if (uselookAtSphere)
        {
            universalY.position = camBase.position - Zcount * 0.3f * transform.forward;
        }
        else
        {
            universalY.position = camBase.position;
        }

        vrMoveForward = false;
    }

    public void MoveToInitPosition(float useTime)
    {
//        GlobalDebug.Addline(this.gameObject.name + "_MoveToInitPosition");
        if (CameraUniversalCenter.isInMirrorHX)
        {
            SetCameraPositionAndXYZCountAllArgs((-defaultPosition.x).ToString(), defaultPosition.y.ToString(), defaultPosition.z.ToString(), defaultXcount.ToString(), (-defaultYcount).ToString(), defaultZcount.ToString(), useTime);
        }
        else
        {
            SetCameraPositionAndXYZCountAllArgs(defaultPosition.x.ToString(), defaultPosition.y.ToString(), defaultPosition.z.ToString(), defaultXcount.ToString(), defaultYcount.ToString(), defaultZcount.ToString(), useTime);
        }

    }
//  public void FlyCamera(Transform cam, Vector3 camToWorldPos,Quaternion camToQuaternion, float useTime)
//  {

//  }

    public void ZPushAdd(float pushAdd)
    {
        Zcount += pushAdd;
    }

    public void EnableSpaceLookAtCanMoveCanMove()
    {
        spaceLookAtCanMove = true;
    }

    public void DisableSpaceLookAtCanMoveCanMove()
    {
        spaceLookAtCanMove = false;
    }

    public void EnableCamera()
    {


        SleepWake();

        gameObject.SetActive(true);

        EnableCameraCtrl();
        if (camBase != null && camBase.GetComponent<CharacterController>() != null)
        {
            camBase.GetComponent<CharacterController>().height = 0.2f;
            camBase.GetComponent<CharacterController>().radius = 0.2f;
            camBase.GetComponent<CharacterController>().stepOffset = 0.01f;
            camBase.GetComponent<CharacterController>().center = new Vector3(0, -1, 0);
        }
    }

    public string DisableCamera()
    {


        SleepWake();
        string currentStateJson = GetCameraSmoothStateJson();

        gameObject.SetActive(false);
        DisableCameraCtrl();

        if (resetCameraStateWhenDisable)
        {
//            Debug.Log(this.gameObject.name+ "ccccccccd");
            ResetCameraStateToInitial();
        }

        if (camBase != null && camBase.GetComponent<CharacterController>() != null)
        {
            camBase.GetComponent<CharacterController>().height = 0.02f;
            camBase.GetComponent<CharacterController>().radius = 0.02f;
            camBase.GetComponent<CharacterController>().stepOffset = 0.01f;
            camBase.GetComponent<CharacterController>().center = new Vector3(0, -1000, 0);
        }


            return currentStateJson;

    }

    public void PauseRender()
    {
        GetComponent<Camera>().Render();
        orginCullMask = GetComponent<Camera>().cullingMask;
        orginCameraCleraFlags=GetComponent<Camera>().clearFlags;
        GetComponent<Camera>().cullingMask = 0;
        GetComponent<Camera>().clearFlags = CameraClearFlags.Nothing;

       
    }

    public void ResumeRender()
    {
        GetComponent<Camera>().cullingMask = orginCullMask;
        GetComponent<Camera>().clearFlags = orginCameraCleraFlags;
        GetComponent<Camera>().Render();
    }












}

