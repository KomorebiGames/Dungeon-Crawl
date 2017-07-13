using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Map generator. Does not generate map mesh. See <see cref="MeshGenerator"/>. 
/// </summary>
public class MapGenerator : MonoBehaviour {

	/// <summary>
	/// The map random fill percent.
	/// </summary>
	/// <remarks>
	/// Value ranges from 0 to 100.
	/// </remarks>
	[Range(0, 100)]
	public int randomFillPercent;
	/// <summary>
	/// The map width.
	/// </summary>
	public int width;
	/// <summary>
	/// The map height.
	/// </summary>
	public int height;
	/// <summary>
	/// The map random seed.
	/// </summary>
	public string seed;
	/// <summary>
	/// <c>true</c> if using a random seed; <c>false</c> otherwise.>.
	/// </summary>
	public bool useRandomSeed;
	/// <summary>
	/// The spawner.
	/// </summary>
	public Spawner spawner;

	/// <summary>
	/// The map.
	/// </summary>
	int[,] map;
	/// <summary>
	/// The room list.
	/// </summary>
	List<Room> roomList;

	/// <summary>
	/// Start this instance.
	/// </summary>
	void Start() {
		//
		// Generate initial map. Add GenerateMap method to NewLevel event to remake map every level.
		//
		GenerateMap();
		spawner.NewLevel += GenerateMap;
	}

	/// <summary>
	/// Generates the map with floor and wall meshes, the player, and enemies.
	/// </summary>
	void GenerateMap() {
		map = new int[width, height];

		//
		// Fill map with 1's and 0's
		//
		RandomFillMap();

		//
		// Smooth map to form cohesive, "cavelike" patterns
		//
		for (int i = 0 ; i < 5; i++) {
			SmoothMap();
		}

		//
		// Remove small rooms and obstacles. Get a list of the major rooms. Connect major rooms.
		//
		roomList = ProcessMap ();

		//
		// Put a walled border around map. Ensures wall mesh method will work.
		//
		int borderSize = 1;
		int[,] borderedMap = new int[width + borderSize * 2, height + borderSize * 2];

		for (int x = 0; x < borderedMap.GetLength(0); x ++) {
			for (int y = 0; y < borderedMap.GetLength(1); y ++) {
				if (x >= borderSize && x < width + borderSize && y >= borderSize && y < height + borderSize) {
					borderedMap[x, y] = map[x - borderSize, y - borderSize];
				}
				else {
					borderedMap[x, y] = 0;
				}
			}
		}

		//
		// Generate the 3D mesh for the map
		//
		MeshGenerator meshGen = GetComponent<MeshGenerator>();
		meshGen.GenerateMesh(map, 1);

		//
		// Spawn the entities
		//
		spawner.SpawnPlayer(roomList);
		spawner.SpawnEnemies(roomList);
	}

	/// <summary>
	/// Removes small wall and room regions, creates a list of surviving rooms,
	/// and connects all rooms together.
	/// </summary>
	/// <returns>List of surviving rooms.</returns>
	List<Room> ProcessMap() {
		//
		// Find all obstacle regions
		//
		List<List<Coord>> wallRegions = GetRegions (0);
		int wallThresholdSize = 50;

		//
		// Remove obstacle regions below threshold size
		//
		foreach (List<Coord> wallRegion in wallRegions) {
			if (wallRegion.Count < wallThresholdSize) {
				foreach (Coord tile in wallRegion) {
					map[tile.tileX,tile.tileY] = 1;
				}
			}
		}

		//
		// Find all open regions
		//
		List<List<Coord>> roomRegions = GetRegions (1);
		int roomThresholdSize = 50;
		List<Room> survivingRooms = new List<Room> ();

		//
		// Remove open regions below threshold size
		//
		foreach (List<Coord> roomRegion in roomRegions) {
			if (roomRegion.Count < roomThresholdSize) {
				foreach (Coord tile in roomRegion) {
					map[tile.tileX,tile.tileY] = 0;
				}
			}
			else {
				survivingRooms.Add(new Room(roomRegion, map));
			}
		}

		//
		// Use list of surviving rooms to mark smallest room as main for player to spawn in.
		//
		survivingRooms.Sort();
		survivingRooms.Reverse();
		survivingRooms [0].isMainRoom = true;
		survivingRooms [0].isAccessibleFromMainRoom = true;

		//
		// Connect all of the rooms
		//
		ConnectClosestRooms (survivingRooms);

		return survivingRooms;
	}

