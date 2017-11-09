using UnityEngine;
using System.Collections;

public class RandomAnimationGroup : MonoBehaviour 
{
	public RandomAnimation[] randomAniGroup;


	public void PlayGroup()
	{
		for(int i=0;i<randomAniGroup.Length;i++)
		{
			randomAniGroup [i].PlayAll ();

		}
	}

	public void StopGroup()
	{
		for(int i=0;i<randomAniGroup.Length;i++)
		{
			randomAniGroup [i].StopAll ();
		}
	}

}
