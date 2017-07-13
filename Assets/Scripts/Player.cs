using System.Collections;
using UnityEngine;

/// <summary>
/// Gets input for player.
/// </summary>
public class Player : LivingEntity {

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
	}

	/// <summary>
	/// Call <see cref="LivingEntity"/> method <c>Die()</c>.
	/// </summary>
	protected override void Die() {
		base.Die ();
	}

}
