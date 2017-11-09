using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackAction : MonoBehaviour
{

    public List<ImageButton> needBackBtnGroup;
    public CanveGroupFade backBtn;
    public CanveGroupFade exitBtn;

    void Awake()
    {
        needBackBtnGroup = new List<ImageButton>();
    }

    void Start()
    {
        if (needBackBtnGroup.Count == 0)
        {
            backBtn.AlphaPlayBackward();
        }

    }


    public void AddToBackBtnGroup(ImageButton iBtn)
    {
        needBackBtnGroup.Add(iBtn);
        backBtn.AlphaPlayForward();
        exitBtn.AlphaPlayBackward();
    }


    public void Back()
    {

//      Debug.Log(needBackBtnGroup.Count);

        if (needBackBtnGroup.Count > 0)
        {
            needBackBtnGroup[needBackBtnGroup.Count - 1].SetBtnState(false, 0);
            needBackBtnGroup.RemoveAt(needBackBtnGroup.Count - 1);

        }

        if (needBackBtnGroup.Count == 0)
        {
            backBtn.AlphaPlayBackward();
            exitBtn.AlphaPlayForward();
        }
    }

    public void RemoveAllBackEvent()
    {
        needBackBtnGroup.Clear();
    }



    



}
