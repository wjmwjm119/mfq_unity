using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BirdAnimation : MonoBehaviour
{

    public Animator animator;

    Vector3 orginPos;
    float flyspeed;
    float sphereR;


    public List<Vector3> flyPath;


    public void StartFly(Vector3 inOringPos,float inSphereR,float inFlyseed)
    {
        flyspeed = inFlyseed * Random.Range(0.8f, 1.1f);
        animator.speed = Random.Range(0.8f, 1.1f);
        orginPos = inOringPos;
        transform.position = inOringPos;
        sphereR = inSphereR;

        flyPath.Add(orginPos);

        Vector3 newPos = Random.onUnitSphere * sphereR;
        newPos = new Vector3(newPos.x, 0.1f * newPos.y, newPos.z) + orginPos;
        flyPath.Add(newPos);

        float flyTime =  (flyPath[1] - flyPath[0]).magnitude;
        flyTime = flyTime / flyspeed;

        transform.DOPath(flyPath.ToArray(), flyTime, PathType.Linear, PathMode.Full3D, 5, new Color(1, 0, 0)).OnComplete(FlyNextPosition).SetEase(Ease.InOutSine);
        transform.DOLookAt(flyPath[1], 1f);

    }

    void FlyNextPosition()
    {
        flyPath.RemoveAt(0);

        Vector3 newPos = Random.onUnitSphere * sphereR;
        newPos = new Vector3(newPos.x, 0.1f * newPos.y, newPos.z) + orginPos;
        flyPath.Add(newPos);


        float flyTime =(flyPath[1] - flyPath[0]).magnitude;
        flyTime = flyTime / flyspeed;

        transform.DOPath(flyPath.ToArray(), flyTime, PathType.Linear, PathMode.Full3D, 5, new Color(1, 0, 0)).OnComplete(FlyNextPosition).SetEase(Ease.InOutSine);
        transform.DOLookAt(flyPath[1], 1f);

    }




}
