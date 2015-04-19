using UnityEngine;
using System.Collections;

public class KillPlayerController : MonoBehaviour {

	//======================================================
	void OnTriggerEnter2D( Collider2D other ) {
		PlayerController player =  other.GetComponent<PlayerController>();
		if( player == null ) {
			return;
		}

		player.InstaDeath();
	}
}
