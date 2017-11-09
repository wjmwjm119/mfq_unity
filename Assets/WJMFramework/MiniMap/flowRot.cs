using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class flowRot : MonoBehaviour
{

    public float offsetRot = 0;
    public CameraUniversalCenter cameraCenter;


	
	// Update is called once per frame
	void Update ()
    {
        transform.localEulerAngles = new Vector3(0, 0, offsetRot + cameraCenter.currentCameraRot);
	}
}
