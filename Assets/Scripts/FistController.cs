using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FistController : MonoBehaviour {

	public AnimationCurve punchCurve;
	public LayerMask collisionMask;

	bool punching;
	bool alreadyHit;
	float damage = 1;

	void Awake() {
		punching = false;
	}

	public void Attack() {
		if (!punching) {
			StartCoroutine(Punch(0.2f, 0.5f));
		}
	}

	void CheckCollisions(float moveDistance) {
		
		Ray ray = new Ray(transform.position, transform.forward);
		RaycastHit hit;

		if (Physics.Raycast(ray, out hit, moveDistance, collisionMask, QueryTriggerInteraction.Collide)) {
			OnHitObject(hit);
		}

	}

	void OnHitObject(RaycastHit hit) {

		IDamageable damageableObject = hit.collider.GetComponent<IDamageable>();

		if(damageableObject != null) {
			alreadyHit = true;
			damageableObject.TakeDamage(damage);
		}
		alreadyHit = true;

	}

	IEnumerator Punch(float duration, float distance) {

		Vector3 nextPosition;
		Vector3 originalPosition = transform.localPosition;
		Vector3 attackPosition = originalPosition + distance * Vector3.forward;
		float percent = 0;

		punching = true;

		while (percent <= 1) {
			percent += Time.deltaTime / duration;
			float interpolation = (-percent * percent + percent) * 4;
			nextPosition = Vector3.Lerp(originalPosition, attackPosition, interpolation);
			if (!alreadyHit) {
				CheckCollisions((nextPosition - originalPosition).magnitude);
			}
			transform.localPosition = nextPosition;

			yield return null;
		}
		transform.localPosition = originalPosition;
		alreadyHit = false;
		punching = false;

	}

}
