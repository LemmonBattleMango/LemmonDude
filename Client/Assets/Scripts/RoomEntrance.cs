using UnityEngine;
using System.Collections;

public class RoomEntrance : MonoBehaviour {

	private RoomController targetRoom;

	//======================================================
	public void Configure( RoomController _targetRoom ) {
		targetRoom = _targetRoom;
	}

	//======================================================
	void OnTriggerExit2D( Collider2D other ) {	
		
		if( MinigameManager.instance.currentRoom == targetRoom ) {
			return;
		}

		//close door?
	}
}
