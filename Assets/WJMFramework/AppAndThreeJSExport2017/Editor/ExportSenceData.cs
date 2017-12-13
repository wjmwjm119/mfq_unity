//同一个mesh被多个物体使用时，只能是一种情况，或者是多维材质，或者是单一材质，不能用一个mesh又是多维材质又是单一材质
//attributes 在shader中使用都必须是float类型，除了index可以是int
//微信中的扩展名不能用bin，所以先用.json
//SpriteMesh Instance 物体，只能一个Mesh名对应一组

//有效范围
//pos用的是int32
//index用的uint16
//uv和uv2用的是int16


//36196  WEBGL_compressed_texture_etc1
//35986  WEBGL_compressed_texture_atc
//35987  Atc
//34798  Atc interlrp Alpha



using UnityEditor;
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Text;

//using Unity.IO.Compression;
//using Ionic.Zlib;
//using SevenZip;
//using ZLibNet;

public class ExportSenceData : EditorWindow
{
    public enum TextrueSize {T128=128,T256=256,T512=512,T1024=1024,T2048=2048};

    TextrueSize customTextrueMaxSize = TextrueSize.T512;
    TextrueSize lightmapTextrueMaxSize = TextrueSize.T512;

    bool onlyHierarchyAndInteractiveinfo = false;

//  public float senceUnit = 1;
    public string savePath	= "/exportdata";
	public string textureFolder="/texture";
	public string geometryFolder="/geometry";
	public string hierarchyFolder="/hierarchy";
	public string tempFolder="/tempTexture";

	string textureFolderFinal="";
//  string lightMapFolderFinal= "";
    string geometryFolderFinal="";
	string hierarchyFolderFinal="";
	string tempTextureFolderFinal="";

//  string exportMeshCount="0";
//  string exportTexCount="0";

    List<byte> allTexHeadInfo;
    List<byte> eachTexDataInfo;

    int binStart;
    int binCount;

    public SenceHierarchyInfo senceHierarchyInfo;
    SenceHierarchyInfo lastSenceHierarchyInfo;

    public SceneAsset sceneAsset;
    SceneAsset lastSceneAsset;

    bool useCompress = false;

    [MenuItem("WJMTooLs/ExportSenceData")]
	static void Init()
	{
        ExportSenceData window = (ExportSenceData)EditorWindow.GetWindow (typeof(ExportSenceData));
	    window.minSize = new Vector2 (300, 300);
		window.wantsMouseMove = true;
		window.Show();
    }

    /// <summary>
    /// 验证创建输出文件夹
    /// </summary>
    void CheckOutPutPathNameAndCreateFolder()
    {
            if (senceHierarchyInfo != null&&sceneAsset!=null)
            {
                if (senceHierarchyInfo.senceInteractiveInfo != null)
                {
                    if (senceHierarchyInfo.senceInteractiveInfo.sceneType != SenceInteractiveInfo.SceneType.大场景)
                    {
                        string tempName = senceHierarchyInfo.senceInteractiveInfo.huXingType.hxName;
                        if (tempName != "")
                        {
                            CreateOutPutFold(tempName);
                        }
                        else
                        {
                            string debugStr = "SenceInteractiveInfo 未设置户型名字!";
                            Debug.Log(debugStr);
                            Debug.LogWarning(debugStr);
                            Debug.LogError(debugStr);
                        }
                    }
                    else
                    {
                        CreateOutPutFold("outdoor");
                    }
                }
            }     
    }

    void OnGUI()
    {
        if(senceHierarchyInfo != null && sceneAsset != null)
        EditorGUI.LabelField(new Rect(20, 10, 1000, 20), new GUIContent("Web 输出:"+savePath));

        if (lastSenceHierarchyInfo != senceHierarchyInfo)
        {
            lastSenceHierarchyInfo = senceHierarchyInfo;
            CheckOutPutPathNameAndCreateFolder();
        }

        if (lastSceneAsset != sceneAsset)
        {
            lastSceneAsset = sceneAsset;
            CheckOutPutPathNameAndCreateFolder();
        }



        senceHierarchyInfo = (SenceHierarchyInfo)EditorGUI.ObjectField(new Rect(20, 40, 300, 16), senceHierarchyInfo, typeof(SenceHierarchyInfo),true);
        sceneAsset =(SceneAsset)EditorGUI.ObjectField(new Rect(20, 60, 300, 16), sceneAsset, typeof(SceneAsset),true);

/*
        savePath = EditorGUI.TextField(new Rect(20, 60, 280, 18), new GUIContent(""), savePath);
        if (GUI.Button(new Rect(320, 59, 150, 20), new GUIContent("Set Path (设置路径)")))
        {
            string temp = EditorUtility.OpenFolderPanel("Select ThreeJS folder", Application.dataPath, "threejsdata");
            if (temp != "")
            {
                savePath = temp;
                Debug.Log(savePath);
            }
        }
*/

        customTextrueMaxSize=(TextrueSize) EditorGUI.EnumPopup(new Rect(170, 110, 300, 25), "贴图输出最大值", customTextrueMaxSize);
        lightmapTextrueMaxSize = (TextrueSize)EditorGUI.EnumPopup(new Rect(170, 150, 300, 25), "unity烘培贴图输出最大值", lightmapTextrueMaxSize);

        onlyHierarchyAndInteractiveinfo = EditorGUI.ToggleLeft(new Rect(170, 190, 300, 25),new GUIContent("只导出Hierarchy+Interactive.info"), onlyHierarchyAndInteractiveinfo);

        if (GUI.Button(new Rect(320, 250, 150, 20), new GUIContent("Export")))
        {

            CheckOutPutPathNameAndCreateFolder();

            if (senceHierarchyInfo != null)
            {
                //BuildAsset时忽略这个物体，这个物体只用在导出web资源，运行时不需要
                senceHierarchyInfo.gameObject.hideFlags = HideFlags.DontSaveInBuild;

                useCompress = true;

                //收集场景中所有需要导出的资源
                senceHierarchyInfo.PrepareThreeJsSceneData();

                //创建输出的文件夹
//              if (!onlyHierarchyAndInteractiveinfo)
//              CreateOutPutFold();

                //创建bin流,包含shader流
                if (!onlyHierarchyAndInteractiveinfo)
                {
                    CreateBuuferGeometryBinFile(senceHierarchyInfo);
                }
                else
                {
                    senceHierarchyInfo.allGameObject3DMesh = senceHierarchyInfo.lastAllGameObject3DMesh;
                }
                    
                //创建场景信息
                    CreateSenceHierarchyJsonFile(senceHierarchyInfo);

                //创建交互信息
                    CreateSenceInteractiveJsonFile(senceHierarchyInfo);

                //复制出所有图片
                if (!onlyHierarchyAndInteractiveinfo)
                    CopyAllTexture(senceHierarchyInfo);

                //清除临时添加的ThreeJsObject3DInfo
                senceHierarchyInfo.DestroyTempData();
            }
            else
            {
                string log = "请选中SenceHierarchyInfo物体，如果场景中没有，请在根目录添加一个";
                GlobalDebug.Addline(log);
                Debug.Log(log);
                Debug.LogWarning(log);
                Debug.LogError(log);
            }
        }
    }

//  void OnEnable()
//  {
//      CreateOutPutFold();
//  }

