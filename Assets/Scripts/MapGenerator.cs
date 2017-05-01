using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour {

	public static MapGenerator Instance;

	public GameObject shopPrefab;
	public Transform preloadedPlayerTransform; //must be preloaded in scene so we can call the tether creation method when designating its position

	public PhysicsMaterial2D rockyMaterial;
	public PhysicsMaterial2D icyMaterial;
	public PhysicsMaterial2D hotMaterial;

	public int rockyRatio; //spawn ratios
	public int icyRatio;
	public int hotRatio;

	public int rockyDensity; //planetoid type density (mass)
	public int icyDensity;
	public int hotDensity;

	public float totalMassInLevel; //total mass in level excluding starting planet, since it cannot be mined
	[Range(0, 100)]
	public float percentageToMine; //a difficulty variable. What percent of the total the mass we should consider the "minimum" to complete the level
	public int minimumMineVariable; //the resulting number
	[Range(0, 100)]
	public float percentageToPricing; //what percent of the minimum mining amount we should call 1 "unit" of price
	public int basePrice; //the resulting number

	public Color hotColor;
	public Color icyColor;
	public Color rockyColor;

	public string hotString;
	public string icyString;
	public string rockyString;

	public int mapWidth; //how wide our generation map is
	public int mapHeight; //how tall it is
	public int smoothPasses = 1; //how many times we should execute the smoothing function. We probably want it to be zero, but maybe increase depending on size/fillpercent?

	public GameObject PlanetoidPrefab;
	public float planetSpacing = 20f; //how far apart spaces in the grid are when spawning planets

	public string seed; //what we generate the level from
	public bool useRandomSeed; //whether or not the seed should be random. Do this if seed is left empty in menu

	[Range(0,100)]
	public int randomFillPercent; //how much space should be occupied by matter in the level

	int[,] map; //our base generation map
	bool[,] mapChecks; //our comparative map for checking of things when we convert to the planet style map
	int[,] planetPassMap; //the planet pass map

	Queue<Vector2> coordsInCheckChain; //the queue we use to perform adjacency checks when transferring from the base map to the planet map

	void Start() {
		Instance = this;
		//initialize containers
		coordsInCheckChain = new Queue<Vector2>();
		mapChecks = new bool[mapWidth,mapHeight];

		//set all items in the comparitive map to false
		for (int x = 0; x < mapWidth; x++){
			for (int y = 0; y < mapHeight; y++){
				mapChecks[x,y] = false;
			}
		}
		GenerateMap(); //base map
		GeneratePlanetPass(); //conversion and placement of game objects
		DesignateStartingPlanet ();
		InitializeRemainingPlanets ();
	}

///	void Update() {
//		if (Input.GetKeyDown(KeyCode.Space)) { //debug stuff for testing generation with random seed on
//			SmoothMap();
//		}
//		else if (Input.GetMouseButtonDown(0)){
//			for (int x = 0; x < mapWidth; x++){
//				for (int y = 0; y < mapHeight; y++){
//					mapChecks[x,y] = false;
//				}
//			}
//			for (int i = 0; i < GameObject.FindGameObjectsWithTag ("Planetoid").Length; i++) {
//				Destroy (GameObject.FindGameObjectsWithTag ("Planetoid")[i]);
//			}
//			GenerateMap();
//			GeneratePlanetPass();
//		}
//	}

	void GenerateMap() {
		map = new int[mapWidth,mapHeight]; //initialize map
		RandomFillMap(); //fill each space with a 1 or zero

		for (int i = 0; i < smoothPasses; i ++) {
			SmoothMap();
		}
	}


	void RandomFillMap() { //fill based on seed
		if (useRandomSeed) { //generate a fresh seed based on date and time
			seed = System.DateTime.Now.ToString();
		}

		System.Random pseudoRandom = new System.Random(seed.GetHashCode()); //generates the random variable necessary for the function below

		for (int x = 0; x < mapWidth; x ++) {
			for (int y = 0; y < mapHeight; y ++) {
				map[x,y] = (pseudoRandom.Next(0,100) < randomFillPercent)? 1: 0; //if the value we get is less than the fill chance, fill
			}
		}
	}

	void GeneratePlanetPass(){
		planetPassMap = new int[mapWidth,mapHeight]; //initialize planetpass map

		for (int x = 0; x < mapWidth; x++){
			for (int y = 0; y < mapHeight; y++){
				if (CheckCoord(x, y)){ //checks the coordinates in order, starting from top left (I think)
					coordsInCheckChain.Enqueue(new Vector2(x,y)); //enqueue the coordinate we are looking at right now
					AdjacencyAndPlacement();
				}
				else { //it either has been checked or is empty, we skip it and say we've seen it
					mapChecks[x,y] = true;
				}
			}
		}
	}

	bool CheckCoord(int x, int y){
		if (!mapChecks[x,y]){ //if it has not been checked off on the boolean comparator map
			if (map[x,y] > 0){ //and if it is not an empty tile
				return true; //return true. Any other case return false so we skip doing any more checks
			}
			else {
				return false;
			}
		}
		else {
			return false;
		}
	}

	void AdjacencyAndPlacement(){
		int planetSize = 0; //the planet we are generating intially has no size because we do at least 1 adjacency check
		Vector2 planetGridCoord = Vector2.zero; //container initialization
		while (coordsInCheckChain.Count > 0){
			
			Vector2 tempVector = coordsInCheckChain.Dequeue();
			mapChecks[(int)tempVector.x, (int)tempVector.y] = true; //we are checking from this coordinate, so it must have substance, and we need to make sure it isn't counted twice
			planetGridCoord += tempVector; //add in all the vectors so we can average them later
			CheckAdjacency((int)tempVector.x, (int)tempVector.y); //do the adjacency check
			planetSize += 1;
		}
		planetGridCoord = planetGridCoord / planetSize; //average the grid positions
		planetGridCoord = new Vector2 (Mathf.RoundToInt(planetGridCoord.x), Mathf.RoundToInt(planetGridCoord.y));
		PlacePlanet(planetGridCoord, planetSize);
	}

	void CheckAdjacency(int x, int y){ //need to add diagonal checks?
		//bools which become true based on initial adjacency checks to enable diagonal checks
		bool canCheckRight = false;
		bool canCheckLeft = false;
		bool canCheckUp = false;
		bool canCheckDown = false;

		if (x < mapWidth - 1){ //not on right border
			if (CheckCoord(x + 1, y)){//check right
				coordsInCheckChain.Enqueue(new Vector2(x + 1, y));
				canCheckRight = true;
			}
		}
		if (x > 0){//not on left border
			if (CheckCoord(x - 1, y)){//check left
				coordsInCheckChain.Enqueue(new Vector2(x - 1, y));
				canCheckLeft = true;
			}
		}
		if (y < mapHeight - 1){//not on top border
			if (CheckCoord(x, y + 1)){//check above
				coordsInCheckChain.Enqueue(new Vector2(x, y + 1));
				canCheckUp = true;
			}
		}
		if (y > 0){//not on bottom border
			if (CheckCoord(x, y - 1)){//check below
				coordsInCheckChain.Enqueue(new Vector2(x, y - 1));
				canCheckDown = true;
			}
		}

		//diagonal checking section
		if (canCheckRight && canCheckUp) {
			if (CheckCoord(x + 1, y + 1)){//check up right
				coordsInCheckChain.Enqueue(new Vector2(x + 1, y + 1));
			}
		}
		if (canCheckRight && canCheckDown) {
			if (CheckCoord(x + 1, y - 1)){//check down right
				coordsInCheckChain.Enqueue(new Vector2(x + 1, y - 1));
			}
		}
		if (canCheckLeft && canCheckUp) {
			if (CheckCoord(x - 1, y + 1)){//check up left
				coordsInCheckChain.Enqueue(new Vector2(x - 1, y + 1));
			}
		}
		if (canCheckLeft && canCheckDown) {
			if (CheckCoord(x - 1, y - 1)){//check down left
				coordsInCheckChain.Enqueue(new Vector2(x - 1, y - 1));
			}
		}
	}

	void PlacePlanet (Vector2 gridSpace, int size){ //populates planet grid map data and instantiates vanilla planets of the correct size
		planetPassMap[(int)gridSpace.x, (int)gridSpace.y] = size;
		//now make the game object
		GameObject newPlanet = (GameObject)Instantiate(PlanetoidPrefab, new Vector3((-mapWidth/2 + gridSpace.x + .5f) * planetSpacing, (-mapHeight/2 + gridSpace.y +.5f) * planetSpacing, 0), Quaternion.identity);
		newPlanet.transform.localScale = new Vector3(size, size, size);
		Planetoid newPlanetData = newPlanet.GetComponent<Planetoid> ();
		newPlanetData.sizeClass = size;
		newPlanetData.PlanetoidCoords = newPlanet.transform.position;
	}

	void SmoothMap() {
				for (int x = 0; x < mapWidth; x ++) {
					for (int y = 0; y < mapHeight; y ++) {
				int filledNeighbourTiles = GetSurroundingFillCount(x,y);

				if (filledNeighbourTiles > 4)
					map[x,y] = 1;
				else if (filledNeighbourTiles < 4)
					map[x,y] = 0;

			}
		}
	}

	int GetSurroundingFillCount(int gridX, int gridY) {
		int filledCount = 0;
		for (int neighbourX = gridX - 1; neighbourX <= gridX + 1; neighbourX ++) {
			for (int neighbourY = gridY - 1; neighbourY <= gridY + 1; neighbourY ++) {
				if (neighbourX >= 0 && neighbourX < mapWidth && neighbourY >= 0 && neighbourY < mapHeight) {
					if (neighbourX != gridX || neighbourY != gridY) {
						filledCount += map[neighbourX,neighbourY];
					}
				}
				else {
					filledCount ++;
				}
			}
		}

		return filledCount;
	}

	void DesignateStartingPlanet(){
		//finds the closest planet to the center, then places the shop and player
		GameObject chosenPlanet = null;
		bool foundPlanet = false;
		float searchRadius = 20f;
		while (!foundPlanet) {
			if (Physics2D.OverlapCircle (Vector2.zero, searchRadius) != null) {
				//found one
				chosenPlanet = Physics2D.OverlapCircle (Vector2.zero, searchRadius).gameObject;
				foundPlanet = true;
			} else {
				//increase search area
				searchRadius += 20f;
			}
		}
		Planetoid chosenPlanetoid = chosenPlanet.GetComponent<Planetoid>();

		if (chosenPlanetoid.sizeClass > 3){
			chosenPlanetoid.sizeClass = 3;
			chosenPlanet.transform.localScale = Vector3.one * 3;
		}

		chosenPlanetoid.IsStartingPlanet = true;
		chosenPlanetoid.moleculeDensity = hotDensity;
		chosenPlanetoid.PlanetoidMass = Mathf.Pow(hotDensity, chosenPlanet.GetComponent<Planetoid>().sizeClass);
		chosenPlanet.name = "StartingPlanetoid";

		//instantiate shop
		GameObject homeShop = (GameObject)Instantiate(shopPrefab, chosenPlanet.transform);
		//homeShop.transform.localPosition = Vector3.zero;
		homeShop.transform.localPosition = Vector3.up * (0.5f + (homeShop.transform.localScale.y / 2)); //in local space, 1 full unit is the whole length of the parent
		//instantiate player at shop location
		preloadedPlayerTransform.position = homeShop.transform.position;
		SpaceBroPlayer.Instance.AddNewTether(chosenPlanetoid, true);
		preloadedPlayerTransform.GetComponent<PolygonCollider2D>().enabled = true;
	}

	void InitializeRemainingPlanets(){ //fills out the molecule density and mass of the remaining planetoids, designates type based on ratio, and 
		int totalRange = (hotRatio + icyRatio + rockyRatio);
		float countingMass = 0; //counts up all mass in level except the starting planet
		System.Random pseudoRandom2 = new System.Random(seed.GetHashCode());
		Planetoid[] allPlanetoids = FindObjectsOfType<Planetoid>();
		for (int i = 0; i < allPlanetoids.Length; i++){
			if (!allPlanetoids[i].IsStartingPlanet){//if it is not the starting planet, randomly set type based on ratios
				int testNum = pseudoRandom2.Next(0, totalRange);
				if (testNum < hotRatio){ //hot planet
					allPlanetoids[i].moleculeDensity = hotDensity;
					allPlanetoids[i].GetComponent<SpriteRenderer>().color = hotColor;
					allPlanetoids[i].GetComponent<Collider2D>().sharedMaterial = hotMaterial;
					allPlanetoids[i].gameObject.name = hotString;
				}
				else if (testNum < icyRatio + hotRatio){ //icy planet
					allPlanetoids[i].moleculeDensity = icyDensity;
					allPlanetoids[i].GetComponent<SpriteRenderer>().color = icyColor;
					allPlanetoids[i].GetComponent<Collider2D>().sharedMaterial = icyMaterial;
					allPlanetoids[i].gameObject.name = icyString;
				}
				else if (testNum < rockyRatio + icyRatio + hotRatio){ //rocky planet
					allPlanetoids[i].moleculeDensity = rockyDensity;
					allPlanetoids[i].GetComponent<SpriteRenderer>().color = rockyColor;
					allPlanetoids[i].GetComponent<Collider2D>().sharedMaterial = rockyMaterial;
					allPlanetoids[i].gameObject.name = rockyString;
				}
			}
			allPlanetoids[i].PlanetoidMass = Mathf.Pow(allPlanetoids[i].moleculeDensity, allPlanetoids[i].sizeClass); //put this outside so we also have mass for starting planet
			countingMass += allPlanetoids[i].PlanetoidMass;
		}
		totalMassInLevel = countingMass;
		minimumMineVariable = Mathf.RoundToInt(totalMassInLevel * (percentageToMine / 100));
		basePrice = Mathf.RoundToInt(minimumMineVariable * (percentageToPricing / 100));
	}

//	void OnDrawGizmos() {
//		if (map != null) {
//			for (int x = 0; x < mapWidth; x ++) {
//				for (int y = 0; y < mapHeight; y ++) {
//					Gizmos.color = (map[x,y] == 1)?Color.black:Color.white;
//					Vector3 pos = new Vector3(-mapWidth/2 + x + .5f,-mapHeight/2 + y+.5f, 0);
//					Gizmos.DrawCube(pos,Vector3.one);
//				}
//			}
////			for (int x = 0; x < mapWidth; x ++) {
////				for (int y = 0; y < mapHeight; y ++) {
////					Gizmos.color = (planetPassMap[x,y] > 1)?Color.black:Color.white;
////					Vector3 pos = new Vector3(-mapWidth/2 + x + .5f,-mapHeight/2 + y+.5f, 0) * 20;
////					Gizmos.DrawSphere(pos, planetPassMap[x,y]);
////				}
////			}
//		}
//	}

}
