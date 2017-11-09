using UnityEngine;
using System.Collections;

public class DontDestroy : MonoBehaviour
{

    bool hasRun = false;


	void Start ()
    {
	    if(!hasRun)
        {
            DontDestroyOnLoad(this);
            hasRun = true;
        }

	}

}
