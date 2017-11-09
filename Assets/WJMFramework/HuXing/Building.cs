using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Building : MonoBehaviour
{

     public Transform modelBuildRoot;

     public int louHaoNo;

    [HideInInspector]
     public string cameraPosAndXYZcount;

     GenHuXingInstance[] allGenHuXingInstance;
     List<HuXingInstance> buildingHuXingInstance;

     void DisplayBuildingMesh()
     {

     }

     void HiddenBuildingMesh()
     {

     }

     public HuXingInstance[] GetBuildingHuXingInstance()
     {

        louHaoNo = int.Parse(transform.name);

        buildingHuXingInstance = new List<HuXingInstance>();
        allGenHuXingInstance = GetComponentsInChildren<GenHuXingInstance>(true);


        for (int i = 0; i < allGenHuXingInstance.Length; i++)
        {
            buildingHuXingInstance.AddRange(allGenHuXingInstance[i].GetHXInstance(i+1));
        }


        return buildingHuXingInstance.ToArray();

     }



    //string boxName;
    //string boxNameSplitLeft;
    //string boxNameSplitRight;

    //public int louHaoNoDisplayUse;
    //public Transform[] selectLouCengBlock;
    //public Transform currentSelectBlock;
    //public Transform colliderBuild;
    //public Transform[] rootOfChildsXK;


    //void HiddenLastLouCengBlock();
    //void HoverHX(int idFormLeft);
    //void SelectHX(int idFormLeft);
    //void DisplayLouCengBlock(int louCeng);
    //void ExitSelectState();

    //void ClickBuilding();
    //void HoverHuXing();
    //void ClickHuXing();
    //int currentBeSelectFangJian;


}
