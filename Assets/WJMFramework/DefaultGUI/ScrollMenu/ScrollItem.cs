using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ScrollItem : MonoBehaviour
{

    public ImageButton imageButton;
    public Text text;


    public Vector2 SetupItem(string itemName,float height,Vector2 pos,float minBtnWidth,int fontSize,float paddingFactor)
    {
        text.fontSize = fontSize;
        text.text = itemName;
        
        imageButton.GetComponent<RectTransform>().sizeDelta = new Vector2(Mathf.Max(minBtnWidth, text.preferredWidth)+fontSize* paddingFactor, height);
        imageButton.GetComponent<RectTransform>().anchoredPosition = pos;
       
        return imageButton.GetComponent<RectTransform>().sizeDelta;

    }

 

}
