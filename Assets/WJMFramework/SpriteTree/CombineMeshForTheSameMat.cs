using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CombineMeshForTheSameMat : MonoBehaviour
{
    public MeshRenderer[] allMeshRenderer;
    
    public List<Material> allChildMatGroup;

    public List<CombinedMesh> allCombineMeshes;


    public void GenCombineMesh()
    {
        allChildMatGroup = new List<Material>();
        allCombineMeshes = new List<CombinedMesh>();
        allMeshRenderer = GetComponentsInChildren<MeshRenderer>();

        foreach (MeshRenderer m in allMeshRenderer)
        {
            if (m.sharedMaterials.Length == 1&&!allChildMatGroup.Contains( m.sharedMaterial))
            {
                allChildMatGroup.Add(m.sharedMaterial);
            }
        }

        for (int i = 0; i < allChildMatGroup.Count; i++)
        {
            CombinedMesh cMesh = new CombinedMesh();
            cMesh.mat = allChildMatGroup[i];

            foreach (MeshRenderer m in allMeshRenderer)
            {
                if (m.sharedMaterials.Length == 1)
                {
                    if (cMesh.mat == m.sharedMaterial)
                    {
                        cMesh.allChildMeshFilter.Add(m.GetComponent<MeshFilter>());
                    }
                }
            }

            cMesh.GenCombineMesh();
            allCombineMeshes.Add(cMesh);
        }


    }

}

[System.Serializable]
public class CombinedMesh
{
    public MeshFilter combineMeshFilter;
    public Material mat;
    public List<MeshFilter> allChildMeshFilter;

    public List<Vector3> vertexList;
    public List<Vector3> normalList;
    public List<Vector2> uvList;
    [HideInInspector]
    public List<Color> colorList;
    public List<int> indices;

    public CombinedMesh()
    {
        allChildMeshFilter = new List<MeshFilter>();
        vertexList = new List<Vector3>();
        normalList = new List<Vector3>();
        uvList = new List<Vector2>();
        colorList = new List<Color>();
        indices = new List<int>();
}

    public void GenCombineMesh()
    {
        for (int i = 0; i < allChildMeshFilter.Count; i++)
        {
            Vector4 worldPosAndScale = new Vector4(allChildMeshFilter[i].transform.position.x, allChildMeshFilter[i].transform.position.y, allChildMeshFilter[i].transform.position.z, allChildMeshFilter[i].transform.lossyScale.x);

            for (int j = 0; j < allChildMeshFilter[i].sharedMesh.vertices.Length; j++)
            {
                vertexList.Add(allChildMeshFilter[i].transform.TransformPoint(allChildMeshFilter[i].sharedMesh.vertices[j]));
                normalList.Add(allChildMeshFilter[i].transform.TransformDirection(allChildMeshFilter[i].sharedMesh.normals[j]));
            }

            uvList.AddRange(allChildMeshFilter[i].sharedMesh.uv);

            for (int j = 0; j < allChildMeshFilter[i].sharedMesh.vertices.Length; j++)
            {
                Color tempCol = new Color(worldPosAndScale.x, worldPosAndScale.y, worldPosAndScale.z, worldPosAndScale.w);
                colorList.Add(tempCol);
//              Debug.Log(tempCol);
            }
            
            int[] singerMeshIndices= allChildMeshFilter[i].sharedMesh.GetIndices(0);

            for (int j = 0; j < singerMeshIndices.Length; j++)
            {
                singerMeshIndices[j] += allChildMeshFilter[i].sharedMesh.vertices.Length * i;
            }

            indices.AddRange(singerMeshIndices);
            

        }
    }

}

