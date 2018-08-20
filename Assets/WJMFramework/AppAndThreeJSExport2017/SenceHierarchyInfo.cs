using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//[RequireComponent(typeof(SenceInteractiveInfo))]
public class SenceHierarchyInfo : MonoBehaviour
{

    public Cubemap envCubeMap;
    public Light sun;
    public Vector2 fogNearAndFar=new Vector2(500,2000);
    public Color fogColor = new Color(0.5f, 0.5f, 0.5f);
    public SenceInteractiveInfo senceInteractiveInfo;


    //场景中所有物体
    List<GameObject> sceneRootGameObject;
    public List<GameObject> sceneAllGameObject;

    List<Object3DInfo> needDeleteThreeJsObject3DInfo;

    public List<Object3DInfo> allTempObject3DInfo;

    public List<Mesh> allMesh;
    public List<Material> allMaterials;
    public List<Texture> allCustomTexture;
    public List<Texture> allLightmapTexture;
    public List<Cubemap> allCubeMapTexture;
    public List<Object3DMesh> allGameObject3DMesh;

    //这个是之前一次完整导出时的的Mesh信息,用来快速导出info时使用的
    public List<Object3DMesh> lastAllGameObject3DMesh;


    public Vector4[] shadersVector4Pos;


    public List<Object3DTexture> allGameObject3DTexture;
    public List<Object3DCubeTexture> allGameObject3DCubeTexture;

    public int allGameObject3DCount;
    public int meshCount;

    public int rawdataVertexCount;
    public int rawdataTriFaceCount;

    public int renderVertexCount;
    public int renderTriFaceCount;

    Texture2D mainTex;
    Texture2D lightMap;
    Cubemap cubeMap;
    Texture2D normalMap;


    /// <summary>
    /// 收集需要导出的物体
    /// </summary>
    void GetSceneAllGameObject()
    {
        sceneAllGameObject = new List<GameObject>();
        sceneRootGameObject = new List<GameObject>();
        allTempObject3DInfo = new List<Object3DInfo>();

        sceneRootGameObject.AddRange(UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects());

        GameObject[] tempRoots = sceneRootGameObject.ToArray();

        foreach (GameObject g in tempRoots)
        {
            if(CheckNeedRemoveObject(g))
            sceneRootGameObject.Remove(g);
        }
        Debug.Log("场景中有"+sceneRootGameObject.Count+"个root物体");

        for (int i = 0; i < sceneRootGameObject.Count; i++)
        {
            GetAllChildsGameObject(sceneRootGameObject[i]);
        }

        //移除intanceGameObject下的子物体和移除不导出的物体
        tempRoots = sceneAllGameObject.ToArray();


        foreach (GameObject g in tempRoots)
        {
            // Debug.Log(g.name);
            if (g.GetComponent<Object3DInfo>() != null)
            {
                if (g.GetComponent<Object3DInfo>().isInstance3DGameObject || !g.GetComponent<Object3DInfo>().isNeedExportChild)
                {

                    Debug.Log("移除 " + g.name + " 物体下的子物体");
                    Transform[] allChild = g.GetComponentsInChildren<Transform>(true);

                    foreach (Transform t in allChild)
                    {
                        if (t.gameObject != g)
                        {
                            sceneAllGameObject.Remove(t.gameObject);
                        }
                    }

                }
            }

            
            if (g.GetComponent<DontExport>() != null)
            {
                Debug.Log("移除 " + g.name + " 物体及子物体");
                Transform[] allChild = g.GetComponentsInChildren<Transform>(true);

                foreach (Transform t in allChild)
                {
                    sceneAllGameObject.Remove(t.gameObject);        
                }

            }
            


        }
    }




