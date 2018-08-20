using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;



[System.Serializable]
public class HuXingType
{
    //网络上的ID;

    //户型名
    public string hxName = "";
    public string displayName = "";
    public string arMapName = "";

    //户型简介
    [HideInInspector]
    public string introduction = "";
    //  public bool hasShare=true;
    [HideInInspector]
    public Texture PMT;


    [HideInInspector]
    public Transform proxyRoot;
    public Transform hxMeshRoot;
    //使用在这个户型在大场景鸟瞰时的位置
    public Vector3 hxNKWorldPos = new Vector3(0, 300, 0);
    //    public Vector3 hxNKWorldRot = new Vector3(0, 0, 0);
    /// <summary>
    /// 旋转偏移,修正选房时的偏差
    /// </summary>
    public float rotOffset = 0;
    public string nkCameraPosAndXYZcount = "";

    public string defaultMYFloorName = "1F";
    public floor[] allFloor;
    public FastMovePointManager fastMovePointManager;

    /*
        public AudioClip hxAudioClip;
        public int cartoonType;
     */
    public bool isMYing = false;


    [HideInInspector]
    public string qj360rul = "";

//    [HideInInspector]
    public string viewDisplayMode ="1";

    string huXingTypeInfo = "";

    [HideInInspector]
    public float normalPrice = -1;
    //    [HideInInspector]
    public float normalArea = -1;
    //    [HideInInspector]
    public string fangXing = "规定待定";
    //    [HideInInspector]
    public string leiXing = "住宅";



    public List<NetTexture2D> netTexture2DGroup;
    public string huXingID = "";


    [System.Serializable]
    public struct floor
    {
        public string floorName;
        public string displayName;
        public Transform meshRoot;
        public Transform pzMesh;
        public Transform fczMesh;
        public CameraUniversal cameraUniversal;
        public string myCameraPosAndXYZcount;
        public Transform[] pointForMove2;

        [HideInInspector]
        public Texture PMT;
        public string pmtName;
        public string pmtUrl;
        //楼层简介
        public string introduction;
        public AnimationClip autoAnimation;

        //进入这一层触发的交互操作
//        [HideInInspector]
        public InteractiveAction interactiveAction;

    }

    public floor currentAtFloor;



    public HuXingType()
    {


    }


    public void RecordEachFloorInteractiveAction()
    {
        for (int i = 0; i < allFloor.Length; i++)
        {
            allFloor[i].interactiveAction.cameraStates = ",,,,,";

            if (allFloor[i].myCameraPosAndXYZcount != "")
            {
                allFloor[i].interactiveAction.cameraStates = allFloor[i].myCameraPosAndXYZcount;
            }

            if(allFloor[i].myCameraPosAndXYZcount == ""&& allFloor[i].cameraUniversal!=null)
            {
                allFloor[i].interactiveAction.cameraStates = allFloor[i].cameraUniversal.GetCameraStateJson();
            }

            allFloor[i].interactiveAction.needDisplayRoot = allFloor[i].meshRoot;
            allFloor[i].interactiveAction.pzMesh = allFloor[i].pzMesh;
            allFloor[i].interactiveAction.cameraUniversal = allFloor[i].cameraUniversal;
        }
    }



    public string GetDefaultMYFloorName()
    {
        foreach (floor f in allFloor)
        {
//            Debug.Log(defaultMYFloorName);
            if (f.floorName == defaultMYFloorName)
            {
                Debug.Log(f.floorName);
                return f.floorName;
            }
        }

        //如果没有找到指定的默认层就设置为allFloor里的第一个楼层
        return allFloor[0].floorName;

    }



    public void SetHuXingTypeInfo()
    {
        //        GetHuXingType();
    }

    public string GetHuXingTypeInfo()
    {

        string[] introductionGroup = introduction.Replace("；",";").Split(';');
        string introductionFinal = "";

        for (int i = 0; i < introductionGroup.Length; i++)
        {
            introductionFinal += introductionGroup[i];
            if(i!= introductionGroup.Length-1)
            introductionFinal +="\n";
        }
//      Debug.Log(introductionFinal);

        huXingTypeInfo = hxName.ToString() + "户型^" + fangXing + "^约" + normalArea.ToString() + "㎡^"+ introductionFinal;
        return huXingTypeInfo;
    }

