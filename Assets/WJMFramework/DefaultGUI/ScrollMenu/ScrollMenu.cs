//ScrollMenu生成Item及添加Event到Item; 点击Item会调用ItemBtnExeTrue; ItemBtnExeTrue会调用trueEvent; tureEvent处理具体逻辑



using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Text.RegularExpressions;

public class ScrollMenu : MonoBehaviour
{
    public ScrollRect scrollRect;

    public enum ItemDirection
    {
        垂直=0,
        水平=1
    }

    public ItemDirection itemDirection;
    public ScrollItem itemPrefab;
    public float scrollItemMinWidth = 120;
    public float scroolItemHeight=70;
    public int fontSize=40;
    /// <summary>
    /// 当前字体大小的倍值
    /// </summary>
    public float paddingFactor = 0.5f;

    /// <summary>
    /// 两个item的间距
    /// </summary>
    public Vector2 itemSpace=new Vector2(30,30);

    public bool btnCantCtrlInEnd;

    public ItemEvent trueEvent;
    public ItemEvent falseEvent;

    ScrollItem currentSelectItem;
    ScrollItem lastSelectItem;

    List<ScrollItem> allScrollItem;

    //用来保存非标准层的楼层,如外墙体,屋顶
    List<ScrollItem> nonStandFloor;


    public int GetallScrollItemLength()
    {
        if (allScrollItem!=null)
        {
            return allScrollItem.Count;
        }
        else
        {
            return 0;
        }    
    }


    public void SetNonStandFloorBtnVisblity(bool v)
    {
        if (nonStandFloor != null)
        {
            foreach (ScrollItem s in nonStandFloor)
            {
                s.gameObject.SetActive(v);
            }
        }
    }


    public void ChooseScroolItemHXName(string hxName)
    {
//        Debug.Log(hxName);
        foreach (ScrollItem s in allScrollItem)
        {
            if (s.GetComponentInChildren<Text>().text == hxName)
            {
                s.imageButton.SetBtnState(true, 0);
//                Debug.Log(s.GetComponentInChildren<Text>().text);
            }
        }
    }


    public void CloseScrollMenu()
    {
        if (lastSelectItem != null)
            lastSelectItem.imageButton.CleanState();
    }

    public void ItemBtnExeTrue(ScrollItem inItem,string parStr )
    {
        currentSelectItem = inItem;

        if (lastSelectItem != null&& lastSelectItem!=currentSelectItem)
            lastSelectItem.imageButton.CleanState();
        
        lastSelectItem = currentSelectItem;

        trueEvent.Invoke(parStr);

        if (btnCantCtrlInEnd)
        {
            currentSelectItem.GetComponent<Image>().raycastTarget = false;
        }

//      Debug.Log("true"+ inItem.name+"----"+parStr);

    }


    public void ItemBtnExeFalse(ScrollItem inItem, string parStr)
    {

        falseEvent.Invoke(parStr);

        if (btnCantCtrlInEnd)
        {
            lastSelectItem.GetComponent<Image>().raycastTarget = true;
        }
        //        Debug.Log("false" + inItem.name + "----" + parStr);
    }


    void Start()
    {
        if (itemDirection == ItemDirection.水平)
        {
            scrollRect.horizontal = true;
            scrollRect.vertical = false;
        }
        else
        {
            scrollRect.horizontal = false;
            scrollRect.vertical = true;
        }

//      CreateItemGroup(new string[] { "1","2", "1", "2", "1", "2", "1", "2", "1", "2", "1", "2", "1", "2", "1", "2", "1", "2"});
//      CreateItemGroup(new string[] { "A-3户型","B-4户型","C-5户型" }, new string[] { "HX128", "HX156", "HX201" });
//      CreateItemGroup(new string[] { "A户型", "B户型", "C户型"}, null);
    }

