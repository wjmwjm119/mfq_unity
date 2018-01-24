using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(SpritePlayer), true)]
public class SpritePlayerEditor : Editor
{

    int numLength = 3;

    public override void OnInspectorGUI()
    {

        SpritePlayer spritePlayer = (SpritePlayer) target;

        if (!Application.isPlaying)
        {
            numLength = EditorGUILayout.IntField("序列中数字长度：",numLength);

            if (GUILayout.Button("顺序排序Sprite", GUILayout.MaxWidth(100), GUILayout.Height(30)))
            {
                spritePlayer.OrderSprite(numLength);
            }
        }



        SerializedObject argsSerializedObject = new SerializedObject(target);
        SerializedProperty sp = argsSerializedObject.GetIterator();
        Undo.RecordObject(target, "SpritePlayer");
        EditorUtility.SetDirty(target);

        //第一步必须加这个
        sp.NextVisible(true);
        EditorGUILayout.PropertyField(sp, true);


        sp.NextVisible(false);
//      EditorGUILayout.PropertyField(sp, true);

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);

        sp.NextVisible(false);
//      EditorGUILayout.PropertyField(sp, true);

        sp.NextVisible(false);
        EditorGUILayout.PropertyField(sp, true);

        sp.NextVisible(false);
//        EditorGUILayout.PropertyField(sp, true);

        sp.NextVisible(false);
        sp.NextVisible(false);
        sp.NextVisible(false);
        sp.NextVisible(false);
        sp.NextVisible(false);
        sp.NextVisible(false);
        sp.NextVisible(false);

        while (sp.NextVisible(false))
        { 
            EditorGUILayout.PropertyField(sp, true);     
        }

        argsSerializedObject.ApplyModifiedProperties();



    }

}
