using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof( BoxCollider2D ) )]
public class PhysicsController : MonoBehaviour {


	protected const float TILE_SIZE = 0.10f;	// TODO: move somewhere else
	protected const float LINECAST_OFFSET = 0.005f;

	// Cache Data
	protected BoxCollider2D boxCollider;
	protected Vector2 colliderCenter;
	protected Vector2 colliderSize;

	//References
	protected List<Vector2> upCollisionPoints = new List<Vector2>();
	protected List<Vector2> downCollisionPoints = new List<Vector2>();
	protected List<Vector2> leftCollisionPoints = new List<Vector2>();
	protected List<Vector2> rightCollisionPoints = new List<Vector2>();

	// Status
	[System.NonSerialized]
	public bool isGrounded;
	[System.NonSerialized]
	public bool didHitCeiling = false;
	[System.NonSerialized]
	public bool didHitLeft = false;
	[System.NonSerialized]
	public bool didHitRight = false;
	[System.NonSerialized]
	public Collider2D lastColliderHit;

	[System.NonSerialized]
	public bool didHitACharacterRight = false;
	[System.NonSerialized]
	public bool didHitACharacterLeft = false;

	protected LayerMask collisionLayerMask;
	protected LayerMask oneWayCollisionLayerMask;
	protected float epsilon = 0.01f;
	protected int offLayer;
	protected int characterLayer;
	protected int oneWayCollisionLayer;

	protected Vector2 leftLowerCollisionPoint;
	protected Vector2 rightLowerCollisionPoint;
	[System.NonSerialized]
	public bool shouldUseSlopes = false;

	// =====================================
	private void Start() {
		offLayer = LayerMask.NameToLayer( "OffLayer" );
		characterLayer = LayerMask.NameToLayer( "PlayerLayer" );
		oneWayCollisionLayer = LayerMask.NameToLayer( "OneWayPlatformLayer" );
		boxCollider = GetComponent<BoxCollider2D>();
		colliderCenter = Vector2.Scale( boxCollider.offset, transform.lossyScale );
		colliderSize = Vector2.Scale( boxCollider.size, transform.lossyScale );
		CreateCollisionPoints();
	}

	// =====================================
	public void SetCollisionLayers( int _collisionLayerMask, int _oneWayCollisionLayerMask ) {
		collisionLayerMask = _collisionLayerMask;
		oneWayCollisionLayerMask = _oneWayCollisionLayerMask;
	}

	// =====================================
	private void OnValidate() {
		boxCollider = GetComponent<BoxCollider2D>();
		boxCollider.offset = Vector2.zero;
	}

	// =====================================
	public void CreateCollisionPoints() {
		upCollisionPoints.Clear();
		downCollisionPoints.Clear();
		leftCollisionPoints.Clear();
		rightCollisionPoints.Clear();

		// create corners...
		Vector2 cornerUR = colliderCenter + colliderSize / 2;
		// to avoid detecting a vertical wall when checking up, we move this corner to the left in LINECAST_OFFSET
		upCollisionPoints.Add( cornerUR );
		// to avoid detecting the ceiling when checking the right, we lower this corner in LINECAST_OFFSET
		rightCollisionPoints.Add( cornerUR );

		Vector2 cornerDL = colliderCenter - colliderSize / 2;
		// to avoid detecting a vertical wall when checking down, we move this corner to the right in LINECAST_OFFSET
		downCollisionPoints.Add( cornerDL );
		// to avoid detecting the ground when checking the left, we raise this corner in LINECAST_OFFSET
		leftCollisionPoints.Add( cornerDL );
		leftLowerCollisionPoint = cornerDL;

		Vector2 cornerDR = colliderCenter + Vector2.Scale( colliderSize, new Vector2( 1f, -1f ) ) / 2f;
		// to avoid detecting a vertical wall when checking down, we move this corner to the left in LINECAST_OFFSET
		downCollisionPoints.Add( cornerDR );
		// to avoid detecting the ground when checking the right, we raise this corner in LINECAST_OFFSET
		rightCollisionPoints.Add( cornerDR );
		rightLowerCollisionPoint = cornerDR;

		Vector2 cornerUL = colliderCenter + Vector2.Scale( colliderSize, new Vector2( -1f, 1f ) ) / 2f;
		// to avoid detecting a vertical wall when checking up, we move this corner to the right in LINECAST_OFFSET
		upCollisionPoints.Add( cornerUL );
		// to avoid detecting the ceiling when checking the left, we lower this corner in LINECAST_OFFSET
		leftCollisionPoints.Add( cornerUL );

		// Add aditional collision points

		int additionalVerticalPoints = ( int ) ( colliderSize.y / TILE_SIZE );
		if( additionalVerticalPoints > 0 ) {
			float deltaY = colliderSize.y / ( additionalVerticalPoints + 1 );
			for( int i = 0; i < additionalVerticalPoints; i++ ) {
				Vector2 point = cornerDR + ( i + 1 ) * deltaY * Vector2.up;
				rightCollisionPoints.Add( point );

				point = cornerDL + ( i + 1 ) * deltaY * Vector2.up;
				leftCollisionPoints.Add( point );
			}
		}

		int additionalHorizontalPoints = ( int )( colliderSize.x / TILE_SIZE );
		if( additionalHorizontalPoints > 0 ) {
			float deltaX = colliderSize.x / ( additionalHorizontalPoints + 1 );
			for( int i = 0; i < additionalHorizontalPoints; i++ ) {
				Vector2 point = cornerDL + ( i + 1 ) * deltaX * Vector2.right;
				downCollisionPoints.Add( point );
				
				point = cornerUL + ( i + 1 ) * deltaX * Vector2.right;
				upCollisionPoints.Add( point );
			}
		}
	}

