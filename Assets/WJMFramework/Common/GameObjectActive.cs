using UnityEngine;
using System.Collections;

public class GameObjectActive :MonoBehaviour
{


   public GameObject[] willActiveObject;
   public GameObject[] willDeActiveObject;



    public void ActiveAndDeActive()
    {
        ActiveObject();
        DeActiveObject();
    }




	public void ActiveObject()
	{          
		if (willActiveObject != null)
		{
			for (int i = 0; i < willActiveObject.Length; i++)
			{
				willActiveObject[i].SetActive(true);
			}
		}
	}

	public void DeActiveObject()
	{          
		if (willDeActiveObject != null)
		{
			for (int i = 0; i < willDeActiveObject.Length; i++)
			{
				willDeActiveObject[i].SetActive(false);
			}
		}
	}


    public void ActiveObjectCollider()
    {
        if (willActiveObject != null)
        {
            for (int i = 0; i < willActiveObject.Length; i++)
            {
                willActiveObject[i].GetComponent<BoxCollider>().enabled = true;
            }
        }
    }

    public void DeActiveObjectCollider()
    {
        if (willActiveObject != null)
        {
            for (int i = 0; i < willActiveObject.Length; i++)
            {
                willActiveObject[i].GetComponent<BoxCollider>().enabled = false;
            }
        }
    }

}
