using UnityEngine;
using System.Collections;
using DG.Tweening;

public class YTFBVerticalLadder : MonoBehaviour 
{

    public float speed=30;
    public GameObject ladderBox;
    public GameObject ladderOut;
    public float eachFloorHeight;
    public int totalFloor;
    public int totalHeight;
    public bool  isMoving;



    void Start()
    {
		ladderOut.transform.DOScale (new Vector3 (1, 1, totalHeight), totalHeight / speed).OnComplete (LadderMove);
//        iTween.ScaleTo(ladderOut, iTween.Hash("speed", speed, "scale", new Vector3(1, 1, totalHeight), "EaseType", iTween.EaseType.easeInSine, "delay", 0f, "oncomplete", "LadderMove", "oncompletetarget", this.gameObject));
    }


    void LadderMove()
    {
		float moveTo = UnityEngine.Random.Range (0, totalFloor) * eachFloorHeight;
		ladderBox.transform.DOLocalMove (new Vector3 (0, moveTo, 0), 4f * moveTo / speed).OnComplete(LadderMove2);
//        iTween.MoveTo(ladderBox, iTween.Hash("speed", 0.25f*speed, "position", new Vector3(0,UnityEngine.Random.Range(0, totalFloor)*eachFloorHeight, 0),"islocal", true, "EaseType", iTween.EaseType.easeInOutCubic, "delay", 0f, "oncomplete", "LadderMove2", "oncompletetarget", this.gameObject));
    }

    void LadderMove2()
    {
		float moveTo = UnityEngine.Random.Range (0, totalFloor) * eachFloorHeight;
		ladderBox.transform.DOLocalMove (new Vector3 (0, moveTo, 0), 4f * moveTo / speed).OnComplete(LadderMove2).SetDelay(5f);
//        iTween.MoveTo(ladderBox, iTween.Hash("speed", 0.25f * speed, "position", new Vector3(0, UnityEngine.Random.Range(0, totalFloor) * eachFloorHeight, 0), "islocal", true, "EaseType", iTween.EaseType.easeInOutCubic, "delay", 5f, "oncomplete", "LadderMove2", "oncompletetarget", this.gameObject));
    }




}
