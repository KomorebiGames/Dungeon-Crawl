using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawner for randomly placed gameobjects on the map.
/// </summary>
public class Spawner : MonoBehaviour {

	/// <summary>
	/// Player prefab.
	/// </summary>
	public Player player;
	/// <summary>
	/// Enemy prefab.
	/// </summary>
	public Enemy enemy;
	/// <summary>
	/// Map generator instance.
	/// </summary>
	public MapGenerator mapGen;
	/// <summary>
	/// Occurs when a new level starts.
	/// </summary>
	public event System.Action NewLevel;

	/// <summary>
	/// The number of enemies still alive.
	/// </summary>
	int enemiesRemaining;

	/// <summary>
	/// Called when an enemy instance dies.
	/// </summary>
	void OnEnemyDeath() {
		enemiesRemaining--;
		if (enemiesRemaining == 0) {
			NewLevel();
		}
	}

	/// <summary>
	/// Spawns the player.
	/// </summary>
	/// <param name="rooms">Rooms.</param>
	public void SpawnPlayer(List<MapGenerator.Room> rooms) {
		//
		// If the player already exists, move to new spawn point
		//
		if (GameObject.FindGameObjectWithTag("Player") != null) {
			foreach (MapGenerator.Room room in rooms) {
				//
				// Spawn the player at a random location within the main (smallest) room
				//
				if (room.isMainRoom) {
					System.Random psuedoRand = new System.Random(1234);
					Vector3 position = mapGen.CoordToWorldPoint(room.tiles[psuedoRand.Next(room.tiles.Count)]);
					position.y = 1;
					GameObject.FindGameObjectWithTag("Player").transform.position = position;
				}
			}
		}
		//
		// Else create new instance
		//
		else {
			foreach (MapGenerator.Room room in rooms) {
				if (room.isMainRoom) {
					System.Random psuedoRand = new System.Random(1234);
					Vector3 position = mapGen.CoordToWorldPoint(room.tiles[psuedoRand.Next(room.tiles.Count)]);
					position.y = 1;
					Instantiate(player, position, Quaternion.identity);
				}
			}
		}
	}

	/// <summary>
	/// Spawns the enemies.
	/// </summary>
	/// <param name="rooms">Rooms.</param>
	public void SpawnEnemies(List<MapGenerator.Room> rooms) {
		System.Random psuedoRand = new System.Random(1234);
		foreach (MapGenerator.Room room in rooms) {
			//
			// In all rooms other than the main room, spawn enemies in random locations. The number of enemies
			// scales with the size of the room.
			//
			if (!room.isMainRoom) {
				for (int enemyCount = 0; enemyCount < Math.Ceiling(room.roomSize / 100.0f); enemyCount++) {
					Vector3 position = mapGen.CoordToWorldPoint(room.tiles[psuedoRand.Next(room.tiles.Count)]);
					position.y = 1;
					Enemy newEnemy = Instantiate(enemy, position, Quaternion.identity);
					newEnemy.OnDeath += OnEnemyDeath;
					enemiesRemaining++;
				}
			}
		}
	}

}
