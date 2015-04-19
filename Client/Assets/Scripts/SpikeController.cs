using UnityEngine;
using System.Collections;

public class SpikeController : MonoBehaviour {

	public Vector2 localSpikeDirection = Vector2.up;
	private Vector2 currentSpeed;
	private Vector2 lastPosition;
	private Transform myTransform;

	// =====================================
	private void Start(){
		gameObject.layer = LayerMask.NameToLayer( "HitDetector" );
		myTransform = transform;
		lastPosition = VectorUtils.GetPosition2D( myTransform.position );
	}

	// =====================================
	private void Update(){
		currentSpeed = VectorUtils.GetPosition2D( ( VectorUtils.GetPosition2D( myTransform.position ) - lastPosition ) / ( MinigameTimeManager.instance.deltaTime ) );
		lastPosition = VectorUtils.GetPosition2D( myTransform.position );
	}

	// =====================================
	private void OnTriggerEnter2D( Collider2D other ) {
		PlayerController player = other.GetComponent<PlayerController>();
		if( player == null ) {
			return;
		}
		Vector2 direction = transform.TransformDirection( localSpikeDirection ).normalized;
		if( Vector2.Dot( direction, player.currentSpeed - currentSpeed ) >= -0.1f ) {
			return;
		}

		player.ApplyDamage( player.hp, Vector2.zero, null, true );

	}

	// =====================================
	private void OnDrawGizmosSelected() {
		Gizmos.color = Color.green;
		Vector3 direction = transform.TransformDirection( localSpikeDirection ).normalized;
		Vector3 orthDirection = Vector3.Cross( direction, Vector3.forward );
		Gizmos.DrawLine( transform.position - orthDirection * 0.03f - direction * 0.15f, transform.position + direction * 0.15f );
		Gizmos.DrawLine( transform.position + orthDirection * 0.03f - direction * 0.15f, transform.position + direction * 0.15f );
	}
}
