using System.Collections;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Enemy controller.
/// </summary>
public class Enemy : LivingEntity {

	/// <summary>
	/// Enemy state.
	/// </summary>
	/// <remarks>
	/// States are Idle, Chasing, and Attacking.
	/// </remarks>
	public enum State {Idle, Chasing, Attacking};

	/// <summary>
	/// The current state.
	/// </summary>
	State currentState;
	/// <summary>
	/// The navigation path finder.
	/// </summary>
	NavMeshAgent pathfinder;
	/// <summary>
	/// The player.
	/// </summary>
	Transform target;
	/// <summary>
	/// The entity component of the player.
	/// </summary>
	LivingEntity targetEntity;
	/// <summary>
	/// The attack distance threshold.
	/// </summary>
	float attackDistanceThreshold = .5f;
	/// <summary>
	/// The time between attacks.
	/// </summary>
	float timeBetweenAttacks = 1;
	/// <summary>
	/// The attack damage.
	/// </summary>
	float damage = 1;
	/// <summary>
	/// The next attack time.
	/// </summary>
	float nextAttackTime;
	/// <summary>
	/// Enemy prefab collision radius.
	/// </summary>
	float myCollisionRadius;
	/// <summary>
	/// The player collision radius.
	/// </summary>
	float targetCollisionRadius;
	/// <summary>
	/// <c>true</c> if enemy currently has a target; <c>false</c> otherwise.
	/// </summary>
	bool hasTarget;

	/// <summary>
	/// Start this instance.
	/// </summary>
	protected override void Start() {
		pathfinder = GetComponent<NavMeshAgent>();

		if (GameObject.FindGameObjectWithTag("Player") != null) {			
			//
			// Set Player as nav target
			//
			target = GameObject.FindGameObjectWithTag("Player").transform;
			targetEntity = target.GetComponent<LivingEntity>();
			hasTarget = true;
			targetEntity.OnDeath += OnTargetDeath;
			currentState = State.Chasing;

			//
			// Store collision radii for attack distance
			//
			myCollisionRadius = GetComponent<SphereCollider>().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

			//
			// Start new thread for pathfinding
			//
			StartCoroutine(UpdatePath());
		}
	}

	/// <summary>
	/// Update this instance.
	/// </summary>
	void Update() {
		if (hasTarget) {
			//
			// If it is time for the next attack
			//
			if (Time.time > nextAttackTime) {
				float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

				//
				// If the distance is too great, do not follow the player
				//
				if (sqrDstToTarget > 100) {
					currentState = State.Idle;
				}
				//
				// If the distance is within the attack radius, attack
				//
				else if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
					nextAttackTime = Time.time + timeBetweenAttacks;
					StartCoroutine(Attack());
				}
				//
				// Otherwise chase the target
				//
				else if (currentState == State.Idle){
					currentState = State.Chasing;
				}
			}
		}
	}

	/// <summary>
	/// Called when player dies. Sets state to Idle.
	/// </summary>
	void OnTargetDeath() {
		hasTarget = false;
		currentState = State.Idle;
	}

	/// <summary>
	/// Coroutine for the attack animation.
	/// </summary>
	IEnumerator Attack() {
		//
		// Make sure nav stops while in animation
		//
		currentState = State.Attacking;
		pathfinder.enabled = false;

		Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - dirToTarget;
		float percent = 0;
		float attackSpeed = 3;
		bool hasAppliedDamage = false;

		while (percent <= 1) {
			percent += Time.deltaTime * attackSpeed;

			//
			// Move the enemy according to interpolation = (-%^2 + %) * 4
			//
			float interpolation = (-percent * percent + percent) * 4;
			transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

			//
			// Apply damage once after the animation has reached the halfway point
			//
			if (percent >= 0.5f && !hasAppliedDamage) {
				hasAppliedDamage = true;
				targetEntity.TakeDamage(damage);
			}
			yield return null;
		}

		//
		// Animation is finished. Restart nav.
		//
		pathfinder.enabled = true;
		currentState = State.Chasing;
	}

	/// <summary>
	/// Updates the nav path.
	/// </summary>
	IEnumerator UpdatePath() {
		//
		// Lower nav refresh rate to make more efficient
		//
		float refreshRate = 0.25f;

		while (hasTarget) {
			if (currentState == State.Chasing) {
				//
				// Desired position is just shy of target's position. Destination set to closest point the desired distance from the target.
				//
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
				if (!dead) {
					pathfinder.SetDestination(targetPosition);
				}
			}

			//
			// Wait for reduced refresh rate
			//
			yield return new WaitForSeconds(refreshRate);
		}
	}

}
