using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastMovePoint : ColliderTriggerButton
{
    public static bool canMove;

    public override void ExeTriggerEvent(TouchCtrl touchCtrl)
    {
        base.ExeTriggerEvent(touchCtrl);
        if (canMove&&touchCtrl != null)
            touchCtrl.cameraCenter.currentCamera.SetCameraPositionAndXYZCountAllArgs(
                transform.position.x.ToString(),
                "",
                transform.position.z.ToString(),
                "",
                "",
                ""
                );


    }

}
