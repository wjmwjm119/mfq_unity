using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//用来记录场景中的所有物体信息
//Instance3DGameObject只能使用单材质物体

public class Object3DInfo:MonoBehaviour
{
    /// <summary>
    /// 是否导出此物体下的所有子物体,如果是Instance3DGameObject物体，子物体都将会忽略不导出
    /// </summary>
    public bool isNeedExportChild = true;
    //    public bool exportNormal = true;

    /// <summary>
    /// 如果使用完全贴图就不需要导出UV1
    /// </summary>
    /// 
    [System.NonSerialized]
    public bool exportUV = true;

    /// <summary>
    /// 不烘培物体不需要导出UV2
    /// </summary>
    /// 
    [System.NonSerialized]
    public bool exportUV2 = true;

    /// <summary>
    /// 使用灯光也将导出法线
    /// </summary>
    /// 
    [System.NonSerialized]
    public bool useRealtimeLight = true;

    /// <summary>
    /// 使用vertexcolor替换烘培贴图,同时UV2将不导出，因为用不上UV2,同时会导出VertexColor
    /// </summary>
    [HideInInspector]
    public bool useVertexColorLightmap = false;

    public uint renderOrder = 0;

    [HideInInspector]
    public bool isInstance3DGameObject = false;
    /// <summary>
    /// 所有需要instance子物体的位置
    /// </summary>
    [HideInInspector]
    public List<float> allChildPositionList;
    /// <summary>
    /// 单个物体使用的模型或者instance时单个物体的模型
    /// </summary>
    /// 

    public Mesh mesh;
    public Material[] mats;

    [HideInInspector]
    public string idJson;
    [HideInInspector]
    public string nameJson;
    [HideInInspector]
    public string layerJson;
    [HideInInspector]
    public string positionJson;
    [HideInInspector]
    public string rotationJson;
    [HideInInspector]
    public string scaleJson;
    [HideInInspector]
    public string parentJson;
    [HideInInspector]
    public string meshJson;
    [HideInInspector]
    public string renderOrderJson;
    [HideInInspector]
    public string materialsJson;
    [HideInInspector]
    public string lightmapOffsetJson;
    [HideInInspector]
    public string instanceChildPosition;



    Vector3[] allChildPosition;
    Vector3[] allChildRotation;
    Vector3[] allChildScale;

    public int objectId;
    //  如果要使用完全贴图，要把UV2输出到UV通道，原UV抛弃以减少体量。
    //  public bool sweepUV = false;
    //  public bool onlyFirstMat = true;

    public int lightmapIndex=-1;
    public Vector4 lightmapScaleOffset;


    public bool hasSpriteTreeMat;
    public bool hasWaterMat;

    public int vertexCount;
    public int triFaceCount;


    public bool RecordInfo(int thisGameObjectID)
    {
        hasWaterMat = false;
        hasSpriteTreeMat = false;

        objectId = thisGameObjectID;

        if (isInstance3DGameObject)
        {
            if (!GenInstance3DGameObjectData())
            {
                string log = gameObject.name + "Instance3DGameObject物体数据错误";
                Debug.Log(log);
                Debug.LogWarning(log);
                Debug.LogError(log);
                return false;
            }
        }
        else
        {
            if (GetComponent<MeshFilter>()!=null&& GetComponent<MeshRenderer>()!=null)
            {
                mesh = GetComponent<MeshFilter>().sharedMesh;
                mats = GetComponent<MeshRenderer>().sharedMaterials;

                if (mesh == null)
                {
                    string log = gameObject.name + "Mesh 丢失，请检查！";
                    Debug.Log(log);
                    Debug.LogWarning(log);
                    Debug.LogError(log);
                    return false;
                }



                if (mesh.uv.Length<1)
                    exportUV = false;

                if (mesh.uv2.Length < 1)
                    exportUV2 = false;

                //unity默认的Plane没有带uv2。因为经常使用的Plane，所有将uv1复制到uv2
                if (mesh.name == "Plane")
                {
                    exportUV2 = true;
                }
                    

                lightmapIndex = GetComponent<MeshRenderer>().lightmapIndex;
                if (lightmapIndex ==65535)
                    lightmapIndex = 1;
                    lightmapScaleOffset = GetComponent<MeshRenderer>().lightmapScaleOffset;

                foreach (Material m in mats)
                {
                    if (m == null)
                    {
                        string log = gameObject.name + "有空材质，请检查材质属性！";
                        Debug.Log(log);
                        Debug.LogWarning(log);
                        Debug.LogError(log);
                        return false;
                    }
                }

            }
        }

        GenGameObjectJsonString();

        return true;
    }

