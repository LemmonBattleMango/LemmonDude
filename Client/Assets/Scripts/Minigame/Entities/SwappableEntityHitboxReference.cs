using UnityEngine;
using System.Collections;

public class SwappableEntityHitboxReference : MonoBehaviour {

	public SwappableEntity swappableEntity;

	// ====================================================
	private void Awake() {
		gameObject.layer =  LayerMask.NameToLayer( "EnemyHitboxLayer" );

		Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
		if( rigidbody2D == null ) {
			rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
		}
		rigidbody2D.isKinematic = true;

		Collider2D collider2D = GetComponent<Collider2D>();
		collider2D.isTrigger = true;
	}
}
