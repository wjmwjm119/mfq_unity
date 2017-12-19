using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using System.IO;
using System;
using System.Collections.Generic;

//using Unity.IO.Compression;
//using Ionic.Zlib;
//using SevenZip;
//using ZLibNet;


public class BuildAssetBundles : EditorWindow
{	
    public string savePath	= "";
	public string andriodFolder="";
	public string iosFolder="";
	public string intermediateFolder = "";

    public ProjectAssetBundlesInfo projectAssetBundlesInfo;

    public List<SceneAsset> needExportScene;
    public List<SceneTypeSet> needExportSceneType;
    public List<string> needExportScenePath;
    public List<string> sceneChineseName;

    public string[] sceneAssetBundle;
    public string[] addCommonAssetBundle;

    string filePath = "";
    string jsonString="";

    //比SenceInteractiveInfo中的SceneType都加1,为了
    public enum SceneTypeSet
    {
        Null=0,
        大场景 = 1,
        外景 = 2,
        平层 = 3,
        loft = 4,
        独栋 =5,
        联排 = 6,
        叠拼 = 7,
        Point360 = 8
    }

    [MenuItem("WJMTooLs/BuildAppAssetBundle")]
	static void Init()
	{
        BuildAssetBundles window = (BuildAssetBundles)EditorWindow.GetWindow (typeof(BuildAssetBundles));
	    window.minSize = new Vector2 (500, 400);
		window.wantsMouseMove = true;
		window.Show();
    }

    void OnEnable()
    {
        filePath = Application.dataPath + "/WJMFramework/BuildAssetBundle/Editor/ProjectAssetBundlesInfo.txt";
        CreateOutPutFolder();
        LoadProjectAssetBundlesInfo();
    }

    void OnDisable()
    {
        SaveProjectAssetBundlesInfo();
    }

    void LoadProjectAssetBundlesInfo()
    {
        if (File.Exists(filePath))
        {
            needExportScenePath = new List<string>();
            needExportSceneType = new List<SceneTypeSet>();

            jsonString = File.ReadAllText(filePath);
            projectAssetBundlesInfo = JsonUtility.FromJson<ProjectAssetBundlesInfo>(jsonString);
            addCommonAssetBundle = projectAssetBundlesInfo.addCommonAssetBundle;


            for (int i = 0; i < projectAssetBundlesInfo.sceneTypeSet.Length; i++)
            {
                needExportSceneType.Add((SceneTypeSet)projectAssetBundlesInfo.sceneTypeSet[i]);
            }

//          foreach()
//          {
//               needExportSceneType.AddRange(projectAssetBundlesInfo.sceneType);
//          }

            needExportScenePath.AddRange(projectAssetBundlesInfo.needExportScenePath);

            needExportScene = new List<SceneAsset>();

            foreach (string p in needExportScenePath)
            {
                SceneAsset s = AssetDatabase.LoadAssetAtPath<SceneAsset>(p);

                if (s != null)
                {
                    needExportScene.Add(s);
                }
                else
                {
                    needExportScene.Add(new SceneAsset());
                }
//                AssetImporter assetImporter = AssetImporter.GetAtPath(p);  //得到Asset
            }

//          AssetDatabase.GetAssetPath(sceneAsset.GetInstanceID())
        }
        else
        {
			projectAssetBundlesInfo=new ProjectAssetBundlesInfo();
			projectAssetBundlesInfo.addCommonAssetBundle=new string[1];
			projectAssetBundlesInfo.addCommonAssetBundle[0]="basesource";

            jsonString = JsonUtility.ToJson(projectAssetBundlesInfo);
            File.WriteAllText(filePath, jsonString);
            AssetDatabase.Refresh();
            LoadProjectAssetBundlesInfo();

        }
    //    XmlWriter writer = XmlWriter.Create(Application.dataPath + "/WJMFramework/Editor/MaterialsEditor/prefabXML/" + xmlName + ".xml", settings);

    }

    void GenScenePath()
    {
        needExportScenePath = new List<string>();

        foreach (SceneAsset s in needExportScene)
        {
            string p = "";

            if (s != null)
            {
                p = AssetDatabase.GetAssetPath(s.GetInstanceID());
            }
            needExportScenePath.Add(p);
        }
    }

