using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpaceBroPlayer : MonoBehaviour {

	public static SpaceBroPlayer Instance;
	public int DensityChipsHeld = 0;



	// Use this for initialization
	void Start () {
		if (Instance == null) {
			Instance = this;
		}
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
