using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class AutoCar : MonoBehaviour
{

    CarManger carManger;
    Vector3[] movePath;
    int currentAtMovePathID;
    float speed=25f;
 
    public void Run()
    {

        if (currentAtMovePathID < movePath.Length - 1)
        {
            float moveTime = (transform.position - movePath[currentAtMovePathID]).magnitude / speed;
            transform.DOMove(movePath[currentAtMovePathID], moveTime).OnComplete(Run).SetEase(Ease.Linear);
            currentAtMovePathID++;
            transform.DOLookAt(movePath[currentAtMovePathID]+new Vector3(0,0.001f,0), moveTime);
            if (currentAtMovePathID == movePath.Length - 1)
                transform.DOScale(new Vector3(0.01f, 0.01f, 0.01f), 0.5f);
        }
        else
        {
            carManger.RemoveCar(this.gameObject);
        }

    }

    public void Initl(CarManger inCarManger, Vector3[] inMovePaht, int startID,float speedFactor)
    {
#if UNITY_EDITOR
        SceneInteractiveManger.RecoverMatShader(transform);
#endif

        speed *= speedFactor;
        carManger = inCarManger;
        movePath = inMovePaht;
        currentAtMovePathID = startID;
        transform.position = movePath[currentAtMovePathID];
        Run();
    }




}
