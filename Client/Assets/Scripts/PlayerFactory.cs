using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerFactory : DisposableSingleton<PlayerFactory> {

	// References
	public GameObject playerPrefab;
	[System.NonSerialized]
	public PlayerController currentPlayer;

	// ====================================================
	public PlayerController CreatePlayer() {
		GameObject player = Instantiate( playerPrefab ) as GameObject;
		player.transform.parent = MinigameManager.instance.playerContainer.transform;

		Transform spawnTransform = MinigameManager.instance.currentRoom.spawnPoint;
		player.transform.position = spawnTransform.position;
		player.transform.rotation = spawnTransform.rotation;
		player.transform.localScale = Vector3.one;
		player.name = "Player";

		PlayerController playerController = player.GetComponent<PlayerController>();
		//playerController.Initialize();
		playerController.onDeath += MinigameManager.instance.OnPlayerDeath;

		currentPlayer = ( playerController );
		return playerController;
	}

	// ====================================================
	public void DestroyPlayer( PlayerController player ) {
		Destroy( player.gameObject );
	}

}
