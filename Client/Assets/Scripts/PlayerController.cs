using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[RequireComponent (typeof( PhysicsController ) )]
public class PlayerController : MonoBehaviour {

	public enum ProjectileDirection {
		UP,
		DOWN,
		FORWARD,
		FORWARD_UP,
		FORWARD_DOWN
	}

	[System.Serializable]
	public struct ProjectileSpawnPointInfo {
		public ProjectileDirection direction;
		public Transform spawnPoint;
	}

	//Player Config
	public float maxVerticalSpeed { get{ return GlobalConfig.instance.maxVerticalSpeed * movementSpeedFactor; } }
	public float maxHorizontalSpeed { get{ return GlobalConfig.instance.maxHorizontalSpeed * movementSpeedFactor; } }
	public float walkingAccel { get{ return GlobalConfig.instance.walkingAccel; } }
	public float groundFrictionAccel { get{ return GlobalConfig.instance.groundFrictionAccel; } }
	public float airFrictionAccel { get{ return GlobalConfig.instance.airFrictionAccel; } }
	public float airHorizontalAccel { get{ return GlobalConfig.instance.airHorizontalAccel; } }
	public float wallFrictionAcc { get{ return GlobalConfig.instance.wallFrictionAcc; } }
	public float wallMaxVeticalSpeed { get{ return GlobalConfig.instance.wallMaxVeticalSpeed; } }
	public float airJumpHeight { get{ return GlobalConfig.instance.airJumpHeight; } }
	public float maxJumpHeight { get{ return GlobalConfig.instance.maxJumpHeight; } }
	public float minJumpHeight { get{ return GlobalConfig.instance.minJumpHeight; } }

	public float airJumpSpeed { get{ return CalculateSpeed( airJumpHeight ); } }
	public float groundJumpSpeed { get{ return CalculateSpeed( minJumpHeight ); } }
	public float wallJumpSpeed { get{ return CalculateSpeed( minJumpHeight ); } }
	public float crouchingColliderHeight { get{ return GlobalConfig.instance.crouchingColliderHeight; } }
	public float projectileThrowingValue { get{ return GlobalConfig.instance.projectileThrowingValue; } }

	public float attackMovementImpulse { get{ return GlobalConfig.instance.attackMovementImpulse; } }

	public float attackSpeed { get{ return GlobalConfig.instance.attackSpeed; } }
	public float hitActiveDuration { get{ return GlobalConfig.instance.hitActiveDuration; } }
	public float respawnDelay { get{ return GlobalConfig.instance.respawnDelay; } }
	public float delayBetweenProjectiles { get{ return GlobalConfig.instance.delayBetweenProjectiles / attackSpeedFactor; } }
	public int initialHP { get{ return GlobalConfig.instance.initialHP; } }

	public float knockBackDuration { get{ return GlobalConfig.instance.knockBackDuration; } }
	public float swordKnockBackSpeed { get{ return GlobalConfig.instance.swordKnockBackSpeed; } }

	public float wallJumpHorizontalSpeed { get{ return GlobalConfig.instance.wallJumpHorizontalSpeed; } }

	public float gravityAccel { get{ return Mathf.Sqrt( movementSpeedFactor ) * GlobalConfig.instance.gravityAccel; } }

	// References
	private JoystickController joystickController;
	[HideInInspector]
	public PhysicsController physicsController;
	private Animator animator;
	private AnimationListener animationListener;

	// Status
	[System.NonSerialized]
	public bool isDead;
	[System.NonSerialized]
	public Vector2 currentSpeed;
	[System.NonSerialized]
	private float movementSpeedFactor = 1f;
	[System.NonSerialized]
	private float attackSpeedFactor = 1f;

	private float nextFireTime;
	private float nextProjectileTime;
	[HideInInspector]
	public bool isAttacking;
	[HideInInspector]
	public bool isGrabbingToWall;

	private bool hasDoubleJumped;
	public int hp;

	private CollisionType lastJumpableSurface;
	public float jumpTimeRangeSecs = 0.1f;
	private float lastJumpButtonTime;
	private float lastAbleToJumpTime;
	private bool hasPendingJump;

	private bool isJumpingFromGround;
	private float lastJumpFromGroundTime;

	private bool isJumpingFromWall;
	private float lastJumpFromWallTime;

	public System.Action<PlayerController, PlayerController> onDeath;
	
	private bool hasKnockback;
	
