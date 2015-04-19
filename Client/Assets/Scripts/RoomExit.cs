using UnityEngine;
using System.Collections;

public class RoomExit : MonoBehaviour {

	private RoomController targetRoom;
	
	//======================================================
	public void Configure( RoomController _targetRoom ) {
		targetRoom = _targetRoom;
	}
	
	//======================================================
	void OnTriggerEnter2D( Collider2D other ) {	
		if( other.GetComponent<PlayerController>() == null ) {
			return;
		}
		if( MinigameManager.instance.currentRoom != targetRoom ) {
			return;
		}
		
		MinigameManager.instance.LoadNextRoom();
	}
}
