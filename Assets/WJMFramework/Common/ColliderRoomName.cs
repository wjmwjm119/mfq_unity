using UnityEngine;
using System.Collections;

public class ColliderRoomName : MonoBehaviour
{

    void OnTriggerEnter(Collider inCamera)
    { 
        if (inCamera.GetComponentInChildren<CameraUniversal>() != null)
        {
            inCamera.GetComponentInChildren<CameraUniversal>().GetRoomName(this.name);
        }

    }

    void OnTriggerExit(Collider other)
    {


    }

}
