using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Gets input for player.
/// </summary>
public class Player : LivingEntity {

	public Slider healthBar;

	/// <summary>
	/// The player controller.
	/// </summary>
	PlayerController controller;

	/// <summary>
	/// Start this instance.
	/// </summary>
	protected override void Start() {
		base.Start();
	}

	/// <summary>
	/// Called on instance awake.
	/// </summary>
	void Awake() {
		controller = GetComponent<PlayerController>();
		healthBar.maxValue = startingHealth;
		healthBar.value = startingHealth;
	}

	/// <summary>
	/// Player takes damage and updates health bar UI.
	/// </summary>
	/// <param name="damage">Damage amount.</param>
	public override void TakeDamage(float damage)
	{
		base.TakeDamage(damage);
		healthBar.value = health;
	}

	/// <summary>
	/// Call <see cref="LivingEntity"/> method <c>Die()</c>.
	/// </summary>
	protected override void Die() {
		base.Die ();
	}

}
