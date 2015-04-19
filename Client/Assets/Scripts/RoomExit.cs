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
		Log.Debug( "OnTriggerEnter2D" );
		if( other.GetComponent<PlayerController>() == null ) {
			return;
		}
		Log.Debug( "OnTriggerEnter2D targetRoom : " + targetRoom );
		Log.Debug( "OnTriggerEnter2D MinigameManager.instance.currentRoom: " + MinigameManager.instance.currentRoom );
		if( MinigameManager.instance.currentRoom != targetRoom ) {
			return;
		}
		
		MinigameManager.instance.LoadNextRoom();
	}
}
