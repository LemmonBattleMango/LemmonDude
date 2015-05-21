using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour {

	// ====================================================
	void Update () {
		if( Input.GetKey( KeyCode.Return ) || Input.GetButton( "Start_1" ) ) {
			Application.LoadLevel( "TutorialScene" );
		}
		else if( Input.GetKey( KeyCode.Escape ) ) {
			Application.Quit();
		}
	
	}
}
