using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class representing anything that has health and can die. 
/// Implements the <see cref="IDamageable"/> interface.
/// </summary>
public class LivingEntity : MonoBehaviour, IDamageable {

	/// <summary>
	/// Occurs on death.
	/// </summary>
	public event System.Action OnDeath;
	/// <summary>
	/// The starting health.
	/// </summary>
	public float startingHealth;
	/// <summary>
	/// Gets or sets the health.
	/// </summary>
	/// <value>The health.</value>
	public float health { get; protected set; }

	/// <summary>
	/// <c>true</c> if entity is dead; <c>false</c> otherwise.
	/// </summary>
	protected bool dead;

	/// <summary>
	/// Start this instance.
	/// </summary>
	protected virtual void Start() {
		health = startingHealth;
	}

	/// <summary>
	/// Causes the entity to take damage and a hit from a certain direction.
	/// </summary>
	/// <param name="damage">Damage amount.</param>
	/// <param name="hit">Hit direction.</param>
	public virtual void TakeHit(float damage, RaycastHit hit) {
		TakeDamage (damage);
	}

	/// <summary>
	/// Causes the entity to lose health.
	/// </summary>
	/// <param name="damage">Damage amount.</param>
	public virtual void TakeDamage(float damage) {
		health -= damage;

		if (health <= 0) {
			Die();
		}
	}

	/// <summary>
	/// Raise the <c>OnDeath</c> event and destroy the instance.
	/// </summary>
	protected virtual void Die() {
		dead = true;
		if (OnDeath != null) OnDeath();
		GameObject.Destroy(gameObject);
	}

}
