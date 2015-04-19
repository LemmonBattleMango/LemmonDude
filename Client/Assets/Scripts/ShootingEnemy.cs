using UnityEngine;
using System.Collections;

public class ShootingEnemy : PatrollingEnemy {

	public EnemyBulletController bulletPrefab;
	private bool isAttacking;
	private float attackDelay = 1f;
	private float lastTryAttackTime;
	private float lastAttackTime;

	LayerMask layerMask= LayerMask.GetMask(new string[] { "LevelLayer", "OneWayPlatformLayer", "PlayerLayer" } );

	// ====================================================
	protected override void UpdateWalking() {
		if( isAttacking ) {
			return;
		}

		TryAttack();

		if( isAttacking ) {
			return;
		}

		base.UpdateWalking();
	}

	// ====================================================
	private void TryAttack() {
		if( MinigameTimeManager.instance.time < lastAttackTime + attackDelay ) {
			return;
		}

		if( MinigameTimeManager.instance.time < lastTryAttackTime + 0.3f ) {
			return;
		}
		lastTryAttackTime = MinigameTimeManager.instance.time;
		PlayerController player = PlayerFactory.instance.currentPlayer;
		if( player == null || player.isDead ) {
			return;
		}

		Vector2 offset = physicsController.colliderCenter + ( physicsController.colliderSize.x * 0.5f + PhysicsController.LINECAST_OFFSET )* currentDirection;
		Vector2 worldPos = VectorUtils.GetPosition2D( myTransform.position ) +  offset;
		
		RaycastHit2D raycastHit = Physics2D.Raycast( worldPos, myTransform.right, (worldPos - VectorUtils.GetPosition2D( player.transform.position ) ).magnitude, layerMask.value );
		if( raycastHit.collider != null && player.gameObject == raycastHit.collider ) {
			StartCoroutine( AttackCoroutine() );
		}
	}

	// ====================================================
	private IEnumerator AttackCoroutine() {
		isAttacking = true;
		lastAttackTime = MinigameTimeManager.instance.time;
		float startTime = MinigameTimeManager.instance.time;
		while( MinigameTimeManager.instance.time < lastTryAttackTime + 0.3f ) {
			yield return 0;
		}
		EnemyBulletController bullet = Instantiate<EnemyBulletController>( bulletPrefab );
		bullet.Configure( myTransform.right, null );
		isAttacking = false;
	}

}
