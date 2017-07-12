using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponController : MonoBehaviour {

	public LayerMask collisionMask;

	Animation anim;
	bool attacking;
	bool alreadyHit;
	float damage = 1;

	void Awake() {
		anim = GetComponent<Animation>();
		attacking = false;
	}

	public void Attack() {
		if (!attacking) {
			StartCoroutine(Attack(0.3f, 80));
		}
	}

	void OnTriggerEnter(Collider other) {

		Debug.Log("hit");
		if (!alreadyHit) {
			IDamageable damageableObject = other.GetComponent<IDamageable>();

			if (damageableObject != null) {
				alreadyHit = true;
				damageableObject.TakeDamage(damage);
			}
		}

	}

	IEnumerator Attack(float duration, float distance) {

		float percent = 0;

		attacking = true;

		while (percent <= 1) {
			percent += Time.deltaTime / duration;
			if(!anim.isPlaying) {
				anim.Play();
			}
			yield return null;
		}
		alreadyHit = false;
		attacking = false;

	}

}
