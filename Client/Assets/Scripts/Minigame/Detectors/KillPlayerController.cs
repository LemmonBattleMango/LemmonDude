using UnityEngine;
using System.Collections;

public class KillPlayerController : MonoBehaviour {

	// ====================================================
	private void Awake() {
		gameObject.layer = LayerMask.NameToLayer( "HitDetector" );
	}

	//======================================================
	void OnTriggerEnter2D( Collider2D other ) {
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
	}
}
