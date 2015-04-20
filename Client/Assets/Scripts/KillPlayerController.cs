using UnityEngine;
using System.Collections;

public class KillPlayerController : MonoBehaviour {

	//======================================================
	void OnTriggerEnter2D( Collider2D other ) {
		PlayerController player =  other.GetComponent<PlayerController>();
		if( player != null ) {
			player.InstaDeath();
		}

		PatrollingEnemy enemy = other.GetComponent<PatrollingEnemy>();
		if( enemy != null ) {
			enemy.InstaDeath();
		}
	}
}
