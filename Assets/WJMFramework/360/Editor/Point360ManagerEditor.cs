using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Point360Manager), true)]
public class Point360ManagerEditor : Editor
{
    Point360Manager p;

    public override void OnInspectorGUI()
    {

        SerializedObject argsSerializedObject = new SerializedObject(target);
        SerializedProperty sp = argsSerializedObject.GetIterator();
        Undo.RecordObject(target, "Point360Manager");
        EditorUtility.SetDirty(target);


        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("生成定点360", GUILayout.MaxWidth(100), GUILayout.Height(30)))
        {
            p = (Point360Manager)target;
            p.GenPoint360();
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
