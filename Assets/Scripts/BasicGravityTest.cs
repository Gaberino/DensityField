using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BasicGravityTest : MonoBehaviour {

	public float GravitationalConstant;

	public GameObject planetObject;
	public GameObject planetObject2;
	private Rigidbody2D planetRB;
	private Rigidbody2D planet2RB;
	private Rigidbody2D playerRB;
	// Use this for initialization
	void Start () {
		planetRB = planetObject.GetComponent<Rigidbody2D>();
		planet2RB = planetObject2.GetComponent<Rigidbody2D>();
		playerRB = this.GetComponent<Rigidbody2D>();

	}
	
	// Update is called once per frame
	void Update () {
		
	}

	void FixedUpdate(){
		

		playerRB.AddForce(GravityVector((float)planetRB.mass, planetObject.transform.position) + GravityVector((float)planet2RB.mass, planetObject2.transform.position), ForceMode2D.Force);
	}

	Vector3 GravityVector(float mass, Vector3 origin){
		float distance = Vector3.Distance(this.transform.position, origin);
		float gravityForce = ((GravitationalConstant * mass * (float)playerRB.mass) / Mathf.Pow(distance, 2));
		Vector3 normalizedDir = origin - this.transform.position;
		normalizedDir.Normalize();
		Vector3 finalVector = gravityForce * normalizedDir;
		return finalVector;
	}
}
