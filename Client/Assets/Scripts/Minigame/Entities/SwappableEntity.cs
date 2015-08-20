using UnityEngine;
using System.Collections;

[RequireComponent (typeof( PhysicsController ) )]
public class SwappableEntity : Entity {

	[HideInInspector]
	public PhysicsController physicsController;

	[HideInInspector]
	public Vector2 currentSpeed = Vector2.zero;
	public float groundFrictionAccel = 5f;
	protected Transform myTranform;

	protected RoomController room;
	protected bool isEnabled{ get{ return MinigameManager.instance.currentRoom == room; } }

	// ====================================================
	protected virtual void Awake() {
		Rigidbody2D rigidbody2D = GetComponent<Rigidbody2D>();
		if( rigidbody2D == null ) {
			rigidbody2D = gameObject.AddComponent<Rigidbody2D>();
		}
		rigidbody2D.isKinematic = true;
		myTranform = transform;
		SwappableEntityHitboxReference[] hitboxReferences = GetComponentsInChildren<SwappableEntityHitboxReference>();
		foreach( SwappableEntityHitboxReference hitboxReference in hitboxReferences ) {
			hitboxReference.swappableEntity = this;
		}
	}

	// ====================================================
	public virtual void Start() {
		physicsController = GetComponent<PhysicsController>();
		physicsController.shouldUseSlopes = true;
		physicsController.SetCollisionLayers( LayerMask.GetMask(new string[] { "InvisibleWallLayer", "LevelLayer", "MoveBoxLayer" } ),
		                                     LayerMask.GetMask(new string[] { "InvisibleWallLayer", "LevelLayer", "MoveBoxLayer", "OneWayPlatformLayer" } ) );
		physicsController.Initialize();
	}

	// ====================================================
	public void Configure( RoomController _room ) {
		room = _room;
	}

	//=====================================
	public Vector2 GetPosition(){
		return myTranform.position;
	}
	
	//=====================================
	public Quaternion GetRotation(){
		return myTranform.rotation; 
	}
	
	//=====================================
	public Vector2 GetVelocity(){
		return currentSpeed;
	}

	//=====================================
	public void SetPosition( Vector2 pos ){
		myTranform.position = VectorUtils.GetPosition3D( pos );
	}
	
	//=====================================
	public void SetRotation( Quaternion rot ){
		myTranform.rotation = rot;
	}
	
	//=====================================
	public void SetVelocity( Vector2 vel ){
		currentSpeed = vel;
	}
	
	// ====================================================
	protected virtual void LateUpdate() {

		float deltaTime = MinigameTimeManager.instance.deltaTime;

		// Apply Gravity
		currentSpeed -= Vector2.up * GlobalConfig.instance.gravityAccel * deltaTime;

		if( physicsController.isGrounded ) {
			ApplyFriction( groundFrictionAccel );
		}
		else {
			ApplyFriction( GlobalConfig.instance.airFrictionHoizontalAccel );
		}

		currentSpeed.x = Mathf.Clamp( currentSpeed.x, -GlobalConfig.instance.maxHorizontalSpeed, GlobalConfig.instance.maxHorizontalSpeed );
		currentSpeed.y = Mathf.Clamp( currentSpeed.y, -GlobalConfig.instance.maxVerticalSpeed, float.MaxValue );
		
		Vector2 prevPos = VectorUtils.GetPosition2D( myTranform.position );
		physicsController.Move( currentSpeed * deltaTime, false );
		
		// updating the current speed
		currentSpeed = ( VectorUtils.GetPosition2D( myTranform.position ) - prevPos ) / MinigameTimeManager.instance.deltaTime;
		//		currentSpeed.x = ( physicsController.didHitLeft || physicsController.didHitRight ) ? 0 : currentSpeed.x;
		//		currentSpeed.y = ( physicsController.didHitCeiling || physicsController.isGrounded ) ? 0 : currentSpeed.y;
		
		if( currentSpeed.x != 0 ) {
			myTranform.localScale = new Vector3( Mathf.Sign( currentSpeed.x ), 1f, 1f );
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

	//=====================================
	public virtual void OnSwap(){
		// nop
	}

	// ====================================================
	public void InstaDeath() {
		SoundManager.instance.PlaySound( SoundManager.SoundType.EnemyDeath );
		Destroy( gameObject );
	}

	// ====================================================
	public override void _OnDrawGizmos( Vector3 position ) {		
		BoxCollider2D boxCollider2D = GetComponent<BoxCollider2D>();
		Gizmos.DrawWireCube( position, boxCollider2D.size );
		
	}
}
