using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[System.Serializable]
public class HuXingInstance 
{

    /// <summary>
    /// id由楼号,单元,楼层,房间号构成,如 17_1_1201,数据表中的id
    /// </summary>
    public string id;
    public string hxName;
    public bool isMirrorHX;
    //Y轴偏移值
    public float yRotOffset;
    public int louHao;
    //unit=-1的时候就是没有单元号
    public int unit = 0;
    public int louCeng;
    public int fangJianHao;



    public Vector3 position;
    public Vector3 eulerAngles;
    public Vector3 scale;


    //占地面积
    //建筑面积
    //套内面积
    //赠送面积
    //花园面积
    //露台面积
    //产权面积
    public float area01;
    public float area02;
    public float area03;
    public float area04;
    public float area05;
    public float area06;
    public float area07;

    //产权单价
    //产权总价
    //精装总价
    public float pricePerMeter;
    public float priceTotal;
    public float PriceJzToTal;

    //预订状态
    //关注人数
    //交房时间
    public enum YuDing
    {
        未售 = 0,
        已售 = 1
    }
    public YuDing yuDingState;
    public int views;
    public string payTime;

    //可见状态
    //备注
    public bool visibility;
    public string moreInfo;

    //
    //  public string GetDi

    public string fangJianInfo;


    public void  Genid()
    {
        id = louHao.ToString() +"_"+ unit.ToString()+"_"+(louCeng * 100 + fangJianHao).ToString();
        GetFangJian();
    }


    void GetFangJian()
    {
        fangJianInfo = (louCeng * 100 + fangJianHao).ToString();
    }

    public string GetFangJianInfo()
    {
        return fangJianInfo;
    }


}


