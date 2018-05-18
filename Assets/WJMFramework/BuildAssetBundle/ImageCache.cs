using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ImageCache : MonoBehaviour
{

//  https://123.5.40.145/APP/map.png
//  public RawImage rawImage;
    public PathAndURL pathAndURL;
    public ServerProjectInfo serverProjectInfo;
    public AssetBundleManager assetBundleManager;
    public NetCtrlManager netCtrlManager;
    public LoadingManager loadingManager;
    public Texture2D placeHoldTex;


    public List<NetTexture2D> allNetTextrue2D;

    public Dictionary<string, Texture2D> allRamCachedImage;
    //缓存在内存中的图片
    public List<Texture2D> allRamImage;

    int currentLoadID;
    bool isLoopLoading;

    void Start()
    {

      allRamCachedImage = new Dictionary<string, Texture2D>();
//    allImage = new List<Texture2D>();
//    LoadAndCacheImageFromServer("http://123.59.40.145/APP", "map", "", "png");
    }

    public void LoopLoadAndCacheImageFromServer()
    {
        if (!isLoopLoading)
        {
            isLoopLoading = true;
            LoopLoadAndCacheImageFromServer(0);
        }
    }

    void LoopLoadAndCacheImageFromServer(int currentID)
    {
        string imageSeverlLoadPath =pathAndURL.imageFinalUrl + allNetTextrue2D[currentID].url;
//      string imageSeverlLoadPath = "http://123.59.40.145/APP/allproject/201708240001/" + allNetTextrue2D[currentID].url;

//        Debug.Log(imageSeverlLoadPath);
        //        Debug.Log(allNetTextrue2D[currentID].texName);
        //        Debug.Log(allNetTextrue2D[currentID].url);
        Debug.Log(pathAndURL.localImageCachePath + "/" + allNetTextrue2D[currentID].texName);
      
        //本地有存在此图片就不再从服务器上下载
        if (File.Exists(pathAndURL.localImageCachePath + "/" + allNetTextrue2D[currentID].texName))
        {
            GlobalDebug.Addline("图片已缓存: " + allNetTextrue2D[currentID].texName);
            allNetTextrue2D[currentID].hasLocalCached = true;
            LoadNext();
            return;
        }

        Loading loading = loadingManager.AddALoading(4);
        netCtrlManager.WebRequest("Loading:" + allNetTextrue2D[currentID].texName, imageSeverlLoadPath, loading.LoadingAnimation,
        (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a, string info) => { Debug.LogError(imageSeverlLoadPath + " Load Failed!");  LoadNext(); },
         null,
         (DownloadHandlerTexture t) =>
         {
             File.WriteAllBytes(pathAndURL.localImageCachePath + "/" + allNetTextrue2D[currentID].texName, t.data);
             //下载的图片暂时不用,所以要销毁.再用的时候从图片缓存里提取

             GlobalDebug.Addline("下载图片到本地: " + allNetTextrue2D[currentID].texName);
             //OnCached会保留下载的图片.如果此图片有被使用.如果此图片没有被使用则会被Destroy;
             allNetTextrue2D[currentID].OnLocalCached(t.texture);
             LoadNext();
         },
         null
         );
    }

    void LoadNext()
    {
        //是否需要打断加载
        if (SceneInteractiveManger.needBreakLoad)
        {
            GlobalDebug.Addline("BreakAddSource");
            Debug.Log("BreakAddSource");
            assetBundleManager.sceneInteractiveManger.unload.LoadUnloadScene();
            return;
        }


        currentLoadID++;
        if (currentLoadID < allNetTextrue2D.Count)
        {
            LoopLoadAndCacheImageFromServer(currentLoadID);
        }
        else if (currentLoadID == allNetTextrue2D.Count)
        {
            OnAllImageLoaded();
            //                currentLoadID++;
        }

    }

    void OnAllImageLoaded() 
    {

/*
        for (int i = 0; i < allNetTextrue2D.Count; i++)
        {
            if (allNetTextrue2D[i].hasLocalCached)
            {
                LoadTexture2D(allNetTextrue2D[i].texName);
                Debug.Log(allRamCachedImage[allNetTextrue2D[i].texName].width);
            }
        }
*/

        currentLoadID = 0;
        isLoopLoading = false;
        Debug.Log("AllImageLoaded");
        GlobalDebug.Addline("AllImageLoaded");

        SceneInteractiveManger.isLoopingAddSource = false;

        //当图片都下载存储了就表示整个项目都已经缓存了,保存从服务上得到的ProjectInfo
        serverProjectInfo.SaveProjectInfoToLocal();


    }



    public Texture2D LoadTexture2D(string imageName)
    {
        string imageLoadPath = pathAndURL.localImageCachePath+"/"+imageName;

        if (allRamCachedImage.ContainsKey(imageName))
        {
            GlobalDebug.Addline("内存已有: " + imageName );
            return allRamCachedImage[imageName];
        }
        else
        {
            Texture2D tempTex = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            byte[] texByte = File.ReadAllBytes(imageLoadPath);
            bool isVaild = tempTex.LoadImage(texByte);

            if (isVaild)
            { 
                tempTex.Apply(false, true);

                tempTex.name = imageName;
                allRamCachedImage.Add(imageName, tempTex);
                allRamImage.Add(tempTex);

                GlobalDebug.Addline("内存加入: " + imageName);

                return tempTex;
            }

        }
        return null;
    }

    public bool UnloadTexture2D(string imageName)
    {
        if (allRamCachedImage.ContainsKey(imageName))
        {
            allRamImage.Remove(allRamCachedImage[imageName]);

            Destroy(allRamCachedImage[imageName]);

            if (allRamCachedImage.Remove(imageName))
            {
                GlobalDebug.Addline("Unload: " + imageName);
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return false;
        }
        
    }

    public void LoadSingerImageFromServer(string urlBase,string imageName,RawImage needSetRawImage = null,Material mat=null)
    {
        string imageSeverlLoadPath = urlBase + "/" + imageName;

        Loading loading = loadingManager.AddALoading(4);
        netCtrlManager.WebRequest("Loading:"+ imageName, imageSeverlLoadPath, loading.LoadingAnimation,
        (NetCtrlManager.RequestHandler r, UnityWebRequestAsyncOperation a, string info) => { Debug.LogError(imageSeverlLoadPath + " Load Failed!"); },
         null,
         (DownloadHandlerTexture t) =>
         {          
             if (needSetRawImage != null)
             needSetRawImage.texture = t.texture;

             if (mat != null)
                 mat.mainTexture = t.texture;

            GlobalDebug.Addline("LoadingImage: "+ imageName);
         },
         null
         );
    }

}

/// <summary>
/// 储存下载下来的图片
/// </summary>
[System.Serializable]
public class NetTexture2D
{
    public string texName;
    public string url;
    public ImageCache imageCache;
    public ScaleImage scaleImage;

    
    public bool hasLocalCached;
    public Texture2D ramCachetexture;

    public NetTexture2D(string inName, string inUrl,ImageCache inImageCache)
    {
        texName = inName;
        url = inUrl;
        imageCache = inImageCache;
    }

    public Texture2D LoadTexture2D(ScaleImage inScaleImage = null)
    {
        if(inScaleImage != null)
         scaleImage = inScaleImage;

        if(ramCachetexture!=null)
        return ramCachetexture;
          
        if (hasLocalCached)
        {
            ramCachetexture = imageCache.LoadTexture2D(texName);
            return ramCachetexture;
        }
       return imageCache.placeHoldTex;
    }

    public void UnloadTexture()
    {
        if (hasLocalCached)
        {
            scaleImage = null;
            ramCachetexture = null;
            imageCache.UnloadTexture2D(texName);
        }

    }

    /// <summary>
    /// 当缓存到本地时,如果这张图有被scaleImage使用就保留,如果没有被使用就Destroy以减少内存使用;
    /// </summary>
    /// <param name="inTex"></param>
    public void OnLocalCached(Texture2D inTex)
    {
        if (!hasLocalCached)
        {
            hasLocalCached = true;

            if (scaleImage != null)
            {
                scaleImage.SetImage(inTex);
                ramCachetexture = inTex;
            }
            else
            {
               Object.Destroy(inTex);
            }
            
        }

//       scaleImage.SetImage(inTex);
    }




}
