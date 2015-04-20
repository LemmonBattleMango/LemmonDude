﻿using UnityEngine;
using System.Collections;

public class PatrollingEnemy : SwappableEntity {

	public float maxHorizontalSpeed = 1f;
	public float horizontalAccel = 90f;

	[HideInInspector]
	public Vector2 currentDirection;
	protected Transform myTransform;

	private float lastDirectionChange;

	protected LayerMask patrolLayerMask;
	protected RoomController room;

	protected bool isEnabled{ get{ return MinigameManager.instance.currentRoom == room; } }

	// ====================================================
	public void Configure( RoomController _room ) {
		room = _room;
	}

	// ====================================================
	public override void Start() {
		base.Start();
		myTransform = transform;
		SetDirectionAsForward();
		patrolLayerMask = LayerMask.GetMask(new string[] { "LevelLayer", "OneWayPlatformLayer" } );
	}

	// ====================================================
	public void SetDirectionAsForward() {
		currentDirection = myTransform.localScale.x > 0 ? Vector2.right : -Vector2.right;
	}
	
	// ====================================================
	protected override void LateUpdate() {
		if( !isEnabled ) {
			return;
		}
		if( physicsController.isGrounded ) {
			UpdateWalking();
		}
		else{ 
			base.LateUpdate();
		}
	}

	// ====================================================
	protected virtual void UpdateWalking() {
		UpdateDirection();
		currentSpeed += currentDirection * horizontalAccel * MinigameTimeManager.instance.deltaTime;
		currentSpeed.x = Mathf.Clamp( currentSpeed.x, -maxHorizontalSpeed, maxHorizontalSpeed );

		Vector2 prevPos = VectorUtils.GetPosition2D( transform.position );
		physicsController.Move( currentSpeed * MinigameTimeManager.instance.deltaTime, false );
		
		// updating the current speed
		currentSpeed = ( VectorUtils.GetPosition2D( transform.position ) - prevPos ) / MinigameTimeManager.instance.deltaTime;
		
		if( currentSpeed.x != 0 ) {
			myTransform.localScale = new Vector3( Mathf.Sign( currentSpeed.x ), 1f, 1f );
		}
	}

	// ====================================================
	public void InstaDeath() {
		Destroy( gameObject );
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
		Vector2 offset = physicsController.colliderCenter + ( physicsController.colliderSize.x * 0.5f + PhysicsController.LINECAST_OFFSET )* currentDirection;
		Vector2 worldPos = VectorUtils.GetPosition2D( myTransform.position ) +  offset;
		Vector2 direction = ( -2 * PhysicsController.LINECAST_OFFSET - physicsController.colliderSize.y * 0.5f ) * Vector2.up + currentDirection * maxHorizontalSpeed * MinigameTimeManager.instance.deltaTime;
		RaycastHit2D raycastHit = Physics2D.Raycast( worldPos, direction.normalized, direction.magnitude, patrolLayerMask.value );
		if( raycastHit.collider == null ) {
			currentDirection *= -1f;
			currentSpeed = Vector2.zero;
			lastDirectionChange = MinigameTimeManager.instance.time;
		}
	}
}
