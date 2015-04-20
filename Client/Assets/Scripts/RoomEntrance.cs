using UnityEngine;
using System.Collections;

public class RoomEntrance : MonoBehaviour {

	private RoomController targetRoom;
	private BoxCollider2D myCollider;

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
		gameObject.SetActive( false );
	}

	//======================================================
	private void CloseDoor() {
		GameObject go = new GameObject();
		go.name = "door";
		go.transform.parent = targetRoom.transform;
		go.layer = MinigameManager.instance.levelLayer;
		BoxCollider2D door = go.AddComponent<BoxCollider2D>();
		door.size = new Vector2( myCollider.size.x, 2f* myCollider.size.y );
		door.transform.position = transform.position - myCollider.size.x * transform.localScale.x * Vector3.right;

	}
}