    public void CreateItemGroup(string[] btnGroupDisplayName,string[] btnGroupParameter)
    {
        nonStandFloor = new List<ScrollItem>();
        allScrollItem = new List<ScrollItem>();

        if (btnGroupParameter == null)
        {
            btnGroupParameter = btnGroupDisplayName;
        }

        ScrollItem[] lastScrollItem = scrollRect.content.GetComponentsInChildren<ScrollItem>();

//      Debug.Log(lastScrollItem.Length);

        for (int i = 0; i < lastScrollItem.Length; i++)
        {
            if (lastScrollItem[i] != null)
                DestroyObject(lastScrollItem[i].gameObject);
        }

        Vector2 startPos= itemSpace;

        for (int i = 0; i < btnGroupDisplayName.Length; i++)
        {
            ScrollItem sItem= Instantiate(itemPrefab,new Vector3(0,0,0),new Quaternion(), scrollRect.content);
            Vector2 itemSize= sItem.SetupItem(btnGroupDisplayName[i], scroolItemHeight, startPos, scrollItemMinWidth, fontSize, paddingFactor);
            Vector2 iSpace = Vector2.zero;

            //
            sItem.imageButton.btnNameForRemote ="B"+i.ToString() + "_"+ this.gameObject.name+btnGroupParameter[i];
            sItem.imageButton.btnNameForRemote = Regex.Replace(sItem.imageButton.btnNameForRemote, @"[\u4e00-\u9fa5]", "");

            RemoteGather.AddImageToGroup(sItem.imageButton,true);

            sItem.imageButton.RecordOrginState();

            if (itemDirection == ItemDirection.水平)
            {
                itemSize = new Vector2(itemSize.x, 0);
                iSpace = new Vector2(itemSpace.x,0);
            }
            else
            {
                itemSize = new Vector2(0, -itemSize.y);
                iSpace = new Vector2(0, itemSpace.y);
            }

//          if(i!= btnGroupDisplayName.Length-1)
            startPos += itemSize+ iSpace;

            //设置item的按钮事件
            BaseEventDelegate itemTrue = new BaseEventDelegate();
            BaseEventDelegate itemfalse = new BaseEventDelegate();

            itemTrue.parameterTargetSlot = new int[] {0,3};
            itemfalse.parameterTargetSlot=new int[] { 0, 3 };

            itemTrue.parameterList = new EventParametar[2];
            itemfalse.parameterList = new EventParametar[2];

            itemTrue.parameterList[0].pObject = sItem;
            itemTrue.parameterList[1].pString = btnGroupParameter[i];

            itemfalse.parameterList[0].pObject = sItem;
            itemfalse.parameterList[1].pString = btnGroupParameter[i];

            itemTrue.excuteMethodName = "ItemBtnExeTrue";
            itemfalse.excuteMethodName = "ItemBtnExeFalse";

            itemTrue.targetMono = this;
            itemfalse.targetMono = this;

            itemTrue.currentEditorChooseFunName = "ScrollMenu/ItemBtnExeTrue";
            itemTrue.lastEditorChooseFunName = "ScrollMenu/ItemBtnExeTrue";

            itemfalse.currentEditorChooseFunName = "ScrollMenu/ItemBtnExeTrue";
            itemfalse.lastEditorChooseFunName = "ScrollMenu/ItemBtnExeTrue";

            sItem.imageButton.trueEventList.Add(itemTrue);
            sItem.imageButton.falseEventList.Add(itemfalse);

            allScrollItem.Add(sItem);

            //判断参数是否可以转成有效的数字,如果可以就是楼层
            int temp;
            if (!int.TryParse(btnGroupParameter[i].Replace("F", ""), out temp))
            {
                nonStandFloor.Add(sItem);
            }
        }

        if (itemDirection == ItemDirection.水平)
        {
            scrollRect.content.sizeDelta = new Vector2(startPos.x, scrollRect.GetComponent<RectTransform>().sizeDelta.y);

            //设置居中
            if (startPos.x < (scrollRect.GetComponent<RectTransform>().sizeDelta.x-200))
            {
                Vector2 offset=new Vector2((scrollRect.GetComponent<RectTransform>().sizeDelta.x-200- startPos.x)*0.5f,0f);

//              Debug.Log(offset);

                foreach (ScrollItem s in scrollRect.content.GetComponentsInChildren<ScrollItem>())
                {
                    s.GetComponent<RectTransform>().anchoredPosition += offset;
                    s.imageButton.RecordOrginState();
                }
            }
        }
        else
        {
            scrollRect.content.sizeDelta = new Vector2(scrollRect.GetComponent<RectTransform>().sizeDelta.x, -startPos.y);
        }
        
    }


    public ScrollItem GetFirstScrollItem()
    {
        if (allScrollItem!=null&&allScrollItem.Count > 0)
        {
            return allScrollItem[0];
        }
        else
        {
            return null;
        }
    }


    [Serializable]
    public class ItemEvent : UnityEvent<string>
    {

    }



}