    void GetAllChildsGameObject(GameObject rootGameObject)
    {
        if (!sceneAllGameObject.Contains(rootGameObject.gameObject))
        {
            if(!CheckNeedRemoveObject(rootGameObject.gameObject))
            sceneAllGameObject.Add(rootGameObject.gameObject);
        }

        Transform[] childGameObjects = rootGameObject.GetComponentsInChildren<Transform>(true);

        for (int i = 0; i < childGameObjects.Length; i++)
        {
            if (!sceneAllGameObject.Contains(childGameObjects[i].gameObject))
            {
                if (!CheckNeedRemoveObject(childGameObjects[i].gameObject))
                sceneAllGameObject.Add(childGameObjects[i].gameObject);
            }
        }
    }

    bool CheckNeedRemoveObject(GameObject g)
    {
        if (g.name == "__UtilTgtTrans" || g.name == "__UtilDirTrans" || g.hideFlags!=HideFlags.None)
        {
            return true;
        }

        if (g.GetComponent<SenceHierarchyInfo>() != null)
        {
            return true;
        }

        if (g.GetComponent<SenceInteractiveInfo>() != null)
        {
            return true;
        }

//        if (g.GetComponent<DontExport>() != null)
//        {
//            return true;
//        }


        return false;
    }


    void AddThreeJsInfoToAllChildsGameObject()
    {
        needDeleteThreeJsObject3DInfo = new List<Object3DInfo>();

        for (int i = 0; i < sceneAllGameObject.Count; i++)
        {

            if (sceneAllGameObject[i].GetComponent<Object3DInfo>() != null)
            {
                needDeleteThreeJsObject3DInfo.Add(sceneAllGameObject[i].GetComponent<Object3DInfo>());
            }
            else
            {
                needDeleteThreeJsObject3DInfo.Add(sceneAllGameObject[i].AddComponent<Object3DInfo>());
            }

        }

    }



    public void DestroyTempData()
    {
        for (int i = 0; i < needDeleteThreeJsObject3DInfo.Count; i++)
        {
            if (needDeleteThreeJsObject3DInfo[i] != null)
            {
                DestroyImmediate(needDeleteThreeJsObject3DInfo[i]);
            }
        }
        needDeleteThreeJsObject3DInfo.Clear();
        allTempObject3DInfo.Clear();

    }

    void GenerateEachGameobjectInfo()
    {
        for (int i = 0; i < sceneAllGameObject.Count; i++)
        {
            if (sceneAllGameObject[i].GetComponent<Object3DInfo>() != null)
            {
                sceneAllGameObject[i].GetComponent<Object3DInfo>().RecordInfo(i);
                allTempObject3DInfo.Add(sceneAllGameObject[i].GetComponent<Object3DInfo>());
            }
        }

    }

    public void PrepareThreeJsSceneData()
    {
        GetSceneAllGameObject();
        AddThreeJsInfoToAllChildsGameObject();
        GenerateEachGameobjectInfo();
        SearchAllSource();

    }

