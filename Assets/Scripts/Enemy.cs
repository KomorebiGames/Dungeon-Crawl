using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : LivingEntity {

	public enum State {Idle, Chasing, Attacking};

	State currentState;
	NavMeshAgent pathfinder;
	Transform target;
	LivingEntity targetEntity;
	float attackDistanceThreshold = .5f;
	float timeBetweenAttacks = 1;
	float damage = 1;
	float nextAttackTime;
	float myCollisionRadius;
	float targetCollisionRadius;
	bool hasTarget;

	 protected override void Start() {
		pathfinder = GetComponent<NavMeshAgent>();

		if (GameObject.FindGameObjectWithTag("Player") != null) {
			target = GameObject.FindGameObjectWithTag("Player").transform;
			targetEntity = target.GetComponent<LivingEntity>();
			targetEntity.OnDeath += OnTargetDeath;
			currentState = State.Chasing;
			hasTarget = true;

			myCollisionRadius = GetComponent<SphereCollider>().radius;
			targetCollisionRadius = target.GetComponent<CapsuleCollider>().radius;

			StartCoroutine(UpdatePath());
		}
	}

	void Update () {

		if (hasTarget) {
			if (Time.time > nextAttackTime) {
				float sqrDstToTarget = (target.position - transform.position).sqrMagnitude;

				if (sqrDstToTarget > 100) {
					currentState = State.Idle;
				}
				else if (sqrDstToTarget < Mathf.Pow(attackDistanceThreshold + myCollisionRadius + targetCollisionRadius, 2)) {
					nextAttackTime = Time.time + timeBetweenAttacks;
					StartCoroutine(Attack());
				}
				else if (currentState == State.Idle){
					currentState = State.Chasing;
				}
			}
		}

	}

	void OnTargetDeath() {

		hasTarget = false;
		currentState = State.Idle;

	}

	IEnumerator Attack() {

		currentState = State.Attacking;
		pathfinder.enabled = false;

		Vector3 originalPosition = transform.position;
		Vector3 dirToTarget = (target.position - transform.position).normalized;
		Vector3 attackPosition = target.position - dirToTarget;
		float percent = 0;
		float attackSpeed = 3;
		bool hasAppliedDamage = false;

		while (percent <= 1) {
			if (percent >= 0.5f && !hasAppliedDamage) {
				hasAppliedDamage = true;
				targetEntity.TakeDamage(damage);
			}

			percent += Time.deltaTime * attackSpeed;
			float interpolation = (-percent * percent + percent) * 4;
			transform.position = Vector3.Lerp(originalPosition, attackPosition, interpolation);

			yield return null;
		}

		pathfinder.enabled = true;
		currentState = State.Chasing;

	}

	IEnumerator UpdatePath() {

		float refreshRate = 0.25f;

		while (hasTarget) {
			if (currentState == State.Chasing) {
				Vector3 dirToTarget = (target.position - transform.position).normalized;
				Vector3 targetPosition = target.position - dirToTarget * (myCollisionRadius + targetCollisionRadius + attackDistanceThreshold / 2);
				if (!dead) {
					pathfinder.SetDestination(targetPosition);
				}
			}
			yield return new WaitForSeconds(refreshRate);
		}

	}

}
