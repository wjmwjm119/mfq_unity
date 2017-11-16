//2013.4.8



using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController) )]
[AddComponentMenu ("Character/Character Motor")]
//[RequireComponent(typeof(CharacterMotorMovement))]

public class CharacterMotor : MonoBehaviour {
	
	//-----------------------------------------------------------------------------------
	public float maxForwardSpeed = 10.0f;
	public float maxSidewaysSpeed = 10.0f;
	public float maxBackwardsSpeed = 10.0f;

	public AnimationCurve slopeSpeedMultiplier = new AnimationCurve(new Keyframe(-90, 1),new Keyframe(0, 1),new Keyframe(90, 0));

	public float maxGroundAcceleration = 30.0f;
	public float maxAirAcceleration = 20.0f;
	
	public float gravity = 10f;
	public float maxFallSpeed = 20.0f;

	public CollisionFlags collisionFlags; 


	public Vector3 velocity;
    Vector3 directionVector;

	public Vector3 frameVelocity = Vector3.zero;

	public Vector3 hitPoint = Vector3.zero;

	public Vector3 lastHitPoint = new Vector3(Mathf.Infinity, 0, 0);
	//-----------------------------------------------------------------------------------
	
	
	
	public bool canControl = true;
	public bool useFixedUpdate = true;
	public Vector3 inputMoveDirection = Vector3.zero;
	public enum MovementTransferOnJump {
		None, // The jump is not affected by velocity of floor at all.
		InitTransfer, // Jump gets its initial velocity from the floor, then gradualy comes to a stop.
		PermaTransfer, // Jump gets its initial velocity from the floor, and keeps that velocity until landing.
		PermaLocked // Jump is relative to the movement of the last touched floor and will move together with that floor.
	}
	
	public bool grounded = false;
	Vector3 groundNormal = Vector3.zero;
	private Vector3 lastGroundNormal = Vector3.zero;
	private Transform tr;
	private CharacterController controller;
	
	public CharacterMotor motor;
    public Camera childCamera;


	public float firstPersonHorizontal;
	public float firstPersonVertical;
	
	void Awake () {
		motor = GetComponent<CharacterMotor>();
		controller = GetComponent<CharacterController>();
		tr = transform;

        if (childCamera == null)
        {
            string log = "childCamera not set ! childCamera 参数未设置";
            Debug.LogError(log);
            GlobalDebug.Addline(log);
        }

	}
	
	public void UpdateFunction () {




        velocity =ApplyInputVelocityChange(velocity);

        velocity = ApplyGravityAndJumping(velocity);




        Vector3 lastPosition = tr.position;
		
		Vector3 currentMovementOffset = velocity * Time.deltaTime;





        float pushDownOffset = Mathf.Max(controller.stepOffset,new Vector3(currentMovementOffset.x, 0, currentMovementOffset.z).magnitude);
		if (grounded)
			currentMovementOffset -= pushDownOffset * Vector3.up;
	
		groundNormal = Vector3.zero;
		
		collisionFlags = controller.Move (currentMovementOffset);
		
		lastHitPoint = hitPoint;
		lastGroundNormal = groundNormal;
		
		Vector3 oldHVelocity = new Vector3(velocity.x, 0, velocity.z);
		velocity = (tr.position - lastPosition) / Time.deltaTime;
		Vector3 newHVelocity = new Vector3(velocity.x, 0, velocity.z);
	
		if (oldHVelocity == Vector3.zero) {
			velocity = new Vector3(0, velocity.y, 0);
		}
		else {
			float projectedNewVelocity = Vector3.Dot(newHVelocity, oldHVelocity) / oldHVelocity.sqrMagnitude;
			velocity = oldHVelocity * Mathf.Clamp01(projectedNewVelocity) + velocity.y * Vector3.up;
		}
	
		if (grounded && !IsGroundedTest()) {
			grounded = false;
	
			SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
	
			tr.position += pushDownOffset * Vector3.up;
		}
		
		else if (!grounded && IsGroundedTest()) {
			grounded = true;
			
			SendMessage("OnLand", SendMessageOptions.DontRequireReceiver);
		}
	
	}
	
	public void FixedUpdate () {
	
		if (useFixedUpdate)
			UpdateFunction();
	}
	
	void Update ()
    {


        if (childCamera!=null&&childCamera.GetComponent<CameraUniversal>().cameraEnableState)
        {
            directionVector = new Vector3(Input.GetAxis("Horizontal") + firstPersonHorizontal, 0, Input.GetAxis("Vertical") + firstPersonVertical);
        }
        else
        {
            directionVector = Vector3.zero;
        }

        if (directionVector != Vector3.zero)
        {
            float directionLength = directionVector.magnitude;
            directionVector = directionVector / directionLength;
            directionLength = Mathf.Min(1, directionLength);
            directionLength = directionLength * directionLength;
            directionVector = directionVector * directionLength;
        }

        motor.inputMoveDirection = transform.rotation * directionVector;


        if (!useFixedUpdate)
			UpdateFunction();
	}
	
