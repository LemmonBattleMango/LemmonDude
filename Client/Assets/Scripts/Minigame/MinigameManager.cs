﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MinigameManager : DisposableSingleton<MinigameManager> {
	
	public Transform playerContainer;
	public Transform projectileContainer;
	public Transform levelContainer;

	public delegate void OnRestartRoundDelegate();

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
	[System.NonSerialized]
	public int offLayer;
	[System.NonSerialized]
	public int invisibleWallLayer;

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
		playerLayer = LayerMask.NameToLayer( "MoveBoxLayer" );
		defaultLayer = LayerMask.NameToLayer( "Default" );
		weaponLayer = LayerMask.NameToLayer( "WeaponLayer" );
		deathLayer = LayerMask.NameToLayer( "DeathLayer" );
		hitLayer = LayerMask.NameToLayer( "HitDetector" );
		offLayer = LayerMask.NameToLayer( "OffLayer" );
		invisibleWallLayer = LayerMask.NameToLayer( "InvisibleWallLayer" );
	}


	//=====================================
	void Update() {
		#if UNITY_EDITOR
		if( Input.GetKeyDown( KeyCode.L ) ) {
			ToggleSlowMo();
		}
		#endif

		if( Input.GetKeyDown( KeyCode.R ) && PlayerFactory.instance.currentPlayer != null && !PlayerFactory.instance.currentPlayer.isDead ) {
			PlayerFactory.instance.currentPlayer.InstaDeath();
			return;
		}

		if( Input.GetKeyDown( KeyCode.Escape ) ) {
			Application.LoadLevel( "MainMenuScene" );
			return;
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
	public void OnPlayerDeath( PlayerController deadPlayer ) {
		StartCoroutine( OnPlayerDeathCoroutine( deadPlayer ) );
	}

	//=====================================
	public IEnumerator OnPlayerDeathCoroutine( PlayerController deadPlayer ) {
		Director.instance.OnPlayerDead( deadPlayer );
		yield return new WaitForSeconds( 1f );
		Destroy( deadPlayer.gameObject );
		ReloadCurrentRoom();
	}

	//=====================================
	public void LoadNextRoom() {
		roomIndex++;
		if( lastRoom != null ) {
			Destroy( lastRoom.gameObject );
		}
		lastRoom = currentRoom;
		currentRoom = Instantiate<RoomController>( rooms[roomIndex] );
		currentRoom.transform.parent = levelContainer;
		Vector3 deltaPos = currentRoom.transform.position - currentRoom.entrance.transform.position;

		currentRoom.transform.position = lastRoom.exit.transform.position + deltaPos;
		currentRoom.entrance.direction = lastRoom.exit.direction;
	}

	//=====================================
	public void ReloadCurrentRoom() {
		Destroy( currentRoom.gameObject );
		currentRoom = Instantiate<RoomController>( rooms[roomIndex] );
		currentRoom.transform.parent = levelContainer;

		if( lastRoom != null ) {
			Vector3 deltaPos = currentRoom.transform.position - currentRoom.entrance.transform.position;
			currentRoom.transform.position = lastRoom.exit.transform.position + deltaPos;
		}
		else {
			currentRoom.transform.position = Vector2.zero;
		}
		if( currentRoom.entrance != null ) {
			currentRoom.entrance.CloseDoor();
		}
		PlayerFactory.instance.CreatePlayer();
	}
}
