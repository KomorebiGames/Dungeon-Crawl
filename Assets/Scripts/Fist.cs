using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fist weapon.
/// </summary>
public class Fist : MonoBehaviour {

	/// <summary>
	/// The fist controller.
	/// </summary>
	FistController fistController;

	/// <summary>
	/// Called at instance awake.
	/// </summary>
	void Awake () {
		fistController = GetComponent<FistController>();
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() {
		//
		// If left mouse button clicked, attack
		//
		if (Input.GetMouseButtonDown(0)) {
			fistController.Attack();
		}
	}

}
