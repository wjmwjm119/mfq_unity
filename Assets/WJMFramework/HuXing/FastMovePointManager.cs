using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FastMovePointManager : MonoBehaviour
{
    public Material movePointMat;

    void Start()
    {
        SetMovePointsState(false);
    }


    public void SetMovePointsState(bool canMove)
    {
        FastMovePoint.canMove = canMove;
        if (canMove)
        {
            movePointMat.SetColor("Color", new Color(0, 0, 0, 1));
            Debug.Log("SetMovePointsState Can Move");
        }
        else
        {
            movePointMat.SetColor("Color", new Color(0, 0, 0, 0));
            Debug.Log("SetMovePointsState Can`t Move");
        }

    }

}