    void SearchAllSource()
    {
        renderVertexCount = 0;
        renderTriFaceCount = 0;
        rawdataVertexCount = 0;
        rawdataTriFaceCount = 0;

        allMesh = new List<Mesh>();
        allMaterials = new List<Material>();

        allGameObject3DMesh = new List<Object3DMesh>();
        allCustomTexture = new List<Texture>();
        allLightmapTexture = new List<Texture>();
        allCubeMapTexture = new List<Cubemap>();
        allGameObject3DTexture = new List<Object3DTexture>();
        allGameObject3DCubeTexture = new List<Object3DCubeTexture>();


        if (envCubeMap == null)
        {
            string log = gameObject.name + "的envCubeMap贴图没设置！";
            GlobalDebug.Addline(log);
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
            return;
        }
  
        //默认环境贴图,第一张图为天空图
        allCubeMapTexture.Add(envCubeMap);
        allGameObject3DCubeTexture.Add(new Object3DCubeTexture((Cubemap)envCubeMap,  "@_CubeMap"));


        ///得到当前Unity3d场景烘培出来的Lightmap
        ///
        for (int i = 0; i < LightmapSettings.lightmaps.Length; i++)
        {
            if (LightmapSettings.lightmaps[i].lightmapColor != null)
            {
                allLightmapTexture.Add(LightmapSettings.lightmaps[i].lightmapColor);
            }
        }


        for (int i = 0; i < allTempObject3DInfo.Count; i++)
        {
            if (allTempObject3DInfo[i].mesh!=null)
            {
                renderVertexCount +=allTempObject3DInfo[i].vertexCount;
                renderTriFaceCount += allTempObject3DInfo[i].triFaceCount;
                Material[] tempMats;
                if (allTempObject3DInfo[i].isInstance3DGameObject)
                {
                    tempMats = allTempObject3DInfo[i].mats;
                }
                else
                {
                    tempMats= allTempObject3DInfo[i].GetComponent<MeshRenderer>().sharedMaterials;
                }
                    
                int matCount = tempMats.Length;
                for (int j = 0; j < matCount; j++)
                {
                    if (!allMaterials.Contains(tempMats[j]))
                    {
                        allMaterials.Add(tempMats[j]);
                    }
                }

                ///首次添加mesh,且记录mesh的拥有者
                if (!allMesh.Contains(allTempObject3DInfo[i].mesh))
                {
                    rawdataVertexCount += allTempObject3DInfo[i].vertexCount;
                    rawdataTriFaceCount += allTempObject3DInfo[i].triFaceCount;
                    allMesh.Add(allTempObject3DInfo[i].mesh);
                    allGameObject3DMesh.Add(new Object3DMesh(allTempObject3DInfo[i].mesh, allTempObject3DInfo[i].transform, allTempObject3DInfo[i].objectId.ToString(), allTempObject3DInfo[i].exportUV,allTempObject3DInfo[i].exportUV2, allTempObject3DInfo[i].useRealtimeLight, allTempObject3DInfo[i].useVertexColorLightmap, allTempObject3DInfo[i].hasSpriteTreeMat, allTempObject3DInfo[i].hasWaterMat));
                }
                else
                { 
                    for(int j=0;j<allGameObject3DMesh.Count;j++)
                    {
                        //再次添拥有者
                        if(allTempObject3DInfo[i].mesh==allGameObject3DMesh[j].mesh)
                        {
                            allGameObject3DMesh[j].InsertOne(allTempObject3DInfo[i].transform, allTempObject3DInfo[i].objectId.ToString(), allTempObject3DInfo[i].hasSpriteTreeMat, allTempObject3DInfo[i].hasWaterMat);
                        }
                    }        
                }

            }

        }


        


        allGameObject3DCount = allTempObject3DInfo.Count;
        meshCount = allGameObject3DMesh.Count;

        for (int j = 0; j < allMaterials.Count; j++)
        {

            //             mainTex=new Texture2D(16,16);
            //             lightMap = new Texture2D(16, 16);
            //             cubeMap = new Cubemap(0,TextureFormat.ARGB32,false);
            //             normalMap = new Texture2D(16, 16);

            mainTex = null;
            lightMap = null;
            cubeMap = null;
            normalMap = null;



            if (allMaterials[j].GetTexture("_MainTex") != null)
             {
                mainTex=(Texture2D)allMaterials[j].GetTexture("_MainTex");
             }

             if (allMaterials[j].HasProperty("_LightMap")&&allMaterials[j].GetTexture("_LightMap") != null)
             {
                 lightMap = (Texture2D)allMaterials[j].GetTexture("_LightMap");
             }

            if (allMaterials[j].HasProperty("_CubeMap") && allMaterials[j].GetTexture("_CubeMap") != null)
            {
                cubeMap = (Cubemap)allMaterials[j].GetTexture("_CubeMap");
            }

            if (allMaterials[j].HasProperty("_BumpMap") && allMaterials[j].GetTexture("_BumpMap") != null)
            {
                normalMap =(Texture2D) allMaterials[j].GetTexture("_BumpMap");
            }

            if (mainTex != null && !allCustomTexture.Contains(mainTex))
             {
                 allCustomTexture.Add(mainTex);
                 allGameObject3DTexture.Add(new Object3DTexture(mainTex, allMaterials[j].name + "@_MainTex"));
             }
             else if(mainTex != null )
             {
                 for (int k = 0; k < allGameObject3DTexture.Count; k++)
                 {
                     if (mainTex == allGameObject3DTexture[k].tex)
                     {
                         allGameObject3DTexture[k].InsertOne(allMaterials[j].name + "@_MainTex");
                     }
                 }
             }

             if (lightMap != null && !allCustomTexture.Contains(lightMap))
             {
                 allCustomTexture.Add(lightMap);
                 allGameObject3DTexture.Add(new Object3DTexture(lightMap, allMaterials[j].name + "@_LightMap"));
             }
             else if(lightMap!=null)
             {
                 for (int k = 0; k < allGameObject3DTexture.Count; k++)
                 {
                     if (lightMap == allGameObject3DTexture[k].tex)
                     {
                         allGameObject3DTexture[k].InsertOne(allMaterials[j].name + "@_LightMap");
                     }
                 }
             }

            if (cubeMap != null && !allCubeMapTexture.Contains(cubeMap))
            {
                allCubeMapTexture.Add(cubeMap);
                allGameObject3DCubeTexture.Add(new Object3DCubeTexture(cubeMap, allMaterials[j].name + "@_CubeMap"));
            }
            else if (cubeMap != null)
            {
                for (int k = 0; k < allGameObject3DCubeTexture.Count; k++)
                {
                    if (cubeMap == allGameObject3DCubeTexture[k].tex)
                    {
                        allGameObject3DCubeTexture[k].InsertOne(allMaterials[j].name + "@_CubeMap");
                    }
                }
            }


            if (normalMap != null && !allCustomTexture.Contains(normalMap))
            {
                allCustomTexture.Add(normalMap);
                allGameObject3DTexture.Add(new Object3DTexture(normalMap, allMaterials[j].name + "@_BumpMap"));
            }
            else if (normalMap != null)
            {
                for (int k = 0; k < allGameObject3DTexture.Count; k++)
                {
                    if (normalMap == allGameObject3DTexture[k].tex)
                    {
                        allGameObject3DTexture[k].InsertOne(allMaterials[j].name + "@_BumpMap");
                    }
                }
            }





        }
    }

