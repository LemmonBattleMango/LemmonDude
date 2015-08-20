using UnityEngine;
using System.Collections;

public class BirdEnemy : PatrollingEnemy {

	public float regularVerticalMaxVelocity = 1f;
	public float regularHorizontalMaxVelocity = 0.3f;
	public float dashVelocity = 2f;
	public float dashDistance = 2f;
	public float maxAccel = 1f;
	public float timeBetweenAccelUpdates = 1f;
	public float vibrationSeconds = 0.5f;
	public float vibrationWidth = 0.1f;
	public GameObject renderObject;
	
	private bool isAttacking;
	private Vector2 dashDirection;
	private Vector2 currentAccel;
	private float lastAccelUpdated = float.MinValue;

	private PlayerDetector playerDetector;

	// ====================================================
	public override void Start ()
	{
		base.Start ();
		playerDetector = GetComponentInChildren<PlayerDetector>();
		playerDetector.onPlayerDetected = OnPlayerDetected;
	}

	// ====================================================
	protected override void LateUpdate() {
//		if( !isEnabled ) {
//			return;
//		}

		float deltaTime = MinigameTimeManager.instance.deltaTime;
		if( deltaTime == 0 ) {
			return;
		}

		if( isAttacking ) {
			return;
		}

		currentSpeed += deltaTime * currentAccel;
		currentSpeed.x = Mathf.Clamp( currentSpeed.x, -regularHorizontalMaxVelocity, regularHorizontalMaxVelocity );
		currentSpeed.y = Mathf.Clamp( currentSpeed.y, -regularVerticalMaxVelocity, regularVerticalMaxVelocity );

		if( lastAccelUpdated + timeBetweenAccelUpdates < MinigameTimeManager.instance.time ) {
			lastAccelUpdated = MinigameTimeManager.instance.time;
			Vector2 randomVector = new Vector2( Random.Range( -1f,1f ), Random.Range( -1f,1f ) ).normalized;
			randomVector.y = Mathf.Clamp( randomVector.y, -0.4f, 0.4f );
			currentAccel = randomVector * maxAccel;
			
			transform.localScale = new Vector3( Mathf.Sign( currentAccel.x ), 1f, 1f );
		}

		Vector2 prevPos = VectorUtils.GetPosition2D( transform.position );
		physicsController.Move( currentSpeed * deltaTime, false );
		
		// updating the current speed
		currentSpeed = ( VectorUtils.GetPosition2D( transform.position ) - prevPos ) / deltaTime;
		
		if( physicsController.didHitLeft && currentAccel.x < 0f ) {
			currentSpeed.x = 0f;
			currentAccel.x *= -1f;
			lastAccelUpdated = MinigameTimeManager.instance.time;
			transform.localScale = new Vector3( Mathf.Sign( currentAccel.x ), 1f, 1f );
		}
		if( physicsController.didHitRight && currentAccel.x > 0f ) {
			currentSpeed.x = 0f;
			currentAccel.x *= -1f;
			lastAccelUpdated = MinigameTimeManager.instance.time;
			transform.localScale = new Vector3( Mathf.Sign( currentAccel.x ), 1f, 1f );
		}
		if( physicsController.isGrounded && currentAccel.y < 0f ) {
			currentSpeed.y = 0f;
			currentAccel.y *= -1f;
			lastAccelUpdated = MinigameTimeManager.instance.time;
		}
		if( physicsController.didHitCeiling && currentAccel.y > 0f ) {
			currentSpeed.y = 0f;
			currentAccel.y *= -1f;
			lastAccelUpdated = MinigameTimeManager.instance.time;
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
		lastAccelUpdated = 0f;
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