	public List <ProjectileSpawnPointInfo> projectileSpawnPointInfos;
	private Dictionary<ProjectileDirection, Vector2> directionsCache = new Dictionary<ProjectileDirection, Vector2> {
		{ ProjectileDirection.DOWN, -Vector2.up },
		{ ProjectileDirection.UP, Vector2.up },
			{ ProjectileDirection.FORWARD, Vector2.right },
				{ ProjectileDirection.FORWARD_DOWN, new Vector2( 1f, -1f ).normalized },
		{ ProjectileDirection.FORWARD_UP, new Vector2( 1f, 1f ).normalized }
	};
	public ProjectileController projectilePrefab;

	// ====================================================
	private void Start() {
		Initialize();
	}

	// ====================================================
	public void Initialize() {

		nextFireTime = nextProjectileTime = MinigameTimeManager.instance.time;

		joystickController = GetComponent<JoystickController>();

		hp = initialHP;
		animationListener = GetComponentInChildren<AnimationListener>();
		MinigameTimeManager.instance.onTimeScaleChanged += OnTimeScaleChangedHandler;

		animator = GetComponentInChildren<Animator>();

		physicsController = GetComponent<PhysicsController>();
		physicsController.shouldUseSlopes = true;
		physicsController.SetCollisionLayers( LayerMask.GetMask(new string[] { "LevelLayer", "PlayerLayer" } ),
		                                     LayerMask.GetMask(new string[] { "LevelLayer", "PlayerLayer", "OneWayPlatformLayer" } ) );
			 
		isDead = false;	
	}

	// ====================================================
	public void OnDestroy() {
		if( MinigameTimeManager.instance != null ) {
			MinigameTimeManager.instance.onTimeScaleChanged -= OnTimeScaleChangedHandler;
		}
	}

	// ====================================================
	public void OnTimeScaleChangedHandler() {
		animator.speed = MinigameTimeManager.instance.timeScale;
	}

	// ====================================================
	public void Respawn() {
		isDead = false;

		transform.GetComponent<Rigidbody2D>().GetComponent<Collider2D>().enabled = true;
		transform.position = MinigameLevelReferences.instance.spawnPoint.position;
		hp = initialHP;
		currentSpeed = Vector2.zero;
		nextProjectileTime = 0;
		nextFireTime = 0;
		physicsController.SetCollisionLayers( LayerMask.GetMask( new string[] { "LevelLayer", "PlayerLayer" } ),
		                                     LayerMask.GetMask( new string[] { "LevelLayer", "PlayerLayer", "OneWayPlatformLayer" } ) );

	
		animator.SetBool( "isDeath", false );
		gameObject.layer = MinigameManager.instance.playerLayer;
		Director.instance.OnPlayerSpawn( this );
	}

	// ====================================================
	public void ApplyDamage( int damage, Vector2 knockBackVelocity, PlayerController damageDealer, bool validateDeath ) {
	}

	// ====================================================
	public void ApplyKnockBack( Vector2 knockbackVelocity, float duration ) {
		StartCoroutine( ApplyKnockBackCoroutine( knockbackVelocity, duration ) );
	}

	// ====================================================
	public IEnumerator ApplyKnockBackCoroutine( Vector2 knockbackVelocity, float duration ) {
		hasKnockback = true;
		currentSpeed = knockbackVelocity;
		yield return StartCoroutine( MinigameTimeManager.instance.WaitForSecs( duration ) );
		hasKnockback = false;
	}

	// ====================================================
	private float CalculateSpeed( float targetHeight ) {
		return Mathf.Sqrt( 2f * gravityAccel * targetHeight );
	}

	// ====================================================
	void Die( PlayerController damageDealer ) {
		if( isDead ) {
			return;
		}

		Director.instance.ScreenShake( 0.05f, 0.1f );

		isDead = true;
		animator.SetBool( "isDeath", true );
		SoundManager.instance.PlaySound(SoundManager.SoundType.Death);

		//yield return StartCoroutine( MinigameTimeManager.instance.WaitForSecs( 0.6f ) );
		gameObject.layer = MinigameManager.instance.deathLayer;
		physicsController.SetCollisionLayers( LayerMask.GetMask( new string[] { "LevelLayer" } ),
		                                     LayerMask.GetMask( new string[] { "LevelLayer", "OneWayPlatformLayer" } ) );
		
		//transform.position = new Vector3( 1000f, 1000f, -1f );
		if( onDeath != null ) {
			onDeath( this, damageDealer );
		}
	}