    /// <summary>
    /// Instance3DGameObject必须有Mesh
    /// </summary>
    /// <returns></returns>
    bool GenInstance3DGameObjectData()
    {
        
        MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
//        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        //如果没有指定mesh，将会用层级下子物体的模型
        if ( meshFilters.Length > 0)
        {
            mesh = meshFilters[0].sharedMesh;
            if (mesh == null)
            {
                string log = gameObject.name + "的mesh不能未空，请保证此物体层级下有子物体mesh";
                Debug.Log(log);
                Debug.LogWarning(log);
                Debug.LogError(log);
                return false;
            }
        }

        mats = meshFilters[0].GetComponent<MeshRenderer>().sharedMaterials;

/*
        if (mats.Length != 1)
        {
            string log = gameObject.name + "子物体的材质个数只能为1个";
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
            return false;
        }
*/

        allChildPositionList = new List<float>();
        allChildPosition = new Vector3[meshFilters.Length];
        allChildRotation = new Vector3[meshFilters.Length];
        allChildScale = new Vector3[meshFilters.Length];

        int i = 0;
        while (i < meshFilters.Length)
        {
            allChildPosition[i] = meshFilters[i].transform.localPosition;
            allChildRotation[i] = meshFilters[i].transform.localEulerAngles;
            allChildScale[i] = meshFilters[i].transform.localScale;
//          combine[i].mesh = meshFilters[i].sharedMesh;
//          combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            //meshFilters[i].gameObject.SetActive(false);
            i++;
        }

        for (int j = 0; j < allChildPosition.Length; j++)
        {
            allChildPositionList.Add(allChildPosition[j].x);
            allChildPositionList.Add(allChildPosition[j].y);
            allChildPositionList.Add(-allChildPosition[j].z);
        }

        return true;

    }

    bool GenGameObjectJsonString()
    {
        vertexCount = 0;
        triFaceCount = 0;

        idJson= "\"objectid\":\"" + objectId + "\"";

        nameJson = "\"name\":\"" + name + "\"";

        layerJson= "\"layer\":\"" + gameObject.layer + "\"";

        //负Z进行反转
        positionJson = "\"position\":[" + transform.localPosition.x.ToString() + "," + transform.localPosition.y.ToString() + "," + (-transform.localPosition.z).ToString() + "]";
        rotationJson = "\"rotation\":[" + transform.rotation.eulerAngles.x.ToString() + "," + transform.localEulerAngles.y.ToString() + "," + transform.localEulerAngles.z.ToString() + "]";
        rotationJson += ",\"rotation2\":[" + transform.rotation.x.ToString() + "," + transform.rotation.y.ToString() + "," + (-transform.rotation.z).ToString() + "," + (-transform.rotation.w).ToString() +"]";
        scaleJson = "\"scale\":[" + transform.localScale.x.ToString() + "," + transform.localScale.y.ToString() + "," + transform.localScale.z.ToString() + "]";

        if (transform.parent != null)
        {
            //使用名字作为父对象，有重名的可能
            // parentJson = "\"parent\":\"" + transform.parent.name + "\"";
               parentJson = "\"parentid\":\"" + transform.parent.GetComponent<Object3DInfo>().objectId + "\"";
        }
        else
        {
            parentJson = "\"parentid\":\"\"";
        }
        meshJson = "\"mesh\":\"" + "\"";

        if (mesh !=null)
        {
            vertexCount = mesh.vertices.Length;
            triFaceCount = mesh.triangles.Length / 3;

            meshJson = "\"mesh\":\"" + mesh.name + "\"";

//        if (isSpritMesh)
//        {
//            meshJson += ",\"renderType\":\"point\"";
//        }

            if (isInstance3DGameObject)
            {
                meshJson += ",\"instance\":\"true\"";

                instanceChildPosition = "\"instancePos\":[";
                for (int k = 0; k < allChildPositionList.Count; k++)
                {
                    instanceChildPosition += StrOptimize(allChildPositionList[k], 3);
                    if (k < allChildPositionList.Count - 1)
                        instanceChildPosition += ",";

                }
                instanceChildPosition += "]";
            }
            else
            {
                instanceChildPosition = "\"instancePos\":[]";
                meshJson += ",\"instance\":\"false\"";
            }

            if (useRealtimeLight)
            {
                meshJson += ",\"light\":\"on\"";
            }
            else
            {
                meshJson += ",\"light\":\"off\"";
            }




            renderOrderJson= "\"renderOrder\":\"" + renderOrder.ToString() + "\"";

            materialsJson = "\"materials\":[";
            int matLength = mats.Length;

            for (int i = 0; i < matLength; i++)
            {
                if (mats[i].shader.name == "@Moblie_WJM_Water")
                    hasWaterMat = true;
                if (mats[i].shader.name == "@Moblie_WJM_SpriteTree")
                    hasSpriteTreeMat = true;

                 materialsJson += "\"" + mats[i].name + "\"";
                 materialsJson += i == (matLength - 1) ? "]" : ",";

            }

            lightmapOffsetJson = "\"lightmapoffsetuv\":[" + lightmapIndex + "," + StrOptimize(lightmapScaleOffset.x) + "," + StrOptimize(lightmapScaleOffset.y) + "," + StrOptimize(lightmapScaleOffset.z) + "," + StrOptimize(lightmapScaleOffset.w) + "]";
        }

//      Debug.Log(objectId);

        return true;

    }


    public string StrOptimize(float inFloat, int keepPointCount = 4)
    {
        //       return inFloat.ToString();

        string outStr = (Mathf.Round(inFloat * Mathf.Pow(10, keepPointCount))).ToString();
        string[] strGroup = outStr.Split('.');

        if (strGroup.Length > 1 && strGroup[1].Length > keepPointCount)
        {
            strGroup[1] = strGroup[1].Remove(keepPointCount);
        }
        else
        {
            return outStr;
        }

        return strGroup[0] + "." + strGroup[1];

    }

}






