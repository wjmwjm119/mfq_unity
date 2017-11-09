using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using DG.Tweening;

public class YTFB : MonoBehaviour 
{

	public Text currentLouCeng;
	public Text displayLouCeng;
	public GameObject[] allFloor;
	public Transform lookAt;

	public Tweener tweener;
	
	public Material[] mat;

	void Awake()
	{
		CloseMatAnimation();

		for(int i=allFloor.Length-1;i>2;i--)
		{
			allFloor[i].transform.DOMoveY(0,i*0.2f).SetDelay((allFloor.Length-1-i)*0.2f);
		}

	}


	public void ChangeHeight(int inID)
	{
		int delayID=0;
		for(int i=2;i<allFloor.Length;i++)
		{
			if(i>inID)
			{

				allFloor[i].transform.DOMoveX(-800,1).SetDelay(delayID*0.15f);
//			allFloor[i].transform.do
				delayID++;
			}
			else
			{
				allFloor[i].transform.DOMoveX(0,1).SetDelay(delayID*0.2f);
			}

		}

		lookAt.DOLocalMove(new Vector3(lookAt.position.x,inID*12f,lookAt.position.z),1);


	}

	
	public void UpStair()
	{
		int id;
		if(currentLouCeng.text!="All")
		{
			id=int.Parse(currentLouCeng.text);
		}
		else
		{
			id=1;
		}


		if(id<14)
		{
			id++;
			currentLouCeng.text=id.ToString();
			displayLouCeng.text=id.ToString();
			ChangeHeight(id);
		}
	}
	
	public void DownStair()
	{

		if(currentLouCeng.text!="All")
		{
			int id=int.Parse(currentLouCeng.text);

			if(id>2)
			{
				id--;
				currentLouCeng.text=id.ToString();
				displayLouCeng.text=id.ToString();
				ChangeHeight(id);
			}
			
			if(id==2)
			{
				id--;
				currentLouCeng.text="All";
				displayLouCeng.text="All";
				for(int i=2;i<allFloor.Length;i++)
				{
					allFloor[i].transform.DOMoveX(0,0.5f).SetDelay(i*0.2f);
				}
					lookAt.DOLocalMove(new Vector3(lookAt.position.x,8*12f,lookAt.position.z),1);
				}
			}

	 }

	public void SetMatAnimation(int inID)
	{
		for(int i=0;i<mat.Length;i++)
		{
			mat[i].SetFloat("_alphaSin",0);
		}

		mat[inID].SetFloat("_alphaSin",1);
	}

	public void CloseMatAnimation()
	{
		for(int i=0;i<mat.Length;i++)
		{
			mat[i].SetFloat("_alphaSin",0);
		}
	}


	


}
