using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlatformPhysicsController : PhysicsController {

	public Dictionary<Collider2D, CollisionInfo> currentCollisions = new Dictionary<Collider2D, CollisionInfo>();

	// =====================================
	public void Move( Vector2 deltaPos, Vector2 fromPosition ) {

		currentCollisions.Clear();

		Vector2 currentPos2D = fromPosition;
		Vector2 worldPoint;
		RaycastHit2D raycastHit;
		
		
		//turning off the collider so the raycast doesn't collide with ourselves (FIXME? HACK?)
		boxCollider.enabled = false;
		
		isGrounded = false;
		didHitCeiling = false;
		didHitLeft = false;
		didHitRight = false;

		Vector2 originalMovement = deltaPos;
		float movementMagnitude = deltaPos.magnitude;
		Vector2 movementDirection = deltaPos.normalized;

		// check upper collisions
		Vector2 offset = originalMovement.y > 0 ? LINECAST_OFFSET * Vector2.up : Vector2.zero;
		Vector2 modifiedMovement = originalMovement + offset;
		float modifiedMovementMagnitude = modifiedMovement.magnitude;
		Vector2 modifiedMovementDirection = modifiedMovement.normalized;
		foreach( Vector2 point in upCollisionPoints ) {

			worldPoint = point + currentPos2D;
			raycastHit = Physics2D.Raycast( worldPoint, modifiedMovementDirection, modifiedMovementMagnitude, collisionLayerMask.value );
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.up ) ) > epsilon
			   && !currentCollisions.ContainsKey( raycastHit.collider ) ) {
				Vector2 deltaPosCharacter = Vector2.zero;
				Vector2 ray = ( raycastHit.point - worldPoint );
				deltaPosCharacter.y = Mathf.Max( deltaPos.y, ray.y );
				deltaPosCharacter.y = deltaPosCharacter.y < 0f ? 0f : deltaPosCharacter.y;
				currentCollisions.Add( raycastHit.collider, new CollisionInfo{
					collider = raycastHit.collider,
					deltaMovement = deltaPosCharacter,
					forcedMovementDirection = Vector2.up
				} );
			}
		
			//check up
			Vector2 worldPoint2 = point + currentPos2D;
			raycastHit = Physics2D.Raycast( worldPoint2, Vector2.up, 0.01f, collisionLayerMask.value );
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.up ) ) > epsilon
			   && !currentCollisions.ContainsKey( raycastHit.collider ) ) {
				currentCollisions.Add( raycastHit.collider, new CollisionInfo{
					collider = raycastHit.collider,
					deltaMovement = deltaPos,
					forcedMovementDirection = Vector2.up
				} );
			}
		}

		// check lower collisions
		offset = originalMovement.y < 0 ? -LINECAST_OFFSET * Vector2.up : Vector2.zero;
		modifiedMovement = originalMovement + offset;
		modifiedMovementMagnitude = modifiedMovement.magnitude;
		modifiedMovementDirection = modifiedMovement.normalized;
		foreach( Vector2 point in downCollisionPoints ) {

			worldPoint = point + currentPos2D;
			raycastHit = Physics2D.Raycast( worldPoint, modifiedMovementDirection, modifiedMovementMagnitude, collisionLayerMask.value );
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, -Vector2.up ) ) > epsilon
			   && !currentCollisions.ContainsKey( raycastHit.collider ) ) {
				Vector2 deltaPosCharacter = Vector2.zero;
				Vector2 ray = ( raycastHit.point - worldPoint );
				deltaPosCharacter.y = Mathf.Min( deltaPos.y, ray.y );
				deltaPosCharacter.y = deltaPosCharacter.y > 0f ? 0f : deltaPosCharacter.y;
				currentCollisions.Add( raycastHit.collider, new CollisionInfo{
					collider = raycastHit.collider,
					deltaMovement = deltaPosCharacter,
					forcedMovementDirection = -Vector2.up
				} );
			}

