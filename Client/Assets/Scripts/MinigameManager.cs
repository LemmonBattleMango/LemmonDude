using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MinigameManager : DisposableSingleton<MinigameManager> {
	
	public Transform playerContainer;
	public Transform projectileContainer;
	public Transform levelContainer;

	public delegate void OnRestartRoundDelegate();
	public event OnRestartRoundDelegate onRestartRound;

	[System.NonSerialized]
	public int levelLayer;
	[System.NonSerialized]
	public int playerLayer;
	[System.NonSerialized]
	public int defaultLayer;
	[System.NonSerialized]
	public int weaponLayer;
	[System.NonSerialized]
	public int deathLayer;
	[System.NonSerialized]
	public int hitLayer;

	public List<RoomController> rooms;
	[System.NonSerialized]
	public RoomController currentRoom;
	[System.NonSerialized]
	public RoomController lastRoom;
	[System.NonSerialized]
	private int roomIndex;

	//=====================================
	public void Start() {
		MinigameStart();
	}

	//=====================================
	public void MinigameStart() {

		//Load first room
		roomIndex = 0;
		currentRoom = Instantiate<RoomController>( rooms[roomIndex] );
		currentRoom.transform.parent = levelContainer;
		currentRoom.transform.position = Vector2.zero;
		PlayerFactory.instance.CreatePlayer();

		Application.targetFrameRate = 60;
		levelLayer = LayerMask.NameToLayer( "LevelLayer" );
		playerLayer = LayerMask.NameToLayer( "PlayerLayer" );
		defaultLayer = LayerMask.NameToLayer( "Default" );
		weaponLayer = LayerMask.NameToLayer( "WeaponLayer" );
		deathLayer = LayerMask.NameToLayer( "DeathLayer" );
		hitLayer = LayerMask.NameToLayer( "HitDetector" );
	}


	//=====================================
	void Update() {
		#if UNITY_EDITOR
		if( Input.GetKeyDown( KeyCode.L ) ) {
			ToggleSlowMo();
		}
		#endif

		if( Input.GetKeyDown( KeyCode.Escape ) ) {
			Application.Quit();
		}
	}

	//=====================================
	void ToggleSlowMo() {
		if( MinigameTimeManager.instance.timeScale == 1f ) {
			MinigameTimeManager.instance.timeScale = 0.3f;
		}
		else{
			MinigameTimeManager.instance.timeScale = 1f;
		}
	}

	//=====================================
	public void OnPlayerDeath( PlayerController deadPlayer, PlayerController killerPlayer ) {
		//TODO
	}

	//=====================================
	public void LoadNextRoom() {
		roomIndex++;
		lastRoom = currentRoom;
		currentRoom = Instantiate<RoomController>( rooms[roomIndex] );
		currentRoom.transform.parent = levelContainer;
		Vector3 deltaPos = currentRoom.transform.position - currentRoom.entrance.transform.position;

		currentRoom.transform.position = lastRoom.exit.transform.position + deltaPos;
	}
}
