using UnityEngine;
using System.Collections;

public class KillPlayerController : MonoBehaviour {

	//======================================================
	void OnTriggerEnter2D( Collider2D other ) {
		PlayerController player =  other.GetComponent<PlayerController>();
		if( player != null ) {
			player.InstaDeath();
		}

		SwappableEntity swappableEntity = other.GetComponent<SwappableEntity>();
		if( swappableEntity != null ) {
			swappableEntity.InstaDeath();
		}
	}
}
