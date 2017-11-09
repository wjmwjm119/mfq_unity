using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using System;




public class Label : MonoBehaviour
{
    public enum Icon
    {
        /// <summary>
        /// 标准
        /// </summary>
        默认 = 0,
        /// <summary>
        /// 地铁
        /// </summary>
        地铁 = 1,
        /// <summary>
        /// 学校
        /// </summary>
        学校 = 2,
        /// <summary>
        /// 医院
        /// </summary>
        医院 = 3,
        /// <summary>
        /// 商业
        /// </summary>
        商业 = 4,
        /// <summary>
        /// 金融
        /// </summary>
        金融 = 5,
        /// <summary>
        /// 超市
        /// </summary>
        超市 = 6,
        /// <summary>
        /// 公园
        /// </summary>
        公园 = 7,
        /// <summary>
        /// 购物
        /// </summary>
        购物 = 8,
        /// <summary>
        /// 道路
        /// </summary>
        道路 = 9,
        /// <summary>
        /// 小汽车
        /// </summary>
        小汽车 = 10,
        /// <summary>
        /// 公交车
        /// </summary>
        公交车 = 11,
        /// <summary>
        /// 出入口
        /// </summary>
        出入口 = 12,
        /// <summary>
        /// 楼号
        /// </summary>
        楼号 = 13
    }

    public string labelText = "";
    public string fileName = "";
    public Icon icon;
    


    public void SetLabelText()
    {
        labelText = transform.name;
        fileName = "";
            
        for (int i = 0; i < labelText.Length; i++)
        {
            byte[] labelTextBytes = Encoding.BigEndianUnicode.GetBytes(labelText[i].ToString());
            //字序大小端

            fileName += BitConverter.ToString(labelTextBytes);
        }
        fileName = fileName.Replace("-", "");

    }


}
