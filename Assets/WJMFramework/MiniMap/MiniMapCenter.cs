//2015.7.18

using UnityEngine.UI;
using UnityEngine;
using System.Collections;
using DG.Tweening;

public class MiniMapCenter: BaseEventCenter 
{

	public CameraUniversalCenter cameraCenter;
	public Vector3 currentCameraPosition;
//	public float currentCameraRotation;
	
	public Image directPointer;

    public Image ContentArea;

    public int currentMiniMapID;
	int lastMiniMapID;
	public CanveGroupFade[] miniMaps;//base,1f,2f,3f


    //世界左下角x，y，世界右上角z，w；
    public Vector4[] leftBottomPosAndRightUpPos;

			
	int actualWidth;
	int actualHighth;
	
	int offsetX;
	int offsetY;
	
	float moveX;
	float moveY;
	
	float scaleX;


    public bool useForRoom360;
    public Material room360Mat;
    public Room360Info[] room360Group;
    public Room360Info currentAtRoom360;

	bool enable;

	public void SetCubemapFromLoadAsset(Cubemap cubemap )
	{
		for (int i = 0; i < room360Group.Length; i++)
		{
			if (room360Group [i].name == cubemap.name)
			{
				room360Group [i].cubemap = cubemap;
			}
		}

	}

    void Update() 
	{
		if (enable) 
		{
            if (!useForRoom360)
            {
                directPointer.rectTransform.localEulerAngles = new Vector3(0, 0, -cameraCenter.currentCameraRot);
                currentCameraPosition = cameraCenter.currentCamera.GetComponent<CameraUniversal>().camBase.localPosition;
                directPointer.rectTransform.localEulerAngles = new Vector3(0, 0, -cameraCenter.currentCameraRot);

                if (cameraCenter.currentCamera.name == "FirstCamera04")
                {
                    currentCameraPosition = cameraCenter.currentCamera.GetComponent<CameraUniversal>().camBase.position;
                    directPointer.rectTransform.localEulerAngles = new Vector3(0, 0, -cameraCenter.currentCameraRot);
                }

                moveX = ContentArea.rectTransform.sizeDelta.x * (currentCameraPosition.x - leftBottomPosAndRightUpPos[currentMiniMapID].x) / (leftBottomPosAndRightUpPos[currentMiniMapID].z - leftBottomPosAndRightUpPos[currentMiniMapID].x);
                moveY = ContentArea.rectTransform.sizeDelta.y * (currentCameraPosition.z - leftBottomPosAndRightUpPos[currentMiniMapID].y) / (leftBottomPosAndRightUpPos[currentMiniMapID].w - leftBottomPosAndRightUpPos[currentMiniMapID].y);
               
                directPointer.rectTransform.anchoredPosition = new Vector2(ContentArea.rectTransform.sizeDelta.x - moveX, ContentArea.rectTransform.sizeDelta.y - moveY);

            }
            else
            {
                directPointer.rectTransform.anchoredPosition = currentAtRoom360.inMiniMapPos;
                directPointer.rectTransform.localEulerAngles = new Vector3(0, 0, -cameraCenter.currentCameraRot + currentAtRoom360.rotateCorret);
                //                moveX
            }
		}

	}


    public void Set360Image(Transform pos)
    {
        for (int i = 0; i < room360Group.Length; i++)
        {
            if (room360Group[i].name == pos.name)
            {
                currentAtRoom360 = room360Group[i];
                currentAtRoom360.inMiniMapPos = pos.localPosition+pos.parent.localPosition;

                room360Mat.SetTexture("_CubeMap", currentAtRoom360.cubemap);
				cameraCenter.currentCamera.GetComponent<CameraUniversal> ().ResetCameraStateToInitial();
				cameraCenter.currentCamera.GetComponent<CameraUniversal> ().SetCameraPositionAndXYZCountAllArgs ("","","","0", currentAtRoom360.defaultCameraRotY.ToString(),"0");
				Debug.Log (pos.name);
            }
        }
    }


	public void ChangeMiniMap(int tarID)
	{
        lastMiniMapID = currentMiniMapID;
        currentMiniMapID = tarID;
        miniMaps[lastMiniMapID].AlphaPlayBackward();
        miniMaps[currentMiniMapID].AlphaPlayForward();
	}


    public void DisplayMiniMap()
	{
        enable=true;

		lastMiniMapID = currentMiniMapID;
		ChangeMiniMap(currentMiniMapID);

        ContentArea.GetComponent<CanveGroupFade>().AlphaPlayForward();
    }
	public void CloseMiniMap()
	{
        enable = false;
        ContentArea.GetComponent<CanveGroupFade>().AlphaPlayBackward();
    }

    public void SetCameraPosition(Transform onlyXZ)
    {
        cameraCenter.currentCamera.GetComponent<CameraUniversal>().camBase.localPosition =new Vector3(onlyXZ.localPosition.x, cameraCenter.currentCamera.GetComponent<CameraUniversal>().camBase.localPosition.y, onlyXZ.localPosition.z);
    }

    public void SetCameraRot(float rot)
    {
        cameraCenter.currentCamera.GetComponent<CameraUniversal>().Ycount = rot;
        cameraCenter.currentCamera.GetComponent<CameraUniversal>().SetCameraPositionAndXYZCountAllArgs("","","",cameraCenter.currentCamera.GetComponent<CameraUniversal>().Xcount.ToString(),rot.ToString(), cameraCenter.currentCamera.GetComponent<CameraUniversal>().Zcount.ToString());
//      cameraCenter.currentCamera.GetComponent<FirstPersonCamera>().fartherBase.localEulerAngles=new Vector3()

    }

}









[System.Serializable]
public class Room360Info
{
    public string name;
    public Cubemap cubemap;
    public Vector3 inMiniMapPos;
    public float rotateCorret;
	public float defaultCameraRotY;

}



