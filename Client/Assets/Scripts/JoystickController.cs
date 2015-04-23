using UnityEngine;
using System.Collections;

public class JoystickController : MonoBehaviour {

	private KeyCode jumpKey = KeyCode.Z;
	private KeyCode fireKey = KeyCode.X;
	private KeyCode altJumpKey = KeyCode.Space;
	private KeyCode altFireKey = KeyCode.LeftShift;

	private Vector2 forcedDirection;
	private float forcedDirectionExpirationTime = -1f;

	//======================================================
	public Vector2 GetDirection() {

		if( MinigameTimeManager.instance.time < forcedDirectionExpirationTime ) {
			return forcedDirection;
		}

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
	public void ForceDirection( Vector2 _forcedDirection, float duration ) {
		forcedDirectionExpirationTime = MinigameTimeManager.instance.time + duration;
		forcedDirection = _forcedDirection.normalized;
	}

	//======================================================
	public bool GetJumpButton() {
		return Input.GetKey( jumpKey ) || Input.GetKey( altJumpKey );
	}

	//======================================================
	public bool GetJumpButtonDown() {
		return Input.GetKeyDown( jumpKey ) || Input.GetKeyDown( altJumpKey );
	}

	//======================================================
	public bool GetFireButton() {
		return Input.GetKey( fireKey ) || Input.GetKey( altFireKey );
	}

	//======================================================
	public bool GetFireButtonDown() {
		return Input.GetKeyDown( fireKey ) || Input.GetKeyDown( altFireKey );
	}

}
