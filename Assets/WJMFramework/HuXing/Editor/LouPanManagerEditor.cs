using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(LouPanManager), true)]
public class LouPanManagerEditor : Editor
{

    LouPanManager l;

    public override void OnInspectorGUI()
    {

        SerializedObject argsSerializedObject = new SerializedObject(target);
        SerializedProperty sp = argsSerializedObject.GetIterator();
        Undo.RecordObject(target, "LouPanManager");
        EditorUtility.SetDirty(target);


        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("更新选房坐标", GUILayout.MaxWidth(100), GUILayout.Height(30)))
        {
            l = (LouPanManager)target;
            l.GenAllHuXingInstance();
        }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.Space();
        GUILayout.TextField("", GUILayout.MaxHeight(1));


        //第一步必须加这个
        sp.NextVisible(true);

        while (sp.NextVisible(false))
        {
            EditorGUILayout.PropertyField(sp, true);
        }
        argsSerializedObject.ApplyModifiedProperties();



    }

}
