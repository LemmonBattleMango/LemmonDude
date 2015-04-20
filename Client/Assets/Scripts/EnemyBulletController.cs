using UnityEngine;
using System.Collections;

public class EnemyBulletController : ProjectileController {

	//======================================================
	protected override void OnTriggerEnter2D( Collider2D other ) {	

		PlayerController player = other.GetComponent<PlayerController>();
		if( player != null ) {
			player.InstaDeath();
		}
		
		//Do something
		Destroy( gameObject );
	}
}
