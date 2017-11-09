using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.UI;


[CustomEditor(typeof(LabelTextrueRender), true)]

public class LabelTextrueRenderEditor : Editor
{
    public Camera renderUseCamera;
    public RenderTexture renderTextrue;
    public Transform labelGroup;
    public Image headICO;
    public Text labelText;
    public Text labelText2;
    public Image labelTextBG;
    public Transform louHaoGroup;
    public Text louHaoText;
    public Sprite[] headICOLib;

    public Label[] labelRenderList;

    public Mesh mesh;
    public Transform needRenderLabelRoot;



    List<string> renderOutPath;

    string intermediateFolder = "";


    public override void OnInspectorGUI()
    {
        SerializedObject argsSerializedObject = new SerializedObject(target);
        SerializedProperty sp = argsSerializedObject.GetIterator();

        Undo.RecordObject(target, "LabelTextrueRender");
        EditorUtility.SetDirty(target);

        //第一步必须加这个
        sp.NextVisible(true);

        while (sp.NextVisible(false))
        {
            EditorGUILayout.PropertyField(sp, true);
            
        }
        argsSerializedObject.ApplyModifiedProperties();


        EditorGUILayout.Space();
        GUILayout.TextField("", GUILayout.MaxHeight(1));

        EditorGUILayout.BeginHorizontal();

        if (GUILayout.Button("生成ICO", GUILayout.MaxWidth(100), GUILayout.MaxHeight(40)))
        {
            LabelTextrueRender l = (LabelTextrueRender)target;
            if (l != null)
            {
                renderOutPath = new List<string>();


                renderUseCamera =l.renderUseCamera;
                renderTextrue= l.renderTextrue;
                labelGroup= l.labelGroup;
                headICO= l.headICO;
                labelText= l.labelText;
                labelText2= l.labelText2;
                labelTextBG= l.labelTextBG;
                louHaoGroup= l.louHaoGroup;
                louHaoText= l.louHaoText;
                headICOLib= l.headICOLib;
                labelRenderList= l.labelRenderList;
                mesh = l.mesh;
                needRenderLabelRoot= l.needRenderLabelRoot;

                StartRenderICO();
            }
        }

        EditorGUILayout.EndHorizontal();
    }

    public void StartRenderICO()
    {

        if (!Directory.Exists(Application.dataPath + "/intermediate"))
        {
            Directory.CreateDirectory(Application.dataPath + "/intermediate");
        }

        if (!Directory.Exists(Application.dataPath + "/intermediate/" + "labelGroup/"))
        {
            Directory.CreateDirectory(Application.dataPath + "/intermediate/" + "labelGroup/");
        }

        intermediateFolder = Application.dataPath + "/intermediate/" + "labelGroup/";
        labelRenderList = needRenderLabelRoot.GetComponentsInChildren<Label>(true);

        for (int i = 0; i < labelRenderList.Length; i++)
        {
            labelRenderList[i].SetLabelText();
            RenderATexture(labelRenderList[i]);
        }




    }

    void RenderATexture(Label inLabel)
    {
        Texture2D t = new Texture2D(256, 48, TextureFormat.ARGB32, false);
        Texture2D tLouHao = new Texture2D(48, 48, TextureFormat.ARGB32, false);

        louHaoGroup.localPosition = new Vector3(-5000, 0, 0);
        labelGroup.localPosition = new Vector3(-5000, 0, 0);

        Canvas.ForceUpdateCanvases();
        RenderTexture.active = renderTextrue;

        if ((int)inLabel.icon == 13)
        {
            //            Debug.Log(inLabel.icon);
            louHaoGroup.localPosition = new Vector3(0, 0, 0);
            louHaoText.text = inLabel.labelText + "<size=30>#</size>";
        }
        else
        {
            //            Debug.Log(inLabel.icon);
            labelGroup.localPosition = new Vector3(0, 0, 0);
            labelText.text = inLabel.labelText;
            labelText2.text = inLabel.labelText;
            headICO.sprite = headICOLib[(int)inLabel.icon];
            labelTextBG.rectTransform.sizeDelta = new Vector2(60f + labelText.preferredWidth, labelTextBG.rectTransform.sizeDelta.y);
        }

        Canvas.ForceUpdateCanvases();
        RenderTexture.active = renderTextrue;

        renderUseCamera.Render();

        byte[] bytes;

        string texOutPutString="";

        if ((int)inLabel.icon == 13)
        {
            tLouHao.ReadPixels(new Rect(0, 0, 48, 48), 0, 0);
            tLouHao.Apply();
            bytes = tLouHao.EncodeToPNG();
            texOutPutString = "Assets/intermediate/labelGroup/" + "lh_" + inLabel.fileName + ".png";
            File.WriteAllBytes(texOutPutString, bytes);
        }
        else
        {
            t.ReadPixels(new Rect(0, 0, 256, 48), 0, 0);
            t.Apply();
            bytes = t.EncodeToPNG();
            texOutPutString = "Assets/intermediate/labelGroup/" + inLabel.fileName + ".png";
            File.WriteAllBytes(texOutPutString, bytes);
        }

        renderOutPath.Add(texOutPutString);

        Debug.Log(texOutPutString);

        AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);


        TextureImporter textureImporter = (TextureImporter)TextureImporter.GetAtPath(texOutPutString);
        textureImporter.textureType = TextureImporterType.Sprite;
        textureImporter.spriteImportMode = SpriteImportMode.Single;
        textureImporter.textureFormat = TextureImporterFormat.ARGB32;
        
        textureImporter.mipmapEnabled = false;
        textureImporter.alphaIsTransparency = true;
        textureImporter.textureCompression = TextureImporterCompression.Uncompressed;

        Material mat=  CreateMat(texOutPutString, texOutPutString.Split('.')[0] +".mat");

        if (inLabel.GetComponent<MeshFilter>() == null)
        {
           MeshFilter m= inLabel.gameObject.AddComponent<MeshFilter>();
            m.sharedMesh = mesh;
        }

        if (inLabel.GetComponent<MeshRenderer>() == null)
        {
            MeshRenderer mRender = inLabel.gameObject.AddComponent<MeshRenderer>();
            mRender.sharedMaterial = mat;
        }
        else
        {
            inLabel.GetComponent<MeshRenderer>().sharedMaterial = mat;
        }




    }

    Material CreateMat(string texPath,string matPath)
    {
        Texture tex=(Texture)AssetDatabase.LoadAssetAtPath(texPath, typeof(Texture));
        Material mat;


        if (File.Exists(matPath))
        {
            mat = (Material)AssetDatabase.LoadAssetAtPath(matPath, typeof(Material));
            mat.mainTexture = tex;
//            Debug.Log("ff");
        }
        else
        {
            mat = new Material(Shader.Find("@Moblie_WJM_WFaceCamera"));
            mat.mainTexture = tex;
            mat.SetFloat("_Width", 256);
            mat.SetFloat("_Height", 48);
            mat.SetFloat("_sizeBlend", 1);
            mat.SetFloat("_PviotOffsetX", 0.4f);
            mat.SetFloat("_PviotOffsetY", 0.4f);

            if (tex.name.Split('_')[0] == "lh")
            {
                mat.SetFloat("_Width", 48);
                mat.SetFloat("_PviotOffsetX", 0.0f);
            }


            AssetDatabase.CreateAsset(mat, matPath);
            AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);
        }


        



        return mat;

    }

    




}
