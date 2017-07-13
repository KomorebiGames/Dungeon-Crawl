using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Weapon controller.
/// </summary>
public class WeaponController : MonoBehaviour {

	/// <summary>
	/// The collision mask.
	/// </summary>
	public LayerMask collisionMask;

	/// <summary>
	/// The attack animation.
	/// </summary>
	Animation anim;
	/// <summary>
	/// <c>true</c> if attack animation is currently playing; otherwise, <c>false</c>.
	/// </summary>
	bool attacking;
	/// <summary>
	/// <c>true</c> if weapon has already done damage during the current animation; otherwise, <c>false</c>.
	/// </summary>
	bool alreadyHit;
	/// <summary>
	/// The weapon damage
	/// </summary>
	float damage = 1;

	/// <summary>
	/// Called on instance awake.
	/// </summary>
	void Awake() {
		anim = GetComponent<Animation>();
		attacking = false;
	}

	/// <summary>
	/// If not already attacking, start attack animation.
	/// </summary>
	public void Attack() {
		if (!attacking) {
			StartCoroutine(Swing());
		}
	}

	/// <summary>
	/// Called when weapon collider encounters another collider
	/// </summary>
	/// <param name="other">The encountered collider.</param>
	void OnTriggerEnter(Collider other) {
		//
		// If damage has not already been applied this animation and
		// the encountered object is damageable, apply damage
		//
		if (!alreadyHit) {
			IDamageable damageableObject = other.GetComponent<IDamageable>();
			if (damageableObject != null) {
				alreadyHit = true;
				damageableObject.TakeDamage(damage);
			}
		}
	}

	/// <summary>
	/// Coroutine to play the attack animation.
	/// </summary>
	IEnumerator Swing() {
		attacking = true;
		anim.Play();
		while (anim.isPlaying) {
			yield return null;
		}
		alreadyHit = false;
		attacking = false;
	}

}