    //记录单个Mesh的信息，及这个Mesh属于哪些gameobject
    //已知问题：Object3DInfo上的exportUV1,useRealtimeLight,useVertexColorLightmap的三个属于是对于Mesh属性修改,
    //但是Object3DInfo是挂载GameObject上，这时如果其他的Gameobject上要修改Mesh的三个属性就会造成冲突

    [System.Serializable]
    public class Object3DMesh
    {
        public Mesh mesh;

        /// public List<float> allChildPositionList;
        /// <summary>
        /// 属于哪些物体的string
        /// </summary>
        public List<string> beToObjectID;
        /// <summary>
        /// 属于哪些物体
        /// </summary>
        public List<Transform> beToGameObjetct;

        public string json;

        public bool exportUV;
        public bool exportUV2;
        public bool useRealtimeLight = false;
        public bool useVertexColorLightmap = false;

       
        public int[] indexBufferPos = new int[] { 0, 0 };
        public int[] vertexBufferPos = new int[] { 0, 0 };
        public int[] normalBufferPos = new int[] { 0, 0 };
        public int[] uvBufferPos = new int[] { 0, 0 };
        public int[] uv2BufferPos = new int[] { 0, 0 };
        public int[] randomBufferPos = new int[] { 0, 0 };
        public int[] instancePosGroupPos = new int[] { 0, 0 };
        public int[] worldPosAndScalePos = new int[] {0,0};
        public int[] tangentsPos = new int[] { 0, 0 };


        public bool hasSpriteTreeMat;
        public bool hasWaterMat;


