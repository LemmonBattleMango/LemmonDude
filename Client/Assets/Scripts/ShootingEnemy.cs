using UnityEngine;
using System.Collections;

public class ShootingEnemy : PatrollingEnemy {

	public EnemyBulletController bulletPrefab;
	private bool isAttacking;
	public float attackDelay = 1f;
	private float lastTryAttackTime;
	private float lastAttackTime;
	public Transform spawnPosition;

	LayerMask layerMask;

	// ====================================================
	protected override void Awake() {
		base.Awake();
		layerMask= LayerMask.GetMask(new string[] { "LevelLayer", "OneWayPlatformLayer", "PlayerLayer" } );
	}

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
		Vector2 direction = Vector2.right * myTransform.localScale.x;
		RaycastHit2D raycastHit = Physics2D.Raycast( spawnPosition.position, direction, VectorUtils.GetPosition2D( player.transform.position - spawnPosition.position ).magnitude, layerMask.value );
		if( raycastHit.collider != null && player.gameObject == raycastHit.collider.gameObject ) {
			StartCoroutine( AttackCoroutine() );
		}
	}

	// ====================================================
	private IEnumerator AttackCoroutine() {
		animator.SetBool( "isMoving", false );
		animator.SetTrigger( "shootTrigger" );
		isAttacking = true;
		lastAttackTime = MinigameTimeManager.instance.time;
		float startTime = MinigameTimeManager.instance.time;
		EnemyBulletController bullet = Instantiate<EnemyBulletController>( bulletPrefab );
		Vector2 direction = Vector2.right * myTransform.localScale.x;
		bullet.Configure( direction, null );
		bullet.transform.position = spawnPosition.position;
		while( MinigameTimeManager.instance.time < lastTryAttackTime + 0.3f ) {
			yield return 0;
		}
		isAttacking = false;
	}

}
