using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planetoid : MonoBehaviour {

	public int sizeClass;
	public float moleculeDensity; //baseline density of a planetoid which is exponentially increased by the size to form initial mass
	public float PlanetoidMass; //mass of planet
	public bool IsStartingPlanet = false;
	public  Vector3 PlanetoidCoords;


//	public Planetoid (int inputSize, float inputMoleculeDensity){
//		sizeClass = inputSize;
//		moleculeDensity = inputMoleculeDensity;
//		PlanetoidMass = Mathf.Pow (moleculeDensity, sizeClass);
//	}
}
