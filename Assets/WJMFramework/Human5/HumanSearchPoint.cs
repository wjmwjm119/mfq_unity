using UnityEngine;
using System.Collections;

public class HumanSearchPoint : MonoBehaviour
 {
    public enum PointType { claphands = 0, dance = 1, eatsitting = 2, idle = 3, listen = 4, manipulate = 5, sitidle = 6, talk = 7, ftStart = 10, ftEnd = 11 }
    public int beloneArea;

	public PointType pointType=(PointType)3;
	public bool beUsed;

}
