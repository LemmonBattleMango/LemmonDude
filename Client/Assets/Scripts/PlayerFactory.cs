using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerFactory : DisposableSingleton<PlayerFactory> {

	// References
	public GameObject playerPrefab;
	public List<PlayerController> players = new List<PlayerController>();
	private List<Transform> recentlyUsedSpawnPoints = new List<Transform>();
	public Dictionary<int, int> playerCloneCounter;

	// ====================================================
	public void Initialize( int numPlayers ) {
		playerCloneCounter = new Dictionary<int, int>();
		for( int i = 0; i < numPlayers; i++ ) {
			playerCloneCounter[i] = 0;
		}

		MinigameLevelReferences levelRef = MinigameLevelReferences.instance;

	}

	// ====================================================
	public PlayerController CreateEntity( int playerId, bool randomSpawnPoint ) {
		GameObject player = Instantiate( playerPrefab ) as GameObject;
		player.transform.parent = MinigameManager.instance.playerContainer.transform;


		playerCloneCounter[playerId]++;

		Transform spawnTransform = MinigameLevelReferences.instance.spawnPoint;
		player.transform.position = spawnTransform.position;
		player.transform.rotation = spawnTransform.rotation;
		player.transform.localScale = Vector3.one;
		player.name = "Player " + ( playerId + 1 );

		PlayerController playerController = player.GetComponent<PlayerController>();
		playerController.Initialize();
		playerController.onDeath += MinigameManager.instance.OnPlayerDeath;
		
		Director.instance.OnPlayerSpawn( playerController );

		players.Add( playerController );
		return playerController;
	}

	// ====================================================
	public void DestroyPlayer( PlayerController player ) {
		Destroy( player.gameObject );
	}

}
