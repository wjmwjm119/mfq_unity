using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleVR : MonoBehaviour
{
    public AppBridge appBridge;
    public CanveGroupFade defaultGUI;
    public Transform globalManager;
    public Transform rotX;
    public Transform rotY;
    public Transform gyroscopeProxy;
    public Collider moveCollider;
    public Image moveImage;
    public CanveGroupFade moveUI;
    public CanveGroupFade zhunxUI;
    public Camera leftEyeCamera;
    public Camera rightEyeCamera;

    public CanveGroupFade vrViewCanveGroupFade;
    public RawImage leftViewImage;
    public RawImage rightViewImage;

    public Gyroscope gyroscope;

    public CameraUniversal cameraUniversal;

    Quaternion gyroQ;
    Vector3 eular;
    float rx, ry;

    Ray ray;
    RaycastHit hit;

	CameraClearFlags orginCameraClearFlags;
	Color orginBackgroundColor;

    public void OpenVRGlass()
    {
        
//     leftViewImage.rectTransform.anchoredPosition = new Vector2(-0.25f * Screen.width, 0);
//     leftViewImage.rectTransform.sizeDelta = new Vector2(-0.5f * Screen.width, 0);
//     rightViewImage.rectTransform.anchoredPosition = new Vector2(0.25f * Screen.width, 0);
//     rightViewImage.rectTransform.sizeDelta = new Vector2(-0.5f * Screen.width, 0);
//     rightViewImage

        defaultGUI.AlphaPlayBackward();
        CameraUniversal inCamera = globalManager.GetComponent<SceneInteractiveManger>().currentActiveSenceInteractiveInfo.cameraUniversalCenter.currentCamera;
        Input.gyro.enabled = true;

        cameraUniversal = inCamera;
//      cameraUniversal.GetComponent<Camera>().enabled = false;

        leftEyeCamera.enabled = true;
        rightEyeCamera.enabled = true;

        this.transform.parent = inCamera.transform;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        moveUI.AlphaPlayForward();
        zhunxUI.AlphaPlayForward();

        leftEyeCamera.cullingMask = inCamera.GetComponent<Camera>().cullingMask;
        rightEyeCamera.cullingMask = inCamera.GetComponent<Camera>().cullingMask;
		orginCameraClearFlags = cameraUniversal.GetComponent<Camera> ().clearFlags;
		orginBackgroundColor = cameraUniversal.GetComponent<Camera> ().backgroundColor;

		cameraUniversal.GetComponent<Camera>().cullingMask=0;
		cameraUniversal.GetComponent<Camera> ().clearFlags = CameraClearFlags.SolidColor;
		cameraUniversal.GetComponent<Camera> ().backgroundColor = Color.white;


        vrViewCanveGroupFade.AlphaPlayForward();

        appBridge.Unity2App("unityVRState", "1");

    }

    public void CloseVRGlass()
    {
        vrViewCanveGroupFade.AlphaPlayBackward();

        Input.gyro.enabled = false;
        this.transform.parent = globalManager;
        this.transform.localPosition = Vector3.zero;
        this.transform.localRotation = Quaternion.identity;
        moveUI.AlphaPlayBackward();
        zhunxUI.AlphaPlayBackward();

        leftEyeCamera.enabled = false;
        rightEyeCamera.enabled = false;
//      cameraUniversal.GetComponent<Camera>().enabled = true;

		cameraUniversal.GetComponent<Camera>().cullingMask=leftEyeCamera.cullingMask;
		cameraUniversal.GetComponent<Camera> ().clearFlags = orginCameraClearFlags;
		cameraUniversal.GetComponent<Camera> ().backgroundColor = orginBackgroundColor;

        cameraUniversal.ResetCameraStateToInitial();
//      cameraUniversal.vrMoveForward = false;
        cameraUniversal = null;
        defaultGUI.AlphaPlayForward();
        appBridge.Unity2App("unityVRState", "0");
    }


    // Update is called once per frame
    void Update()
    {


        if (cameraUniversal != null)
        {

            gyroQ = GyroToUnity(Quaternion.Euler(-90, 0, 0) * Input.gyro.attitude);
            gyroscopeProxy.transform.rotation = Quaternion.Slerp(gyroscopeProxy.transform.rotation, gyroQ, 0.33f);
            Vector3 f = gyroscopeProxy.transform.forward;


            cameraUniversal.Ycount = LookFlowGryo(f);
            cameraUniversal.SetYSmooth(cameraUniversal.Ycount);

            ray = new Ray(rotX.position, rotX.forward);

            if (moveCollider.Raycast(ray, out hit, 20))
            {

                    Debug.DrawLine(ray.origin, hit.point);
                    Vector3 toPos = new Vector3(hit.point.x, hit.point.y, hit.point.z);
                    GlobalDebug.ReplaceLine(toPos.ToString(), 9);

                    moveImage.color = new Color(0, 0.5f, 1f);
                    cameraUniversal.vrMoveForward = true;

//                 GlobalDebug.ReplaceLine("True", 10);
            }
            else
            {
                moveImage.color = new Color(1f, 1f, 1f);
                cameraUniversal.vrMoveForward = false;
                GlobalDebug.ReplaceLine("False", 10);
            }


            rotX.localEulerAngles = new Vector3((f.y > 0 ? -1 : 1) * Vector3.Angle(f, new Vector3(f.x, 0, f.z)), 0, 0);

            GlobalDebug.ReplaceLine("VR X: "+rotX.localEulerAngles.ToString(), 7);
            GlobalDebug.ReplaceLine("VR Y: "+cameraUniversal.Ycount.ToString(), 8);

        }


    }



    private static Quaternion GyroToUnity(Quaternion q)
    {
        return new Quaternion(q.x, q.y, -q.z, -q.w);
    }


    public float LookFlowGryo(Vector3 lookForward)
    {
        lookForward = Vector3.Normalize(lookForward);
        float mYconut = 180 + Mathf.Rad2Deg * Mathf.Atan2(lookForward.x, lookForward.z);
        return ModiferYCount(rotY.localEulerAngles.y, mYconut);
    }

    float ModiferYCount(float currentYcount, float toYcount)
    {
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
}
