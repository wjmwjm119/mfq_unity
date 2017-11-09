using UnityEngine;
using System.Collections;

public class AnimatorCtrl : MonoBehaviour
{

    public Animator animator;

    public void SetTrigger(string name)
    {
        
        animator.SetTrigger(name);
    }

    public void StopPlayback()
    {
        animator.enabled = false;

    }

    public void StartPlayback()
    {
        animator.enabled = true;
    }




		

}
