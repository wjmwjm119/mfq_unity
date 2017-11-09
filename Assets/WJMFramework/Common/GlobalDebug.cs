using UnityEngine;
using System.Collections;

public class GlobalDebug : MonoBehaviour 
{

	public bool globalDebug=true;
    static bool staticGlobalDebug = true;

    static string[] GlobalInfoString=new string[21];

    GUIStyle myStyle;

	void Awake ()
	{
//		DontDestroyOnLoad (this);

		myStyle = new GUIStyle();
		myStyle.fontSize = 17;
        myStyle.wordWrap = true;
		myStyle.normal.textColor = new Color(1, 0, 0, 1);

        //0～19是替换更新
        //20是叠加更新
        GlobalInfoString[20] = "Start\n";
        staticGlobalDebug = globalDebug;

    }

    public void SetDebugDisplay(bool d)
    {
        globalDebug = d;
        staticGlobalDebug = d;
    }

	void OnGUI()
	{
		GUI.depth=-1000;

		if (staticGlobalDebug)
		{
            for (int i = 0; i < GlobalInfoString.Length; i++)
            {
                GUILayout.Label(i+"-->"+GlobalInfoString[i], myStyle,GUILayout.MaxWidth(Screen.width*0.9f));
            }

		}
	}

    static public void ReplaceLine(string debugInfo, int rowID)
    {
        if (staticGlobalDebug)
        {
            GlobalInfoString[rowID] = debugInfo;
        }
    }

 	static	public  void Addline(string debugInfo,bool clearLastInfo=false)
	{
        if (staticGlobalDebug)
        {
            if (clearLastInfo)
                GlobalInfoString[20] = "";
            GlobalInfoString[20] += debugInfo;
            GlobalInfoString[20] += "\n";
        }
    }

    static public void Clear()
    {
        if (staticGlobalDebug)
        {
            GlobalInfoString[20] = "";
        }

    }


}