        //Mesh首次添加的时候使用这个函数
        public Object3DMesh( Mesh inMesh, Transform inT, string beToID,bool iExportUV, bool iExportUV2,bool inRealTime,bool inVertexColorLM,bool inHasSpriteTreeMat,bool inHasWaterMat)
        {
            beToObjectID = new List<string>();
            beToGameObjetct = new List<Transform>();
            mesh = inMesh;
            exportUV = iExportUV;
            exportUV2 = iExportUV2;
            useRealtimeLight = inRealTime;
            useVertexColorLightmap = inVertexColorLM;
            hasSpriteTreeMat = inHasSpriteTreeMat;
            hasWaterMat = inHasWaterMat;
            InsertOne(inT, beToID, hasSpriteTreeMat, hasWaterMat);

        }

        public void InsertOne(Transform gameT, string beToID, bool inHasSpriteTreeMat, bool inHasWaterMat)
        {
           beToGameObjetct.Add(gameT);
           beToObjectID.Add(beToID);
           hasSpriteTreeMat = inHasSpriteTreeMat;
           hasWaterMat = inHasWaterMat||hasWaterMat;

           json =GetJson();
        }

        string GetJson()
        {
            string final = "\"name\":\""+mesh.name+ "\",\"beTo\":[";
            for (int i = 0; i < beToObjectID.Count; i++)
            {
                //string[] spiltStr = beTo[i].Split('-');
                
                final += "\"" + beToObjectID[i] + "\"";
 
 //               final += "\"" + "" + "\"";

                if (i != beToObjectID.Count - 1)
                    final += ",";
            }
            final += "]";

            json = final;
            return final;
        }

    }

    [System.Serializable]
    public class Object3DTexture
    {
        public Texture tex;
        public List<string> beToMat = new List<string>();
        public string json;
        public Object3DTexture(Texture inTex, string matName)
        {
            //           beToMat = new List<string>();
            tex = inTex;
            beToMat.Add(matName);
            json = GetJson();
        }

        public void InsertOne(string matName)
        {
            if (!beToMat.Contains(matName))
            {
                beToMat.Add(matName);
            }
            json = GetJson();
        }

        string GetJson()
        {

            string final = "\"beTo\":[";
            for (int i = 0; i < beToMat.Count; i++)
            {
                string[] spiltStr = beToMat[i].Split('@');
                if (spiltStr.Length > 2)
                    Debug.LogError(beToMat[i] + "材质名‘@’号太多");
                final += "{\"" + "name" + "\":";
                final += "\"" + spiltStr[0] + "\"";
                final += ",\"" + "slot" + "\":";
                final += "\"" + (spiltStr.Length > 1 ? spiltStr[1] : "") + "\"";

                if (i != beToMat.Count - 1)
                    final += "},";
            }
            final += "}]";

            return final;
        }

    }


    [System.Serializable]
    public class Object3DCubeTexture
    {
        public Cubemap tex;
        public List<string> beToMat = new List<string>();
        public string json;
        public Object3DCubeTexture(Cubemap inTex, string matName)
        {
            //           beToMat = new List<string>();
            tex = inTex;
            beToMat.Add(matName);
            json = GetJson();
        }

        public void InsertOne(string matName)
        {
            if (!beToMat.Contains(matName))
            {
                beToMat.Add(matName);
            }
            json = GetJson();
        }

        string GetJson()
        {

            string final = "\"beTo\":[";
            for (int i = 0; i < beToMat.Count; i++)
            {
                string[] spiltStr = beToMat[i].Split('@');
                if (spiltStr.Length > 2)
                    Debug.LogError(beToMat[i] + "材质名‘@’号太多");
                final += "{\"" + "name" + "\":";
                final += "\"" + spiltStr[0] + "\"";
                final += ",\"" + "slot" + "\":";
                final += "\"" + (spiltStr.Length > 1 ? spiltStr[1] : "") + "\"";

                if (i != beToMat.Count - 1)
                    final += "},";
            }
            final += "}]";

            return final;
        }

    }



}




