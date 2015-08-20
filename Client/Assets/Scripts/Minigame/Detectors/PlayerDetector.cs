using UnityEngine;
using System.Collections;
using System;

public class PlayerDetector : MonoBehaviour {

	[System.NonSerialized]
	public Action<PlayerController, PlayerDetector> onPlayerDetected; 

	//======================================================
	void OnTriggerEnter2D( Collider2D other ) {
		PlayerHitboxReference playerReference = other.GetComponent<PlayerHitboxReference>();
		if( playerReference != null && playerReference.player != null ) {
			if( onPlayerDetected != null ) {
				onPlayerDetected( playerReference.player, this );
			}
		}
	}
}
