using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CameraUniversal), true)]
public class CameraUniversalEditor : Editor
{

    bool inEditor;
    CameraUniversal c;
    CameraUniversal lastC;

    bool cOriginActive;
    bool lastCOriginActive;

    string preinfo="提示";

    Vector3 pos;
    Vector3 xyzCount;



    void Start()
    {
        inEditor = false;
    }


    public override void OnInspectorGUI()
    {

    //      Debug.Log("111111111");
            SerializedObject argsSerializedObject = new SerializedObject(target);

            if(Application.isPlaying)
            c = (CameraUniversal)target;

            SerializedProperty sp = argsSerializedObject.GetIterator();

            Undo.RecordObject(target, "LabelTextrueRender");
            EditorUtility.SetDirty(target);


            EditorGUILayout.Space();
            GUILayout.TextField("", GUILayout.MaxHeight(1));

            EditorGUILayout.BeginHorizontal();

        if (!Application.isPlaying)
        {
            if (GUILayout.Button("调整位置", GUILayout.MaxWidth(100), GUILayout.Height(30)))
            {
                inEditor = !inEditor;
                if (inEditor)
                {
                    c.gameObject.SetActive(true);
                }
                else
                {
                    c.gameObject.SetActive(cOriginActive);
                }
            }
        }

            if (GUILayout.Button("打印当前参数", GUILayout.MaxWidth(100), GUILayout.Height(30)))
            {
                if (c != null)
                {
                    Debug.Log(preinfo + ":    " + c.GetCameraStateJson());
                }
            }
            preinfo = GUILayout.TextArea(preinfo, GUILayout.MaxWidth(60), GUILayout.Height(30));

            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();
            GUILayout.TextField("", GUILayout.MaxHeight(1));

            if (inEditor)
            {
                pos = c.camBase.localPosition;
                xyzCount = new Vector3(c.Xcount, c.Ycount, c.Zcount);

                GUI.backgroundColor = new Color(0, 1, 0);

                pos = EditorGUILayout.Vector3Field("Position:", pos);
                xyzCount= EditorGUILayout.Vector3Field("xyzCount:", xyzCount);

                c.camBase.localPosition = pos;
                c.Xcount = xyzCount.x;
                c.Ycount = xyzCount.y;
                c.Zcount = xyzCount.z;

                GUI.backgroundColor = new Color(1, 1, 1);
            }




        

        //第一步必须加这个
        sp.NextVisible(true);

            while (sp.NextVisible(false))
            {
                EditorGUILayout.PropertyField(sp, true);

            }
            argsSerializedObject.ApplyModifiedProperties();

            if (inEditor)
                c.InitlCameraInEditor();

      


    }


    void OnEnable()
    {

        if (!Application.isPlaying)
        {
            c = (CameraUniversal)target;
            cOriginActive = c.gameObject.activeInHierarchy;

            pos = c.camBase.localPosition;
            xyzCount = new Vector3(c.Xcount, c.Ycount, c.Zcount);

        }
    }

    void OnDisable()
    {
        
        if (c != null&&!Application.isPlaying&&inEditor)
        {
            c.gameObject.SetActive(cOriginActive);
        }

    }

}
