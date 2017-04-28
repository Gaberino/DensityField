using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class MapGenerator : MonoBehaviour {

	public int mapWidth; //how wide our generation map is
	public int mapHeight; //how tall it is
	public int smoothPasses = 1; //how many times we should execute the smoothing function. We probably want it to be zero, but maybe increase depending on size?

	public string seed; //what we generate the level from
	public bool useRandomSeed; //whether or not the seed should be random. Do this if seed is left empty in menu

	[Range(0,100)]
	public int randomFillPercent; //how much space should be occupied by matter in the level

	int[,] map; //our base generation map
	bool[,] mapChecks; //our comparative map for checking of things when we convert to the planet style map
	int[,] planetPassMap; //the planet pass map

	Queue<Vector2> coordsInCheckChain; //the queue we use to perform adjacency checks when transferring from the base map to the planet map

	void Start() {
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
	}

	void Update() {
		if (Input.GetKeyDown(KeyCode.Space)) { //debug stuff for testing generation with random seed on
			SmoothMap();
		}
		else if (Input.GetMouseButtonDown(0)){
			GenerateMap();
			GeneratePlanetPass();
		}
	}

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
		foreach (var item in planetPassMap){
			Debug.Log(item);
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
		Debug.Log("I'm fucking right here you pompous ass");
		planetGridCoord = planetGridCoord / planetSize; //average the grid positions
		planetGridCoord = new Vector2 (Mathf.RoundToInt(planetGridCoord.x), Mathf.RoundToInt(planetGridCoord.y));
		PlacePlanet(planetGridCoord, planetSize);

	}

	void CheckAdjacency(int x, int y){ //need to add diagonal checks?
		if (x < mapWidth - 1){
			if (CheckCoord(x + 1, y)){
				coordsInCheckChain.Enqueue(new Vector2(x + 1, y));
			}
		}
		if (x > 0){
			if (CheckCoord(x - 1, y)){
				coordsInCheckChain.Enqueue(new Vector2(x - 1, y));
			}
		}
		if (y < mapHeight - 1){
			if (CheckCoord(x, y + 1)){
				coordsInCheckChain.Enqueue(new Vector2(x, y + 1));
			}
		}
		if (y > 0){
			if (CheckCoord(x, y - 1)){
				coordsInCheckChain.Enqueue(new Vector2(x, y - 1));
			}
		}
	}

	void PlacePlanet (Vector2 gridSpace, int size){ //populates planet grid map data and places vanilla planets without size
		planetPassMap[(int)gridSpace.x, (int)gridSpace.y] = size;
		//create the planet below
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


	void OnDrawGizmos() {
		if (map != null) {
			for (int x = 0; x < mapWidth; x ++) {
				for (int y = 0; y < mapHeight; y ++) {
					Gizmos.color = (map[x,y] == 1)?Color.black:Color.white;
					Vector3 pos = new Vector3(-mapWidth/2 + x + .5f,-mapHeight/2 + y+.5f, 0);
					Gizmos.DrawCube(pos,Vector3.one);
				}
			}
			for (int x = 0; x < mapWidth; x ++) {
				for (int y = 0; y < mapHeight; y ++) {
					Gizmos.color = (planetPassMap[x,y] > 1)?Color.black:Color.white;
					Vector3 pos = new Vector3(-mapWidth/2 + x + .5f,-mapHeight/2 + y+.5f, 0) * 10;
					Gizmos.DrawSphere(pos, planetPassMap[x,y]);
				}
			}
		}
	}

}