	// ====================================================
	private IEnumerator DieCoroutine( PlayerController damageDealer ) {
		//revive after X seconds
		yield return new WaitForSeconds( 3f );
		Respawn();
	}
	
	// ====================================================
	private void LateUpdate() {

		isGrabbingToWall = false;

		float deltaTime = MinigameTimeManager.instance.deltaTime;

		bool jumpButtonDown = false;
		bool jumpButtonHeld = false;
		bool fireButtonDown = false;
		Vector2 direction = Vector2.zero;
	
		if( !isDead ) {
			direction = joystickController.GetDirection();
			fireButtonDown = joystickController.GetFireButtonDown();

			if( !hasKnockback ) {
				jumpButtonDown = joystickController.GetJumpButtonDown();
				jumpButtonHeld = joystickController.GetJumpButton();
			}
		}

		Vector2 movementDirection = Vector2.Scale( direction, Vector2.right );

		bool isNextToWall = ( ( physicsController.didHitLeft && !physicsController.didHitACharacterLeft ) ||
		                     ( physicsController.didHitRight && !physicsController.didHitACharacterRight ) );

		float fallingDownFactor = 1.0f;
		if( currentSpeed.y > 0f && isNextToWall ) {
			fallingDownFactor = 0.7f;
		}

		// Apply Gravity
		currentSpeed -= Vector2.up * gravityAccel * deltaTime * fallingDownFactor;

		if( physicsController.isGrounded || physicsController.didHitLeft || physicsController.didHitRight ) {
			lastAbleToJumpTime = MinigameTimeManager.instance.time;
		}

		if( jumpButtonDown ) {
			lastJumpButtonTime = MinigameTimeManager.instance.time;
			hasPendingJump = true;
		}

		bool canJump = ( Mathf.Abs( lastAbleToJumpTime - lastJumpButtonTime ) <= jumpTimeRangeSecs ) && hasPendingJump;

		if( isJumpingFromGround ) {
			float traveledHeight = ( MinigameTimeManager.instance.time - lastJumpFromGroundTime ) * groundJumpSpeed;
			bool canKeepJumping = ( traveledHeight < ( maxJumpHeight - minJumpHeight ) );
			if( canKeepJumping ) {
				if( jumpButtonHeld ) {
					currentSpeed.y = groundJumpSpeed;
				}
				else{
					isJumpingFromGround = false;
				}
			}
			else {
				isJumpingFromGround = false;
				currentSpeed.y = CalculateSpeed( maxJumpHeight - traveledHeight );
			}
		}
		else if( isJumpingFromWall ) {
			float traveledHeight = ( MinigameTimeManager.instance.time - lastJumpFromWallTime ) * wallJumpSpeed;
			bool canKeepJumping = ( traveledHeight < ( maxJumpHeight - minJumpHeight ) );
			if( canKeepJumping ) {
				if( jumpButtonHeld ) {
					currentSpeed.y = groundJumpSpeed;
				}
				else{
					isJumpingFromWall = false;
				}
			}
			else {
				isJumpingFromWall = false;
				currentSpeed.y = CalculateSpeed( maxJumpHeight - traveledHeight );
			}
		}

		if( physicsController.isGrounded ) {
			hasDoubleJumped = false;
			lastJumpableSurface = CollisionType.GROUND;
			if( movementDirection != Vector2.zero ) {
				currentSpeed += movementDirection * walkingAccel * deltaTime;
			}
			else {
				ApplyFriction( groundFrictionAccel );
			}
		}
		else if( physicsController.didHitLeft ) {
			hasDoubleJumped = false;
			lastJumpableSurface = CollisionType.LEFT_WALL;
			if( movementDirection != Vector2.zero ) {
				currentSpeed += movementDirection * walkingAccel * deltaTime;
			}
			if( movementDirection.x < 0f && !physicsController.didHitACharacterLeft ) {
				ApplyWallFriction();
				if( !isAttacking ) {
					transform.localScale = new Vector3( 1f, 1f, 1f );
				}
			}
		}
		else if( physicsController.didHitRight ) {
			hasDoubleJumped = false;
			lastJumpableSurface = CollisionType.RIGHT_WALL;
			if( movementDirection != Vector2.zero ) {
				currentSpeed += movementDirection * walkingAccel * deltaTime;
			}
			if( movementDirection.x > 0f && !physicsController.didHitACharacterRight ) {
				ApplyWallFriction();
				if( !isAttacking ) {
					transform.localScale = new Vector3( -1f, 1f, 1f );
				}
			}
		}
		else {
			if( movementDirection != Vector2.zero ) {
				currentSpeed += movementDirection * airHorizontalAccel * deltaTime;
			}
			else {
				ApplyFriction( airFrictionAccel );
			}
		}

		bool startJumpingDown = false;
		if( canJump ) {
			SoundManager.instance.PlaySound( SoundManager.SoundType.Jump );
			hasPendingJump = false;
			if( lastJumpableSurface == CollisionType.LEFT_WALL ) {
				currentSpeed.y = wallJumpSpeed;
				currentSpeed.x = wallJumpHorizontalSpeed;
				//currentSpeed.x += 0.7f * movementDirection.x * maxHorizontalSpeed;
				isJumpingFromWall = true;
				isJumpingFromGround = false;
				lastJumpFromWallTime = MinigameTimeManager.instance.time;
			}
			else if( lastJumpableSurface == CollisionType.RIGHT_WALL ) {
				currentSpeed.y = wallJumpSpeed;
				currentSpeed.x = -wallJumpHorizontalSpeed;
				//currentSpeed.x += 0.7f * movementDirection.x * maxHorizontalSpeed;
				isJumpingFromWall = true;
				isJumpingFromGround = false;
				lastJumpFromWallTime = MinigameTimeManager.instance.time;
			}
			else if( physicsController.isGrounded && Vector2.Dot( direction, -Vector2.up ) > 0.7f ) {
				startJumpingDown = true;
				currentSpeed.y = -0.5f * groundJumpSpeed;
				//isJumpingFromGround = true;
				//lastJumpFromGroundTime = MinigameTimeManager.instance.time;
			}
			else {
				currentSpeed.y = groundJumpSpeed;
				currentSpeed.x += 0.7f * movementDirection.x * maxHorizontalSpeed;
				isJumpingFromGround = true;
				isJumpingFromWall = false;
				lastJumpFromGroundTime = MinigameTimeManager.instance.time;
			}
		}
		else if( !hasDoubleJumped && jumpButtonDown ) {
			SoundManager.instance.PlaySound( SoundManager.SoundType.DoubleJump );
			animator.SetFloat( "isSecondJumping", 1f );
			hasPendingJump = false;
			hasDoubleJumped = true;
			currentSpeed.y = airJumpSpeed;
			currentSpeed.x += 0.7f * movementDirection.x * maxHorizontalSpeed;
		}
		if( !hasDoubleJumped ) {
			animator.SetFloat( "isSecondJumping", 0f );
		}

		if( fireButtonDown ) {
			StartCoroutine( FireCoroutine( direction ) );
		}

		currentSpeed.x = Mathf.Clamp( currentSpeed.x, -maxHorizontalSpeed, maxHorizontalSpeed );
		currentSpeed.y = Mathf.Clamp( currentSpeed.y, -maxVerticalSpeed, float.MaxValue );

		Vector2 prevPos = VectorUtils.GetPosition2D( transform.position );
		physicsController.Move( currentSpeed * deltaTime, startJumpingDown );

		// updating the current speed
		currentSpeed = ( VectorUtils.GetPosition2D( transform.position ) - prevPos ) / MinigameTimeManager.instance.deltaTime;
//		currentSpeed.x = ( physicsController.didHitLeft || physicsController.didHitRight ) ? 0 : currentSpeed.x;
//		currentSpeed.y = ( physicsController.didHitCeiling || physicsController.isGrounded ) ? 0 : currentSpeed.y;

		if( direction.x != 0 && !isAttacking && !isGrabbingToWall ) {
			transform.localScale = new Vector3( Mathf.Sign( direction.x ), 1f, 1f );
		}
		if( physicsController.isGrounded ) {
			if( Math.Abs( currentSpeed.x ) < 0.1f ) {
				animator.SetBool( "isMoving", false );
				animator.SetBool( "isJumping", false );
				animator.SetFloat( "isInAir", 0f );
				animator.SetBool( "isWallGrabbing", false );
			}
			else {
				animator.SetBool( "isMoving", true );
				animator.SetBool( "isJumping", false );
				animator.SetFloat( "isInAir", 0f );
				animator.SetBool( "isWallGrabbing", false );
			}
		}
		else {
			if( isGrabbingToWall ) {
				animator.SetBool( "isMoving", false );
				animator.SetBool( "isJumping", false );
				animator.SetFloat( "isInAir", 0f );
				animator.SetBool( "isWallGrabbing", true );
			}
			else {
				animator.SetBool( "isMoving", false );
				animator.SetBool( "isJumping", true );
				animator.SetFloat( "isInAir", 1f );
				animator.SetBool( "isWallGrabbing", false );
			}
		}
	}

