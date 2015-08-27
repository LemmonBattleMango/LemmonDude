using UnityEngine;
using System.Collections;

public class JoystickController : MonoBehaviour {

	public enum JoystickType {
		XBOX,
		PS4,
		OSX
	}

	private JoystickType joystickType;
	
	// Keyboard
	private KeyCode JUMP_KEY = KeyCode.Z;
	private KeyCode FIRE_KEY = KeyCode.X;
	private KeyCode ALT_JUMP_KEY = KeyCode.Space;
	private KeyCode ALT_FIRE_KEY = KeyCode.LeftShift;

	// XBOX joystick
	const string JUMP_BUTTON_ID_XBOX = "A_1";
	const string FIRE_BUTTON_ID_XBOX = "X_1";
	const string LEFT_HORIZONTAL_AXIS_ID = "L_XAxis_1";
	const string LEFT_VERTICAL_AXIS_ID = "L_YAxis_1";
	const string DPAD_HORIZONTAL_AXIS_ID_XBOX = "DPad_XAxis_1";
	const string DPAD_VERTICAL_DPAD_AXIS_ID_XBOX = "DPad_YAxis_1";

	// PS4 joystick
	const string JUMP_BUTTON_ID_PS4 = "B_1";
	const string FIRE_BUTTON_ID_PS4 = "A_1";
	const string RIGHT_HORIZONTAL_AXIS_ID = "R_XAxis_1";
	const string RIGHT_VERTICAL_AXIS_ID = "R_YAxis_1";
	const string DPAD_HORIZONTAL_DPAD_AXIS_ID_PS4 = "Dpad_XAxis_PS4_1";
	const string DPAD_VERTICAL_DPAD_AXIS_ID_PS4 = "Dpad_YAxis_PS4_1";

	// XBOX joystick
	const string JUMP_BUTTON_ID_OSX = "OSX_A_1";
	const string FIRE_BUTTON_ID_OSX = "OSX_X_1";

	private Vector2 forcedDirection;
	private float forcedDirectionExpirationTime = -1f;

	public string dPadVerticalAxisId {
		get{ return ( joystickType == JoystickType.XBOX ) ? DPAD_VERTICAL_DPAD_AXIS_ID_XBOX : DPAD_VERTICAL_DPAD_AXIS_ID_PS4; }
	}
	public string dPadHorizontalAxisId {
		get{ return ( joystickType == JoystickType.XBOX ) ? DPAD_HORIZONTAL_AXIS_ID_XBOX : DPAD_HORIZONTAL_DPAD_AXIS_ID_PS4; }
	}
	public string jumpButtonId {
		get{ 
			switch( joystickType ) {
				case JoystickType.OSX : 
					return JUMP_BUTTON_ID_OSX;
					break;
				case JoystickType.PS4:
					return JUMP_BUTTON_ID_PS4;
					break;
				default:
					break;
			}
			return JUMP_BUTTON_ID_XBOX;
		}
	}
	public string firekButtonId {
		get{
			switch( joystickType ) {
				case JoystickType.OSX : 
					return FIRE_BUTTON_ID_OSX;
					break;
				case JoystickType.PS4:
					return FIRE_BUTTON_ID_PS4;
					break;
				default:
					break;
			}
			return FIRE_BUTTON_ID_XBOX;
		}
	}
	
	//======================================================
	public void Initialize() {
		string[] joystickNames = Input.GetJoystickNames();
		string joystickName = "";
		if( joystickNames.Length > 0 ) {
			joystickName = joystickNames[0];
		}

		if( !string.IsNullOrEmpty( joystickName ) ) {
			joystickType = ( joystickName == "Wireless Controller" ) ? JoystickType.PS4 : JoystickType.XBOX;
			Log.Debug( "Joystick name: '" + joystickName + "' ...name: " + joystickName + " ..type: " + joystickType.ToString() );
		}
		else {
			Log.Debug( "Joystick not found." );
		}
	}
	
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

		if( direction != Vector2.zero ) {
			direction.Normalize();
		}
		else{
			direction.x = Input.GetAxisRaw( LEFT_HORIZONTAL_AXIS_ID );
			direction.y = Input.GetAxisRaw( LEFT_VERTICAL_AXIS_ID );
			if( direction.sqrMagnitude < 0.09f ) {
				direction = Vector2.zero;
				direction.x = Input.GetAxisRaw( dPadHorizontalAxisId );
				direction.y = -Input.GetAxisRaw( dPadVerticalAxisId );
			}
			//rotatin Y axis
			direction.y *= -1;
			if ( direction.magnitude > 1f ) {
				direction.Normalize();
			}
		}

		return direction;
	}

	//======================================================
	public void ForceDirection( Vector2 _forcedDirection, float duration ) {
		forcedDirectionExpirationTime = MinigameTimeManager.instance.time + duration;
		forcedDirection = _forcedDirection.normalized;
	}

	//======================================================
	public bool GetJumpButton() {
		return Input.GetKey( JUMP_KEY ) || Input.GetKey( ALT_JUMP_KEY ) || Input.GetButton( jumpButtonId );
	}

	//======================================================
	public bool GetJumpButtonDown() {
		return Input.GetKeyDown( JUMP_KEY ) || Input.GetKeyDown( ALT_JUMP_KEY ) || Input.GetButtonDown( jumpButtonId );
	}

	//======================================================
	public bool GetFireButton() {
		return Input.GetKey( FIRE_KEY ) || Input.GetKey( ALT_FIRE_KEY ) || Input.GetButton( firekButtonId );;
	}

	//======================================================
	public bool GetFireButtonDown() {
		return Input.GetKeyDown( FIRE_KEY ) || Input.GetKeyDown( ALT_FIRE_KEY ) || Input.GetButtonDown( firekButtonId );
	}

}
