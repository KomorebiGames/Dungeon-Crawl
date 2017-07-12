using System.Collections;
using UnityEngine;

public class Player : LivingEntity {

	public float moveSpeed = 5;

	PlayerController controller;

	protected override void Start() {
		base.Start();
	}

	void Awake() {
		controller = GetComponent<PlayerController>();
	}

	protected override void Die() {
		base.Die ();
	}

}
