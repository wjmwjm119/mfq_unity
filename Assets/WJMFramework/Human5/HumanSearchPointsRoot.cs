using UnityEngine;
using System.Collections;

public class HumanSearchPointsRoot: MonoBehaviour 
{


	public Transform[] beingSearchPointPositionGroup;
	public int childLength=0;
    HumanSearchPoint[] humanSearchPointGroup;

	void Awake()
	{

       humanSearchPointGroup= GetComponentsInChildren<HumanSearchPoint>();
       beingSearchPointPositionGroup = new Transform[humanSearchPointGroup.Length];
       childLength = humanSearchPointGroup.Length;
       for (int i = 0; i < humanSearchPointGroup.Length; i++)
       {
           beingSearchPointPositionGroup[i] = humanSearchPointGroup[i].transform;

       }

	
	}
	
	
	
}
