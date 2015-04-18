using UnityEngine;
using System.Collections;

public class JoystickController : MonoBehaviour {

	private KeyCode jumpKey = KeyCode.Space;
	private KeyCode fireKey = KeyCode.LeftAlt;

	//======================================================
	public Vector2 GetDirection() {
		Vector2 direction = Vector2.zero;
		if( Input.GetKey( KeyCode.LeftArrow ) ) {
			direction.x = -1;
		}
		
		if( Input.GetKey( KeyCode.RightArrow ) ) {
			direction.x = 1;
		}
		
		if( Input.GetKey( KeyCode.UpArrow ) ) {
			direction.y = 1;
		}
		
		if( Input.GetKey( KeyCode.DownArrow ) ) {
			direction.y = -1;
		}
		direction.Normalize();

		return direction;
	}

	//======================================================
	public bool GetJumpButton() {
		return Input.GetKey( jumpKey );
	}

	//======================================================
	public bool GetJumpButtonDown() {
		return Input.GetKeyDown( jumpKey );
	}

	//======================================================
	public bool GetFireButton() {
		return Input.GetKey( fireKey );
	}

	//======================================================
	public bool GetFireButtonDown() {
		return Input.GetKeyDown( fireKey );
	}

}