    void SaveProjectAssetBundlesInfo()
    {
        projectAssetBundlesInfo.addCommonAssetBundle = addCommonAssetBundle;

        GenScenePath();
        projectAssetBundlesInfo.needExportScenePath = needExportScenePath.ToArray();


        int[] sceneTypeGroup = new int[needExportSceneType.Count];


        for (int i = 0; i < needExportSceneType.Count; i++)
        {
            sceneTypeGroup[i] =(int)needExportSceneType[i];
        }

        projectAssetBundlesInfo.sceneTypeSet = sceneTypeGroup;

        jsonString = JsonUtility.ToJson(projectAssetBundlesInfo);
        File.WriteAllText(filePath, jsonString);

        #if UNITY_IPHONE || UNITY_IOS
                File.WriteAllText(iosFolder+ "/ProjectAssetBundlesInfo.txt", jsonString);
        #elif UNITY_ANDROID
                File.WriteAllText(andriodFolder + "/ProjectAssetBundlesInfo.txt", jsonString);
        #endif

    }

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

//      Debug.Log(parentPath);
        return parentPath;
    }

    void DeleteOutPutFolder()
    {
        if (Directory.Exists(savePath))
        {
//          Directory.Delete(savePath,true);
            Directory.Delete(iosFolder, true);
            Directory.Delete(andriodFolder, true);
        }

        if (Directory.Exists(intermediateFolder))
        {
//            Directory.Delete(intermediateFolder, true);
        }
    }

    void CreateOutPutFolder()
    {
        string projectFolderPath = GetParentPath(Application.dataPath);

        savePath = "/ExportABFolder";
        andriodFolder = "/Andriod";
        iosFolder = "/IOS";
        intermediateFolder = "/intermediate";

        savePath = projectFolderPath + savePath;
        andriodFolder = savePath + andriodFolder;
        iosFolder = savePath + iosFolder;
        intermediateFolder = Application.dataPath + intermediateFolder;

        if (!Directory.Exists(savePath))
        {
            Debug.Log("创建输出文件夹" + savePath);
            Directory.CreateDirectory(savePath);
        }

        if (!Directory.Exists(andriodFolder))
        {
            Debug.Log("创建输出文件夹" + andriodFolder);
            Directory.CreateDirectory(andriodFolder);
        }

        if (!Directory.Exists(iosFolder))
        {
            Debug.Log("创建输出文件夹" + iosFolder);
            Directory.CreateDirectory(iosFolder);
        }

        if (!Directory.Exists(intermediateFolder))
        {
            Debug.Log("创建输出文件夹" + intermediateFolder);
            Directory.CreateDirectory(intermediateFolder);
        }

        AssetDatabase.Refresh();
        //        intermediateFolder

    }


    void OnGUI()
    {
        if (GUI.Button(new Rect(600, 15, 150, 20), new GUIContent("清空文件夹")))
        {
            DeleteOutPutFolder();
            CreateOutPutFolder();
        }

        EditorGUILayout.LabelField("");
        EditorGUILayout.LabelField("Andriod  输出：" + andriodFolder);
        EditorGUILayout.LabelField("    IOS   输出：" + iosFolder);
        EditorGUILayout.LabelField("");

            EditorGUILayout.LabelField("上一次输出时间:" + projectAssetBundlesInfo.buildTime);

            EditorGUILayout.LabelField("");
            projectAssetBundlesInfo.hiddenDefaultUI = EditorGUILayout.ToggleLeft("Hidden Default UI", projectAssetBundlesInfo.hiddenDefaultUI);

            EditorGUILayout.LabelField("");
            //        projectAssetBundlesInfo

            //projectAssetBundlesInfo
            // "target" can be any class derrived from ScriptableObject 
            // (could be EditorWindow, MonoBehaviour, etc)
            //       ScriptableObject target = this;
            SerializedObject so = new SerializedObject(this);

            SerializedProperty displayProperty = so.FindProperty("needExportScene");
            EditorGUILayout.PropertyField(displayProperty, true); // True means show children
            EditorGUILayout.LabelField("");

            displayProperty = so.FindProperty("needExportSceneType");
            EditorGUILayout.PropertyField(displayProperty, true); // True means show children
            EditorGUILayout.LabelField("");


            displayProperty = so.FindProperty("addCommonAssetBundle");
            EditorGUILayout.PropertyField(displayProperty, true); // True means show children

            so.ApplyModifiedProperties(); // Remember to apply modified properties

 //       Debug.Log(addCommonAssetBundle.Length);


        EditorGUILayout.LabelField("");

        if (GUILayout.Button(new GUIContent("Export")))
        {

            GenScenePath();

            sceneAssetBundle = new string[needExportScenePath.Count];

            for (int i = 0; i < needExportScenePath.Count; i++)
            {
                sceneAssetBundle[i] = needExportScenePath[i].Split('.')[0];
                string[] splitStr = sceneAssetBundle[i].Split('/');
                sceneAssetBundle[i] = splitStr[splitStr.Length - 1];
                sceneAssetBundle[i] = ExportSenceData.GetUTF16(sceneAssetBundle[i]);
                sceneAssetBundle[i] = sceneAssetBundle[i].ToLower();
                AssetImporter assetImporter = AssetImporter.GetAtPath(needExportScenePath[i]);  //得到Asset
                assetImporter.assetBundleName = sceneAssetBundle[i];    //最终设置assetBundleName
            }

            projectAssetBundlesInfo.sceneAssetBundle = sceneAssetBundle;
            projectAssetBundlesInfo.sceneAssetBundleHash = new string[needExportScene.Count];
            projectAssetBundlesInfo.sceneAssetBundleCRC = new uint[needExportScene.Count];



            AssetDatabase.RemoveUnusedAssetBundleNames();

            //AssetBundleManifest abManifest=new AssetBundleManifest();

            //lzma
            BuildAssetBundleOptions bOption = BuildAssetBundleOptions.None;

            string currentAssetBundlePath="";
#if UNITY_IPHONE || UNITY_IOS
            BuildPipeline.BuildAssetBundles(iosFolder + "/", bOption, BuildTarget.iOS);
            currentAssetBundlePath=iosFolder;
#elif UNITY_ANDROID
            BuildPipeline.BuildAssetBundles(andriodFolder + "/", bOption, BuildTarget.Android);
            currentAssetBundlePath = andriodFolder;
#endif

            for (int i = 0; i < sceneAssetBundle.Length; i++)
            {
                Hash128 hash;
                BuildPipeline.GetCRCForAssetBundle(currentAssetBundlePath + "/" + sceneAssetBundle[i], out projectAssetBundlesInfo.sceneAssetBundleCRC[i]);
                BuildPipeline.GetHashForAssetBundle(currentAssetBundlePath + "/" + sceneAssetBundle[i], out hash);
                projectAssetBundlesInfo.sceneAssetBundleHash[i] = hash.ToString();
            }

            //           TimeSpan tiemSpan = DateTime.Now.ToShortTimeString;
            
            projectAssetBundlesInfo.buildTime = DateTime.Today.Year.ToString()+",";
            projectAssetBundlesInfo.buildTime += DateTime.Today.Month.ToString()+",";
            projectAssetBundlesInfo.buildTime += DateTime.Today.Day.ToString() + ",";
            projectAssetBundlesInfo.buildTime += DateTime.Now.Hour.ToString() + ",";
            projectAssetBundlesInfo.buildTime += DateTime.Now.Minute.ToString() + ",";
            projectAssetBundlesInfo.buildTime += DateTime.Now.Second.ToString() ;

            

            SaveProjectAssetBundlesInfo();

        }

		
//       foreach ()
//       {

//       }


    }

   
    bool CheckNeedRemoveObject(GameObject g)
    {
        if (g.name == "__UtilTgtTrans" || g.name == "__UtilDirTrans" || g.hideFlags != HideFlags.None)
        {
            return true;
        }


        return false;
    }

    void CreateSceneRootPerfab(SceneAsset sceneAsset)
    {

        Scene needOpenScene = EditorSceneManager.OpenScene(AssetDatabase.GetAssetPath(sceneAsset.GetInstanceID()));
        
        List<GameObject> sceneRootGameObjects = new List<GameObject>();

        sceneRootGameObjects.AddRange(needOpenScene.GetRootGameObjects());

        foreach (GameObject g in sceneRootGameObjects)
        {
            if (CheckNeedRemoveObject(g))
            sceneRootGameObjects.Remove(g);
        }

        GameObject sceneRoot = new GameObject();
        sceneRoot.name = name + "SceneRoot";
        foreach (GameObject g in sceneRootGameObjects)
        {
            g.transform.parent = sceneRoot.transform;
        }

        string createPrefabPath = "Assets/intermediate/" + needOpenScene.name + "_SceneRoot.prefab";
        GameObject createPrefab=  PrefabUtility.CreatePrefab(createPrefabPath, sceneRoot);

        AssetImporter assetImporter = AssetImporter.GetAtPath(createPrefabPath);  //得到Asset
        assetImporter.assetBundleName = createPrefab.name.ToLower();    //最终设置assetBundleName
       

        EditorSceneManager.NewScene(new NewSceneSetup());

    }


}
