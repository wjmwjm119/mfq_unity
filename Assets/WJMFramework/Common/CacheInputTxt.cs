using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.UI;

public class CacheInputTxt : MonoBehaviour
{
    public InputField InputField;

    void Start()
    {
        LoadInputTxtCache();
    }

    public void SaveInputTxt()
    {
        File.WriteAllText(Application.persistentDataPath + "/testInputCache.txt", InputField.text);
    }

    public void LoadInputTxtCache()
    {
        if (File.Exists(Application.persistentDataPath + "/testInputCache.txt"))
        {
            InputField.text = File.ReadAllText(Application.persistentDataPath + "/testInputCache.txt");
        }
    }

}