	// ====================================================
	private IEnumerator FireCoroutine( Vector2 direction ) {
		if( nextFireTime > MinigameTimeManager.instance.time ) {
			yield break;
		}
		nextFireTime = MinigameTimeManager.instance.time + 1/attackSpeed;
		isAttacking = true;

		ProjectileDirection projectileDirection = ProjectileDirection.FORWARD;
		if( direction == Vector2.zero ) {
			direction = Vector3.right * Mathf.Sign( transform.localScale.x );
			projectileDirection = ProjectileDirection.FORWARD;
		}
		else {
			float oldHorizontalDir = Mathf.Sign( transform.localScale.x );
			direction.x *= oldHorizontalDir;
			float maxValue = float.MinValue;
			foreach( ProjectileDirection key in directionsCache.Keys ) {
				float value = Vector2.Dot( direction, directionsCache[key] );
				if( value < maxValue ) {
					continue;
				}
				maxValue = value;
				projectileDirection = key;
			}
			direction = directionsCache[projectileDirection];
			// return to old direction
			direction.x *= oldHorizontalDir;
		}

		ProjectileController projectile = Instantiate<ProjectileController>( projectilePrefab );
		foreach( ProjectileSpawnPointInfo info in projectileSpawnPointInfos ) {
			if( info.direction == projectileDirection ) {
				projectile.transform.position = info.spawnPoint.position;
				projectile.transform.right = direction;
			}
		}
		projectile.Configure( direction, this );
//		bool waitingForAnimationEvent = true;
//		animationListener.cb = () => {
//			waitingForAnimationEvent = false;
//		};
	
//		HitController hitController = horizontalHitController;
//		if( direction.magnitude > 0f && Mathf.Abs( direction.x ) <  Mathf.Abs( direction.y ) ){
//			if( direction.y > 0 ) {
//				hitController = upHitController;
//				animator.SetTrigger( "attackUp" );
//				SoundManager.instance.PlaySound( SoundManager.SoundType.Swing );
//			}
//			else {
//				//regular down attack:
//				if( physicsController.isGrounded ) {
//					hitController = downHitController;
//					animator.SetTrigger( "attackDown" );
//					SoundManager.instance.PlaySound( SoundManager.SoundType.Swing );
//				}
//				// down link attack
//				else {
//					StartCoroutine( LinkDown( direction ) );
//					SoundManager.instance.PlaySound( SoundManager.SoundType.LinkDown);
//					yield break;
//				}
//			}
//		}
//		else {
//			SoundManager.instance.PlaySound( SoundManager.SoundType.Swing );
//			animator.SetTrigger( "attackFront" );
//		}

		//player is still.. using its scale


//		while( waitingForAnimationEvent && !attackAnimationInterrupted ) {
//			yield return 0;
//		}
//
//		if( attackAnimationInterrupted ) {
//			isAttacking = false;
//			yield break;
//		}

//		float endTime = MinigameTimeManager.instance.time + hitActiveDuration;
//		while( MinigameTimeManager.instance.time < endTime && !attackAnimationInterrupted ) {
//			yield return 0;
//		}

		isAttacking = false;
	}

	// ====================================================
	public void Squash() {
		Die( null );
	}

	// ====================================================
	private void ApplyWallFriction() {
		if( hasKnockback ) {
			return;
		}

		if( currentSpeed.y >= 0 ) {
			return;
		}
		float deltaYSpeed = wallFrictionAcc * MinigameTimeManager.instance.deltaTime;
		if( Mathf.Abs( currentSpeed.y ) <= deltaYSpeed ) {
			currentSpeed.y = 0;
			return;
		}

		isGrabbingToWall = true;
		currentSpeed.y -= Mathf.Sign( currentSpeed.y ) * deltaYSpeed;
		currentSpeed.y = currentSpeed.y < -wallMaxVeticalSpeed ? -wallMaxVeticalSpeed : currentSpeed.y;
	}

	// ====================================================
	private void ApplyFriction( float frictionAcc ) {

		if( hasKnockback ) {
			return;
		}
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

	// ====================================================
	public void Swap( SwappableEntity swappableEntity ) {
		//TODO
	}
}