//2015.7.16.handsome studio

using UnityEngine;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using System;


public class CameraUniversalCenter : BaseEventCenter
{
    //public int initialCameraID=1;
    //int currentCameraID = -1;
    //int targetCameraID = -1;
    //int lastCameraID=-1;

    public CameraUniversal currentCamera;

    public List<CameraUniversal> cameras;

//  public CameraUniversal[] cameras;

    CameraUniversal lastCamera;

    public float zbzOffset=160;
    public float rotForZBZ;
    public float currentCameraRot;

    //是否在镜像户型
//  [HideInInspector]
    public static bool isInMirrorHX;

    //----------------------------------------------------------------------------

    public UnityEvent OnChangeToMYCamera;
    public UnityEvent OnChangeToNKCamera;


    


    //    public UnityEvent OnSceneRemoved;



    void Start()
    {
        //以下要加是因为有些相机在场景中是关闭的,关闭时没法自动执行Start
        foreach(CameraUniversal c in cameras)
        {
//            Debug.Log("ddddddd");
            c.InitlCameraInRuntime();
        }
//        RemoteGather.currentCameraUniversal = currentCamera;
//        RemoteGather.lastSendCameraCameraState = currentCamera.GetCameraState();
     isInMirrorHX=false;

}

    void Update()
    {
        if (currentCamera != null)
        {
            currentCameraRot = currentCamera.transform.eulerAngles.y + currentCamera.zhiBeiZhenCorrect;
            rotForZBZ = currentCameraRot + zbzOffset;
        }
    }

    public void AddCameraToGroup(CameraUniversal addIn)
    {
        if (addIn == null)
        {
            GlobalDebug.Addline("AddCameraToGroup addIn 为空");
            Debug.LogError("AddCameraToGroup addIn 为空");
            return;
        }

        if (!cameras.Contains(addIn))
        {
            addIn.InitlCameraInRuntime();
            cameras.Add(addIn);
        }
    }


     void ChangeCameraWithName(string targetCameraName, float mTimeUseFly = 2)
    {
        CameraUniversal c = cameras.Find(s => s.camName == targetCameraName);
        if (c != null)
        {
            ChangeCamera(c, mTimeUseFly);
        }
        else
        {
            Debug.Log("找不到" + targetCameraName + "相机");
        }
    }


    /// <summary>
    /// 切换相机方式一,直接参数切换
    /// </summary>
    public void ChangeCamera(CameraUniversal targetCamera,float mTimeUseFly = 2) 
	{
        AddCameraToGroup(targetCamera);

        if (targetCamera != null && targetCamera != currentCamera)
        {
            Debug.Log("ChangeCamera");
            lastCamera = currentCamera;

//          if (currentCamera == null)
//          currentCamera = targetCamera;
            
            if (currentCamera.playAnimationPath)
            currentCamera.CameraPathStopPlay();

            string currentState=currentCamera.DisableCamera();

            targetCamera.EnableCamera();

//         Vector3 toPos = targetCamera.transform.position;
//         Debug.Log(Vector3.Distance(toPos, lastCamera.transform.position));
//         Quaternion toRo = targetCamera.transform.localRotation;

            targetCamera.SetCameraPositionAndXYZCount(currentState, 0);
            targetCamera.DisableCameraCtrl();
            targetCamera.MoveToInitPosition(mTimeUseFly);

            currentCamera = targetCamera;
//          RemoteGather.currentCameraUniversal = currentCamera;
//          Debug.Log(2222);
            RemoteGather.lastSendCameraCameraState = currentCamera.GetCameraState();

            if (currentCamera.name == "CameraNK")
            {
                OnChangeToNKCamera.Invoke();
            }
            else if(currentCamera.name == "CameraMY")
            {
                OnChangeToMYCamera.Invoke();
            }


        }
	}


    void ChangeCameraWithName2(string targetCameraName, float mTimeUseFly = 2)
    {
        CameraUniversal c = cameras.Find(s => s.camName == targetCameraName);
        if (c != null)
        {
            ChangeCamera2(c, mTimeUseFly);
        }
        else
        {
            Debug.Log("找不到" + targetCameraName + "相机");
        }
    }


    /// <summary>
    /// 切换相机方式二,位置移动及参数变化
    /// </summary>
    public void ChangeCamera2(CameraUniversal targetCamera, float mTimeUseFly = 2)
    {
        Debug.Log("ChangeCamera2");

        AddCameraToGroup(targetCamera);

        if (targetCamera != null && targetCamera != currentCamera)
        {

            lastCamera = currentCamera;

            if (currentCamera.playAnimationPath)
            currentCamera.CameraPathStopPlay();

            DOTween.Kill(currentCamera.transform);
            DOTween.Kill(targetCamera.transform);

            currentCamera.DisableCamera();
            targetCamera.EnableCamera();

            //判断是否是镜像户型,会进行镜像操作
            targetCamera.MoveToInitPosition(0);

            targetCamera.DisableCameraCtrl();
            targetCamera.changingCamera = true;

            Vector3 toPos = targetCamera.transform.localPosition;
            Quaternion toRo = targetCamera.transform.localRotation;


            targetCamera.transform.position = lastCamera.transform.position;
            targetCamera.transform.rotation = lastCamera.transform.rotation;

            DOTween.Kill(targetCamera.transform);
            targetCamera.transform.DOLocalMove(toPos, mTimeUseFly);
            targetCamera.transform.DOLocalRotateQuaternion(toRo, mTimeUseFly).OnComplete(() => { targetCamera.changingCamera = false; targetCamera.EnableCameraCtrl(); targetCamera.transform.localRotation = new Quaternion(); });

            currentCamera = targetCamera;


            if (currentCamera.name == "CameraNK")
            {
                OnChangeToNKCamera.Invoke();
            }
            else if (currentCamera.name == "CameraMY")
            {
                OnChangeToMYCamera.Invoke();
            }

        }
    }


    public void ReturnToLastCamera(float mTimeUseFly = 2)
	{
		ChangeCamera (lastCamera,mTimeUseFly);
	}

	
    public void PlayPathAnimation()
    {
        currentCamera.GetComponent<CameraUniversal>().CameraPathAutoPlay();
    }

    public void StopPathAnimation()
    {
        currentCamera.GetComponent<CameraUniversal>().CameraPathStopPlay();
    }


       
   




}

