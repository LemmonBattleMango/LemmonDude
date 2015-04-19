using UnityEngine;
using System.Collections;

[RequireComponent (typeof( PlatformPhysicsController ) )]
public class MovablePlatform : MonoBehaviour {

	// Status Vars
	private Vector3 lastPosition;
	private Transform myTransform;
	private Animator animator;

	// References
	private PlatformPhysicsController platformPhysicsController;

	// ====================================================
	void Start () {
		myTransform = transform;
		lastPosition = myTransform.position;
		platformPhysicsController = GetComponent<PlatformPhysicsController>();
		animator = GetComponent<Animator>();
//		animator.enabled = false;
		platformPhysicsController.SetCollisionLayers( LayerMask.GetMask( new string[] { "PlayerLayer" } ), LayerMask.GetMask( new string[] { "PlayerLayer" } ) ); 
	}
	
	// ====================================================
	void LateUpdate () {

		//animator.Update( MinigameTimeManager.instance.deltaTime );

		Vector2 deltaPos = myTransform.position - lastPosition;
		platformPhysicsController.Move( deltaPos, lastPosition );

		foreach( CollisionInfo collisionInfo in platformPhysicsController.currentCollisions.Values ) {
			PlayerController player = collisionInfo.collider.GetComponent<PlayerController>();
			if( player == null ) {
				continue;
			}
			Vector2 deltaMovement = collisionInfo.deltaMovement;

			if( collisionInfo.forcedMovementDirection == Vector2.right && !player.isGrabbingToWall ) {
				deltaMovement.y = 0;
			}

			if( collisionInfo.forcedMovementDirection == -Vector2.right && !player.isGrabbingToWall ) {
				deltaMovement.y = 0;
			}

			if( collisionInfo.forcedMovementDirection == -Vector2.up ) {
				deltaMovement.x = 0;
				deltaMovement.y = deltaMovement.y > 0 ? 0 : deltaMovement.y;
			}
			Vector2 deltaResult = player.physicsController.Move( deltaMovement, false );

			if( collisionInfo.forcedMovementDirection == Vector2.up ) {
				player.physicsController.isGrounded = true;
				if( deltaResult.y < deltaMovement.y ) {
					player.Squash();
				}
			}
			else if( collisionInfo.forcedMovementDirection == -Vector2.up ) {
				player.physicsController.didHitCeiling = true;
				if( deltaResult.y > deltaMovement.y ) {
					player.Squash();
				}
			}
			else if( collisionInfo.forcedMovementDirection == Vector2.right ) {
				player.physicsController.didHitLeft = true;
				if( deltaResult.x < deltaMovement.x ) {
					player.Squash();
				}
			}
			else if( collisionInfo.forcedMovementDirection == -Vector2.right ) {
				player.physicsController.didHitRight = true;
				if( deltaResult.x > deltaMovement.x ) {
					player.Squash();
				}
			}

		}


		lastPosition = myTransform.position;
	}
}