    string GetParentPath(string inPath)
    {
        string[] allSubPath;
        string parentPath = "";
        allSubPath = Application.dataPath.Split('/');//拆分路径为几个字符串
        for (int i = 0; i < allSubPath.Length - 1; i++)//得到上级路径
        {
            if (i == 0)
            {
                parentPath += allSubPath[i];
            }
            else
            {
                parentPath += '/' + allSubPath[i];
            }
        }
        //        Debug.Log(parentPath);
        return parentPath;
    }

    void CreateOutPutFold(string sceneName)
	{

        //除去中文名
        sceneName = GetUTF16(sceneName);

        string projectFolderPath = GetParentPath(Application.dataPath);

        savePath = "/ExportABFolder/Web/";
       
        savePath = projectFolderPath + savePath+sceneName;

        if (!Directory.Exists(savePath))
		{
			Debug.Log("创建输出文件夹"+ savePath);
			Directory.CreateDirectory( savePath);			
		}

        if (!Directory.Exists( savePath + textureFolder))
        {
            Directory.CreateDirectory( savePath + textureFolder);
        }

        if (!Directory.Exists( savePath + hierarchyFolder))
        {
            Directory.CreateDirectory( savePath + hierarchyFolder);
        }

        if (!Directory.Exists( savePath + geometryFolder))
        {
            Directory.CreateDirectory( savePath + geometryFolder);
        }

        if (!Directory.Exists(Application.dataPath + "/intermediate"))
        {
            Directory.CreateDirectory(Application.dataPath + "/intermediate");
        }

        textureFolderFinal =  savePath + textureFolder;
        hierarchyFolderFinal =  savePath + hierarchyFolder;
	    geometryFolderFinal =  savePath + geometryFolder;

        tempFolder = "/intermediate/tempTextrue_" + UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        tempTextureFolderFinal ="Assets"+  tempFolder;
        if (!Directory.Exists(tempTextureFolderFinal))
        Directory.CreateDirectory(tempTextureFolderFinal);
    }

    void CopyAllTexture(SenceHierarchyInfo sceneH)
    {
        BackUpAndScaleAndFormatAndSizeAndCopyTexture(sceneH.allCustomTexture,(int)customTextrueMaxSize);
        BackUpAndScaleAndFormatAndSizeAndCopyTexture(sceneH.allLightmapTexture,(int)lightmapTextrueMaxSize,true);
        BackUpAndScaleAndFormatAndSizeAndCopyCubeTexture(sceneH.allCubeMapTexture,256);
    }

