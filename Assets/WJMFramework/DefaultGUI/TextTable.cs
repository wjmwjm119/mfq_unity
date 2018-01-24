using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextTable : MonoBehaviour
{
    public Text headText;
    public Text headTextShadow;
    public Text infoText;
    public Text infoTextShadow;
    public RectTransform vLine;
    public RectTransform bg;

    public void SetString(string headString,string infoString)
    {
 //       Debug.Log(infoString);

        if (infoString==null|| infoString == "")
        {
            vLine.gameObject.SetActive(false);
            bg.gameObject.SetActive(false);
        }
        headText.text = headString;
        headTextShadow.text = headString;
        infoText.text = infoString;
        infoTextShadow.text = infoString;

    }

}
