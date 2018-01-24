using UnityEngine;
using System.Collections;



public class HumanAutoAnimation : MonoBehaviour
 {

    /*
    claphands
    dance
    eatsitting
    idle
    listen
    manipulate
    sitidle
    talk
    walk
    walkcarry
    */
    //	public bool randomGenerate;


    //	public string named;		
    //	public enum Sex{Men,Women}
    //	public Sex sex=Sex.Men;
    //	public int age=25;	
    //	public enum Job{Retail=0,Entertainment=1,Catering=2,Service=3}
    //	public Job job=0;

    public Material[] materialsGroup;

    public enum BodyState { claphands = 0, dance = 1, eatsitting = 2, idle = 3, listen = 4, manipulate = 5, sitidle = 6, talk = 7, walk = 8, walkcarry = 9, ftStart = 10}
	public BodyState bodyState=(BodyState)8;
	public int searchedAreaValue;
//	public enum PointType{claphands=0,dance=1,eatsitting=2,idle=3,listen=4,manipulate=5,sitidle=6,talk=7}
//	public float waitTimeForAni;
	public bool likeClaphands;//0
	public bool likeDance;//1
	public bool likeEatsitting;//2	
	public bool likeIdle;//3
	public bool likeListen;//4	
	public bool likeManipulate;	//5
	public bool likeSitidle;	//6
	public bool likeTalk;	//7
    public bool likeWalk;	//8
    public bool likeWalkCarry;	//9
    public bool likeFtStart=true;//10
    public bool likeFtEnd;//11

	bool[] likeGroup;
	
	
//	public float walkSpeed=0.5f;
//	public float eatSpeed=0.5f;
//	public float danceSpeed=0.5f;
	
	public  string 	skinnedMeshBasePath="Bip01/Bip01 Pelvis";
	public	Transform skinnedMeshBase;

	int ID;
	

	public string defaultAnimaiton="idle";	
	public float aniSpeed=1f;	
	public float currentMoveSpeed;
	
	public float betweenDistance;
	
	public Transform SearchPointGroup;
	public Transform targetPoint;

 



    public int currentAtArea;

    public bool isMoving;


    public void Start()
    {
  //      Debug.Log("ddd");
 //       Start();
    }




    public void StartMove(Transform searchRoot)
	{

//#if UNITY_EDITOR
        SceneInteractiveManger.RecoverMatShaderSkinMesh(transform);
//#endif

        SearchPointGroup = searchRoot;
        likeGroup = new bool[12] { likeClaphands, likeDance, likeEatsitting, likeIdle, likeListen, likeManipulate, likeSitidle, likeTalk, likeWalk, likeWalkCarry, likeFtStart, likeFtEnd };
		
	
		ID=(int)(materialsGroup.Length*Random.value);
		skinnedMeshBase=transform.Find(skinnedMeshBasePath);
		skinnedMeshBase.GetComponent<SkinnedMeshRenderer>().sharedMaterial=materialsGroup[ID];
		
		aniSpeed=aniSpeed*(0.5f*Random.value+0.75f);
		GetComponent<UnityEngine.AI.NavMeshAgent>().speed=aniSpeed;
		foreach(AnimationState state in GetComponent<Animation>())
		{
			state.speed=aniSpeed;
			
		}		
		GetComponent<Animation>().Play(defaultAnimaiton);
//		animation["walk"].weight = 0.01F;
//		animation["dance"].weight = 1F;
//		animation["dance"].blendMode = AnimationBlendMode.Additive;
//		animation.Play("claphands");
//		animation.Play("dance");
//		animation.Play("eatsitting");
//		animation.Play("idle");
//		animation.Play("listen");
//		animation.Play("manipulate");
//		animation.Play("sitidle");
//		animation.Play("talk");
//		animation["claphands"].wrapMode = WrapMode.Once;
//		animation.Play("walk");
//		animation["talk"].layer =1 ;
//		animation["dance"].blendMode = AnimationBlendMode.Blend;
//		animation["dance"].AddMixingTransform(shoulder);
//		animation["dance"].weight = 0.01F;
//		animation.Play("dance");
//		animation.Play("walk");
//		animation.Blend("dance",0.5f,0.3f);
//		animation.Play("manipulate");
//		animation.CrossFadeQueued("walk", 1F, QueueMode.CompleteOthers);	     

		StartCoroutine(AutoMove());	
				
	}
	

	void Update () 
	{
	
	}
	
	
	
	
	Vector3 RandomPoint(Transform pointGroup)
	{

		int NO;

        NO = (int)(pointGroup.GetComponent<HumanSearchPointsRoot>().childLength * Random.value);
        targetPoint = pointGroup.GetComponent<HumanSearchPointsRoot>().beingSearchPointPositionGroup[NO];

        while (targetPoint.GetComponent<HumanSearchPoint>().beloneArea != currentAtArea)
        {
            NO = (int)(pointGroup.GetComponent<HumanSearchPointsRoot>().childLength * Random.value);
            targetPoint = pointGroup.GetComponent<HumanSearchPointsRoot>().beingSearchPointPositionGroup[NO];
        }

		searchedAreaValue=(int)(targetPoint.GetComponent<HumanSearchPoint>().pointType);

//		pointGroup.GetComponent<GetChildsPosition>().beingSearchPointPositionGroup[NO];
		return targetPoint.position;
	}

	
	
	IEnumerator AutoMove()
	{
        //实例生成的行走人的NavMeshAgent组件得先关闭.实例生成且位置设置好后,再将NavMeshAgent组件启用,这是行走人才会正常运行
        GetComponent<UnityEngine.AI.NavMeshAgent>().enabled = true;

        GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(RandomPoint(SearchPointGroup));	
//		searchedAreaValue=(int)(targetPoint.GetComponent<HumanSearchPoint>().pointType);
				
		isMoving=true;		
		
		while(isMoving)
		{

			currentMoveSpeed=Mathf.Sqrt(GetComponent<UnityEngine.AI.NavMeshAgent>().velocity.x*GetComponent<UnityEngine.AI.NavMeshAgent>().velocity.x+GetComponent<UnityEngine.AI.NavMeshAgent>().velocity.z*GetComponent<UnityEngine.AI.NavMeshAgent>().velocity.z);
			if(currentMoveSpeed>0.1f)	
			{
				GetComponent<Animation>().CrossFade("walk");
			}
			else
			{
				GetComponent<Animation>().CrossFade("idle");
			}
			
			betweenDistance=(transform.position-targetPoint.position).magnitude;
			
/*			
			if(betweenDistance<3f)
			{

				GetComponent<NavMeshAgent>().speed=Mathf.Lerp(GetComponent<NavMeshAgent>().speed, 0.5f*aniSpeed,Time.deltaTime * 10f);	
//				animation.Blend("idle");				
			}
			else
			{
				GetComponent<NavMeshAgent>().speed=Mathf.Lerp(GetComponent<NavMeshAgent>().speed, aniSpeed,Time.deltaTime * 5f);		
//				GetComponent<NavMeshAgent>().speed=aniSpeed;
					
			}
*/						
						
				
			if(betweenDistance<0.5f)
			{
//				print("111");
				if(likeGroup[searchedAreaValue]&&!targetPoint.GetComponent<HumanSearchPoint>().beUsed)
				{
					targetPoint.GetComponent<HumanSearchPoint>().beUsed=true;
					isMoving=false;
                    GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = true;
					bodyState=(BodyState)searchedAreaValue;
//					print("222");
					switch(bodyState)       
					{
                        
						case BodyState.claphands:   
						StartCoroutine(AutoClaphands());
						break;                  

						case BodyState.dance:  
						StartCoroutine(AutoDance());	
						break;        
						
						case BodyState.eatsitting:   
						StartCoroutine(AutoEatsitting());
						break;        
						
						case BodyState.idle:   
						StartCoroutine(AutoIdle());	
						break;        
						
						case BodyState.listen:   
						StartCoroutine(AutoListen());
						break;        
						
						case BodyState.manipulate:   
						StartCoroutine(AutoManipulate());
						break;        
						
						case BodyState.sitidle:   
						StartCoroutine(AutoSitidle());
						break;        
						
						case BodyState.talk:   
						StartCoroutine(AutoTalk());
						break;


						
						default :
						break;
					}
		
		
		
				}
				else
				{
//					print("444");
					GetComponent<UnityEngine.AI.NavMeshAgent>().SetDestination(RandomPoint(SearchPointGroup));
//					searchedAreaValue=(int)(targetPoint.GetComponent<HumanSearchPoint>().pointType);					
				}
			}

									
			yield return new WaitForSeconds(0.2f);	
			
		}	
				
	}

	
	
	IEnumerator AutoClaphands()
	{
		GetComponent<Animation>().CrossFade("claphands");
		yield return new WaitForSeconds(5f);
		ResetState();
		
	}
	
	
	IEnumerator AutoDance()
	{
		GetComponent<Animation>().CrossFade("dance");
		yield return new WaitForSeconds(5f);
		ResetState();
	}
		
	
	IEnumerator AutoEatsitting()
	{
		transform.position=targetPoint.position;
		transform.eulerAngles=targetPoint.eulerAngles;
		GetComponent<Animation>().CrossFade("eatsitting");
//		print("eatsitting");
		yield return new WaitForSeconds(5f);
		ResetState();


	}	
	
	
	IEnumerator AutoIdle()
	{
		
		GetComponent<Animation>().CrossFade("idle");
		yield return new WaitForSeconds(5f);
		ResetState();


	}		
	
	
	IEnumerator AutoListen()
	{
		GetComponent<Animation>().CrossFade("listen");
		yield return new WaitForSeconds(5f);
		ResetState();
	}	
	
	IEnumerator AutoManipulate()
	{
		GetComponent<Animation>().CrossFade("manipulate");
		yield return new WaitForSeconds(5f);
		ResetState();
	}		
	
	IEnumerator AutoSitidle()
	{
		transform.position=targetPoint.position;
		transform.eulerAngles=targetPoint.eulerAngles;
		GetComponent<Animation>().CrossFade("sitidle");
		yield return new WaitForSeconds(5f);
		ResetState();
	}	
	
	IEnumerator AutoTalk()
	{
		GetComponent<Animation>().CrossFade("talk");
		yield return new WaitForSeconds(5f);
		ResetState();
	}


    IEnumerator AutoFtStart()
    {
        GetComponent<Animation>().CrossFade("Idle");
        yield return new WaitForSeconds(5f);
        ResetState();
    }	



	void ResetState()
	{
        GetComponent<UnityEngine.AI.NavMeshAgent>().isStopped = false;
        targetPoint.GetComponent<HumanSearchPoint>().beUsed=false;
		StartCoroutine(AutoMove());
	}
		
	
}
