using UnityEngine;
using System.Collections;

public class WinZone : MonoBehaviour {

	private const float DURATION_SECS = 3f;

	//======================================================
	private void OnTriggerStay2D( Collider2D other ) {	
		
		PlayerController player = other.GetComponent<PlayerController>();
		if( player == null || !player.isDead ) {
			return;
		}
		
		StartCoroutine( WinCoroutine( player ) );
	}

	//======================================================
	private IEnumerator WinCoroutine( PlayerController player ) {
		MinigameTimeManager.instance.timeScale = 0.1f;
		player.joystickController.ForceDirection( Vector2.zero, DURATION_SECS );
		yield return new WaitForSeconds( DURATION_SECS );
		Application.LoadLevel( "SplashScene" );
	}
}
