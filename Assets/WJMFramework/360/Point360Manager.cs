using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using DG.Tweening;


public class Point360Manager : MonoBehaviour
{
    public TouchCtrl touchCtrl;
    public CameraUniversalCenter cameraUniversalCenter;
    public bool getPositionFromName;
    public Unit posUnit;
    public UpAxe posUpAxe;
    public GameObject point360Perfab;
    public Transform point360Sphere;
    public Material point360Mat;

    Cubemap lastCubemap;
    public float baseHeight = 1.5f;
    //能见到Point360最远范围
    public float rangeCanSee = 10;

    public string defaultFloorName = "1F";
    public Point360Floor[] point360Floors;

    List<ColliderTriggerButton> allColliderTriggerButtons;
    List<ColliderTriggerButton> canViewColliderTriggerButtons;


    Ray ray;
    RaycastHit hit;
    RaycastHit hit2;

    bool isChangeCubemap;



    [System.Serializable]
    public class Point360Floor
    {
        public string floorName;
        public string displayName;
//      public float defaultYRot;
        public ColliderTriggerButton defaultColliderTriggerButton;
        public Transform colliderTriggerRoot;
        public Cubemap[] cubemapGroup;

        public void SetDefaultCubemap()
        {
            colliderTriggerRoot.gameObject.SetActive(true);
            ColliderTriggerButton.touchRayCastFrom = ColliderTriggerButton.TouchRayCastFrom.Minimap;
            if (defaultColliderTriggerButton!=null)
            defaultColliderTriggerButton.ExeTriggerEvent();

        }

    }



    public enum UpAxe
    {
        Y,
        Z
    }

    public enum Unit
    {
        mm,
        cm,
        m
    }

    void Start()
    {

        if (point360Floors.Length > 0)
        {
            allColliderTriggerButtons = new List<ColliderTriggerButton>();

            foreach (ColliderTriggerButton c in GetComponentsInChildren<ColliderTriggerButton>())
            {
                if (!allColliderTriggerButtons.Contains(c))
                {
                    allColliderTriggerButtons.Add(c);
//                    c.gameObject.SetActive(false);
                }
            }

            ChangeFloor(defaultFloorName);
        }
    }


    public void GenPoint360()
    {

        Debug.Log("GenPoint360");

        for (int i = 0; i < point360Floors.Length; i++)
        {
            ColliderTriggerButton[] childTran = point360Floors[i].colliderTriggerRoot.GetComponentsInChildren<ColliderTriggerButton>();

            for (int k = 0; k < childTran.Length; k++)
            {
                DestroyImmediate(childTran[k].gameObject);
            }

            for (int j = 0; j < point360Floors[i].cubemapGroup.Length; j++)
            {
                string[] splitNameString = point360Floors[i].cubemapGroup[j].name.Split('_');

                //          Debug.Log(splitNameString.Length);
                if (splitNameString.Length != 4)
                {
                    Debug.LogError(point360Floors[i].cubemapGroup[j].name + "名字不标准");
                    return;
                }

                string cubemapName = splitNameString[0];
                float fx, fy, fz;
                Vector3 pos = new Vector3();

                if (float.TryParse(splitNameString[1], out fx) && float.TryParse(splitNameString[2], out fy) && float.TryParse(splitNameString[3], out fz))
                {
                    if (posUnit == Unit.mm)
                    {
                        fx = 0.001f * fx;
                        fy = 0.001f * fy;
                        fz = 0.001f * fz;
                    }
                    else if (posUnit == Unit.cm)
                    {
                        fx = 0.01f * fx;
                        fy = 0.01f * fy;
                        fz = 0.01f * fz;
                    }

                    if (posUpAxe == UpAxe.Y)
                    {
                        pos = new Vector3(fx, fy, fz);
                    }
                    else
                    {
                        pos = new Vector3(-fx, fz, fy);
                    }

                    GameObject point = GameObject.Instantiate(point360Perfab, Vector3.zero, new Quaternion(), point360Floors[i].colliderTriggerRoot);
                    point.name = cubemapName;
                    point.transform.localPosition = new Vector3(pos.x, pos.y - baseHeight, pos.z);

                    ColliderTriggerButton co = point.GetComponent<ColliderTriggerButton>();
                    co.btnNameForRemote = cubemapName;

                    //设置colliderTriggerTrue的按钮事件
                    BaseEventDelegate colliderTriggerTrue = new BaseEventDelegate();
                    colliderTriggerTrue.parameterTargetSlot = new int[] { 0, 6,2 };
                    colliderTriggerTrue.parameterList = new EventParametar[3];
                    colliderTriggerTrue.parameterList[0].pObject = point360Floors[i].cubemapGroup[j];
                    colliderTriggerTrue.parameterList[1].pVec3 = new Vector3(point.transform.position.x,point.transform.position.y+baseHeight,point.transform.position.z);
                    colliderTriggerTrue.parameterList[2].pFloat = 0;
                    colliderTriggerTrue.excuteMethodName = "ChangeCubemap";
                    colliderTriggerTrue.targetMono = this;
                    colliderTriggerTrue.currentEditorChooseFunName = "Point360Manager/ChangeCubemap";
                    colliderTriggerTrue.lastEditorChooseFunName = "Point360Manager/ChangeCubemap";

                    co.trueEventList.Add(colliderTriggerTrue);



                }
                else
                {
                    Debug.LogError(point360Floors[i].cubemapGroup[j].name + "位置信息错误，请检查！");
                }

            }
        }
    }


