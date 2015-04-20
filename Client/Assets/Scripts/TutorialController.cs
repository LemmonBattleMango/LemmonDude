using UnityEngine;
using System.Collections;

public class TutorialController : MonoBehaviour {

	// ====================================================
	void Update () {
		if( Input.GetKey( KeyCode.Return ) ) {
			Application.LoadLevel( "GameScene" );
		}
		else if( Input.GetKey( KeyCode.Escape ) ) {
			Application.LoadLevel( "MainMenuScene" );
		}
		
	}
}
