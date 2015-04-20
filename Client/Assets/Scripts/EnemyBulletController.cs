using UnityEngine;
using System.Collections;

public class EnemyBulletController : ProjectileController {

	public override float speed{ get{ return bulletSpeed; } }

	public float bulletSpeed = 20f;
	

	//======================================================
	protected override void OnTriggerEnter2D( Collider2D other ) {	

		PlayerController player = other.GetComponent<PlayerController>();
		if( player != null ) {
			player.InstaDeath();
		}

		PatrollingEnemy enemy = other.GetComponent<PatrollingEnemy>();
		if( enemy != null ) {
			enemy.InstaDeath();
		}
		
		//Do something
		Destroy( gameObject );
	}
}
