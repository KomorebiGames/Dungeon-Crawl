using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Compass : MonoBehaviour {

	/// <summary>
	/// The player transform.
	/// </summary>
	public Transform playerTransform;
	
	/// <summary>
	/// Rotates the compass based on the player's rotation.
	/// </summary>
	void Update () {
		Vector3 playerRotation = playerTransform.rotation.eulerAngles;
		Vector3 oldRotation = gameObject.transform.rotation.eulerAngles;
		float rotateAngle = playerRotation.y - oldRotation.z;
		gameObject.transform.Rotate(new Vector3(0, 0, rotateAngle));
	}
}