//			//check down
//			Vector2 worldPoint2 = point + currentPos2D;
//			raycastHit = Physics2D.Raycast( worldPoint2, -Vector2.up, 0.01f, collisionLayerMask.value );
//			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.up ) ) > epsilon
//			   && !currentCollisions.ContainsKey( raycastHit.collider ) ) {
//				currentCollisions.Add( raycastHit.collider, new CollisionInfo{
//					collider = raycastHit.collider,
//					deltaMovement = deltaPos,
//					forcedMovementDirection =  -Vector2.up
//				} );
//			}
		}

		// check right collisions
		offset = originalMovement.x > 0 ? LINECAST_OFFSET * Vector2.right : Vector2.zero;
		modifiedMovement = originalMovement + offset;
		modifiedMovementMagnitude = modifiedMovement.magnitude;
		modifiedMovementDirection = modifiedMovement.normalized;
		foreach( Vector2 point in rightCollisionPoints ) {

			worldPoint = point + currentPos2D;
			raycastHit = Physics2D.Raycast( worldPoint, modifiedMovementDirection, modifiedMovementMagnitude, collisionLayerMask.value );
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.right ) ) > epsilon
			   && !currentCollisions.ContainsKey( raycastHit.collider ) ) {
				//Vector2 deltaPosCharacter = modifiedMovement - ( raycastHit.point - worldPoint );
				//Vector2 deltaPosCharacter = deltaPos;
				Vector2 deltaPosCharacter = Vector2.zero;
				Vector2 ray = ( raycastHit.point - worldPoint );
				deltaPosCharacter.x = Mathf.Max( deltaPos.x, ray.x );
				deltaPosCharacter.x = deltaPosCharacter.x < 0f ? 0f : deltaPosCharacter.x;
				currentCollisions.Add( raycastHit.collider, new CollisionInfo{
					collider = raycastHit.collider,
					deltaMovement = deltaPosCharacter,
					forcedMovementDirection = Vector2.right
				} );
			}


			//check right
//			Vector2 worldPoint2 = point + currentPos2D;
//			raycastHit = Physics2D.Raycast( worldPoint2, Vector2.right, 0.01f, collisionLayerMask.value );
//			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.right ) ) > epsilon
//			   && !currentCollisions.ContainsKey( raycastHit.collider ) ) {
//				currentCollisions.Add( raycastHit.collider, new CollisionInfo{
//					collider = raycastHit.collider,
//					deltaMovement = deltaPos,
//					forcedMovementDirection = Vector2.right
//				} );
//			}
			//Debug.DrawLine( worldPoint, worldPoint + movementDirection *( movementMagnitud + LINECAST_OFFSET ) );
		}

		// check left collisions
		offset = originalMovement.x < 0 ? -LINECAST_OFFSET * Vector2.right : Vector2.zero;
		modifiedMovement = originalMovement + offset;
		modifiedMovementMagnitude = modifiedMovement.magnitude;
		modifiedMovementDirection = modifiedMovement.normalized;
		foreach( Vector2 point in leftCollisionPoints ) {

			worldPoint = point + currentPos2D;
			raycastHit = Physics2D.Raycast( worldPoint, movementDirection, movementMagnitude + LINECAST_OFFSET, collisionLayerMask.value );
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.right ) ) > epsilon
			   && !currentCollisions.ContainsKey( raycastHit.collider ) ) {
				Vector2 deltaPosCharacter = Vector2.zero;
				Vector2 ray = ( raycastHit.point - worldPoint );
				deltaPosCharacter.x = Mathf.Min( deltaPos.x, ray.x );
				deltaPosCharacter.x = deltaPosCharacter.x > 0f ? 0f : deltaPosCharacter.x;
				currentCollisions.Add( raycastHit.collider, new CollisionInfo{
					collider = raycastHit.collider,
					deltaMovement = deltaPosCharacter,
					forcedMovementDirection = -Vector2.right
				} );
			}


			//check left
			Vector2 worldPoint2 = point + currentPos2D;
			raycastHit = Physics2D.Raycast( worldPoint2, -Vector2.right, 0.01f, collisionLayerMask.value );
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.right ) ) > epsilon
			   && !currentCollisions.ContainsKey( raycastHit.collider ) ) {
				currentCollisions.Add( raycastHit.collider, new CollisionInfo{
					collider = raycastHit.collider,
					deltaMovement = deltaPos,
					forcedMovementDirection = -Vector2.right
				} );
			}
			//Debug.DrawLine( worldPoint, worldPoint + movementDirection *( movementMagnitud + LINECAST_OFFSET ) );
		}

		//turning on the collider on again(FIXME? HACK?)
		boxCollider.enabled = true;
	}

}

public struct CollisionInfo {
	public Collider2D collider;
	public Vector2 deltaMovement;
	public Vector2 forcedMovementDirection;
}
