using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Weapon : MonoBehaviour {

	WeaponController weaponController;

	void Awake () {
		weaponController = GetComponent<WeaponController>();
	}

	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			weaponController.Attack();
		}
	}
}
