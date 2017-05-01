using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBroPlayer : MonoBehaviour {

	public static SpaceBroPlayer Instance;
	public int DensityChipsHeld = 0;
	public float oxygenDepletionRate = 0.5f;
	public float oxygenInSuit = 100f;
	public bool usingOwnAir = false;
	public int lives = 5;

	public Stack<GameObject> InventoryStack;
	public int inventorySize = 1;

	//public bool hasRecycler = false;

	public Transform currentTouchingPlanetoid;

	public float gravConstant = 3f;
	float originalGravconstant;

	public float gravityIncreaseSpeed = 3f;
//	public float heightenedAngDrag = 2f;
//	float originalAngDrag;
//	public float heightenedLinDrag = 1f;
//	float originalLinDrag;
//	bool reverseGravityNature = false;

	public float maxControllableSpeed = 2f;
	public float surfaceSpeed = 10f;
	public float jumpForce = 5f;
	private bool jumpQueued = false;
	private bool canJump = true;

	public bool canSurfaceMove = true;
	Vector3 surfaceMoveForces = Vector3.zero;

	public List<PlanetoidTether> pTethers;
	public int tetherCapacity = 3; //how many planetoids we can have in the list

	private KeyCode[] numberKeys = {
	KeyCode.Alpha1, 
	KeyCode.Alpha2, 
	KeyCode.Alpha3, 
	KeyCode.Alpha4, 
	KeyCode.Alpha5, 
	KeyCode.Alpha6, 
	KeyCode.Alpha7, 
	KeyCode.Alpha8,
	KeyCode.Alpha9,
	KeyCode.Alpha0
	};

	private Rigidbody2D playerRB;


	// Use this for initialization
	void Start () {
		if (Instance != null) {
			Destroy(this);
		}
		else {
			Instance = this;
		}
		originalGravconstant = gravConstant;
		playerRB = this.GetComponent<Rigidbody2D>();
//		originalAngDrag = playerRB.angularDrag;
//		originalLinDrag = playerRB.drag;
		pTethers = new List<PlanetoidTether>(tetherCapacity);
	}
	
	// Update is called once per frame
	void Update () {
		if (canSurfaceMove){
			surfaceMoveForces = new Vector3 (Input.GetAxis("Horizontal"), 0) * surfaceSpeed;
			if (Input.GetKeyDown(KeyCode.UpArrow)){
				jumpQueued = true;
			}
		}
		for (int i = 0; i < tetherCapacity; i++){
			if (Input.GetKeyDown(numberKeys[i])){
				ToggleTether(i);
			}
		}
		if (Input.GetKey(KeyCode.Space)){
			gravConstant += Time.deltaTime * gravityIncreaseSpeed * playerRB.velocity.magnitude;
//			playerRB.angularDrag = heightenedAngDrag;
//			playerRB.drag = heightenedLinDrag;
		}
		if (Input.GetKeyUp(KeyCode.Space)){
			gravConstant = originalGravconstant;
//			playerRB.angularDrag = originalAngDrag;
//			playerRB.drag = originalLinDrag;
		}
//		if (usingOwnAir){
//			oxygenInSuit -= oxygenDepletionRate * Time.deltaTime;
//			if (oxygenInSuit < 0){
//				//die and lose a life
//			}
//		}
	}

	void FixedUpdate(){
		Vector3 finalMovementVector = Vector3.zero;
		foreach (PlanetoidTether someTether in pTethers){
			if (someTether.tetherState){
				finalMovementVector += GravityVector(someTether.targetPlanetoid.PlanetoidMass, someTether.targetPlanetoid.PlanetoidCoords);
			}
		}
		playerRB.AddForce(finalMovementVector);
		if (playerRB.velocity.magnitude <= maxControllableSpeed){
			playerRB.AddRelativeForce(surfaceMoveForces);
		}


		if (jumpQueued && canJump){
			playerRB.AddRelativeForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
			canJump = false;
			jumpQueued = false;
		}
	}

	void OnCollisionEnter2D(Collision2D col){
		if (col.collider.tag == "Planetoid"){
			canJump = true;
			bool tetherPossessed = false;
			Planetoid testPlanetoid = col.gameObject.GetComponent<Planetoid>();
			foreach (PlanetoidTether testTether in pTethers){
				if (testTether.targetPlanetoid == testPlanetoid){
					tetherPossessed = true;
				}
			}
			if (!tetherPossessed){
				AddNewTether(testPlanetoid, true);
			}
		}
	}

	void OnTriggerEnter2D(Collider2D col){
		if (col.tag == "Planetoid"){
			canSurfaceMove = true;
			currentTouchingPlanetoid = col.transform;
		}
	}

	void OnTriggerExit2D(Collider2D col){
		if (col.tag == "Planetoid"){
			canSurfaceMove = false;
			currentTouchingPlanetoid = null;
		}
	}

	Vector3 GravityVector(float mass, Vector3 origin){
		float distance = Vector3.Distance(this.transform.position, origin);
		float gravityForce = 0f;

		gravityForce = ((gravConstant * mass * playerRB.mass) / Mathf.Pow(distance, 2));

		Vector3 normalizedDir = origin - this.transform.position;
		normalizedDir.Normalize();
		Vector3 finalVector = gravityForce * normalizedDir;
		return finalVector;
	}

	public void AddNewTether(Planetoid planet, bool startingState){
		PlanetoidTether newTether = new PlanetoidTether(planet, startingState);

		pTethers.Insert(0, newTether);

		if (pTethers.Count > tetherCapacity){
			pTethers.RemoveAt(tetherCapacity - 1);
		}
	}

	public void ToggleTether(int tetherIndex){
		pTethers[tetherIndex].tetherState =  !pTethers[tetherIndex].tetherState;
	}
}

public class PlanetoidTether {

	public PlanetoidTether (Planetoid p, bool t) {
		targetPlanetoid = p;
		tetherState = t;
	}

	public Planetoid targetPlanetoid;
	public bool tetherState;
}
