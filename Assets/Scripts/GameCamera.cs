using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameCamera : MonoBehaviour { //this is a statemachine
	StateMachine myStateMachine = new StateMachine();
	public int stateNumber = 0; //0 is planet, 1 is free

	public float freeLerpSpeed = 1f;

	public AnimationCurve toPlanetLerpCurve;

	public float initialCamSize;

	public static GameCamera Instance;

	void Start(){
		if (Instance != null){
			Destroy(this);
		}
		else {
			Instance = this;
		}
		initialCamSize = Camera.main.orthographicSize;
	}

	// Update is called once per frame
	void Update () {

		if (SpaceBroPlayer.Instance.currentTouchingPlanetoid && stateNumber != 0){
			myStateMachine.ChangeState(new Camera_PlanetState(this, SpaceBroPlayer.Instance.currentTouchingPlanetoid));
			stateNumber = 0;
		}
		else if (!SpaceBroPlayer.Instance.currentTouchingPlanetoid && stateNumber != 1){
			myStateMachine.ChangeState(new Camera_FreeFollowState(this, freeLerpSpeed));
			stateNumber = 1;
		}
		myStateMachine.ExecuteCurrent();
	}
}


//Statemachine stuff for camera
public class Camera_PlanetState: IState {
	GameCamera ownerClass;
	Transform planetTransform;

	float initialSize;
	float sizeToBecome;
	float lerpTime;

	Transform playerTransform;
	Transform cameraTransform;

	Vector3 initialCamPos;
	Vector3 targetPos;

	AnimationCurve curveToUse;

	public Camera_PlanetState (GameCamera inputOwner, Transform inputTransform){
		ownerClass = inputOwner;
		planetTransform = inputTransform;
	}

	public void Enter() {
		//reveal planet  details, initialize necessary cam size
		cameraTransform = Camera.main.transform;
		playerTransform = SpaceBroPlayer.Instance.transform;

		initialSize = Camera.main.orthographicSize;
		sizeToBecome = GameCamera.Instance.initialCamSize + planetTransform.localScale.x;
		initialCamPos = cameraTransform.position;
		targetPos = new Vector3(planetTransform.position.x, planetTransform.position.y, cameraTransform.position.z);
		curveToUse = GameCamera.Instance.toPlanetLerpCurve;
	}

	public void  Execute() {
		//center camera, widen and rotate

		if (lerpTime < 1f){
			lerpTime += Time.deltaTime;
		}
		else {
			lerpTime = 1f;
		}
		Camera.main.orthographicSize = Mathf.Lerp(initialSize, sizeToBecome, curveToUse.Evaluate(lerpTime));
		cameraTransform.position = Vector3.Lerp(initialCamPos, targetPos, curveToUse.Evaluate(lerpTime));
	}

	public void Exit() {
		//hide planet info, delete this state
	}
}

public class Camera_FreeFollowState : IState {
	GameCamera ownerClass;
	float lerpSpeed;
	Rigidbody2D playersRB;
	Transform playerTransform;
	Transform cameraTransform;


	public Camera_FreeFollowState (GameCamera inputOwner, float inputSpeed){
		ownerClass = inputOwner;
		lerpSpeed = inputSpeed;
	}

	public void Enter() {
		//turn on vector previews and initialize
		cameraTransform = Camera.main.transform;
		playerTransform = SpaceBroPlayer.Instance.transform;
		playersRB = SpaceBroPlayer.Instance.GetComponent<Rigidbody2D>();
	}

	public void  Execute() {
		//orient with player, move ahead of player

		Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, playersRB.velocity.magnitude + GameCamera.Instance.initialCamSize, lerpSpeed * Time.deltaTime); ;
		cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation, playerTransform.rotation, lerpSpeed * Time.deltaTime);
		Vector3 velocityVector = new Vector3(playersRB.velocity.x + playerTransform.position.x, playersRB.velocity.y + playerTransform.position.y, cameraTransform.position.z);

		cameraTransform.position = Vector3.Lerp(cameraTransform.position, velocityVector, lerpSpeed * Time.deltaTime);
	}

	public void Exit() {
		//hide vector previews, delete this state
	}
}