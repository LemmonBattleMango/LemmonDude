using UnityEngine;
using System.Collections;

public class PlayerHitboxReference : MonoBehaviour {

	public PlayerController player;

	// ====================================================
	private void Awake() {
		gameObject.layer =  LayerMask.NameToLayer( "PlayerHitboxLayer" );
		Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
		if( rigidbody2D == null ) {
			rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
		}
		rigidbody2D.isKinematic = true;
		Collider2D collider2D = GetComponent<Collider2D>();
		collider2D.isTrigger = true;
	}

	//======================================================
	private void OnTriggerEnter2D( Collider2D other ) {	

		if( player == null || player.isDead ) {
			return;
		}
		SwappableEntityHitboxReference swappableEntityReference = other.GetComponent<SwappableEntityHitboxReference>();
		if( swappableEntityReference != null && swappableEntityReference.swappableEntity != null ) {
			PatrollingEnemy patrollingEnemy = swappableEntityReference.swappableEntity as PatrollingEnemy;
			if( patrollingEnemy != null ) {
				player.InstaDeath();
			}
		}
	}
}
