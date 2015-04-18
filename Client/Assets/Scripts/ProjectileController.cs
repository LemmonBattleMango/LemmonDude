using UnityEngine;
using System.Collections;

public class ProjectileController : MonoBehaviour {

	private Vector2 direction;
	public float projectilePushBackForce = 0.15f;
	public float lifeSpan = 3f;


	private PlayerController owner;

	// ====================================================
	public void Configure( Vector2 _direction, PlayerController _owner ) {
		direction = _direction.normalized;
		owner = _owner;
		StartCoroutine( AutoDestroyCoroutine() );
	}

	//======================================================
	public IEnumerator AutoDestroyCoroutine() {
		float initialTime = MinigameTimeManager.instance.time;
		while( MinigameTimeManager.instance.time < initialTime + lifeSpan ) {
			yield return null;
		}
		Destroy( gameObject );
	}

	//======================================================
	void OnTriggerEnter2D( Collider2D other ) {	

		PlayerController entity = other.GetComponent<PlayerController>();
		if( entity == owner ) {
			return;
		}

		SwappableEntity swappableEntity = other.GetComponent<SwappableEntity>();
		if( swappableEntity != null ) {
			owner.Swap( swappableEntity );
		}

		//Do something
		Destroy( gameObject );
	}

	// ====================================================
	void FixedUpdate() {
		transform.position += VectorUtils.GetPosition3D( MinigameTimeManager.instance.fixedDeltaTime * direction * GlobalConfig.instance.projectileVelocity );
	}
}
