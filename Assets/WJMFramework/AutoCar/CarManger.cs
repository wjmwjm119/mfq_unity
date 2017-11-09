using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CarManger : MonoBehaviour
{

    public bool autoSpawn=true;
    public int maxCountCar=16;
    public float scale = 1.0f;
    public GameObject[] carPerfabGroup;
    public RoadPath[] roadGroup;

    List<GameObject> carList;
    List<Vector3[]> lanePathGroup;


    void Start()
    {
        if (autoSpawn)
        {
            GenAllLanePath();
            SpawnCars();
        }
    }

    public void GenAllLanePath()
    {

        lanePathGroup = new List<Vector3[]>();
        for (int i = 0; i < roadGroup.Length; i++)
        {
            for (int j = 0; j < roadGroup[i].rightLanePath.Length; j++)
            {
                lanePathGroup.Add(roadGroup[i].rightLanePath[j].lanePath);
            }

            for (int j = 0; j < roadGroup[i].leftLanePath.Length; j++)
            {
                lanePathGroup.Add(roadGroup[i].leftLanePath[j].lanePath);
            }
        }

    }


    public void SpawnCars()
    {
        carList = new List<GameObject>();

        for (int i = 0; i < maxCountCar; i++)
        {
            SpawnACar();
        }


    }

    public void SpawnACar()
    {
        int carPrefabID = Random.Range(0, carPerfabGroup.Length);
        int lanePathID = Random.Range(0, lanePathGroup.Count);
        Vector3[] lanePath = lanePathGroup[lanePathID];
        int startPosID = Random.Range(0,lanePath.Length-1);
        startPosID = startPosID - startPosID % 2;

        float speedFactor = Random.Range(0.8f, 1.2f);

        GameObject car = GameObject.Instantiate(carPerfabGroup[carPrefabID], lanePath[startPosID],new Quaternion());
        car.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        car.transform.DOScale(new Vector3(scale, scale, scale),0.5f);

        carList.Add(car);
        car.GetComponent<AutoCar>().Initl(this,lanePath,startPosID,speedFactor);

    }


    public void RemoveCar(GameObject inCar)
    {
        if (inCar != null)
        {
            carList.Remove(inCar);
            Destroy(inCar);
            SpawnACar();
        }
    }

    public void RemoveAllCar()
    {
        if (carList != null)
        {
            foreach (GameObject c in carList)
            {
                Destroy(c);
            }
            carList.Clear();
        }

    }


}
