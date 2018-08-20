using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastMovePoint : ColliderTriggerButton
{

// public bool isInMirror;

    public static bool canMove;

    public override void ExeTriggerEvent(TouchCtrl touchCtrl)
    {
        base.ExeTriggerEvent(touchCtrl);
        if (canMove&&touchCtrl != null)
            touchCtrl.cameraCenter.currentCamera.SetCameraPositionAndXYZCountAllArgs(
                (CameraUniversalCenter.isInMirrorHX?-transform.localPosition.x:transform.localPosition.x).ToString(),
                "",
                transform.localPosition.z.ToString(),
                "",
                "",
                "",
                0.3f
                );


    }

}
