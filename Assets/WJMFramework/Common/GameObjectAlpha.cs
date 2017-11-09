using UnityEngine;
using System.Collections;
using DG.Tweening;

public class GameObjectAlpha : MonoBehaviour
{

    public Material mat;

    public void Display()
    {
        mat.DOFade(1, 1);
    }

    public void Hidden()
    {
        mat.DOFade(0, 1);
    }


}
