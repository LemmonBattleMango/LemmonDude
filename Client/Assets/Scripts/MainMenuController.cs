using UnityEngine;
using System.Collections;

public class MainMenuController : MonoBehaviour {

	// ====================================================
	void Update () {
		if( Input.GetKey( KeyCode.Return ) ) {
			Application.LoadLevel( "TutorialScene" );
		}
		else if( Input.GetKey( KeyCode.Escape ) ) {
			Application.Quit();
		}
	
	}
}
