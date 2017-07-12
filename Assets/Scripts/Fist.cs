using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fist : MonoBehaviour {

	FistController fistController;

	void Awake () {
		fistController = GetComponent<FistController>();
	}
	
	void Update() {
		if (Input.GetMouseButtonDown(0)) {
			fistController.Attack();
		}
	}

}