	/// <summary>
	/// Connect all rooms using the shortest paths possible.
	/// </summary>
	/// <param name="allRooms">List of all rooms.</param>
	/// <param name="forceAccessibilityFromMainRoom">If set to <c>true</c> force accessibility from main room.</param>
	void ConnectClosestRooms(List<Room> allRooms, bool forceAccessibilityFromMainRoom = false) {
		List<Room> roomListA = new List<Room> ();
		List<Room> roomListB = new List<Room> ();

		//
		// If forcing access from main room, list all rooms already accesible to the main room
		// so that the remaining rooms can find their closest room that is connected has access
		// to the main room
		//
		if (forceAccessibilityFromMainRoom) {
			foreach (Room room in allRooms) {
				if (room.isAccessibleFromMainRoom) {
					roomListB.Add (room);
				} 
				else {
					roomListA.Add (room);
				}
			}
		} 

		//
		// Or else just connect to the closest room
		//
		else {
			roomListA = allRooms;
			roomListB = allRooms;
		}

		int bestDistance = 0;
		Coord bestTileA = new Coord();
		Coord bestTileB = new Coord();
		Room bestRoomA = new Room();
		Room bestRoomB = new Room();
		bool possibleConnectionFound = false;

		foreach (Room roomA in roomListA) {
			//
			// If not forcing access from main room, check to see if room already has a connection
			//
			if (!forceAccessibilityFromMainRoom) {
				possibleConnectionFound = false;
				if (roomA.connectedRooms.Count > 0) {
					continue;
				}
			}

			foreach (Room roomB in roomListB) {
				//
				// If comparing the same room or if the rooms are already connected, skip
				//
				if (roomA == roomB || roomA.IsConnected(roomB)) {
					continue;
				}

				//
				// Check edge tiles to find the closest pair for the rooms. This is the shortest distance between them.
				//
				for (int tileIndexA = 0; tileIndexA < roomA.edgeTiles.Count; tileIndexA++) {
					for (int tileIndexB = 0; tileIndexB < roomB.edgeTiles.Count; tileIndexB++) {
						Coord tileA = roomA.edgeTiles[tileIndexA];
						Coord tileB = roomB.edgeTiles[tileIndexB];
						int distanceBetweenRooms = (int)(Mathf.Pow(tileA.tileX - tileB.tileX, 2) + Mathf.Pow(tileA.tileY - tileB.tileY, 2));

						//
						// If no possible connections have been found, or the new distance is smaller than
						// the previous, save the rooms and the tiles in the rooms to connect later.
						//
						if (distanceBetweenRooms < bestDistance || !possibleConnectionFound) {
							bestDistance = distanceBetweenRooms;
							possibleConnectionFound = true;
							bestTileA = tileA;
							bestTileB = tileB;
							bestRoomA = roomA;
							bestRoomB = roomB;
						}
					}
				}
			}
			//
			// If not forcing access from the main room and a connection was found, make the connection
			//
			if (possibleConnectionFound && !forceAccessibilityFromMainRoom) {
				CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			}
		}

		//
		// If forcing access from the main room and a connection was found, make the best connection
		// and restart algorithm with newly connected rooms.
		//
		if (possibleConnectionFound && forceAccessibilityFromMainRoom) {
			CreatePassage(bestRoomA, bestRoomB, bestTileA, bestTileB);
			ConnectClosestRooms(allRooms, true);
		}

		//
		// After an initial run without forcing access from main room, run again with forcing
		//
		if (!forceAccessibilityFromMainRoom) {
			ConnectClosestRooms(allRooms, true);
		}
	}

	/// <summary>
	/// Creates a passage between room a and room b.
	/// </summary>
	/// <param name="roomA">Room a.</param>
	/// <param name="roomB">Room b.</param>
	/// <param name="tileA">Tile a.</param>
	/// <param name="tileB">Tile b.</param>
	void CreatePassage(Room roomA, Room roomB, Coord tileA, Coord tileB) {
		Room.ConnectRooms (roomA, roomB);
		List<Coord> line = GetLine (tileA, tileB);

		//
		// Clear a circle around all coordinates on the line
		//
		foreach (Coord c in line) {
			DrawCircle(c,2);
		}
	}

	/// <summary>
	/// Clears a circle around the given coordinate.
	/// </summary>
	/// <param name="c">Coordinate.</param>
	/// <param name="r">Radius.</param>
	void DrawCircle(Coord c, int r) {
		for (int x = -r; x <= r; x++) {
			for (int y = -r; y <= r; y++) {
				if (x*x + y*y <= r*r) {
					int drawX = c.tileX + x;
					int drawY = c.tileY + y;
					if (IsInMapRange(drawX, drawY)) {
						map[drawX,drawY] = 1;
					}
				}
			}
		}
	}

