using UnityEngine;
using System.Collections;

[RequireComponent (typeof( PlatformPhysicsController ) )]
public class MovablePlatform : MonoBehaviour {

	// Status Vars
	private Vector3 lastPosition;
	private Transform myTransform;

	// References
	private PlatformPhysicsController platformPhysicsController;

	// ====================================================
	void Start () {
		myTransform = transform;
		lastPosition = myTransform.position;
		platformPhysicsController = GetComponent<PlatformPhysicsController>();
		platformPhysicsController.Initialize();
		platformPhysicsController.SetCollisionLayers( LayerMask.GetMask( new string[] { "MoveBoxLayer" } ), LayerMask.GetMask( new string[] { "MoveBoxLayer" } ) ); 
	}
	
	// ====================================================
	void LateUpdate () {

		//animator.Update( MinigameTimeManager.instance.deltaTime );

		Vector2 deltaPos = myTransform.position - lastPosition;
		platformPhysicsController.Move( deltaPos, lastPosition );

		foreach( CollisionInfo collisionInfo in platformPhysicsController.currentCollisions.Values ) {
			PhysicsController other = collisionInfo.collider.GetComponent<PhysicsController>();
			if( other == null || other is PlatformPhysicsController ) {
				continue;
			}
			Vector2 deltaMovement = collisionInfo.deltaMovement;

			PlayerController player = other.GetComponent<PlayerController>();
			if( player != null ) {
				if( collisionInfo.forcedMovementDirection == Vector2.right && !player.isGrabbingToWall ) {
					deltaMovement.y = 0;
				}

				if( collisionInfo.forcedMovementDirection == -Vector2.right && !player.isGrabbingToWall ) {
					deltaMovement.y = 0;
				}
			}

			if( collisionInfo.forcedMovementDirection == -Vector2.up ) {
				deltaMovement.y = deltaMovement.y > 0 ? 0 : deltaMovement.y;
			}
			Vector2 deltaResult = other.Move( deltaMovement, false );

			if( collisionInfo.forcedMovementDirection == Vector2.up ) {
				other.isGrounded = true;
				if( deltaResult.y < deltaMovement.y ) {
					Squash( other );
				}
			}
			else if( collisionInfo.forcedMovementDirection == -Vector2.up ) {
				other.didHitCeiling = true;
				if( deltaResult.y > deltaMovement.y ) {
					Squash( other );
				}
			}
			else if( collisionInfo.forcedMovementDirection == Vector2.right ) {
				other.didHitLeft = true;
				if( deltaResult.x < deltaMovement.x ) {
					Squash( other );
				}
			}
			else if( collisionInfo.forcedMovementDirection == -Vector2.right ) {
				other.didHitRight = true;
				if( deltaResult.x > deltaMovement.x ) {
					Squash( other );
				}
			}

		}


		lastPosition = myTransform.position;
	}

	// ====================================================
	void Squash( PhysicsController other ) {
		PlayerController player = other.GetComponent<PlayerController>();
		if( player != null ) {
			player.InstaDeath();
			return;
		}
		PatrollingEnemy patroller = other.GetComponent<PatrollingEnemy>();
		if( patroller != null ) {
			patroller.InstaDeath();
			return;
		}
		Destroy( other.gameObject );
	}
}
