using UnityEngine;
using System.Collections;

public class RoomExit : MonoBehaviour {

	private RoomController targetRoom;

	// ====================================================
	private void Awake() {
		gameObject.layer = LayerMask.NameToLayer( "Default" );
	}

	//======================================================
	public void Configure( RoomController _targetRoom ) {
		targetRoom = _targetRoom;
	}
	
	//======================================================
	void OnTriggerEnter2D( Collider2D other ) {	
		PlayerController player = other.GetComponent<PlayerController>();
		if( player == null || player.isDead ) {
			return;
		}
		if( MinigameManager.instance.currentRoom != targetRoom ) {
			return;
		}

		player.joystickController.ForceDirection( Vector2.right, 0.15f );
		MinigameManager.instance.LoadNextRoom();
	}
}
