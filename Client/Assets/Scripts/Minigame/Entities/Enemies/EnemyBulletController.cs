using UnityEngine;
using System.Collections;

public class EnemyBulletController : ProjectileController {

	public override float speed{ get{ return bulletSpeed; } }

	public float bulletSpeed = 20f;
	

	// ====================================================
	private void Awake() {
		gameObject.layer = LayerMask.NameToLayer( "HitDetector" );
	}

	//======================================================
	protected override void OnTriggerEnter2D( Collider2D other ) {	

		PlayerHitboxReference playerReference = other.GetComponent<PlayerHitboxReference>();
		if( playerReference != null && playerReference.player != null ) {
			playerReference.player.InstaDeath();
		}

		SwappableEntityHitboxReference swappableEntityReference = other.GetComponent<SwappableEntityHitboxReference>();
		if( swappableEntityReference != null && swappableEntityReference.swappableEntity != null ) {
			PatrollingEnemy patrollingEnemy = swappableEntityReference.swappableEntity as PatrollingEnemy;
			if( patrollingEnemy != null ) {
				patrollingEnemy.InstaDeath();
			}
		}
		
		//Do something
		Destroy( gameObject );
	}
}
