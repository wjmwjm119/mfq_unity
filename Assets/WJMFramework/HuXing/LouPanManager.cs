using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

public class LouPanManager : MonoBehaviour
{

    //每组用逗号分开, 如234,456将用456替换234 
    static public string[] needReplaceString;

    public int instanceCount;

    [HideInInspector]
    public HuXingType[] allHuXingType;


//  public Transform[] allBuildingMeshRoot;

    public List<Building> allBuilding;

    public List<HuXingInstance> allHuXingInstance;

    public List<HuXingInstance> selectHXInstance;

    public string fork = "-----------------------";


    public int[] oringLouHao;
    public string[] mapLouHao;
    public int[] oringUnit;
    public string[] mapUnit;

    public Dictionary<int, string> louHaoNameDictionary;
    public Dictionary<int, string> unitNameDictionary;

    //  public List<HuXingInstance> selectBuildingInstance;
    //  public HuXingType()
    //  {
    //        needReplaceString = new string[1];
    //        needReplaceString[0] = "_,-";
    //  }

    void Start()
    {
        if (allBuilding != null)
        {
            for (int i = 0; i < allBuilding.Count; i++)
            {
                allBuilding[i].gameObject.SetActive(false);
            }
        }


        if (oringLouHao != null)
        {
            //生成楼号名映射
            if (oringLouHao.Length == mapLouHao.Length)
            {
                louHaoNameDictionary = new Dictionary<int, string>();

                for (int i = 0; i < oringLouHao.Length; i++)
                {
                    if (!louHaoNameDictionary.ContainsKey(oringLouHao[i]))
                    {
                        louHaoNameDictionary.Add(oringLouHao[i], mapLouHao[i]);
                    }
                }
            }
            else
            {
                Debug.LogError("LouPanManager 楼号名映射长度不统一");
            }
        }


        if (oringUnit != null)
        {

            //生成单元名映射
            if (oringUnit.Length == mapUnit.Length)
            {
                unitNameDictionary = new Dictionary<int, string>();

                for (int i = 0; i < oringUnit.Length; i++)
                {
                    if (!unitNameDictionary.ContainsKey(oringUnit[i]))
                    {
                        unitNameDictionary.Add(oringUnit[i], mapUnit[i]);
                    }
                }

            }
            else
            {
                Debug.LogError("LouPanManager 单元名映射长度不统一");
            }
        }
        

    }


    public void GenAllHuXingInstance()
    {
        allHuXingInstance = new List<HuXingInstance>();
        allBuilding = new List<Building>();
        allBuilding.AddRange( GetComponentsInChildren<Building>(true));

        for (int i = 0; i < allBuilding.Count; i++)
        {
            allHuXingInstance.AddRange(allBuilding[i].GetBuildingHuXingInstance());
        }

        instanceCount = allHuXingInstance.Count;

    }

    public HuXingType GetHuXingType(string hxName)
    {

        for (int i = 0; i < allHuXingType.Length; i++)
        {
            if (hxName == allHuXingType[i].hxName)
                return allHuXingType[i];
        }

        return null;
    }



    public void GetSelectHuXinginstance(string hxName)
    {
        selectHXInstance = new List<HuXingInstance>();
        selectHXInstance = SelectInstances(allHuXingInstance, hxName);
    }


    public void ClearXFHuXingInstance()
    {
        selectHXInstance.Clear();
    }


    public int[] GetDistinctLouHao(List<HuXingInstance> hxInsGorup)
    {
        return hxInsGorup.Select(h => h.louHao).Distinct().ToArray();     
    }

    public int[] GetDistinctUnit(List<HuXingInstance> hxInsGorup,int louHao)
    {
        return hxInsGorup.Where(h => h.louHao == louHao).Select(h => h.unit).Distinct().ToArray();
    }

    public string[] GetFangJianHao(List<HuXingInstance> hxInsGorup, int louHao,int unit)
    {
        return hxInsGorup.Where(h => h.louHao == louHao&& h.unit == unit).Select(h => h.fangJianInfo).Reverse().ToArray();
    }


    public List<HuXingInstance> SelectInstances(List<HuXingInstance> hxInsGorup,string hxName="", int louHao=-1,int unit=-1)
    {
        return hxInsGorup.Where(h =>(hxName==""?true:h.hxName==hxName)&&(louHao==-1?true:h.louHao == louHao )&&(unit==-1?true:h.unit == unit)).Select(h => h).ToList();
    }


    public HuXingInstance[] SelectSingerInstance(List<HuXingInstance> hxInsGorup, int louHao , int unit ,string fangJianInfo)
    {
        return hxInsGorup.Where(h => (h.louHao == louHao && h.unit == unit && h.fangJianInfo == fangJianInfo)).Select(h => h).ToArray();
    }




}
