﻿using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {
	private const float OFF_SCREEN_MARGIN = 0.5f;
	private Vector2 direction;
	public float projectilePushBackForce = 0.15f;
	//public float lifeSpan = 3f;
	public float maxTraveledDistance = 3f;


	public virtual float speed{ get{ return GlobalConfig.instance.projectileVelocity; } }

	private PlayerController owner;
	private float traveledDistance = 0f;

	// ====================================================
	public void Configure( Vector2 _direction, PlayerController _owner ) {
		gameObject.layer = LayerMask.NameToLayer( "HitDetector" );
		direction = _direction.normalized;
		owner = _owner;
		//StartCoroutine( AutoDestroyCoroutine() );
	}

//	//======================================================
//	public IEnumerator AutoDestroyCoroutine() {
//		//wait for one frame to be on screen
//		yield return null;
//		float initialTime = MinigameTimeManager.instance.time;
//		while( MinigameTimeManager.instance.time < initialTime + lifeSpan && IsOnScreen() ) {
//			yield return null;
//		}
//		Destroy( gameObject );
//	}

	//======================================================
	private bool IsOnScreen() {
		Camera cam = Director.instance.cameraBox.mainCamera;
		float minX = cam.transform.position.x - cam.orthographicSize * cam.aspect - OFF_SCREEN_MARGIN;
		float maxX = cam.transform.position.x + cam.orthographicSize * cam.aspect  + OFF_SCREEN_MARGIN;
		float minY = cam.transform.position.y - cam.orthographicSize - OFF_SCREEN_MARGIN;
		float maxY = cam.transform.position.y + cam.orthographicSize + OFF_SCREEN_MARGIN;
		Vector3 pos = transform.position;
		if( ( pos.x < minX ) || ( pos.x > maxX )
		   || ( pos.y < minY ) || ( pos.y > maxY ) ) {
			return false;
		}

		return true;
	}

	//======================================================
	protected virtual void OnTriggerEnter2D( Collider2D other ) {	

		PlayerHitboxReference playerReference = other.GetComponent<PlayerHitboxReference>();
		if( playerReference != null && playerReference.player == owner ) {
			Destroy( gameObject );
			return;
		}
		SwappableEntityHitboxReference swappableEntityReference = other.GetComponent<SwappableEntityHitboxReference>();
		if( swappableEntityReference != null && swappableEntityReference.swappableEntity != null ) {
			owner.Swap( swappableEntityReference.swappableEntity );
		}
		else {
			SoundManager.instance.PlaySound( SoundManager.SoundType.NoSwap );
		}

		//Do something
		Destroy( gameObject );
	}

	// ====================================================
	void FixedUpdate() {
		if( traveledDistance > maxTraveledDistance ) {
			Destroy( gameObject );
			return;
		}
		float deltaPos = MinigameTimeManager.instance.fixedDeltaTime * speed;
		traveledDistance += deltaPos;
		transform.position += VectorUtils.GetPosition3D( deltaPos * direction );
	}
}
