using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HumanSpawn : MonoBehaviour
{
    public bool autoSpawn = true;
    public int humanCount = 3;
    public float scale = 1.0f;
    /// <summary>
    ///最大范围
    /// </summary>
    public float circleR = 5;
    int randomType;
    public HumanSearchPointsRoot humanSearchPointsRoot;
    public GameObject[] humanPrefabe;



    List<GameObject> genHuman;


    void Start ()
    {

        if (autoSpawn&&humanSearchPointsRoot!=null&&humanPrefabe.Length>0)
        {
            SpawnHuman();
        }


	}

    void SpawnHuman()
    {
        genHuman = new List<GameObject>();

        for (int i = 0; i < humanCount; i++)
        {

            randomType = Random.Range(0, humanPrefabe.Length);

            Vector2 singerHumanCenterPos = Random.insideUnitCircle * circleR;
//            Debug.Log(singerHumanCenterPos);

            Vector3 initPos = new Vector3(singerHumanCenterPos.x, 0, singerHumanCenterPos.y) + transform.position;

            genHuman.Add(GameObject.Instantiate(humanPrefabe[randomType], initPos, new Quaternion()));
            genHuman[i].transform.localScale = new Vector3(scale, scale, scale);
            genHuman[i].GetComponent<HumanAutoAnimation>().StartMove(humanSearchPointsRoot.transform);


        }

    }



}
