using UnityEngine;
using System.Collections;

public class AutoSendMessage : MonoBehaviour
{

    public float waitTime = 3;

    public enum SendMessageState { Down, Up }
    public SendMessageState sendMessageState = SendMessageState.Down;



    void Start()
    {
        StartCoroutine(SetMessage());
    
    }


    IEnumerator SetMessage()
    {
        yield return new WaitForSeconds(waitTime);
        if (sendMessageState == SendMessageState.Down)
        {
            this.gameObject.SendMessage("WJMDownMessage", SendMessageOptions.DontRequireReceiver);
        }
        else if (sendMessageState == SendMessageState.Up)
        {
            this.gameObject.SendMessage("WJMUpMessage", SendMessageOptions.DontRequireReceiver);
        }
    }

}
