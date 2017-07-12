using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LivingEntity : MonoBehaviour, IDamageable {

	public event System.Action OnDeath;
	public float startingHealth;
	public float health { get; protected set; }

	protected bool dead;

	protected virtual void Start () {
		health = startingHealth;
	}

	public virtual void TakeHit(float damage, RaycastHit hit) {
		TakeDamage (damage);
	}

	public virtual void TakeDamage(float damage) {
		health -= damage;

		if (health <= 0) {
			Die();
		}
	}

	protected virtual void Die() {
		dead = true;
		if (OnDeath != null) OnDeath ();
		GameObject.Destroy(gameObject);
	}
}
