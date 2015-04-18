﻿using UnityEngine;
using System.Collections;

[RequireComponent (typeof( PhysicsController ) )]
public class SwappableEntity : MonoBehaviour {

	[HideInInspector]
	public PhysicsController physicsController;
	
	private Vector2 currentSpeed = Vector2.zero;
	public float groundFrictionAccel = 5f;

	// ====================================================
	public void Start() {
		physicsController = GetComponent<PhysicsController>();
		physicsController.shouldUseSlopes = true;
		physicsController.SetCollisionLayers( LayerMask.GetMask(new string[] { "LevelLayer", "PlayerLayer" } ),
		                                     LayerMask.GetMask(new string[] { "LevelLayer", "PlayerLayer", "OneWayPlatformLayer" } ) );
	}

	//=====================================
	public Vector2 GetPosition(){
		return transform.position;
	}
	
	//=====================================
	public Quaternion GetRotation(){
		return transform.rotation; 
	}
	
	//=====================================
	public Vector2 GetVelocity(){
		return currentSpeed;
	}

	//=====================================
	public void SetPosition( Vector2 pos ){
		transform.position = VectorUtils.GetPosition3D( pos );
	}
	
	//=====================================
	public void SetRotation( Quaternion rot ){
		transform.rotation = rot;
	}
	
	//=====================================
	public void SetVelocity( Vector2 vel ){
		currentSpeed = vel;
	}
	
	// ====================================================
	private void LateUpdate() {

		float deltaTime = MinigameTimeManager.instance.deltaTime;

		// Apply Gravity
		currentSpeed -= Vector2.up * GlobalConfig.instance.gravityAccel * deltaTime;

		if( physicsController.isGrounded ) {
			ApplyFriction( groundFrictionAccel );
		}

		currentSpeed.x = Mathf.Clamp( currentSpeed.x, -GlobalConfig.instance.maxHorizontalSpeed, GlobalConfig.instance.maxHorizontalSpeed );
		currentSpeed.y = Mathf.Clamp( currentSpeed.y, -GlobalConfig.instance.maxVerticalSpeed, float.MaxValue );
		
		Vector2 prevPos = VectorUtils.GetPosition2D( transform.position );
		physicsController.Move( currentSpeed * deltaTime, false );
		
		// updating the current speed
		currentSpeed = ( VectorUtils.GetPosition2D( transform.position ) - prevPos ) / MinigameTimeManager.instance.deltaTime;
		//		currentSpeed.x = ( physicsController.didHitLeft || physicsController.didHitRight ) ? 0 : currentSpeed.x;
		//		currentSpeed.y = ( physicsController.didHitCeiling || physicsController.isGrounded ) ? 0 : currentSpeed.y;
		
		if( currentSpeed.x != 0 ) {
			transform.localScale = new Vector3( Mathf.Sign( currentSpeed.x ), 1f, 1f );
		}
	}

	// ====================================================
	private void ApplyFriction( float frictionAcc ) {
		if( currentSpeed.x == 0 ) {
			return;
		}
		float deltaXSpeed = frictionAcc * MinigameTimeManager.instance.deltaTime;
		if( Mathf.Abs( currentSpeed.x ) <= deltaXSpeed ) {
			currentSpeed.x = 0;
			return;
		}
		
		currentSpeed.x -= Mathf.Sign( currentSpeed.x ) * deltaXSpeed;
	}
}
