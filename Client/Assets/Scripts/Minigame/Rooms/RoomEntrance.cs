using UnityEngine;
using System.Collections;

public class RoomEntrance : MonoBehaviour {

	[System.NonSerialized]
	public RoomController.Direction direction;

	private RoomController targetRoom;
	private BoxCollider2D myCollider;

	// ====================================================
	private void Awake() {
		gameObject.layer = LayerMask.NameToLayer( "Default" );
	}

	//======================================================
	public void Configure( RoomController _targetRoom ) {
		targetRoom = _targetRoom;
		myCollider = GetComponent<BoxCollider2D>();
	}

	//======================================================
	private void OnTriggerStay2D( Collider2D other ) {	
		
		if( MinigameManager.instance.currentRoom != targetRoom ) {
			return;
		}

		PlayerController player = other.GetComponent<PlayerController>();
		if( player == null ) {
			return;
		}

		float ourLeftX = transform.position.x - myCollider.size.x * 0.5f * transform.localScale.x;
		float playerLeftX = player.transform.position.x - player.physicsController.colliderSize.x * 0.5f * player.transform.localScale.x;

		if( ourLeftX > playerLeftX ) {
			return;
		}

		CloseDoor();
	}

	//======================================================
	public void CloseDoor() {
		GameObject go = new GameObject();
		go.name = "door";
		go.transform.parent = targetRoom.transform;
		go.layer = MinigameManager.instance.levelLayer;
		BoxCollider2D door = go.AddComponent<BoxCollider2D>();
		door.enabled = false;
		door.size = new Vector2( myCollider.size.x, 2f* myCollider.size.y );

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
		door.transform.position = transform.position - myCollider.size.x * transform.localScale.x * VectorUtils.GetPosition3D( dir );
		door.enabled = true;
		gameObject.SetActive( false );
	}
}