    public void ChangeFloor(string floorName)
    {
        for (int i = 0; i < point360Floors.Length; i++)
        {
            if (point360Floors[i].floorName == floorName)
            {
                point360Floors[i].SetDefaultCubemap();
//              cameraUniversalCenter.currentCamera.Ycount = rotY;
//              cameraUniversalCenter.currentCamera.SetCameraPositionAndXYZCountAllArgs("","","","", rotY.ToString(),"",0.5f);
            }
            else
            {
                point360Floors[i].colliderTriggerRoot.gameObject.SetActive(false);
            }

        }
    }


    public void ChangeCubemap(Cubemap cubemap,Vector3 viewPos,float yRot)
    {
        if(!isChangeCubemap)
        {
            CheckColliderVisibility(viewPos);

            isChangeCubemap = true;
            point360Mat.SetTexture("_CubeMap", cubemap);
            lastCubemap = cubemap;
            point360Mat.DOFade(1, 0.7f).OnComplete(ChangeCubemapEnd);
            string xRotString = "";
            string yRotString = "";
            if (ColliderTriggerButton.touchRayCastFrom == ColliderTriggerButton.TouchRayCastFrom.Minimap)
            {
                xRotString = "0";
                yRotString = yRot.ToString();
            }

            cameraUniversalCenter.currentCamera.SetCameraPositionAndXYZCountAllArgs(viewPos.x.ToString(), viewPos.y.ToString(), viewPos.z.ToString(), xRotString, yRotString, "",0.5f);
            point360Sphere.transform.DOMove(viewPos, 0.5f);

        }
    }

    void ChangeCubemapEnd()
    {
        isChangeCubemap = false;
        point360Mat.DOFade(0, 0);
        point360Mat.SetTexture("_CubeMap2", lastCubemap);
    }

    void CheckColliderVisibility(Vector3 viewPos)
    {
//        Debug.Log("CheckColliderVisibility");
//        GlobalDebug.Addline("CheckColliderVisibility");

        canViewColliderTriggerButtons = new List<ColliderTriggerButton>();

        foreach (ColliderTriggerButton c in allColliderTriggerButtons)
        {
            c.transform.gameObject.SetActive(false);
        }

        foreach (ColliderTriggerButton c in allColliderTriggerButtons)
        {
            c.transform.gameObject.SetActive(true);

            if (c.transform.position.y > (viewPos.y - 0.2f - baseHeight) && c.transform.position.y < (viewPos.y + 0.2f - baseHeight))
            {
                if (Physics.Raycast(viewPos, c.transform.position - viewPos, out hit2))
                {
                    if (hit2.collider.name == c.name)
                    {
                        canViewColliderTriggerButtons.Add(c);
                    }
                }
            }

            c.transform.gameObject.SetActive(false);
        }

        foreach (ColliderTriggerButton cB in canViewColliderTriggerButtons)
        {
            cB.transform.gameObject.SetActive(true);
        }


    }


}
