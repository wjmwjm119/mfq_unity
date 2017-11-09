using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingManager : MonoBehaviour
{
    public Transform loadingGroupRoot;
    public Loading[] loadingTypeGroup;


    void Awake()
    {
//        DontDestroyOnLoad(this);
    }

    public Loading AddALoading(int targetTypeID)
    {
        Loading l = GameObject.Instantiate(loadingTypeGroup[targetTypeID],loadingGroupRoot,false);
        return l;
    }


}