	// =====================================
	private Vector2 ModifyDirection( Vector2 deltaPos, Vector2 currentPos2D, bool jumpDown, int iteration ) {
	
		if( iteration == 8 ) {
			Log.Warning("ModifyDirection: exceeded max iteration: " + iteration );
			return deltaPos;
		}
		iteration++;
		if( deltaPos.x == 0 ) {
			return deltaPos;
		}

		RaycastHit2D raycastHit;

		LayerMask layerMask = jumpDown ? collisionLayerMask : oneWayCollisionLayerMask;
		Vector2 offset = LINECAST_OFFSET * new Vector2( 0 , -1f );
		Vector2 modifiedMovement = deltaPos + offset;
		Vector2 modifiedMovementDirection = modifiedMovement.normalized;
		float modifiedMovementMagnitude = modifiedMovement.magnitude;
		raycastHit = Physics2D.Raycast( currentPos2D, modifiedMovementDirection, modifiedMovementMagnitude, layerMask.value );
		if( raycastHit.collider != null ) {

			if( raycastHit.collider.gameObject.layer == oneWayCollisionLayer && raycastHit.collider.OverlapPoint( currentPos2D ) ) {
				return deltaPos;
			}
			if( raycastHit.collider.gameObject.layer == oneWayCollisionLayer &&  raycastHit.normal.y < 0 ) {
				return deltaPos;
			}
			// above this value, its considered a wall, so we dont do anything...
			if( Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.right ) ) > 0.7f ) {
				return deltaPos;
			}
			//force to be grounded
			isGrounded = true;
			Vector2 ray = ( raycastHit.point - currentPos2D - offset );
			Vector2 notRay = deltaPos - ray;
			Vector2 slopeVector = ( notRay - Vector2.Dot( raycastHit.normal, notRay ) * raycastHit.normal );
			slopeVector = slopeVector.normalized * Mathf.Abs( notRay.x );
			//iterate again, to check if we find a new slope in the trajectory
			slopeVector = ModifyDirection( slopeVector, currentPos2D + ray, jumpDown, iteration );
			deltaPos = ray + slopeVector;
		}

		return deltaPos;
	}

	// =====================================
	public Vector2 Move( Vector2 deltaPos, bool jumpDown ) {
		Vector2 currentPos2D = transform.position;
		Vector2 worldPoint;
		RaycastHit2D raycastHit;


		lastColliderHit = null;

		//changing the layer to the off layer so the raycast doesn't collide with ourselves (FIXME? HACK?)
		int layer = gameObject.layer;
		gameObject.layer = offLayer;

		bool wasGrounded = isGrounded;
		isGrounded = false;
		didHitCeiling = false;
		didHitLeft = false;
		didHitRight = false;

		if( shouldUseSlopes ) { 
			deltaPos = ModifyDirection( deltaPos, currentPos2D + rightLowerCollisionPoint, jumpDown, 0 );
			deltaPos = ModifyDirection( deltaPos, currentPos2D + leftLowerCollisionPoint, jumpDown, 0 );
		}

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
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.up ) ) > epsilon ) {
				Vector2 ray = ( raycastHit.point - worldPoint - offset );
				deltaPos.y = Mathf.Min( deltaPos.y, ray.y );
				didHitCeiling = true;
				lastColliderHit = raycastHit.collider;
			}
			//Debug.DrawLine( worldPoint, worldPoint + movementDirection *( movementMagnitud + LINECAST_OFFSET ) );
		}

		// check lower collisions
		LayerMask layerMask = jumpDown ? collisionLayerMask : oneWayCollisionLayerMask;
		offset = originalMovement.y < 0 ? -LINECAST_OFFSET * Vector2.up : Vector2.zero;
		modifiedMovement = originalMovement + offset;
		modifiedMovementMagnitude = modifiedMovement.magnitude;
		modifiedMovementDirection = modifiedMovement.normalized;
		foreach( Vector2 point in downCollisionPoints ) {
			worldPoint = point + currentPos2D;
			raycastHit = Physics2D.Raycast( worldPoint, modifiedMovementDirection, modifiedMovementMagnitude, layerMask.value );
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.up ) ) > epsilon ) {
				if( raycastHit.collider.gameObject.layer == oneWayCollisionLayer && raycastHit.collider.OverlapPoint( worldPoint ) ) {
					continue;
				}
				if( raycastHit.collider.gameObject.layer == oneWayCollisionLayer &&  raycastHit.normal.y < 0 ) {
					continue;
				}
				Vector2 ray = ( raycastHit.point - worldPoint - offset );
				deltaPos.y =  Mathf.Max( deltaPos.y, ray.y );
				isGrounded = true;
				lastColliderHit = raycastHit.collider;
			}
			//Debug.DrawLine( worldPoint, worldPoint + movementDirection *( movementMagnitud + LINECAST_OFFSET ) );
		}

		
		// check right collisions
		offset = originalMovement.x > 0 ? LINECAST_OFFSET * Vector2.right : Vector2.zero;
		modifiedMovement = originalMovement + offset;
		modifiedMovementMagnitude = modifiedMovement.magnitude;
		modifiedMovementDirection = modifiedMovement.normalized;
		foreach( Vector2 point in rightCollisionPoints ) {
			worldPoint = point + currentPos2D;
			raycastHit = Physics2D.Raycast( worldPoint, modifiedMovementDirection, modifiedMovementMagnitude, collisionLayerMask.value );
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.right ) ) > epsilon ) {
				Vector2 ray = ( raycastHit.point - worldPoint - offset );
				deltaPos.x = Mathf.Min( deltaPos.x, ray.x );
				didHitRight = true;
				lastColliderHit = raycastHit.collider;
				didHitACharacterRight = raycastHit.collider.gameObject.layer == characterLayer;
			}
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
			if( raycastHit.collider != null && Mathf.Abs( Vector2.Dot( raycastHit.normal, Vector2.right ) ) > epsilon ) {
				Vector2 ray = ( raycastHit.point - worldPoint - offset );
				deltaPos.x = Mathf.Max( deltaPos.x, ray.x );
				didHitLeft = true;
				lastColliderHit = raycastHit.collider;
				didHitACharacterLeft = raycastHit.collider.gameObject.layer == characterLayer;
			}
			//Debug.DrawLine( worldPoint, worldPoint + movementDirection *( movementMagnitud + LINECAST_OFFSET ) );
		}

		//turning on the collider on again(FIXME? HACK?)
		gameObject.layer = layer;
		transform.Translate( deltaPos, Space.World );
		return deltaPos;
	}

	// =====================================
	public void ChangeColliderHeight( float newHeight ) {
		if( newHeight == colliderSize.y ) {
			return;
		}

		if( newHeight > colliderSize.y ) {
			// Check collisions above...
			boxCollider.enabled = false;
			Vector2 currentPos2D = transform.position;
			Vector2 movementDirection = Vector2.up;
			float movementMagnitud = newHeight - colliderSize.y;
			Vector2 offset = -movementDirection * LINECAST_OFFSET;
			foreach( Vector2 point in upCollisionPoints ) {
				//TODO: refactor in a function
				Vector2 worldPoint = point + currentPos2D + offset;
				RaycastHit2D raycastHit = Physics2D.Raycast( worldPoint, Vector2.up, movementMagnitud + LINECAST_OFFSET, collisionLayerMask.value );
				if( raycastHit.collider != null && Vector2.Dot( raycastHit.normal, Vector2.up ) != 0 ) {
					float deltaY = ( ( raycastHit.point - worldPoint ) + offset ).y;
					if ( deltaY < movementMagnitud ) {
						boxCollider.enabled = true;
						return;
					}
				}
			}
			boxCollider.enabled = true;
		}
		float oldBottomPosY = colliderCenter.y - colliderSize.y * 0.5f;

		boxCollider.size = new Vector2( boxCollider.size.x, newHeight );
		boxCollider.offset = Vector2.up * ( oldBottomPosY + newHeight * 0.5f );
		colliderSize = boxCollider.size;
		colliderCenter = boxCollider.offset;
		CreateCollisionPoints();
	}

	// =====================================
	public void OnDrawGizmosSelected() {
		Vector3 pos = transform.position;
		Gizmos.color = new Color( 1f, 0, 0, 0.5f );
		if( downCollisionPoints != null ) {
			foreach( Vector2 point in downCollisionPoints ) {
				Vector3 point3D = new Vector3( point.x, point.y, transform.position.z ) + pos;
				Gizmos.DrawSphere( point3D, 0.005f );
			}
		}
		if( leftCollisionPoints != null ) {
			foreach( Vector2 point in leftCollisionPoints ) {
				Vector3 point3D = new Vector3( point.x, point.y, transform.position.z ) + pos;
				Gizmos.DrawSphere( point3D, 0.005f );
			}
		}
		if( rightCollisionPoints != null ) {
			foreach( Vector2 point in rightCollisionPoints ) {
				Vector3 point3D = new Vector3( point.x, point.y, transform.position.z ) + pos;
				Gizmos.DrawSphere( point3D, 0.005f );
			}
		}
		if( upCollisionPoints != null ) {
			foreach( Vector2 point in upCollisionPoints ) {
				Vector3 point3D = new Vector3( point.x, point.y, transform.position.z ) + pos;
				Gizmos.DrawSphere( point3D, 0.005f );
			}
		}
	}
}

public enum CollisionType {
	NOTHING = 0,
	GROUND = 1,
	LEFT_WALL = 2,
	RIGHT_WALL = 4,
	CEILING = 8
}

