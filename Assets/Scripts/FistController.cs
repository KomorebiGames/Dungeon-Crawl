using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Fist controller.
/// </summary>
public class FistController : MonoBehaviour {

	/// <summary>
	/// The collision mask.
	/// </summary>
	public LayerMask collisionMask;

	/// <summary>
	/// <c>true</c> if currently in punching animation; <c>false</c> otherwise.
	/// </summary>
	bool punching;
	/// <summary>
	/// <c>true</c> if punch has already cause damage; <c>false</c> otherwise.
	/// </summary>
	bool alreadyHit;
	/// <summary>
	/// The damage amount when punching.
	/// </summary>
	float damage = 1;

	/// <summary>
	/// Called at instance awake.
	/// </summary>
	void Awake() {
		punching = false;
	}

	/// <summary>
	/// If not already in it, start attack animation.
	/// </summary>
	public void Attack() {
		if (!punching) {
			StartCoroutine(Punch(0.2f, 0.5f));
		}
	}

	/// <summary>
	/// Called when the fist's box collider collides with another object.
	/// </summary>
	/// <param name="other">Collider encountered by the fist.</param>
	void OnTriggerEnter(Collider other) {
		IDamageable damageableObject = other.GetComponent<IDamageable>();

		//
		// If this attack hasn't damaged anything yet and it hit a damageable object, apply damage
		//
		if(!alreadyHit && damageableObject != null) {
			alreadyHit = true;
			damageableObject.TakeDamage(damage);
		}
	}
		
	/// <summary>
	/// Punch for the specified duration and distance.
	/// </summary>
	/// <param name="duration">Duration in sec.</param>
	/// <param name="distance">Distance.</param>
	IEnumerator Punch(float duration, float distance) {

		Vector3 nextPosition;
		Vector3 originalPosition = transform.localPosition;
		Vector3 attackPosition = originalPosition + distance * Vector3.forward;
		float percent = 0;

		punching = true;

		while (percent <= 1) {

			//
			// Perform the animation over the distance within the duration
			//
			percent += Time.deltaTime / duration;
			float interpolation = (-percent * percent + percent) * 4;
			nextPosition = Vector3.Lerp(originalPosition, attackPosition, interpolation);
			transform.localPosition = nextPosition;

			yield return null;
		}

		//
		// Reset variables
		//
		transform.localPosition = originalPosition;
		alreadyHit = false;
		punching = false;
	}

}
