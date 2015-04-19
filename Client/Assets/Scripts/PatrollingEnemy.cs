using UnityEngine;
using System.Collections;

public class PatrollingEnemy : SwappableEntity {

	public float maxHorizontalSpeed = 1f;
	public float horizontalAccel = 90f;

	public Vector2 currentDirection;
	private Transform myTransform;

	private float lastDirectionChange;

	// ====================================================
	public override void Start() {
		base.Start();
		myTransform = transform;
		currentDirection = myTransform.localScale.x > 0 ? Vector2.right : -Vector2.right;
	}
	
	// ====================================================
	protected override void LateUpdate() {
		if( physicsController.isGrounded ) {
			UpdateWalking();
		}
		else{ 
			base.LateUpdate();
		}
	}

	// ====================================================
	private void UpdateWalking() {
		UpdateDirection();
		currentSpeed += currentDirection * horizontalAccel * MinigameTimeManager.instance.deltaTime;
		currentSpeed.x = Mathf.Clamp( currentSpeed.x, -maxHorizontalSpeed, maxHorizontalSpeed );

		Vector2 prevPos = VectorUtils.GetPosition2D( transform.position );
		physicsController.Move( currentSpeed * MinigameTimeManager.instance.deltaTime, false );
		
		// updating the current speed
		currentSpeed = ( VectorUtils.GetPosition2D( transform.position ) - prevPos ) / MinigameTimeManager.instance.deltaTime;
		
		if( currentSpeed.x != 0 ) {
			transform.localScale = new Vector3( Mathf.Sign( currentSpeed.x ), 1f, 1f );
		}
	}

	// ====================================================
	private void UpdateDirection() {
		if( MinigameTimeManager.instance.time < lastDirectionChange + 0.3f ) {
			return;
		}

		if( currentDirection.x > 0 && physicsController.didHitRight ) {
			currentDirection = -Vector2.right;
			currentSpeed = Vector2.zero;
			lastDirectionChange = MinigameTimeManager.instance.time;
			return;
		}

		if( currentDirection.x < 0 && physicsController.didHitLeft ) {
			currentDirection = Vector2.right;
			currentSpeed = Vector2.zero;
			lastDirectionChange = MinigameTimeManager.instance.time;
			return;
		}
	}
}
