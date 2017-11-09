using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BirdsSpawn : MonoBehaviour
{

    public bool autoSpawn=true;
    /// <summary>
    ///最大范围
    /// </summary>
    public float sphereR = 50;
    public int birdCount = 3;
    public float flyspeed = 10.0f;
    public float scale = 1.0f;
    int randomBirdType;
    public GameObject[] birdsPerfab;





    List< GameObject> genBirds;



    void Start()
    {

        if (autoSpawn&&birdsPerfab.Length>0)
        {
            SpawnBirds();
        }
            
    }


    public void SpawnBirds()
    {
        genBirds = new List<GameObject>();

        for (int i = 0; i < birdCount; i++)
        {
            
            randomBirdType = Random.Range(0, birdsPerfab.Length);

            genBirds.Add(GameObject.Instantiate(birdsPerfab[randomBirdType]));

            Vector3 singerBirdCenterPos =  Random.onUnitSphere *sphereR;
            singerBirdCenterPos = new Vector3(singerBirdCenterPos.x, singerBirdCenterPos.y*0.1f, singerBirdCenterPos.z) + transform.position;

            genBirds[i].transform.localScale = new Vector3(scale, scale, scale);
            genBirds[i].GetComponent<BirdAnimation>().StartFly(singerBirdCenterPos,sphereR, flyspeed);

        }

    }

    public void DeleteAllBirds()
    {
        if (genBirds != null)
        {
            foreach (GameObject b in genBirds)
            {
                Destroy(b);
            }
            genBirds.Clear();
        }
    }
    








}