	/// <summary>
	/// Gets the line between the given coordinates.
	/// </summary>
	/// <returns>The line.</returns>
	/// <param name="from">Start coordinate.</param>
	/// <param name="to">End coordinate.</param>
	List<Coord> GetLine(Coord from, Coord to) {
		List<Coord> line = new List<Coord> ();

		int x = from.tileX;
		int y = from.tileY;

		int dx = to.tileX - from.tileX;
		int dy = to.tileY - from.tileY;

		bool inverted = false;
		int step = Math.Sign (dx);
		int gradientStep = Math.Sign (dy);

		int longest = Mathf.Abs (dx);
		int shortest = Mathf.Abs (dy);

		//
		// Change the way the gradient accumulator is calculated based on the size of the differentials
		//
		if (longest < shortest) {
			inverted = true;
			longest = Mathf.Abs(dy);
			shortest = Mathf.Abs(dx);

			step = Math.Sign (dy);
			gradientStep = Math.Sign (dx);
		}

		//
		// In order to only use whole numbers, only increase the smaller differential when the 
		// accumulator has passed a threshold. Increment larger differential by 1 every iteration.
		//
		int gradientAccumulation = longest / 2;
		for (int i = 0; i < longest; i++) {
			line.Add(new Coord(x,y));

			if (inverted) {
				y += step;
			}
			else {
				x += step;
			}

			gradientAccumulation += shortest;
			if (gradientAccumulation >= longest) {
				if (inverted) {
					x += gradientStep;
				}
				else {
					y += gradientStep;
				}
				gradientAccumulation -= longest;
			}
		}

		return line;
	}

	/// <summary>
	/// Converts tile coordinates to world points.
	/// </summary>
	/// <returns>The world point.</returns>
	/// <param name="tile">Tile coordinate.</param>
	public Vector3 CoordToWorldPoint(Coord tile) {
		return new Vector3 (-width / 2 + .5f + tile.tileX, 2, -height / 2 + .5f + tile.tileY);
	}

