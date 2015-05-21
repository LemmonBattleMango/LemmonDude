using UnityEngine;
using System.Collections;

public class TutorialController : MonoBehaviour {

	// ====================================================
	void Update () {
		if( Input.GetKey( KeyCode.Return ) || Input.GetButton( "Start_1" ) ) {
			Application.LoadLevel( "GameScene" );
		}
		else if( Input.GetKey( KeyCode.Escape ) ) {
			Application.LoadLevel( "MainMenuScene" );
		}
		
	}
}
