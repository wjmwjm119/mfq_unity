using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenHuXingInstance : MonoBehaviour
{

    public bool autoFangJianHao=true;
    public bool isMirrorHX;
    public float yRotOffset;
    public int unit=0;

    public int startFloor=1;
    public int endFloor=12;

    public float eachFloorHeight=3.0f;
    public int eachFloorAdd = 1;
    public int eachFloorNameAdd = 1;

    public string line = "-----------";

    public string hxName;
    public int louHao;
    public int fangJianHao = 1;

    List<HuXingInstance> genHXInstance;

    public HuXingInstance[] GetHXInstance(int inFangJianHao)
    {
        hxName = transform.name;
        louHao = transform.parent.GetComponent<Building>().louHaoNo;

        if(autoFangJianHao)
        fangJianHao = inFangJianHao;

        int count = 0;
        int currentFLoor = 0;

        for (int i = startFloor; i <= endFloor; i += eachFloorAdd)
        {
            count++;
        }
        genHXInstance =new List<HuXingInstance>();

        for (int i = startFloor; i <= endFloor; i += eachFloorAdd)
        {
            HuXingInstance h = new HuXingInstance();
            hxName = gameObject.name;
            h.hxName = hxName;
            h.isMirrorHX = isMirrorHX;
            h.yRotOffset = yRotOffset;
            h.louHao = louHao;
            h.unit = unit;
            h.louCeng = startFloor+ currentFLoor;
            h.fangJianHao = fangJianHao;
            h.unit = unit;

            h.position = transform.position + new Vector3(0, eachFloorHeight*(i-startFloor), 0);
            h.eulerAngles = transform.eulerAngles;
            h.scale = transform.lossyScale;

            currentFLoor+=eachFloorNameAdd;

            h.Genid();

            genHXInstance.Add(h);

        }


        return genHXInstance.ToArray();
    }


}
