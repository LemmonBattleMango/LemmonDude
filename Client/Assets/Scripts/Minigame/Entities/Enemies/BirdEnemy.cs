using UnityEngine;
using System.Collections;

public class BirdEnemy : PatrollingEnemy {

	public float movingRange = 2f;
	public float regularHorizontalMaxVelocity = 0.3f;
	public float dashVelocity = 2f;
	public float dashDistance = 2f;
	public float maxAccel = 1f;
	public float vibrationSeconds = 0.5f;
	public float vibrationWidth = 0.1f;
	public GameObject renderObject;

	private bool isReturning;
	private bool isAttacking;
	private Vector2 dashDirection;
	private Vector2 currentAccel;
	private Vector2 initialPos;

	private PlayerDetector playerDetector;

	// ====================================================
	public override void Start ()
	{
		base.Start ();
		playerDetector = GetComponentInChildren<PlayerDetector>();
		playerDetector.onPlayerDetected = OnPlayerDetected;
		initialPos = VectorUtils.GetPosition2D( transform.position );
		currentAccel = Vector2.right * regularHorizontalMaxVelocity;
		if( Random.value < 0.5f ) {
			currentAccel *= -1f;
			transform.localScale = new Vector3( Mathf.Sign( currentAccel.x ), 1f, 1f );
		}
	}

	// ====================================================
	protected override void LateUpdate() {
		if( !isEnabled ) {
			return;
		}

		float deltaTime = MinigameTimeManager.instance.deltaTime;
		if( deltaTime == 0 ) {
			return;
		}

		if( isAttacking ) {
			return;
		}

		if( isReturning ) {
			Vector2 movement = initialPos - VectorUtils.GetPosition2D( transform.position );
			movement.x = Mathf.Clamp( movement.x, -regularHorizontalMaxVelocity * deltaTime, regularHorizontalMaxVelocity * deltaTime );
			movement.y = Mathf.Clamp( movement.y, -regularHorizontalMaxVelocity * deltaTime, regularHorizontalMaxVelocity * deltaTime );
			physicsController.Move( movement, false );
			if( movement.x != 0f ) {
				transform.localScale = new Vector3( Mathf.Sign( movement.x ), 1f, 1f );
			}
			if( movement == Vector2.zero ) {
				currentAccel = new Vector3( Mathf.Sign( transform.localScale.x ) * maxAccel, 1f, 1f );
				isReturning = false;
				currentAccel.y = 0;
			}
			return;
		}

		currentSpeed += deltaTime * currentAccel;
		currentSpeed.x = Mathf.Clamp( currentSpeed.x, -regularHorizontalMaxVelocity, regularHorizontalMaxVelocity );
		currentSpeed.y = 0f;


		if( Mathf.Abs( initialPos.x - transform.position.x ) > movingRange ) {
			currentAccel = Vector2.right * maxAccel;
			if( initialPos.x < transform.position.x ) {
				currentAccel *= -1f;
			}
			
			transform.localScale = new Vector3( Mathf.Sign( currentAccel.x ), 1f, 1f );
		}

		Vector2 prevPos = VectorUtils.GetPosition2D( transform.position );
		physicsController.Move( currentSpeed * deltaTime, false );
		
		// updating the current speed
		currentSpeed = ( VectorUtils.GetPosition2D( transform.position ) - prevPos ) / deltaTime;
		
		if( physicsController.didHitLeft && currentAccel.x < 0f ) {
			currentSpeed.x = 0f;
			currentAccel.x *= -1f;
			transform.localScale = new Vector3( Mathf.Sign( currentAccel.x ), 1f, 1f );
		}
		if( physicsController.didHitRight && currentAccel.x > 0f ) {
			currentSpeed.x = 0f;
			currentAccel.x *= -1f;
			transform.localScale = new Vector3( Mathf.Sign( currentAccel.x ), 1f, 1f );
		}
		if( physicsController.isGrounded && currentAccel.y < 0f ) {
			currentSpeed.y = 0f;
			currentAccel.y *= -1f;
		}
		if( physicsController.didHitCeiling && currentAccel.y > 0f ) {
			currentSpeed.y = 0f;
			currentAccel.y *= -1f;
		}
	}

	// ====================================================
	private void OnPlayerDetected( PlayerController player, PlayerDetector playerDetector ) {
		StartCoroutine( ActivateCorrutine( player ) );
	}

	// ====================================================
	private IEnumerator ActivateCorrutine( PlayerController player ) {
		isAttacking = true;
		playerDetector.gameObject.SetActive( false );
		Vector2 direction = VectorUtils.GetPosition2D( player.transform.position - transform.position ).normalized;
		yield return StartCoroutine( VibrateCorrutine() );
		yield return StartCoroutine( DashCorrutine( direction ) );
		isAttacking = false;
		playerDetector.gameObject.SetActive( true );
	}

	// ====================================================
	private IEnumerator DashCorrutine( Vector2 direction ) {
		float distanceTraveled = 0f;
		transform.localScale = new Vector3( Mathf.Sign( direction.x ), 1f, 1f );
		Vector2 speed = direction * dashVelocity;
		while( distanceTraveled < dashDistance ) {
			distanceTraveled += MinigameTimeManager.instance.deltaTime * dashVelocity;
			physicsController.Move( speed * MinigameTimeManager.instance.deltaTime, false );
			yield return null;
		}
		isReturning = true;
	}

	// ====================================================
	private IEnumerator VibrateCorrutine () {
		Vector3 initialPos = renderObject.transform.localPosition;
		float initialTime = MinigameTimeManager.instance.time;
		float factor = 1f;
		while( MinigameTimeManager.instance.time < initialTime + vibrationSeconds ) {
			renderObject.transform.localPosition = initialPos + Vector3.right * vibrationWidth * factor;
			factor *= -1f;
			yield return null;
			yield return null;
		}
		renderObject.transform.localPosition = initialPos;
	}
}
