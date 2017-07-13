using UnityEngine;

/// <summary>
/// Interface to be implemented by anything damageable.
/// </summary>
public interface IDamageable {

	/// <summary>
	/// Causes the object to take damage from a certain direction.
	/// </summary>
	/// <param name="damage">Damage amount.</param>
	/// <param name="hit">Hit direction.</param>
	void TakeHit(float damage, RaycastHit hit);
	/// <summary>
	/// Causes the object to take damage.
	/// </summary>
	/// <param name="damage">Damage amount.</param>
	void TakeDamage(float damage);

}