	public Vector3 ApplyInputVelocityChange (Vector3 velocity) {	
		if (!canControl)
			inputMoveDirection = Vector3.zero;
		
		Vector3 desiredVelocity;
		if (grounded && TooSteep()) {
			desiredVelocity = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
		}
		else
			desiredVelocity = GetDesiredHorizontalVelocity();
	
		if (grounded)
			desiredVelocity = AdjustGroundVelocityToNormal(desiredVelocity, groundNormal);
		else
			velocity.y = 0;
		
		float maxVelocityChange = GetMaxAcceleration(grounded) * Time.deltaTime;
		Vector3 velocityChangeVector = (desiredVelocity - velocity);
		if (velocityChangeVector.sqrMagnitude > maxVelocityChange * maxVelocityChange) {
			velocityChangeVector = velocityChangeVector.normalized * maxVelocityChange;
		}
	
		if (grounded || canControl)
			velocity += velocityChangeVector;
		
		if (grounded) {
	
			velocity.y = Mathf.Min(velocity.y, 0);
		}
		
		return velocity;
	}


    Vector3 ApplyGravityAndJumping (Vector3 velocity) {


    if (grounded)
    {
       velocity = new Vector3(velocity.x, Mathf.Min(0, velocity.y) - gravity * Time.deltaTime, velocity.z);
    }
    else
    {
        velocity = new Vector3(velocity.x, velocity.y - gravity * Time.deltaTime*10, velocity.z);

        velocity = new Vector3(velocity.x, Mathf.Max(velocity.y, -maxFallSpeed), velocity.z);
    }
	
	return velocity;
}



	
	void OnControllerColliderHit ( ControllerColliderHit hit ) {
		if (hit.normal.y > 0 && hit.normal.y > groundNormal.y && hit.moveDirection.y < 0) {
			if ((hit.point - lastHitPoint).sqrMagnitude > 0.001 || lastGroundNormal == Vector3.zero)
				groundNormal = hit.normal;
			else
				groundNormal = lastGroundNormal;
				
			hitPoint = hit.point;
			frameVelocity = Vector3.zero;
		}
	}
	
	Vector3 GetDesiredHorizontalVelocity () {
		Vector3 desiredLocalDirection = tr.InverseTransformDirection(inputMoveDirection);
		float maxSpeed = MaxSpeedInDirection(desiredLocalDirection);
		if (grounded) {
			float movementSlopeAngle = Mathf.Asin(velocity.normalized.y)  * Mathf.Rad2Deg;
			maxSpeed *= slopeSpeedMultiplier.Evaluate(movementSlopeAngle);
		}
		return tr.TransformDirection(desiredLocalDirection * maxSpeed);
	}
	
	Vector3 AdjustGroundVelocityToNormal (Vector3 hVelocity,Vector3 groundNormal){
		Vector3 sideways = Vector3.Cross(Vector3.up, hVelocity);
		return Vector3.Cross(sideways, groundNormal).normalized * hVelocity.magnitude;
	}
	
	bool IsGroundedTest () {
		return (groundNormal.y > 0.01);
	}
	
	float GetMaxAcceleration (bool grounded) {
		if (grounded)
			return maxGroundAcceleration;
		else
			return maxAirAcceleration;
	}
	
	float CalculateJumpVerticalSpeed (float targetJumpHeight) {
		return Mathf.Sqrt (2 * targetJumpHeight * gravity);
	}
	
	bool IsTouchingCeiling () {
		return (collisionFlags & CollisionFlags.CollidedAbove) != 0;
	}
	
	bool IsGrounded () {
		return grounded;
	}
	
	bool TooSteep () {
		return (groundNormal.y <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad));
	}
	
	Vector3 GetDirection () {
		return inputMoveDirection;
	}
	
	void SetControllable (bool controllable) {
		canControl = controllable;
	}
	
	float MaxSpeedInDirection (Vector3 desiredMovementDirection ) {
		if (desiredMovementDirection == Vector3.zero)
			return 0;
		else {
			float zAxisEllipseMultiplier = (desiredMovementDirection.z > 0 ? maxForwardSpeed : maxBackwardsSpeed) / maxSidewaysSpeed;
			Vector3 temp = new Vector3(desiredMovementDirection.x, 0, desiredMovementDirection.z / zAxisEllipseMultiplier).normalized;
			float length = new Vector3(temp.x, 0, temp.z * zAxisEllipseMultiplier).magnitude * maxSidewaysSpeed;
			return length;
		}
	}
	
	void SetVelocity (Vector3 veloCity) {
		grounded = false;
		velocity = veloCity;
		frameVelocity = Vector3.zero;
		SendMessage("OnExternalVelocity");
	}

    void OnTriggerEnter(Collider other)
    {
		/*
        if (other.transform.GetComponent<HumanSearchPoint>() != null)
        {
            childCamera.GetComponent<FirstPersonCamera>().StartDianTi(other);
        }

        childCamera.GetComponent<FirstPersonCamera>().GetInRoom(other.name);
*/
//        Debug.Log(other.name);
    }


// Require a character controller to be attached to the same game object
	
}
