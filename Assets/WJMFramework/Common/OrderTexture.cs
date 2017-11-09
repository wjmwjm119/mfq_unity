using UnityEngine;
using System.Collections;

public static class OrderTexture 
{
	public static Texture2D[]  OrderTexture2D(Texture2D[] inTexture2DGroup,int numberCount)
	{
		int lastID=int.MaxValue;
		int currentID=0;
		int findID=0;

		Texture2D[] orderTextureGroup=new Texture2D[inTexture2DGroup.Length];



		for(int i=0;i<orderTextureGroup.Length;i++)
		{


			lastID=int.MaxValue;

			if(i>0)
			{
				string leftName2=orderTextureGroup[i-1].name.Split('.')[0];
				leftName2=leftName2.Substring(leftName2.Length-numberCount,numberCount);
				findID=int.Parse(leftName2);
				findID++;
			}

			for(int j=0;j<inTexture2DGroup.Length;j++)
			{
				string leftName=inTexture2DGroup[j].name.Split('.')[0];
				leftName=leftName.Substring(leftName.Length-numberCount,numberCount);
				currentID=int.Parse(leftName);
			
				if(	(findID<=currentID)&&(currentID<lastID))		
				{
					lastID=currentID;

					orderTextureGroup[i]=inTexture2DGroup[j];

				}
		
			}








		}
		return orderTextureGroup;

	}

    public static Sprite[] OrderTexture2D(Sprite[] inTexture2DGroup, int numberCount)
    {
        int lastID = int.MaxValue;
        int currentID = 0;
        int findID = 0;

        Sprite[] orderTextureGroup = new Sprite[inTexture2DGroup.Length];



        for (int i = 0; i < orderTextureGroup.Length; i++)
        {


            lastID = int.MaxValue;

            if (i > 0)
            {
                string leftName2 = orderTextureGroup[i - 1].name.Split('.')[0];
                leftName2 = leftName2.Substring(leftName2.Length - numberCount, numberCount);
                findID = int.Parse(leftName2);
                findID++;
            }

            for (int j = 0; j < inTexture2DGroup.Length; j++)
            {
                string leftName = inTexture2DGroup[j].name.Split('.')[0];
                leftName = leftName.Substring(leftName.Length - numberCount, numberCount);
                currentID = int.Parse(leftName);

                if ((findID <= currentID) && (currentID < lastID))
                {
                    lastID = currentID;

                    orderTextureGroup[i] = inTexture2DGroup[j];

                }

            }








        }
        return orderTextureGroup;

    }



}
