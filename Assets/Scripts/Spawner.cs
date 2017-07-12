using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

	public GameObject[] walls;
	public Player player;
	public Enemy enemy;
	public MapGenerator mapGen;
	public event System.Action NewLevel;

	int enemiesRemaining;

	void OnEnemyDeath() {
		enemiesRemaining--;
		if (enemiesRemaining == 0) {
			NewLevel();
		}
	}

	public void SpawnPlayer(List<MapGenerator.Room> rooms) {
		if (GameObject.FindGameObjectWithTag("Player") != null) {
			foreach (MapGenerator.Room room in rooms) {
				if (room.isMainRoom) {
					System.Random psuedoRand = new System.Random(1234);
					Vector3 position = mapGen.CoordToWorldPoint(room.tiles[psuedoRand.Next(room.tiles.Count)]);
					position.y = 1;
					GameObject.FindGameObjectWithTag("Player").transform.position = position;
				}
			}
		}
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

	public void SpawnEnemies(List<MapGenerator.Room> rooms) {
		System.Random psuedoRand = new System.Random(1234);
		foreach (MapGenerator.Room room in rooms) {
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
