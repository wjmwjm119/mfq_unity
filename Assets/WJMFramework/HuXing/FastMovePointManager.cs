using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastMovePointManager : MonoBehaviour
{
    public Material movePointMat;
    public Color orginColor=Color.white;

    void Start()
    {
        SetMovePointsState(false);
 //       orginColor= movePointMat.GetColor("_Color");
    }

    void OnDisable()
    {
#if UNITY_EDITOR
        if(movePointMat!=null)
        movePointMat.SetColor("_Color", new Color(orginColor.r, orginColor.g, orginColor.b, 1));
#endif
    }

    public void SetMovePointsState(bool canMove)
    {
        FastMovePoint.canMove = canMove;
        if (canMove)
        {
            movePointMat.SetColor("_Color", new Color(orginColor.r, orginColor.g, orginColor.b, 1));
            Debug.Log("SetMovePointsState Can Move");
        }
        else
        {
            movePointMat.SetColor("_Color", new Color(orginColor.r, orginColor.g, orginColor.b, 0));
            Debug.Log("SetMovePointsState Can`t Move");
        }

    }

}
