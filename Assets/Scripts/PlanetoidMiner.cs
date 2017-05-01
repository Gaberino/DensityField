using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlanetoidMiner : MonoBehaviour {

	//public int upgradeLevel = 0;
	public float DensityChipsHeld = 0;

	Planetoid targetPlanetoid;
	SpriteRenderer planetSR;

	Color planetoidOrigColor;
	float planetoidOrigMass;
	float planetoidCurrentMass;
	Text infoText;
	public float mineSpeed = 2f;
	float currentTimer = 0f;


	// Use this for initialization
	void Start () {
		planetSR = targetPlanetoid.GetComponent<SpriteRenderer>();
		planetoidOrigColor = planetSR.color;
		infoText = this.transform.GetChild(0).GetComponent<Text>();
	}
	
	// Update is called once per frame
	void Update () {
		planetoidCurrentMass = targetPlanetoid.PlanetoidMass;
	}

	void DrainChips(){
		targetPlanetoid.PlanetoidMass -= 1;
	}
}
