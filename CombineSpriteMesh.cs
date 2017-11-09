using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using System.Collections;
using System.Linq;
using System;


public class CombineSpriteMesh : ScriptableWizard
{

	public Transform combineSpriteMeshRoot;
    MeshFilter[] allChildMeshFilters;


    List<Vector3> vertexList;
    List<Vector3> normalList;
    List<Color> colorList;
    List<Vector2> uvList;
    //for sprite postion and scale
//    List<Vector4> tangentLisht;
    List<List<int>> subMeshIndiceOrder;

    int singerSubMeshCount;

    Mesh outPutMesh;



    [MenuItem ("WJMTooLs/CombineSpriteMesh")]
	
	static void createWizard() 
	{
		ScriptableWizard.DisplayWizard("Combine Sprite Mesh", typeof (CombineSpriteMesh), "CombineSpriteMesh");

    }
	
	void OnWizardCreate ()
	{
        if (combineSpriteMeshRoot!=null)
        {
            vertexList = new List<Vector3>();
            normalList = new List<Vector3>();
            uvList = new List<Vector2>();
            //rgb-worldPos,a-Scale
            colorList = new List<Color>();

//          subMeshIndiceOrder = new List<List<int>>();

            allChildMeshFilters = combineSpriteMeshRoot.GetComponentsInChildren<MeshFilter>();

            if (allChildMeshFilters.Length > 0)
            {
                singerSubMeshCount = allChildMeshFilters[0].sharedMesh.subMeshCount;

                subMeshIndiceOrder = new List<List<int>>();

                for (int i = 0; i < singerSubMeshCount; i++)
                {
                    subMeshIndiceOrder.Add(new List<int>());
                }
                Debug.Log(subMeshIndiceOrder.Count);
            }

            for (int i = 0; i < allChildMeshFilters.Length; i++)
            {
                Vector4 worldPosAndScale = new Vector4(allChildMeshFilters[i].transform.position.x, allChildMeshFilters[i].transform.position.y, allChildMeshFilters[i].transform.position.z, allChildMeshFilters[i].transform.lossyScale.x);

                for (int j = 0; j < allChildMeshFilters[i].sharedMesh.vertices.Length; j++)
                {
                    vertexList.Add(allChildMeshFilters[i].transform.TransformPoint(allChildMeshFilters[i].sharedMesh.vertices[j]));
                    normalList.Add(allChildMeshFilters[i].transform.TransformDirection(allChildMeshFilters[i].sharedMesh.normals[j]));
                }
                //              vertexList.AddRange(allChildMeshFilters[i].sharedMesh.vertices);
                //                normalList.AddRange(allChildMeshFilters[i].sharedMesh.normals);
                uvList.AddRange(allChildMeshFilters[i].sharedMesh.uv);
                //rgb-worldPos,a-Scale
                

                for (int j = 0; j < allChildMeshFilters[i].sharedMesh.vertices.Length; j++)
                {
                     Color tempCol = new Color(worldPosAndScale.x, worldPosAndScale.y, worldPosAndScale.z, worldPosAndScale.w);
                    // Debug.Log(tempCol);
                    colorList.Add(tempCol);
                }

                for (int j = 0; j < singerSubMeshCount; j++)
                {
                 //   allChildMeshFilters[j].sharedMesh.tr

                    int[] getIndices = allChildMeshFilters[i].sharedMesh.GetIndices(j);

                    for (int k = 0; k < getIndices.Length; k++)
                    {
                        getIndices[k] += allChildMeshFilters[i].sharedMesh.vertices.Length* i;

//                      Debug.Log(getIndices[k]);
                    }

                    subMeshIndiceOrder[j].AddRange(getIndices);

                    for (int z = 0; z < subMeshIndiceOrder[j].Count; z++)
                    {
                 //       Debug.Log(subMeshIndiceOrder[j][z]);
                    }
                }


            }

            outPutMesh = new Mesh();

            outPutMesh.vertices = vertexList.ToArray();
            outPutMesh.normals = normalList.ToArray();
            outPutMesh.uv = uvList.ToArray();
            outPutMesh.colors= colorList.ToArray();
            outPutMesh.subMeshCount = singerSubMeshCount;
//            outPutMesh.


            for (int i = 0; i < subMeshIndiceOrder.Count; i++)
            {
//                Debug.Log(subMeshIndiceOrder[i].ToArray());
                outPutMesh.SetIndices(subMeshIndiceOrder[i].ToArray(), MeshTopology.Triangles, i);
            }

            if (!Directory.Exists(Application.dataPath + "/intermediate"))
            {
                Directory.CreateDirectory(Application.dataPath + "/intermediate");
            }


            string tempCombineSpriteMeshFolderFinal = "Assets/intermediate/CombineSpritMesh";

            if(!Directory.Exists(tempCombineSpriteMeshFolderFinal))
            {
                Directory.CreateDirectory(tempCombineSpriteMeshFolderFinal);
            }

            string finalName= tempCombineSpriteMeshFolderFinal + "/" + combineSpriteMeshRoot.name + "_CombineSpriteMesh.asset";


            Unwrapping.GenerateSecondaryUVSet(outPutMesh);


            if (!File.Exists(finalName))
            {
                AssetDatabase.CreateAsset(outPutMesh, finalName);

            }
            else
            {
               Mesh existMesh=(Mesh) AssetDatabase.LoadAssetAtPath(finalName, typeof(Mesh));
                existMesh.Clear();
                existMesh.vertices = outPutMesh.vertices;
                existMesh.normals = outPutMesh.normals;
                existMesh.uv = outPutMesh.uv;
                existMesh.colors = outPutMesh.colors;



                //                existMesh.triangles = outPutMesh.triangles;
                existMesh.subMeshCount= outPutMesh.subMeshCount;
                for (int i = 0; i < outPutMesh.subMeshCount; i++)
                {
                    existMesh.SetIndices(outPutMesh.GetIndices(i), MeshTopology.Triangles, i);
                }

                Unwrapping.GenerateSecondaryUVSet(existMesh);
            }

            combineSpriteMeshRoot.gameObject.SetActive(false);

            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            Debug.Log("合并成功");
        }
	}


}
