using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

[CustomEditor(typeof(CombineMeshForTheSameMat), true)]

public class CombineMeshForTheSameMatEditor : Editor
{
    CombineMeshForTheSameMat c;

    public override void OnInspectorGUI()
    {

            SerializedObject argsSerializedObject = new SerializedObject(target);

            c = (CombineMeshForTheSameMat)target;

            SerializedProperty sp = argsSerializedObject.GetIterator();

            Undo.RecordObject(target, "CombineMeshForTheSameMat");
            EditorUtility.SetDirty(target);


            EditorGUILayout.Space();
            GUILayout.TextField("", GUILayout.MaxHeight(1));

            EditorGUILayout.BeginHorizontal();


            if (GUILayout.Button("合并Mesh", GUILayout.MaxWidth(100), GUILayout.Height(30)))
            {
                if (c != null)
                {
                    c.GenCombineMesh();

                    foreach (CombinedMesh cMesh in c.allCombineMeshes)
                    {
                        SaveOutCombineMesh(cMesh);
                    }

    //              c.gameObject.SetActive(false);
                    AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
                    Debug.Log("合并完成");
                }

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

    public void SaveOutCombineMesh(CombinedMesh cMesh)
    {

        Mesh outPutMesh = new Mesh();

        outPutMesh.vertices = cMesh.vertexList.ToArray();
        outPutMesh.normals = cMesh.normalList.ToArray();
        outPutMesh.uv = cMesh.uvList.ToArray();
        outPutMesh.colors = cMesh.colorList.ToArray();
        outPutMesh.subMeshCount = 1;

        outPutMesh.SetIndices(cMesh.indices.ToArray(), MeshTopology.Triangles,0);
        
        if (!Directory.Exists(Application.dataPath + "/intermediate"))
        {
            Directory.CreateDirectory(Application.dataPath + "/intermediate");
        }


        string tempCombineSpriteMeshFolderFinal = "Assets/intermediate/CombineSpritMesh";

        if (!Directory.Exists(tempCombineSpriteMeshFolderFinal))
        {
            Directory.CreateDirectory(tempCombineSpriteMeshFolderFinal);
        }

        string finalName = tempCombineSpriteMeshFolderFinal + "/" + cMesh.mat.name + "_CombineSpriteMesh.asset";


        Unwrapping.GenerateSecondaryUVSet(outPutMesh);


        if (!File.Exists(finalName))
        {
            AssetDatabase.CreateAsset(outPutMesh, finalName);

        }
        else
        {
            Mesh existMesh = (Mesh)AssetDatabase.LoadAssetAtPath(finalName, typeof(Mesh));
            existMesh.Clear();
            existMesh.vertices = outPutMesh.vertices;
            existMesh.normals = outPutMesh.normals;
            existMesh.uv = outPutMesh.uv;
            existMesh.colors = outPutMesh.colors;
            //existMesh.triangles = outPutMesh.triangles;
            existMesh.subMeshCount = outPutMesh.subMeshCount;
            for (int i = 0; i < outPutMesh.subMeshCount; i++)
            {
                existMesh.SetIndices(outPutMesh.GetIndices(i), MeshTopology.Triangles, i);
            }

            Unwrapping.GenerateSecondaryUVSet(existMesh);
        }

        Debug.Log("生成"+cMesh.mat.name + "_CombineSpriteMesh");
    }

}
