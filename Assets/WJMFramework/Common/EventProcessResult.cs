using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class EventProcessResult : MonoBehaviour
{
   public ScrollRect scrollRect;
 //  int eachlineHeigh = 12;
   public  Text resultinfo;
   public Text percentInfo;
   public string lastInfo;
	int infoCount=0;

   public  void DisplayInfo(string info,bool clearLastInfo=false)
    {

        if (clearLastInfo)
        {
			infoCount=0;
            lastInfo = "";
        }
		lastInfo += "\n"+infoCount+"---";
		resultinfo.text = lastInfo+info;
		lastInfo = resultinfo.text;
		infoCount++;
        scrollRect.content.sizeDelta = new Vector2(scrollRect.content.sizeDelta.x,infoCount*resultinfo.fontSize*2f);

    }

    public void DisplayPercent(string info)
    {

        percentInfo.text = info;

    }

}
