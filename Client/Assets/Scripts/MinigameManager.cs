using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MinigameManager : DisposableSingleton<MinigameManager> {
	
	public Transform playerContainer;
	public Transform projectileContainer;

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

	//=====================================
	public void MinigameStart() {
		Application.targetFrameRate = 60;
		levelLayer = LayerMask.NameToLayer( "LevelLayer" );
		playerLayer = LayerMask.NameToLayer( "PlayerLayer" );
		defaultLayer = LayerMask.NameToLayer( "Default" );
		weaponLayer = LayerMask.NameToLayer( "WeaponLayer" );
		deathLayer = LayerMask.NameToLayer( "DeathLayer" );
		hitLayer = LayerMask.NameToLayer( "HitDetector" );
	}

#if UNITY_EDITOR
	//=====================================
	void Update() {
		if( Input.GetKeyDown( KeyCode.L ) ) {
			ToggleSlowMo();
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
#endif

	//=====================================
	private void RestartRound() {
		StartCoroutine( RestartRoundCoroutine() );
	}

	//=====================================
	private IEnumerator RestartRoundCoroutine() {
		yield return StartCoroutine( MinigameTimeManager.instance.WaitForSecs( 0.7f ) );
		if( onRestartRound != null ) {
			onRestartRound();
		}
		foreach( PlayerController p in PlayerFactory.instance.players ) {
			p.Respawn();
		}
	}

	//=====================================
	public void OnPlayerDeath( PlayerController deadPlayer, PlayerController killerPlayer ) {
	}
}