    void BackUpAndScaleAndFormatAndSizeAndCopyCubeTexture(List<Cubemap> cubeMapGroup,int maxSize)
    {
        for (int i = 0; i < cubeMapGroup.Count; i++)
        {
            string fileName = CopyTextureToFolder(cubeMapGroup[i].GetInstanceID(), tempTextureFolderFinal, true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
            
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(fileName);
            textureImporter.textureShape = TextureImporterShape.TextureCube;
            textureImporter.isReadable = true;
            textureImporter.maxTextureSize = maxSize;
            textureImporter.mipmapEnabled = false;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            
            AssetDatabase.ImportAsset(fileName);
            Cubemap loadTex = (Cubemap)AssetDatabase.LoadAssetAtPath(fileName, typeof(Cubemap));

            Texture2D exportSaveTex;
            exportSaveTex = new Texture2D(loadTex.width, loadTex.height,TextureFormat.RGB24, false, false);
            
            for (int j = 0; j < 6; j++)
            {
                for (int k = 0; k < exportSaveTex.width; k++)
                {
                    for (int z = 0; z < exportSaveTex.height; z++)
                    {
                        exportSaveTex.SetPixel(k, exportSaveTex.height-z-1, loadTex.GetPixel((CubemapFace)j, k, z));
                    }
                }
//              exportSaveTex.SetPixels(loadTex.GetPixels((CubemapFace)j));
                exportSaveTex.Apply();
                var bytes = exportSaveTex.EncodeToJPG(95);
                File.WriteAllBytes(textureFolderFinal + "/" + loadTex.name+"_"+j + ".jpg", bytes);
            }
        }
    }


    void BackUpAndScaleAndFormatAndSizeAndCopyTexture(List<Texture> inTexGroup,int maxSize,bool isLightmap=false)
    {
        for (int i = 0; i < inTexGroup.Count; i++)
        {
            string fileName = CopyTextureToFolder(inTexGroup[i].GetInstanceID(), tempTextureFolderFinal, true);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
           
            TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(fileName);
            textureImporter.isReadable = true;
            textureImporter.maxTextureSize = maxSize;
            textureImporter.mipmapEnabled = false;
            textureImporter.wrapMode = TextureWrapMode.Clamp;
            textureImporter.filterMode = FilterMode.Point;
            textureImporter.textureCompression = TextureImporterCompression.Uncompressed;
            if (isLightmap)
            textureImporter.textureType = TextureImporterType.Lightmap;

            AssetDatabase.ImportAsset(fileName);
            Texture2D loadTex = (Texture2D)AssetDatabase.LoadAssetAtPath(fileName, typeof(Texture2D));
            
            Texture2D exportSaveTex;
//          Texture2D exportSaveTexAlpha;

            exportSaveTex = new Texture2D(loadTex.width, loadTex.height, loadTex.format, false,false);
//          exportSaveTexAlpha = new Texture2D(loadTex.width, loadTex.height, loadTex.format, false, false);

            exportSaveTex.SetPixels(loadTex.GetPixels());
            exportSaveTex.Apply();

            if (exportSaveTex.format == TextureFormat.RGBA32)
            {
                //导出rgb
                var bytes = exportSaveTex.EncodeToPNG();
                File.WriteAllBytes(textureFolderFinal + "/" + loadTex.name + ".png", bytes);
            }
            else
            {
                var bytes = exportSaveTex.EncodeToJPG(95);
                File.WriteAllBytes(textureFolderFinal + "/" + loadTex.name + ".jpg", bytes);
            }
        }

    }

    /// <summary>
    /// 返回储存后的路径
    /// </summary>
    /// <param name="textureID"></param>
    /// <param name="folder"></param>
    /// <param name="exportFile"></param>
    /// <returns></returns>
    string  CopyTextureToFolder(int textureID, string folder,bool exportFile)
    {
      string path=AssetDatabase.GetAssetPath(textureID);

      if (exportFile)
      {
         File.Copy(path, folder + "/" + path.Split('/')[path.Split('/').Length - 1], true);
         Debug.Log("Copy " + path + " To " + folder + "/" + path.Split('/')[path.Split('/').Length - 1]);

      }
        return folder + "/" + path.Split('/')[path.Split('/').Length - 1];
    }


    void CreateSenceHierarchyJsonFile(SenceHierarchyInfo senceH)
    {
        string senceHierarchy = "{\"v\":\"2017_0_0\",\"compress\":\"LZF\",";

        senceHierarchy += "\"SceneBuildTime\":\"" + DateTime.Now.ToString("d") + "\",";

        senceHierarchy += "\"envCubeMap\":\"" + senceH.envCubeMap.name + "\",";

        senceHierarchy += "\"SenceHierarchy\":[";

        for (int i = 0; i < senceH.allTempObject3DInfo.Count; i++)
        {
            senceHierarchy += "{" + senceH.allTempObject3DInfo[i].nameJson + ",";
            senceHierarchy += senceH.allTempObject3DInfo[i].idJson + ",";
            senceHierarchy += senceH.allTempObject3DInfo[i].layerJson + ",";
            senceHierarchy += senceH.allTempObject3DInfo[i].positionJson + ",";
            senceHierarchy += senceH.allTempObject3DInfo[i].rotationJson + ",";
            senceHierarchy += senceH.allTempObject3DInfo[i].scaleJson + ",";


            if (senceH.allTempObject3DInfo[i].mesh != null)
            {
                senceHierarchy += senceH.allTempObject3DInfo[i].lightmapOffsetJson + ",";
                senceHierarchy += senceH.allTempObject3DInfo[i].meshJson + ",";
                senceHierarchy += senceH.allTempObject3DInfo[i].renderOrderJson + ",";
                senceHierarchy += senceH.allTempObject3DInfo[i].instanceChildPosition + ",";
                senceHierarchy += senceH.allTempObject3DInfo[i].materialsJson + ",";
                senceHierarchy += senceH.allTempObject3DInfo[i].parentJson + "}";
            }
            else
            {
                senceHierarchy += senceH.allTempObject3DInfo[i].parentJson + "}";
            }

            if (i != (senceH.allTempObject3DInfo.Count - 1))
            {
                senceHierarchy += ",";
            }

        }

        senceHierarchy += "]";

        senceHierarchy += ",\"allMeshbeTo\":[";

        for (int i = 0; i < senceH.allGameObject3DMesh.Count; i++)
        {
            senceHierarchy += "{" + senceH.allGameObject3DMesh[i].json;
            senceHierarchy += ",\"type\": \"BufferGeometry\",\"data\": {";

            if (senceH.allGameObject3DMesh[i].mesh.subMeshCount > 0)
            {
                int meshCount = senceH.allGameObject3DMesh[i].mesh.subMeshCount;

                senceHierarchy += "\"groups\": [";
                int lastCount = 0;

                string indexGroupJson = "";
                for (int j = 0; j < meshCount; j++)
                {

                    indexGroupJson += "{\"start\":" + lastCount;
                    indexGroupJson += ",\"count\":" + senceH.allGameObject3DMesh[i].mesh.GetTriangles(j).Length;
                    lastCount += senceH.allGameObject3DMesh[i].mesh.GetTriangles(j).Length;

                    indexGroupJson += ",\"materialIndex\":" + j;

                    if (j == meshCount - 1)
                    {
                        indexGroupJson += "}]";
                    }
                    else
                    {
                        indexGroupJson += "},";
                    }
                }
                senceHierarchy += indexGroupJson;
            }

            senceHierarchy += ",\"index\": {\"itemSize\": 1,\"type\": \"Uint16Array\",";
            senceHierarchy += "\"array\":[" + senceH.allGameObject3DMesh[i].indexBufferPos[0] + "," + senceH.allGameObject3DMesh[i].indexBufferPos[1] + "]}";

            senceHierarchy += ",\"attributes\": {";

            senceHierarchy += "\"position\": {\"itemSize\": 3,\"type\": \"Float32Array\",";
            senceHierarchy += "\"array\":[" + senceH.allGameObject3DMesh[i].vertexBufferPos[0] + "," + senceH.allGameObject3DMesh[i].vertexBufferPos[1] + "]}";

            /*
                        if (senceH.allGameObject3DMesh[i].allChild!=null)
                        {
                            senceHierarchy += ",\"instancePosGroup\": {\"itemSize\": 3,\"type\": \"Float32Array\",";
                            senceHierarchy += "\"array\":[" + senceH.allGameObject3DMesh[i].instancePosGroupPos[0] + "," + senceH.allGameObject3DMesh[i].instancePosGroupPos[1] + "]}";
                        }
            */

            if (senceH.allGameObject3DMesh[i].useRealtimeLight)
            {
                senceHierarchy += ",\"normal\": {\"itemSize\": 3,\"type\": \"Float32Array\",";
                senceHierarchy += "\"array\": [" + senceH.allGameObject3DMesh[i].normalBufferPos[0] + "," + senceH.allGameObject3DMesh[i].normalBufferPos[1] + "]}";
            }

            if (senceH.allGameObject3DMesh[i].exportUV)
            {
                senceHierarchy += ",\"uv\": {\"itemSize\": 2,\"type\": \"Float32Array\",";
                senceHierarchy += "\"array\": [" + senceH.allGameObject3DMesh[i].uvBufferPos[0] + "," + senceH.allGameObject3DMesh[i].uvBufferPos[1] + "]}";
            }

            if (senceH.allGameObject3DMesh[i].exportUV2)
            {
                senceHierarchy += ",\"uv2\": {\"itemSize\": 2,\"type\": \"Float32Array\",";
                senceHierarchy += "\"array\": [" + senceH.allGameObject3DMesh[i].uv2BufferPos[0] + "," + senceH.allGameObject3DMesh[i].uv2BufferPos[1] + "]}";
            }

            if (senceH.allGameObject3DMesh[i].worldPosAndScalePos[1] > 0)
            {
                senceHierarchy += ",\"worldPosAndScale\": {\"itemSize\": 4,\"type\": \"Float32Array\",";
                senceHierarchy += "\"array\": [" + senceH.allGameObject3DMesh[i].worldPosAndScalePos[0] + "," + senceH.allGameObject3DMesh[i].worldPosAndScalePos[1] + "]}";
            }

            if (senceH.allGameObject3DMesh[i].hasWaterMat)
            {
                senceHierarchy += ",\"tangent\": {\"itemSize\": 3,\"type\": \"Float32Array\",";
                senceHierarchy += "\"array\": [" + senceH.allGameObject3DMesh[i].tangentsPos[0] + "," + senceH.allGameObject3DMesh[i].tangentsPos[1] + "]}";
            }

            senceHierarchy += "}}";

            if (i != senceH.allGameObject3DMesh.Count - 1)
            {
                senceHierarchy += "},";
            }
            else
            {
                senceHierarchy += "}";
            }
        }

        senceHierarchy += "]";

        if (senceH.sun != null)
        {
            senceHierarchy += ",\"sun\":[" + senceH.sun.transform.forward.x+ "," + senceH.sun.transform.forward.y + "," + (-senceH.sun.transform.forward.z) + "]";
            senceHierarchy += ",\"sunIntensity\":[" + senceH.sun.color.r * senceH.sun.intensity + "," + senceH.sun.color.g * senceH.sun.intensity + "," + senceH.sun.color.b * senceH.sun.intensity + "]";
        }
        else
        {
            Debug.LogError("SenceHierarchyInfo物体未设置sun!");
            return;
        }
		
        senceHierarchy += ",\"fogNearAndFar\":[" + senceH.fogNearAndFar.x + "," + senceH.fogNearAndFar.y + "]";
        senceHierarchy += ",\"fogColor\":[" + senceH.fogColor.r + "," + senceH.fogColor.g + "," + senceH.fogColor.b + "]";

        senceHierarchy += ",\"allCustomTexture\":[";

        for (int i = 0; i < senceH.allCustomTexture.Count; i++)
        {
            string expandName = AssetDatabase.GetAssetPath(senceH.allCustomTexture[i]).Split('.')[1];
            //  替换扩展名
            expandName = "jpg";
            Texture2D te = (Texture2D)senceH.allCustomTexture[i];
            if (te.format == TextureFormat.RGBA32 || te.format == TextureFormat.DXT5 || te.format == TextureFormat.BC6H || te.format == TextureFormat.ETC2_RGBA8||te.format==TextureFormat.PVRTC_RGBA2|| te.format == TextureFormat.PVRTC_RGBA4)
                expandName = "png";

//          Debug.Log(te.format);
            senceHierarchy += "{\"name\":\"" + senceH.allCustomTexture[i].name + "\",\"type\":\"" + expandName + "\"";

            //    +senceH.diffuseMapBelongGameObjects[i].json;
            for (int j = 0; j < senceH.allGameObject3DTexture.Count; j++)
            {
                if (senceH.allGameObject3DTexture[j].tex == senceH.allCustomTexture[i])
                {
                    senceHierarchy += "," + senceH.allGameObject3DTexture[j].json;
                }
            }

            if (i != senceH.allCustomTexture.Count - 1)
            {
                senceHierarchy += "},";
            }
            else
            {
                senceHierarchy += "}";
            }
        }
        senceHierarchy += "]";

        //不需要知道allLightmap被谁用了，因为每个物体都有个Lightmap Index
        senceHierarchy += ",\"allLightmapTexture\":[";

        for (int i = 0; i < senceH.allLightmapTexture.Count; i++)
        {
            string expandName = AssetDatabase.GetAssetPath(senceH.allLightmapTexture[i]).Split('.')[1];

            expandName = "jpg";
            Texture2D te = (Texture2D)senceH.allLightmapTexture[i];
            if (te.format == TextureFormat.RGBA32 || te.format == TextureFormat.DXT5 || te.format == TextureFormat.BC6H || te.format == TextureFormat.ETC2_RGBA8)
                expandName = "png";

            senceHierarchy += "{\"name\":\"" + senceH.allLightmapTexture[i].name + "\",\"type\":\"" + expandName + "\"";

            if (i != senceH.allLightmapTexture.Count - 1)
            {
                senceHierarchy += "},";
            }
            else
            {
                senceHierarchy += "}";
            }
        }
        senceHierarchy += "]";

        //cubemapGroup
        senceHierarchy += ",\"allCubeMapTexture\":[";

        for (int i = 0; i < senceH.allCubeMapTexture.Count; i++)
        {
            string expandName = AssetDatabase.GetAssetPath(senceH.allCubeMapTexture[i]).Split('.')[1];

            //  替换扩展名
            expandName = "jpg";
            Cubemap te = (Cubemap)senceH.allCubeMapTexture[i];
            if (te.format == TextureFormat.RGBA32 || te.format == TextureFormat.DXT5 || te.format == TextureFormat.BC6H || te.format == TextureFormat.ETC2_RGBA8)
                expandName = "png";

            senceHierarchy += "{\"name\":\"" + senceH.allCubeMapTexture[i].name + "\",\"type\":\"" + expandName + "\"";

            //    +senceH.diffuseMapBelongGameObjects[i].json;
            for (int j = 0; j < senceH.allGameObject3DCubeTexture.Count; j++)
            {
                if (senceH.allGameObject3DCubeTexture[j].tex == senceH.allCubeMapTexture[i])
                {
                    senceHierarchy += "," + senceH.allGameObject3DCubeTexture[j].json;
                }
                else
                {
                    //senceHierarchy += ",\"beTo\":[]";
                }
            }

            if (i != senceH.allCubeMapTexture.Count - 1)
            {
                senceHierarchy += "},";
            }
            else
            {
                senceHierarchy += "}";
            }
        }
        senceHierarchy += "]";

        senceHierarchy += ",\"materials\":[";

        Material[] mats = senceH.allMaterials.ToArray();

        for (int i = 0; i < mats.Length; i++)
        {
            senceHierarchy += "{\"name\":\"" + mats[i].name + "\",";
            senceHierarchy += "\"shader\":" + ShaderJsonPos(senceH,mats[i].shader.name) + ",";
            //          senceHierarchy += "\"_SpecColor2\":\"" + ColorToString(mats[i].GetColor("_SpecColor2")) + "\",";
            //          senceHierarchy += "\"_Shininess\":\"" + mats[i].GetFloat("_Shininess").ToString() + "\",";
            senceHierarchy += "\"_Color\":\"" + ColorToString(mats[i].GetColor("_Color")) + "\",";

            if (mats[i].HasProperty("_MainTex") && mats[i].GetTexture("_MainTex") != null)
            {
                string textureName = mats[i].GetTexture("_MainTex").name;
                if (mats[i].shader.name == "@WJM_Glass_Middle")
                {

                }
                senceHierarchy += "\"_MainTex\":\"" + mats[i].GetTexture("_MainTex").name + "\",";
            }
            else
            {
                senceHierarchy += "\"_MainTex\":\"null\",";
            }

            if (mats[i].HasProperty("_Smooth"))
            {
                senceHierarchy += "\"_Smooth\":\"" + mats[i].GetFloat("_Smooth") + "\",";
            }

            if (mats[i].HasProperty("_Reflect"))
            {
                senceHierarchy += "\"_Reflect\":\"" + mats[i].GetFloat("_Reflect") + "\",";
            }

            if (mats[i].HasProperty("_FresnelPower"))
            {
                senceHierarchy += "\"_FresnelPower\":\"" + mats[i].GetFloat("_FresnelPower") + "\",";
            }

            if (mats[i].HasProperty("_FresnelBias"))
            {
                senceHierarchy += "\"_FresnelBias\":\"" + mats[i].GetFloat("_FresnelBias") + "\",";
            }

            if (mats[i].HasProperty("_alphaSin"))
            {
                senceHierarchy += "\"_alphaSin\":\"" + mats[i].GetFloat("_alphaSin") + "\",";
            }

            if (mats[i].HasProperty("_Width"))
            {
                senceHierarchy += "\"_Width\":\"" + mats[i].GetFloat("_Width") + "\",";
            }

            if (mats[i].HasProperty("_Height"))
            {
                senceHierarchy += "\"_Height\":\"" + mats[i].GetFloat("_Height") + "\",";
            }

            if (mats[i].HasProperty("_sizeBlend"))
            {
                senceHierarchy += "\"_sizeBlend\":\"" + mats[i].GetFloat("_sizeBlend") + "\",";
            }

            if (mats[i].HasProperty("_scale"))
            {
                senceHierarchy += "\"_scale\":\"" + mats[i].GetFloat("_scale") + "\",";
            }

            if (mats[i].HasProperty("_PviotOffsetX"))
            {
                senceHierarchy += "\"_PviotOffsetX\":\"" + mats[i].GetFloat("_PviotOffsetX") + "\",";
            }

            if (mats[i].HasProperty("_PviotOffsetY"))
            {
                senceHierarchy += "\"_PviotOffsetY\":\"" + mats[i].GetFloat("_PviotOffsetY") + "\",";
            }

            if (mats[i].HasProperty("_WaveSpeed"))
            {
                Vector4 v = mats[i].GetVector("_WaveSpeed");
                senceHierarchy += "\"_WaveSpeed\":\"" + ColorToString(new Color(v.x, v.y, v.z, v.w)) + "\",";
            }

            if (mats[i].HasProperty("_Cutoff"))
            {
                senceHierarchy += "\"_Cutoff\":\"" + mats[i].GetFloat("_Cutoff") + "\",";
            }

            if (mats[i].HasProperty("_lightmap_color"))
            {
                senceHierarchy += "\"_lightmap_color\":\"" + ColorToString(mats[i].GetColor("_lightmap_color")) + "\",";
            } else if (mats[i].HasProperty("_LightmapColor"))
            {
                senceHierarchy += "\"_lightmap_color\":\"" + ColorToString(mats[i].GetColor("_LightmapColor")) + "\",";
            }
            else
            {
                senceHierarchy += "\"_lightmap_color\":\"" + ColorToString(new Color(0.0f, 0.0f, 0.0f)) + "\",";
            }

            if (mats[i].HasProperty("_LightMap") && mats[i].GetTexture("_LightMap") != null)
            {
                senceHierarchy += "\"_LightMap\":\"" + mats[i].GetTexture("_LightMap").name + "\"";
            }
            else
            {
                senceHierarchy += "\"_LightMap\":\"null\"";
            }

            senceHierarchy += i == (mats.Length - 1) ? "}" : "},";
        }

        senceHierarchy += "]}";

        Debug.Log(hierarchyFolderFinal + "/senceHierarchy.json");

        if (useCompress)
        {
            System.IO.File.WriteAllBytes(hierarchyFolderFinal + "/senceHierarchy.json", CLZF2.Compress(System.Text.Encoding.Default.GetBytes(senceHierarchy)));
//          byte[] tempByte=System.Text.Encoding.Default.GetBytes(senceHierarchy);
//          System.IO.File.WriteAllBytes(hierarchyFolderFinal + "/senceHierarchy.json", LZ4.LZ4Codec.EncodeHC(tempByte,0,tempByte.Length));
        }
        else
        {
            System.IO.File.WriteAllText(hierarchyFolderFinal + "/senceHierarchy.json", senceHierarchy);
        }
    }

    void CreateSenceInteractiveJsonFile(SenceHierarchyInfo senceH)
    {
        if (senceH.senceInteractiveInfo != null)
        {
            senceH.senceInteractiveInfo.RecordInfo();
            string senceInteractiveJson = senceH.senceInteractiveInfo.senceInteractiveInfoJson;

            if (useCompress)
            {
                System.IO.File.WriteAllBytes(hierarchyFolderFinal + "/senceInteractive.json", CLZF2.Compress(System.Text.Encoding.Default.GetBytes(senceInteractiveJson)));
            }
            else
            {
                System.IO.File.WriteAllText(hierarchyFolderFinal + "/senceInteractive.json", senceInteractiveJson);
            }
        }
        else
        {
            string log =senceH.gameObject.name+ "中的senceInteractiveInfo未设置，请检查是否已设置！";
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
        }

    }

    void CreateBuuferGeometryBinFile(SenceHierarchyInfo threeJsSenceHierarchy)
    {
        List<byte> binFileBuffer = new List<byte>();
//      GenOneGameObjectBinBuffer(threeJsSenceHierarchy.allGameObject3DMesh[1], binFileBuffer.Count);

        for (int i = 0; i < threeJsSenceHierarchy.allGameObject3DMesh.Count; i++)
        {
//          WaitForSeconds(0.2f);
            binFileBuffer.AddRange
            (
                  GenOneGameObjectBinBuffer(threeJsSenceHierarchy.allGameObject3DMesh[i], binFileBuffer.Count)
            );
        }

        Debug.Log("BinFileBuffer Length: "+binFileBuffer.Count);
        string savePath = geometryFolderFinal + "/" + "123.json";

        threeJsSenceHierarchy.lastAllGameObject3DMesh = threeJsSenceHierarchy.allGameObject3DMesh;

        if (useCompress)
        {
            System.IO.File.WriteAllBytes(savePath, CLZF2.Compress(binFileBuffer.ToArray()));
//          byte[] tempByte = binFileBuffer.ToArray();
//          System.IO.File.WriteAllBytes(savePath, LZ4.LZ4Codec.EncodeHC(tempByte, 0, tempByte.Length));
        }
        else
        {
            System.IO.File.WriteAllBytes(savePath, binFileBuffer.ToArray());
        }

        string[] shadersGroup = new string[] { "@Moblie_WJM_Alpha", "@Moblie_WJM_Base", "@Moblie_WJM_CubeMap", "@Moblie_WJM_Glass_Middle", "@Moblie_WJM_Mirror", "@Moblie_WJM_Sky", "@Moblie_WJM_SpriteTree", "@Moblie_WJM_TreeLeaf", "@Moblie_WJM_Water", "@Moblie_WJM_WFaceCamera", "@Moblie_WJM_SpriteTreeLeaf" };

        List<Vector4> shadersVec4Pos = new List<Vector4>();

         int allCount = 0;
         int start = 0;
         int count = 0;
         int start2 = 0;
         int count2 = 0;

        string shaderTxtRootPath = Application.dataPath + "/WJMFramework/AppAndThreeJSExport2017/Editor/";
        List<byte> shaderBytes=new List<byte>();

        for (int i = 0; i < shadersGroup.Length; i++)
        {
            byte[] vertexShaderStr = System.IO.File.ReadAllBytes(shaderTxtRootPath + "vertex" + shadersGroup[i] + ".txt");
            shaderBytes.AddRange(vertexShaderStr);
            start = allCount;
            count = vertexShaderStr.Length;
            allCount += count;

            byte[] fragmentShaderStr = System.IO.File.ReadAllBytes(shaderTxtRootPath + "fragment" + shadersGroup[i] + ".txt");
            shaderBytes.AddRange(fragmentShaderStr);
            start2 = allCount;
            count2 = fragmentShaderStr.Length;
            allCount += count2;
            shadersVec4Pos.Add(new Vector4(start,count,start2,count2));
//            Debug.Log(new Vector4(start, count, start2, count2));
        }
        shaderBytes.Add(new byte());
        shaderBytes.Add(new byte());

        threeJsSenceHierarchy.shadersVector4Pos = shadersVec4Pos.ToArray();
//        Debug.Log(shaderBytes.Count);

        string savePath2 = geometryFolderFinal + "/" + "456.json";

        if (useCompress)
        {
           System.IO.File.WriteAllBytes(savePath2, CLZF2.Compress(shaderBytes.ToArray()));
        }
        else
        {
           System.IO.File.WriteAllBytes(savePath2, shaderBytes.ToArray());
        }

    }

    byte[] GenOneGameObjectBinBuffer(SenceHierarchyInfo.Object3DMesh obj3d, int LastBinCount)
    {

        string meshName;

        List<byte> finalBinary=new List<byte>();
        List<float> verticesList = new List<float>();
        List<float> normalList = new List<float>();
        List<float> uvList = new List<float>();
        List<float> uv2List = new List<float>();
        List<float> worldPosAndScaleList=new List<float>();
        List<float> tangentsList = new List<float>();
        
        Vector3[] vertices;
        Vector3[] normals;
        Vector2[] uv;
        Vector2[] uv2;

        int[] index;
        UInt16[] reorderIndex;

        meshName = obj3d.mesh.name;
        vertices = obj3d.mesh.vertices;
        normals = obj3d.mesh.normals;

        uv = obj3d.mesh.uv;
        uv2 = obj3d.mesh.uv2;

        index = obj3d.mesh.triangles;

        //index的类型是Uint16 取值最大范围为65535，所以vertices的数组长度最长为65535

        if (vertices.Length > 65535)
        {
            string log = meshName + "的模型的顶点数超过65535,请减面！";
            Debug.Log(log);
            Debug.LogWarning(log);
            Debug.LogError(log);
        }

        //单物体最大面数Uint16决定了只能为21845面
        reorderIndex = new UInt16[index.Length];

        //反转三角形渲染顺序
        for (int i = 0; i < index.Length / 3; i++)
        {
            reorderIndex[i * 3] = (UInt16)index[i * 3 + 2];
            reorderIndex[i * 3 + 1] = (UInt16)index[i * 3 + 1];
            reorderIndex[i * 3 + 2] = (UInt16)index[i * 3];
        }

        ////////////

        byte[] binaryHold;

        finalBinary.AddRange(BuQuanByte(finalBinary.Count + LastBinCount, 2));

        binaryHold = GetUInt16ArrayBuffer(reorderIndex);
        obj3d.indexBufferPos = new int[] { finalBinary.Count + LastBinCount, binaryHold.Length / 2 };
        finalBinary.AddRange(binaryHold);
        

        for (int i = 0; i < vertices.Length; i++)
        {
            verticesList.Add(vertices[i].x);
            verticesList.Add(vertices[i].y);
            verticesList.Add(-vertices[i].z);
        }


        finalBinary.AddRange(BuQuanByte(finalBinary.Count + LastBinCount, 4));
        binaryHold = GetInt32ArrayBuffer(FloatArrayToInt32Array(verticesList.ToArray()));
        obj3d.vertexBufferPos = new int[] { finalBinary.Count + LastBinCount, binaryHold.Length / 4 };
        finalBinary.AddRange(binaryHold);

        if (obj3d.useRealtimeLight)
        {
            for (int i = 0; i < normals.Length; i++)
            {
                normals[i].Normalize();
                normalList.Add(normals[i].x);
                normalList.Add(normals[i].y);
                normalList.Add(-normals[i].z);
            }

//          finalBinary.AddRange(BuQuanByte(finalBinary.Count + LastBinCount, 2));
            binaryHold = FloatArrayToInt8Arrar(normalList.ToArray());
            obj3d.normalBufferPos = new int[] { finalBinary.Count + LastBinCount, binaryHold.Length / 1 };
            if (obj3d.normalBufferPos[1] == 0)
            {
                Debug.LogError(obj3d.mesh.name + " DontHave Normal");
            }
            finalBinary.AddRange(binaryHold);

        }

        if (obj3d.exportUV)
        {
            for (int i = 0; i < uv.Length; i++)
            {
                uvList.Add(uv[i].x);
                uvList.Add(uv[i].y);
            }

            finalBinary.AddRange(BuQuanByte(finalBinary.Count + LastBinCount, 2));
            binaryHold = GetInt16ArrayBuffer(FloatArrayToInt16Array(uvList.ToArray()));
            obj3d.uvBufferPos = new int[] { finalBinary.Count + LastBinCount, binaryHold.Length / 2 };
            if (obj3d.uvBufferPos[1] == 0)
            {
                Debug.LogError(obj3d.mesh.name + "DontHave UV");
            }
            finalBinary.AddRange(binaryHold);

        }

        if (obj3d.exportUV2)
        {
            //unity默认的Plane没有带uv2。因为经常使用的Plane，所有将uv1复制到uv2
            if (uv2.Length < 1 && obj3d.mesh.name == "Plane")
            {
                uv2List = uvList;
            }
            else
            {
                for (int i = 0; i < uv2.Length; i++)
                {
                    uv2List.Add(uv2[i].x);
                    uv2List.Add(uv2[i].y);
                }
            }

            finalBinary.AddRange(BuQuanByte(finalBinary.Count + LastBinCount, 2));
            binaryHold = GetInt16ArrayBuffer(FloatArrayToInt16Array(uv2List.ToArray()));
            obj3d.uv2BufferPos = new int[] { finalBinary.Count + LastBinCount, binaryHold.Length / 2 };
            if (obj3d.uv2BufferPos[1] == 0)
            {
                Debug.LogError(obj3d.mesh.name + " DontHave UV2");
            }
            finalBinary.AddRange(binaryHold);
        }


        if (obj3d.mesh.colors.Length > 0)
        {

            
            for (int i = 0; i < obj3d.mesh.colors.Length; i++)
            {
                worldPosAndScaleList.Add(obj3d.mesh.colors[i].r);
                worldPosAndScaleList.Add(obj3d.mesh.colors[i].g);
                worldPosAndScaleList.Add(-obj3d.mesh.colors[i].b);
                worldPosAndScaleList.Add(obj3d.mesh.colors[i].a);
            }

            finalBinary.AddRange(BuQuanByte(finalBinary.Count + LastBinCount, 4));
            binaryHold = GetInt32ArrayBuffer(FloatArrayToInt32Array(worldPosAndScaleList.ToArray()));
            obj3d.worldPosAndScalePos = new int[] { finalBinary.Count + LastBinCount, binaryHold.Length / 4 };
            finalBinary.AddRange(binaryHold);

        }

        if (obj3d.hasWaterMat)
        {
 //           Debug.Log("-----------------------------------");
 //           Debug.Log(obj3d.mesh.tangents.Length);

            for (int i = 0; i < obj3d.mesh.tangents.Length; i++)
            {
                obj3d.mesh.tangents[i].Normalize();

                tangentsList.Add(obj3d.mesh.tangents[i].x);
                tangentsList.Add(obj3d.mesh.tangents[i].y);
                tangentsList.Add(-obj3d.mesh.tangents[i].z);

            }

            finalBinary.AddRange(BuQuanByte(finalBinary.Count + LastBinCount, 4));
            binaryHold = FloatArrayToInt8Arrar(tangentsList.ToArray());
            obj3d.tangentsPos = new int[] { finalBinary.Count + LastBinCount, binaryHold.Length / 1 };
            finalBinary.AddRange(binaryHold);
        }



            //这个补足四位是为了，后面压缩使用utf32进行
            finalBinary.AddRange(BuQuanByte(finalBinary.Count + LastBinCount, 4));
        
        return finalBinary.ToArray();

    }

    //补全byte位数，如果是int32array 开始的位置的数字必须被4整除，int16arrary必须被2整除
    byte[] BuQuanByte(int count,int needBit)
    {
        int last = count % needBit;       
        last = needBit - last;
        return new byte[last];
    }

    public string Vector3JsonString(string nodeName, Vector3[] inVec3s)
    {
//      string formatStyle = "";
        string final = "\"" + nodeName + "\":[";
        for (int i = 0; i < inVec3s.Length; i++)
        {
            if (i == inVec3s.Length - 1)
            {
                //点点负Z进行反转
                final += StrOptimize(inVec3s[i].x) + "," + StrOptimize(inVec3s[i].y) + "," + StrOptimize(-inVec3s[i].z);
            }
            else
            {
                //点点负Z进行反转
                final += StrOptimize(inVec3s[i].x) + "," + StrOptimize(inVec3s[i].y) + "," + StrOptimize(-inVec3s[i].z) + ",";
            }
        }
        final += "]";

        return final;
    }

    public string Vector2JsonString(string nodeName, Vector2[] inVec2s,bool reverse=false)
    {
        string final = "\"" + nodeName + "\":[";
        for (int i = 0; i < inVec2s.Length; i++)
        {
			if(reverse)
				inVec2s[i]=new Vector2(1-inVec2s[i].y,1-inVec2s[i].x);

            if (i == inVec2s.Length - 1)
            {
                final += StrOptimize(inVec2s[i].x)+ "," + StrOptimize(inVec2s[i].y);
            }
            else
            {
                final +=StrOptimize(inVec2s[i].x)+ "," + StrOptimize(inVec2s[i].y)+ ",";
            }
        }
        final += "]";

        return final;
    }

    public string IntJsonString(string nodeName, int[] inInt)
    {
        string final = "\"" + nodeName + "\":[";
        for (int i = 0; i < inInt.Length; i++)
        {
            if (i == inInt.Length - 1)
            {
                final += inInt[i].ToString();
            }
            else
            {
                final += inInt[i].ToString() + ",";
            }
        }
        final += "]";
        return final;
    }

    public string StrOptimize(float inFloat,int keepPointCount=3)
    {
 //       return inFloat.ToString();

        string outStr = (Mathf.Round(inFloat*Mathf.Pow(10,keepPointCount))).ToString();
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

    public Int32[] FloatArrayToInt32Array(float[] floatArray)
    {
        Int32[] result=new Int32[floatArray.Length];
        for (int i = 0; i < floatArray.Length; i++)
        {
            result[i] = Int32Optimize(floatArray[i],3);
        }
        return result;
    }


    public Int16[] FloatArrayToInt16ArrayUV(float[] floatArray)
    {
        Int16[] result = new Int16[floatArray.Length];
        for (int i = 0; i < floatArray.Length; i++)
        {
            result[i] = Int16Optimize(floatArray[i],4);
        }
        return result;
    }

    public Int16[] FloatArrayToInt16Array(float[] floatArray)
    {
        Int16[] result = new Int16[floatArray.Length];
        for (int i = 0; i < floatArray.Length; i++)
        {
            result[i] =Int16Optimize(floatArray[i]);
        }
        return result;
    }

    public UInt16[] FloatArrayToUInt16Array(float[] floatArray)
    {
        UInt16[] result = new UInt16[floatArray.Length];
        for (int i = 0; i < floatArray.Length; i++)
        {
            result[i] = UInt16Optimize(floatArray[i]);
        }
        return result;
    }


    //保证normal值小于1
    public byte[] FloatArrayToInt8Arrar(float[] floatArray)
    {
        byte[] result = new byte[floatArray.Length];
        for (int i = 0; i < floatArray.Length; i++)
        {
            result[i] =(byte)(floatArray[i]*127f);
        }
        return result;
    }

    public Int32 Int32Optimize(float inFloat, int keepPointCount = 3)
    {
        return (Int32)Mathf.Round(inFloat * Mathf.Pow(10, keepPointCount));
    }

    public Int16 Int16Optimize(float inFloat, int keepPointCount = 3)
    {
        return (Int16)Mathf.Round(inFloat * Mathf.Pow(10, keepPointCount));
    }

    public UInt16 UInt16Optimize(float inFloat, int keepPointCount = 3)
    {
        return (UInt16)Mathf.Round(inFloat * Mathf.Pow(10, keepPointCount));
    }


    string ColorToString(Color inColor)
    {
        //to LinearSpace
        //        Color t = new Color(Mathf.Pow(inColor.r, 2.2f), Mathf.Pow(inColor.g, 2.2f), Mathf.Pow(inColor.b, 2.2f), Mathf.Pow(inColor.a, 1.0f));
        Color t = inColor;

        return "[" + t.r + "," + t.g + "," + t.b + "," + t.a + "]";
    }

    int CheckMeshDataCount(SenceHierarchyInfo.Object3DMesh inObj)
    {
        int finalCount = inObj.mesh.vertexCount * 3;

        if (inObj.exportUV)
        finalCount += inObj.mesh.uv.Length * 2;
        if (inObj.exportUV2)
        finalCount += inObj.mesh.uv2.Length * 2;
        return finalCount;
    }

    Vector3 PointThreeToOne(Vector3 first,Vector3 second,Vector3 third)
    {
        Vector3 one;

        one=first+second+third;
        one=new Vector3(one.x*0.333333f,one.y*0.333333f,one.z*0.333333f);
        return one;
    }

    public byte[] GetInt32ArrayBuffer(Int32[] inIntArray)
        {
            List<byte> result = new List<byte>();

            for (int i = 0; i < inIntArray.Length; i++)
            {
                result.AddRange( BitConverter.GetBytes(inIntArray[i]));
            }
            return result.ToArray();
        }

    public byte[] GetInt16ArrayBuffer(Int16[] inIntArray)
        {
            List<byte> result = new List<byte>();

            for (int i = 0; i < inIntArray.Length; i++)
            {
                result.AddRange(BitConverter.GetBytes(inIntArray[i]));
            }
            return result.ToArray();
        }

    public byte[] GetUInt16ArrayBuffer(UInt16[] inIntArray)
    {
        List<byte> result = new List<byte>();

        for (int i = 0; i < inIntArray.Length; i++)
        {
            result.AddRange(BitConverter.GetBytes(inIntArray[i]));
        }
        return result.ToArray();
    }


    public byte[] GetInt8ArrayBuffer(Int16[] inIntArray)
        {
            List<byte> result = new List<byte>();

            for (int i = 0; i < inIntArray.Length; i++)
            {
                result.Add(BitConverter.GetBytes(inIntArray[i])[1]);
            }
            return result.ToArray();
        }

    public byte[] GetBytesFormFile(string filePath)
    {
        FileStream fileRead = new FileStream(filePath, FileMode.Open);
        byte[] fileBytes=new byte[(int)fileRead.Length];
        fileRead.Read(fileBytes, 0, (int)fileRead.Length);
        fileRead.Close();
        return fileBytes;
    }

    public string ShaderJsonPos(SenceHierarchyInfo s, string shaderName)
    {
        switch (shaderName)
        {
            //shadersVector4Pos 保存有所有shader字符窜的位置；
            //            case "Standard":
//            case "@Moblie_WJM_Mirror":
            case "@Moblie_WJM_Alpha":
                return Vector4ToString(s.shadersVector4Pos[0],0);

            case "@Moblie_WJM_Base":
                return Vector4ToString(s.shadersVector4Pos[1],1);

            case "@Moblie_WJM_CubeMap":
                return Vector4ToString(s.shadersVector4Pos[2],2);

            case "@Moblie_WJM_Glass_Middle":
                return Vector4ToString(s.shadersVector4Pos[3],3);

            case "@Moblie_WJM_Mirror":
                return Vector4ToString(s.shadersVector4Pos[4],4);

            case "@Moblie_WJM_Sky":
                return Vector4ToString(s.shadersVector4Pos[5],5);

            case "@Moblie_WJM_SpriteTree":
                return Vector4ToString(s.shadersVector4Pos[6],6);

            case "@Moblie_WJM_TreeLeaf":
                return Vector4ToString(s.shadersVector4Pos[7],7);

            case "@Moblie_WJM_Water":
                return Vector4ToString(s.shadersVector4Pos[8], 8);

            case "@Moblie_WJM_WFaceCamera":
                return Vector4ToString(s.shadersVector4Pos[9], 9);

            case "@Moblie_WJM_SpriteTreeLeaf":
                return Vector4ToString(s.shadersVector4Pos[10], 10);
            default:
                Debug.LogError(shaderName+"材质不支持");
                return "[]";
        }

    }

    public string Vector4ToString(Vector4 inVec4,int id)
    {
        string outStr = "[" + (int)inVec4.x + "," + (int)inVec4.y + "," + (int)inVec4.z + "," + (int)inVec4.w +","+id+ "]";
//        Debug.Log(outStr);
        return outStr;    
    }


    public static string GetUTF16(string inStr)
    {


        string outStr = "";

        char[] inStrAllChar = inStr.ToCharArray();

//      Debug.Log(inStrAllChar.Length);

        for (int i = 0; i < inStrAllChar.Length; i++)
        {
//            Debug.Log(inStrAllChar[i]);

            if (Regex.IsMatch(inStrAllChar[i].ToString(), @"[\u4e00-\u9fa5]"))
            {
                byte[] labelTextBytes = Encoding.BigEndianUnicode.GetBytes(inStrAllChar, i, 1);

//                Debug.Log(labelTextBytes.Length);

                foreach (byte b in labelTextBytes)
                {
                    outStr += b.ToString("X2");
                }
            }
            else
            {
                outStr += inStrAllChar[i];
            }
        }
        return outStr;
    }



}
