using UnityEngine;
using System.Collections;

public class RoomExit : MonoBehaviour {

	private RoomController targetRoom;
	public RoomController.Direction direction;


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
		Vector2 dir = Vector2.zero;
		switch ( direction ) {
		case RoomController.Direction.DOWN:
			dir = -Vector2.up;
			break;
		case RoomController.Direction.UP:
			dir = Vector2.up;
			break;
		case RoomController.Direction.LEFT:
			dir = -Vector2.right;
			break;
		case RoomController.Direction.RIGHT:
			dir = Vector2.right;
			break;
		}

		player.joystickController.ForceDirection( dir, 0.15f );
		MinigameManager.instance.LoadNextRoom();
	}
}
