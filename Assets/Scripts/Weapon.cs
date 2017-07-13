using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Gets input for a weapon.
/// </summary>
public class Weapon : MonoBehaviour {

	/// <summary>
	/// The weapon controller.
	/// </summary>
	WeaponController weaponController;

	/// <summary>
	/// Called on instance awake.
	/// </summary>
	void Awake () {
		weaponController = GetComponent<WeaponController>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			weaponController.Attack();
		}
	}

}
