using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SenceInteractiveInfo), true)]
public class SenceInteractiveInfoEditor : Editor
{

    public override void OnInspectorGUI()
    {

        SenceInteractiveInfo senceInteractiveInfo =(SenceInteractiveInfo) target;

        int propertyID=0;

        SerializedObject argsSerializedObject = new SerializedObject(target);
        SerializedProperty sp = argsSerializedObject.GetIterator();
        Undo.RecordObject(target, "SenceInteractiveInfo");
        EditorUtility.SetDirty(target);

        //第一步必须加这个
        sp.NextVisible(true);
        EditorGUILayout.PropertyField(sp, true);
        propertyID++;

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);
        propertyID++;

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);
        propertyID++;

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);
        propertyID++;

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);
        propertyID++;

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);
        propertyID++;

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);
        propertyID++;



        while (sp.NextVisible(false))
        {
            propertyID++;

            if (senceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.大场景)
            {
                if (propertyID < 16)
                {
                    EditorGUILayout.PropertyField(sp, true);
                }
            }
//            else if (senceInteractiveInfo.sceneType == SenceInteractiveInfo.SceneType.Point360)
//            {
//                if (propertyID > 15)
//                {
//                    EditorGUILayout.PropertyField(sp, true);
//                }
//            }
            else
            {
                if (propertyID > 14)
                {
                    EditorGUILayout.PropertyField(sp, true);
                }
            }

        }

        argsSerializedObject.ApplyModifiedProperties();


        if (senceInteractiveInfo.sceneType != SenceInteractiveInfo.SceneType.大场景)
        {
            EditorGUILayout.Space();
            GUILayout.TextField("", GUILayout.MaxHeight(1));
            //GUI.color = new Color(1, 1, 0);

            EditorGUILayout.LabelField("All Floor 中的Floor Name参数设置时,请按以下方式设置,\n从最底层开始如-2F,-1F,1F,2F;\n屋顶一层请命名为WD;外墙体请命名为WQT;\n", GUILayout.Height(60));
        }

    }

}
