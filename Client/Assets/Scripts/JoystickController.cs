using UnityEngine;
using System.Collections;

public class JoystickController : MonoBehaviour {

	private KeyCode jumpKey = KeyCode.Space;
	private KeyCode fireKey = KeyCode.Return;

	//======================================================
	public Vector2 GetDirection() {
		Vector2 direction = Vector2.zero;
		if( Input.GetKey( KeyCode.A ) ) {
			direction.x = -1;
		}
		
		if( Input.GetKey( KeyCode.D ) ) {
			direction.x = 1;
		}
		
		if( Input.GetKey( KeyCode.W ) ) {
			direction.y = 1;
		}
		
		if( Input.GetKey( KeyCode.S ) ) {
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