	/// <summary>
	/// Gets the regions.
	/// </summary>
	/// <returns>The regions.</returns>
	/// <param name="tileType">Tile type.</param>
	List<List<Coord>> GetRegions(int tileType) {
		List<List<Coord>> regions = new List<List<Coord>> ();
		int[,] mapFlags = new int[width,height];

		for (int x = 0; x < width; x ++) {
			for (int y = 0; y < height; y ++) {
				if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
					//
					// Get the accessible tiles to define the new region
					//
					List<Coord> newRegion = GetRegionTiles(x,y);
					regions.Add(newRegion);

					//
					// Mark the tiles in the new region as checked
					//
					foreach (Coord tile in newRegion) {
						mapFlags[tile.tileX, tile.tileY] = 1;
					}
				}
			}
		}
		return regions;
	}

	/// <summary>
	/// Gets the region tiles.
	/// </summary>
	/// <returns>The region tiles.</returns>
	/// <param name="startX">Start x.</param>
	/// <param name="startY">Start y.</param>
	List<Coord> GetRegionTiles(int startX, int startY) {
		List<Coord> tiles = new List<Coord> ();
		int[,] mapFlags = new int[width,height];
		int tileType = map [startX, startY];

		//
		// Start the algorithm by adding the starting coord and marking it as checked
		//
		Queue<Coord> queue = new Queue<Coord> ();
		queue.Enqueue (new Coord (startX, startY));
		mapFlags [startX, startY] = 1;

		//
		// Loop through tiles until queue is empty
		//
		while (queue.Count > 0) {
			Coord tile = queue.Dequeue();
			tiles.Add(tile);

			for (int x = tile.tileX - 1; x <= tile.tileX + 1; x++) {
				for (int y = tile.tileY - 1; y <= tile.tileY + 1; y++) {
					if (IsInMapRange(x,y) && (y == tile.tileY || x == tile.tileX)) {
						if (mapFlags[x,y] == 0 && map[x,y] == tileType) {
							//
							// Mark as checked and add to queue
							//
							mapFlags[x,y] = 1;
							queue.Enqueue(new Coord(x,y));
						}
					}
				}
			}
		}
		return tiles;
	}

	/// <summary>
	/// Determines whether this coordinate is in the map range.
	/// </summary>
	/// <returns><c>true</c> if this coordinate is in the map range; otherwise, <c>false</c>.</returns>
	/// <param name="x">The x coordinate.</param>
	/// <param name="y">The y coordinate.</param>
	bool IsInMapRange(int x, int y) {
		return x >= 0 && x < width && y >= 0 && y < height;
	}

	/// <summary>
	/// Randomly fills the map.
	/// </summary>
	void RandomFillMap() {
		if(useRandomSeed) {
			//
			// Use the current real time as the seed
			//
			seed = ((int)System.DateTime.Now.Ticks).ToString();
		}

		System.Random psuedoRandom = new System.Random(seed.GetHashCode());
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				//
				// If a boundary tile, make a wall, else fill randomly
				//
				if (x == 0 || x == width - 1 || y == 0 || y == height - 1) {
					map[x, y] = 0;
				} else {
					map[x, y] = (psuedoRandom.Next(0, 100) < randomFillPercent) ? 0 : 1;
				}
			}
		}
	}

	/// <summary>
	/// Smooths the map using a cellular automata algorithm.
	/// </summary>
	void SmoothMap() {
		for (int x = 0; x < width; x++) {
			for (int y = 0; y < height; y++) {
				//
				// Number of neighboring wall tiles determines if it "lives" or "dies"
				//
				int neighborWallTiles = GetSurroundingWallCount(x, y);
				if(neighborWallTiles < 4) {
					map[x, y] = 1;
				}
				else if (neighborWallTiles > 4) {
					map[x, y] = 0;
				}
			}
		}
	}

	/// <summary>
	/// Gets the surrounding wall count.
	/// </summary>
	/// <returns>The surrounding wall count.</returns>
	/// <param name="gridX">Grid x.</param>
	/// <param name="gridY">Grid y.</param>
	int GetSurroundingWallCount(int gridX, int gridY) {
		int wallCount = 0;
		for ( int neighborX = gridX - 1; neighborX <= gridX + 1; neighborX++) {
			for (int neighborY = gridY - 1; neighborY <= gridY + 1; neighborY++) {
				//
				// If the neighbor is within the map boundaries, is not the spot in question, 
				// and is a wall, add to the wall count
				//
				if (IsInMapRange(neighborX, neighborY)) {
					if (neighborX != gridX || neighborY != gridY) {
						wallCount += (map[neighborX, neighborY] == 1) ? 0 : 1;
					}
				}
				else {
					wallCount++;
				}
			}
		}
		return wallCount;
	}

	/// <summary>
	/// Tile coordinate.
	/// </summary>
	public struct Coord {
		public int tileX;
		public int tileY;

		/// <summary>
		/// Initializes a new instance of the <see cref="MapGenerator+Coord"/> struct.
		/// </summary>
		/// <param name="x">The x coordinate.</param>
		/// <param name="y">The y coordinate.</param>
		public Coord(int x, int y) {
			tileX = x;
			tileY = y;
		}
	}
		
	/// <summary>
	/// Class representing an open region on the map.
	/// </summary>
	public class Room : IComparable<Room> {
		public List<Coord> tiles;
		public List<Coord> edgeTiles;
		public List<Room> connectedRooms;
		public int roomSize;
		public bool isAccessibleFromMainRoom;
		public bool isMainRoom;

		/// <summary>
		/// Initializes a new instance of the <see cref="MapGenerator+Room"/> class.
		/// </summary>
		public Room() {
			
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MapGenerator+Room"/> class.
		/// </summary>
		/// <param name="roomTiles">Room tiles.</param>
		/// <param name="map">Map.</param>
		public Room(List<Coord> roomTiles, int[,] map) {
			tiles = roomTiles;
			roomSize = tiles.Count;
			connectedRooms = new List<Room>();

			edgeTiles = new List<Coord>();
			foreach (Coord tile in tiles) {
				for (int x = tile.tileX-1; x <= tile.tileX+1; x++) {
					for (int y = tile.tileY-1; y <= tile.tileY+1; y++) {
						if (x == tile.tileX || y == tile.tileY) {
							if (x >= 0 && x < 128 && y >= 0 && y < 80 && map[x,y] == 0) {
								edgeTiles.Add(tile);
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Sets the accessible from main room.
		/// </summary>
		public void SetAccessibleFromMainRoom() {
			if (!isAccessibleFromMainRoom) {
				isAccessibleFromMainRoom = true;
				foreach (Room connectedRoom in connectedRooms) {
					connectedRoom.SetAccessibleFromMainRoom();
				}
			}
		}

		/// <summary>
		/// Connects the rooms.
		/// </summary>
		/// <param name="roomA">Room a.</param>
		/// <param name="roomB">Room b.</param>
		public static void ConnectRooms(Room roomA, Room roomB) {
			if (roomA.isAccessibleFromMainRoom) {
				roomB.SetAccessibleFromMainRoom ();
			} else if (roomB.isAccessibleFromMainRoom) {
				roomA.SetAccessibleFromMainRoom();
			}
			roomA.connectedRooms.Add (roomB);
			roomB.connectedRooms.Add (roomA);
		}

		/// <summary>
		/// Determines whether this instance is connected to the specified otherRoom.
		/// </summary>
		/// <returns><c>true</c> if this instance is connected to the specified otherRoom; otherwise, <c>false</c>.</returns>
		/// <param name="otherRoom">Other room.</param>
		public bool IsConnected(Room otherRoom) {
			return connectedRooms.Contains(otherRoom);
		}

		/// <summary>
		/// Compares the current object with another object of the same type.
		/// </summary>
		/// <returns>int representing the comparison.</returns>
		/// <param name="otherRoom">Other room.</param>
		public int CompareTo(Room otherRoom) {
			return otherRoom.roomSize.CompareTo (roomSize);
		}
	}

}
