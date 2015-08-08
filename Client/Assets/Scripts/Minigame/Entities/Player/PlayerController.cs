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
	public float maxVerticalSpeed { get{ return GlobalConfig.instance.maxVerticalSpeed; } }
	public float maxHorizontalSpeed { get{ return GlobalConfig.instance.maxHorizontalSpeed; } }
	public float walkingAccel { get{ return GlobalConfig.instance.walkingAccel; } }
	public float groundFrictionHorizontalAccel { get{ return GlobalConfig.instance.groundFrictionHorizontalAccel; } }
	public float airFrictionHorizontalAccel { get{ return GlobalConfig.instance.airFrictionHoizontalAccel; } }
	public float airHorizontalAccel { get{ return GlobalConfig.instance.airHorizontalAccel; } }
	public float wallFrictionVerticalAcc { get{ return GlobalConfig.instance.wallFrictionVerticalAcc; } }
	public float wallMaxVerticalFallingSpeed { get{ return GlobalConfig.instance.wallMaxVerticalFallingSpeed; } }
	public float airJumpHeight { get{ return GlobalConfig.instance.airJumpHeight; } }
	public float maxJumpHeight { get{ return GlobalConfig.instance.maxJumpHeight; } }
	public float minJumpHeight { get{ return GlobalConfig.instance.minJumpHeight; } }

	public float airJumpSpeed { get{ return CalculateSpeed( airJumpHeight ); } }
	public float groundJumpSpeed { get{ return CalculateSpeed( minJumpHeight ); } }
	public float wallJumpVerticalSpeed { get{ return CalculateSpeed( minJumpHeight ); } }

	public float attackDelay { get{ return GlobalConfig.instance.attackDelay; } }

	public float wallJumpHorizontalSpeed { get{ return GlobalConfig.instance.wallJumpHorizontalSpeed; } }

	public float gravityAccel { get{ return GlobalConfig.instance.gravityAccel; } }

	// References
	[NonSerialized]
	public JoystickController joystickController;
	[HideInInspector]
	public PhysicsController physicsController;
	public Animator animator;
	public ParticleSystem runningParticles;
	private PlayerHitboxReference[] hitboxReferences;

	// Status
	[System.NonSerialized]
	public bool isDead;
	[System.NonSerialized]
	public Vector2 currentSpeed;
	[System.NonSerialized]
	public bool isSwapping;

	private float nextFireTime;
	[HideInInspector]
	public bool isAttacking;
	[HideInInspector]
	public bool isGrabbingToWall;

	private bool canDoubleJumped;
	public int hp;

	private CollisionType lastJumpableSurface;
	public float jumpTimeRangeSecs = 0.1f;
	private float lastJumpButtonTime;
	private float lastAbleToJumpTime;
	private bool hasPendingJump;
	public bool didWon;

	private bool isJumpingFromGround;
	private float lastJumpFromGroundTime;

	private bool isJumpingFromWall;
	private float lastJumpFromWallTime;

	public System.Action<PlayerController> onDeath;
	Vector2 prevPos;
	
	public List <ProjectileSpawnPointInfo> projectileSpawnPointInfos;
	private Dictionary<ProjectileDirection, Vector2> directionsCache = new Dictionary<ProjectileDirection, Vector2> {
		{ ProjectileDirection.DOWN, -Vector2.up },
		{ ProjectileDirection.UP, Vector2.up },
		{ ProjectileDirection.FORWARD, Vector2.right },
		//{ ProjectileDirection.FORWARD_DOWN, new Vector2( 1f, -1f ).normalized },
		//{ ProjectileDirection.FORWARD_UP, new Vector2( 1f, 1f ).normalized }
	};
	public ProjectileController projectilePrefab;
	[NonSerialized]
	public ProjectileController currentProjectile;
	public Animator shootVfxAnimator;
	public Animator deathVfxAnimator;

	// ====================================================
	public void Initialize() {

		prevPos = VectorUtils.GetPosition2D( transform.position );
		nextFireTime = MinigameTimeManager.instance.time;

		joystickController = GetComponent<JoystickController>();
		joystickController.Initialize();

		MinigameTimeManager.instance.onTimeScaleChanged += OnTimeScaleChangedHandler;

		Director.instance.OnPlayerSpawn( this );

		physicsController = GetComponent<PhysicsController>();
		physicsController.shouldUseSlopes = true;
		physicsController.SetCollisionLayers( LayerMask.GetMask(new string[] { "InvisibleWallLayer", "LevelLayer", "MoveBoxLayer" } ),
		                                     LayerMask.GetMask(new string[] { "InvisibleWallLayer", "LevelLayer", "MoveBoxLayer", "OneWayPlatformLayer" } ) );

		physicsController.Initialize();

		hitboxReferences = GetComponentsInChildren<PlayerHitboxReference>();
		foreach( PlayerHitboxReference hitboxReference in hitboxReferences ) {
			hitboxReference.player = this;
		}
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
	private float CalculateSpeed( float targetHeight ) {
		return Mathf.Sqrt( 2f * gravityAccel * targetHeight );
	}

	// ====================================================
	void Die() {
		if( isDead || didWon || isSwapping ) {
			return;
		}
		Log.Debug( "PlayerController.Die()" );
		Director.instance.ScreenShake( 0.05f, 0.1f );
		foreach( PlayerHitboxReference hitboxReference in hitboxReferences ) {
			hitboxReference.gameObject.SetActive( false );
		}

		isDead = true;
		deathVfxAnimator.SetTrigger( "deathTrigger" );
		//animator.SetBool( "isDeath", true );
		SoundManager.instance.PlaySound(SoundManager.SoundType.Death);

		//yield return StartCoroutine( MinigameTimeManager.instance.WaitForSecs( 0.6f ) );
		gameObject.layer = MinigameManager.instance.deathLayer;
		physicsController.SetCollisionLayers( LayerMask.GetMask( new string[] { "InvisibleWallLayer", "LevelLayer" } ),
		                                     LayerMask.GetMask( new string[] { "InvisibleWallLayer", "LevelLayer", "OneWayPlatformLayer" } ) );
		
		//transform.position = new Vector3( 1000f, 1000f, -1f );
		if( onDeath != null ) {
			onDeath( this );
		}
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

			jumpButtonDown = joystickController.GetJumpButtonDown();
			jumpButtonHeld = joystickController.GetJumpButton();
		}

		Vector2 movementDirection = Vector2.Scale( direction, Vector2.right );

		bool isNextToWall = ( ( physicsController.didHitLeft && !physicsController.didHitACharacterLeft && !physicsController.didHitAnInvisibleWallLeft ) ||
		                     ( physicsController.didHitRight && !physicsController.didHitACharacterRight && !physicsController.didHitAnInvisibleWallRight ) );

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

		if( isSwapping ) {
			return;
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
			float traveledHeight = ( MinigameTimeManager.instance.time - lastJumpFromWallTime ) * wallJumpVerticalSpeed;
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
			canDoubleJumped = false;
			lastJumpableSurface = CollisionType.GROUND;
			if( movementDirection != Vector2.zero ) {
				currentSpeed += movementDirection * walkingAccel * deltaTime;
			}
			else {
				ApplyFriction( groundFrictionHorizontalAccel );
			}
		}
		else if( physicsController.didHitLeft ) {
			canDoubleJumped = false;
			lastJumpableSurface = CollisionType.LEFT_WALL;
			if( movementDirection != Vector2.zero ) {
				currentSpeed += movementDirection * walkingAccel * deltaTime;
			}
			if( movementDirection.x < 0f && !physicsController.didHitACharacterLeft && !physicsController.didHitAnInvisibleWallLeft ) {
				ApplyWallFriction();
				if( !isAttacking ) {
					transform.localScale = new Vector3( 1f, 1f, 1f );
				}
			}
		}
		else if( physicsController.didHitRight ) {
			canDoubleJumped = false;
			lastJumpableSurface = CollisionType.RIGHT_WALL;
			if( movementDirection != Vector2.zero ) {
				currentSpeed += movementDirection * walkingAccel * deltaTime;
			}
			if( movementDirection.x > 0f && !physicsController.didHitACharacterRight && !physicsController.didHitAnInvisibleWallRight ) {
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
				ApplyFriction( airFrictionHorizontalAccel );
			}
		}

		bool startJumpingDown = false;
		if( canJump ) {
			SoundManager.instance.PlaySound( SoundManager.SoundType.Jump );
			hasPendingJump = false;
			if( lastJumpableSurface == CollisionType.LEFT_WALL ) {
				currentSpeed.y = wallJumpVerticalSpeed;
				currentSpeed.x = wallJumpHorizontalSpeed;
				//currentSpeed.x += 0.7f * movementDirection.x * maxHorizontalSpeed;
				isJumpingFromWall = true;
				isJumpingFromGround = false;
				lastJumpFromWallTime = MinigameTimeManager.instance.time;
			}
			else if( lastJumpableSurface == CollisionType.RIGHT_WALL ) {
				currentSpeed.y = wallJumpVerticalSpeed;
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
		else if( canDoubleJumped && hasPendingJump ) {
//			SoundManager.instance.PlaySound( SoundManager.SoundType.DoubleJump );
//			animator.SetFloat( "isSecondJumping", 1f );
			hasPendingJump = false;
			canDoubleJumped = false;
			currentSpeed.y = airJumpSpeed;
			currentSpeed.x += 0.7f * movementDirection.x * maxHorizontalSpeed;
		}

		if( fireButtonDown ) {
			StartCoroutine( FireCoroutine( direction ) );
		}

		currentSpeed.x = Mathf.Clamp( currentSpeed.x, -maxHorizontalSpeed, maxHorizontalSpeed );
		currentSpeed.y = Mathf.Clamp( currentSpeed.y, -maxVerticalSpeed, float.MaxValue );

		prevPos = VectorUtils.GetPosition2D( transform.position );
		physicsController.Move( currentSpeed * deltaTime, startJumpingDown );

		// updating the current speed
		currentSpeed = ( VectorUtils.GetPosition2D( transform.position ) - prevPos ) / MinigameTimeManager.instance.deltaTime;

		if( direction.x != 0 && !isAttacking && !isGrabbingToWall ) {
			transform.localScale = new Vector3( Mathf.Sign( direction.x ), 1f, 1f );
			if( transform.localScale.x < 0 ) {
				runningParticles.transform.localScale = new Vector3( 1f, -1f, 1f );
				runningParticles.transform.rotation = Quaternion.Euler( 0f, 90f, 0f );
			}
			else {
				runningParticles.transform.localScale = new Vector3( 1f, 1f, 1f );
				runningParticles.transform.rotation = Quaternion.Euler( 0f, 270f, 0f );
			}
		}
		if( physicsController.isGrounded ) {
			if( Math.Abs( currentSpeed.x ) < 0.1f ) {
				runningParticles.enableEmission = false;
				SoundManager.instance.LoopSound( SoundManager.SoundType.Walk, false );
				animator.SetBool( "isMoving", false );
				animator.SetBool( "isJumping", false );
				animator.SetBool( "isWallGrabbing", false );
			}
			else {
				SoundManager.instance.LoopSound( SoundManager.SoundType.Walk, true );
				runningParticles.enableEmission = true;
				animator.SetBool( "isMoving", true );
				animator.SetBool( "isJumping", false );
				animator.SetBool( "isWallGrabbing", false );
			}
		}
		else {
			SoundManager.instance.LoopSound( SoundManager.SoundType.Walk, false );
			runningParticles.enableEmission = false;
			if( isGrabbingToWall ) {
				animator.SetBool( "isMoving", false );
				animator.SetBool( "isJumping", false );
				animator.SetBool( "isWallGrabbing", true );
			}
			else {
				animator.SetBool( "isMoving", false );
				animator.SetBool( "isJumping", true );
				animator.SetBool( "isWallGrabbing", false );
			}
		}
	}

	// ====================================================
	private IEnumerator FireCoroutine( Vector2 direction ) {
		direction.Normalize();
		if( currentProjectile != null ) {
			yield break;
		}

		if( nextFireTime > MinigameTimeManager.instance.time ) {
			yield break;
		}
		nextFireTime = MinigameTimeManager.instance.time + attackDelay;
		isAttacking = true;

		ProjectileDirection projectileDirection = ProjectileDirection.FORWARD;
		if( direction == Vector2.zero ) {
			direction = Vector3.right * Mathf.Sign( transform.localScale.x );
			projectileDirection = ProjectileDirection.FORWARD;
		}
		if( isGrabbingToWall ) {
			//TODO: check
			direction = Vector3.right * Mathf.Sign( transform.localScale.x );
			projectileDirection = ProjectileDirection.FORWARD;
		}
		else {
			float oldHorizontalDir = Mathf.Sign( transform.localScale.x );
			direction.x *= oldHorizontalDir;
			/*float maxValue = float.MinValue;
			foreach( ProjectileDirection key in directionsCache.Keys ) {
				float value = Vector2.Dot( direction, directionsCache[key] );
				if( value < maxValue ) {
					continue;
				}
				maxValue = value;
				projectileDirection = key;
			}*/

			if( direction.y > 0.4f ) {
				projectileDirection = ProjectileDirection.UP;
			}
			else if( direction.y < -0.4f ) {
				projectileDirection = ProjectileDirection.DOWN;
			}
			direction = directionsCache[projectileDirection];
			// return to old direction
			direction.x *= oldHorizontalDir;
		}
		SoundManager.instance.PlaySound( SoundManager.SoundType.Throw );
		currentProjectile = Instantiate<ProjectileController>( projectilePrefab );
		foreach( ProjectileSpawnPointInfo info in projectileSpawnPointInfos ) {
			if( info.direction == projectileDirection ) {
				currentProjectile.transform.position = info.spawnPoint.position;
				currentProjectile.transform.right = direction;
			}
		}
		shootVfxAnimator.SetTrigger( "shootTrigger" );
		currentProjectile.Configure( direction, this );
		isAttacking = false;
	}

	// ====================================================
	public void InstaDeath() {
		Die();
	}

	// ====================================================
	private void ApplyWallFriction() {

		if( currentSpeed.y >= 0 ) {
			return;
		}

		isGrabbingToWall = true;
		float deltaYSpeed = wallFrictionVerticalAcc * MinigameTimeManager.instance.deltaTime;
		if( Mathf.Abs( currentSpeed.y ) <= deltaYSpeed ) {
			currentSpeed.y = 0;
			return;
		}

		currentSpeed.y -= Mathf.Sign( currentSpeed.y ) * deltaYSpeed;
		currentSpeed.y = currentSpeed.y < -wallMaxVerticalFallingSpeed ? -wallMaxVerticalFallingSpeed : currentSpeed.y;
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

	// ====================================================
	public void Swap( SwappableEntity swappableEntity ) {
		StartCoroutine( SwapCoroutine( swappableEntity ) );
	}

	// ====================================================
	private IEnumerator SwapCoroutine( SwappableEntity swappableEntity ) {
		SoundManager.instance.PlaySound( SoundManager.SoundType.Swap );

		Vector2 previousPos = transform.position;
		Vector2 previousSpeed = currentSpeed;

		transform.position = VectorUtils.GetPosition3D( swappableEntity.GetPosition() );

		bool wasSwappableEntityGrounded = swappableEntity.physicsController.isGrounded;

		swappableEntity.SetPosition( previousPos );
		swappableEntity.SetVelocity( previousSpeed );

		PatrollingEnemy patroller = swappableEntity as PatrollingEnemy;
		if( patroller != null ) {
			patroller.SetDirectionAsForward();
		}
		swappableEntity.OnSwap();

		isSwapping = true;

		deathVfxAnimator.SetTrigger( "deathTrigger" );
		animator.gameObject.SetActive( false );
		yield return new WaitForSeconds( 0.2f );
		isSwapping = false;
		animator.gameObject.SetActive( true );
		currentSpeed = previousSpeed;
		if( wasSwappableEntityGrounded ) {
			currentSpeed.y = 0f;
			Vector2 direction = joystickController.GetDirection();
			if( direction.x > 0.4f ) {
				currentSpeed.x = maxHorizontalSpeed;
			}
			else if( direction.x < -0.4f ) {
				currentSpeed.x = -maxHorizontalSpeed;
			}
			else {
				currentSpeed.x = 0f;
			}
		}
		else {
			currentSpeed.y = currentSpeed.y < 0f ? 0f : currentSpeed.y;
		}
		canDoubleJumped = true;

		prevPos = VectorUtils.GetPosition2D( transform.position );


	}
}