    public string[] GetHuXingAllFloorName()
    {
        string[] allFloorName = new string[allFloor.Length];

        for (int i = 0; i < allFloor.Length; i++)
        {
            allFloorName[i] = allFloor[i].floorName;
        }

        return allFloorName;

    }

    public string[] GetHuXingAllDisplayName()
    {
        string[] allDisplayName = new string[allFloor.Length];

            for (int i = 0; i < allFloor.Length; i++)
            {
                if (allFloor[i].displayName == "")
                    allFloor[i].displayName = allFloor[i].floorName;
                 allDisplayName[i] = allFloor[i].displayName;
            }


        return allDisplayName;
    }

    public void EnterHuXingFloor(CameraUniversalCenter camCenter, string floorName,CanveGroupFade triggerFastMoveSM, ScrollMenu fastMoveSM)
    {

        DisplayAllFloorMesh();

        currentAtFloor.floorName = "";


        foreach (floor f in allFloor)
        {
            if (f.floorName == floorName)
            {
                currentAtFloor = f;

                isMYing = true;
                camCenter.ChangeCamera(f.cameraUniversal,0.5f);

                if (f.pointForMove2!=null&&f.pointForMove2.Length > 0)
                {
                    fastMoveSM.GetComponent<RectTransform>().DOAnchorPosY(60, 1);
                    triggerFastMoveSM.AlphaPlayForward();

                    string[] displayGroup = new string[f.pointForMove2.Length];
                    string[] paraGroup = new string[f.pointForMove2.Length];

                    for (int i = 0; i < f.pointForMove2.Length; i++)
                    {
                        displayGroup[i] = f.pointForMove2[i].name;
                        paraGroup[i] = i.ToString();
                    }

                    fastMoveSM.CreateItemGroup(displayGroup, paraGroup);

                }
                else
                {
                    fastMoveSM.GetComponent<RectTransform>().DOAnchorPosY(60, 1);
                    triggerFastMoveSM.AlphaPlayBackward();
                }


            }
        }

    }

    public void ExitHuXingFloor(CameraUniversalCenter camCenter)
    {
        isMYing = false;
        camCenter.ChangeCamera(camCenter.cameras[0]);

//        fastMoveSM.GetComponent<RectTransform>().DOAnchorPosY(60, 1);
//        trrigerFastMoveSM.AlphaPlayBackward();

    }

    public void HiddenAllFloorMesh()
    {
        for (int i = 0; i < allFloor.Length; i++)
        {
            if (allFloor[i].meshRoot != null)
                allFloor[i].meshRoot.gameObject.SetActive(false);
        }
    }

        /// <summary>
        /// 显示全部楼层模型除了外墙体
        /// </summary>
        public void DisplayAllFloorMesh()
        {
            for (int i = 0; i < allFloor.Length; i++)
            {
                if (allFloor[i].meshRoot != null)
                    allFloor[i].meshRoot.gameObject.SetActive(true);
                if (allFloor[i].floorName == "WQT"&& allFloor[i].meshRoot != null)
                    allFloor[i].meshRoot.gameObject.SetActive(false);
            }
        }



       void DisplayFloorMesh(string floorName)
       {
                DisplayAllFloorMesh();
                for (int i = allFloor.Length - 1; i > 0; i--)
                {
                    if (allFloor[i].meshRoot != null)
                    {
                        if (allFloor[i].floorName == "WQT")
                        {
                            HiddenAllFloorMesh();
                            allFloor[i].meshRoot.gameObject.SetActive(true);
                            return;
                        }

                        if (allFloor[i].floorName != floorName)
                        {
                            allFloor[i].meshRoot.gameObject.SetActive(false);
                        }
                        else
                        {
                            return;
                        }
                    }
                }
        }



        public void HuXingChooseFloor(CameraUniversalCenter camCenter, string floorName)
        {
            if (!isMYing)
            {
                DisplayFloorMesh(floorName);
            }
            else
            {
//                DisplayAllFloorMesh();
                foreach (floor f in allFloor)
                {
                    if (f.floorName == floorName)
                    {
                        camCenter.ChangeCamera(f.cameraUniversal);
                    }
                }
            }

        }
}


