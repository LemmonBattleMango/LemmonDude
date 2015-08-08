using UnityEngine;
using System.Collections;

[RequireComponent (typeof( PhysicsController ) )]
public class IcicleController : MonoBehaviour {

	public GameObject deathCollider;
	public GameObject renderObject;
	public float vibrationSeconds = 0.5f;
	public float vibrationWidth = 0.01f;

	public float gravityAccel = 10f;
	public float maxVerticalSpeed = 10f;
	private Vector2 currentSpeed = Vector2.zero;

	[HideInInspector]
	public PhysicsController physicsController;

	// ====================================================
	void Start () {
		gameObject.layer = MinigameManager.instance.offLayer;
		deathCollider.SetActive( false );
		physicsController = GetComponent<PhysicsController>();
		PlayerDetector playerDetector = GetComponentInChildren<PlayerDetector>();
		playerDetector.onPlayerDetected = OnPlayerDetected;

		physicsController.shouldUseSlopes = true;
		physicsController.SetCollisionLayers( LayerMask.GetMask(new string[] { "InvisibleWallLayer", "LevelLayer" } ),
		                                     LayerMask.GetMask(new string[] { "InvisibleWallLayer", "LevelLayer", "OneWayPlatformLayer" } ) );
		
		physicsController.Initialize();
	}

	// ====================================================
	private void OnPlayerDetected( PlayerDetector playerDetector ) {
		playerDetector.onPlayerDetected = null;
		playerDetector.gameObject.SetActive( false );
		StartCoroutine( ActivateCorrutine() );
	}

	// ====================================================
	private IEnumerator ActivateCorrutine () {
		yield return StartCoroutine( VibrateCorrutine() );
		StartCoroutine( FallCorrutine() );
	}

	// ====================================================
	private IEnumerator FallCorrutine () {
		Vector2 prevPos;
		deathCollider.SetActive( true );

		while( true ) {
			currentSpeed.y -= gravityAccel * MinigameTimeManager.instance.deltaTime;
			currentSpeed.y = Mathf.Clamp( currentSpeed.y, -maxVerticalSpeed, float.MaxValue );
			
			prevPos = VectorUtils.GetPosition2D( transform.position );
			physicsController.Move( currentSpeed * MinigameTimeManager.instance.deltaTime, false );
			
			// updating the current speed
			currentSpeed = ( VectorUtils.GetPosition2D( transform.position ) - prevPos ) / MinigameTimeManager.instance.deltaTime;
			deathCollider.SetActive( !physicsController.isGrounded && currentSpeed.y < -1f );
			yield return 0;
		}
	}

	// ====================================================
	private IEnumerator VibrateCorrutine () {
		Vector3 initialPos = renderObject.transform.localPosition;
		float initialTime = MinigameTimeManager.instance.time;
		float factor = 1f;
		while( MinigameTimeManager.instance.time < initialTime + vibrationSeconds ) {
			renderObject.transform.localPosition = initialPos + Vector3.right * vibrationWidth * factor;
			factor *= -1f;
			yield return 0;
			yield return 0;
		}
		renderObject.transform.localPosition = initialPos;
	}
}
