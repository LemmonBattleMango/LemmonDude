using UnityEngine;
using System.Collections;

public class SplashController : MonoBehaviour {

	// ====================================================
	IEnumerator Start () {
		yield return new WaitForSeconds( 2f );
		Application.LoadLevel( "MainMenuScene" );
	}

	// ====================================================
	void Update () {
		if( Input.GetKey( KeyCode.Escape ) ) {
			Application.Quit();
		}
		
	}
}
