using UnityEngine;
using System.Collections;

public class UVAnimation : MonoBehaviour 
{

    public Material mat;
	public Vector2 defaultVal;

	public float x=1;
	public float y=1;
	public float xSpeed = 0.5F;
	public float ySpeed = 0.5F;

	int i;
	float speedx=0;
	float speedy=0;

    void Start()
    {
        if (mat == null)
            mat = GetComponent<Renderer>().sharedMaterial;
    }

	void Update () 
	{
			
		speedx = x*(xSpeed * Time.time) % 1;
		speedy = y*(ySpeed * Time.time) % 1;

        mat.SetTextureOffset("_MainTex", new Vector2(speedx, speedy));
			
	}
	
}
