using UnityEngine;
using System.Collections;
using System;

public class BatEntity : SwappableEntity {

	private Vector2 guardPosition;
	public float movingSpeed = 2f;
	private bool shouldReturnToGuardPos = false;
	public float timeBeforeReturning = 0.5f;

	// ====================================================
	public override void Start() {
		base.Start();
		guardPosition = transform.position;
		Collider2D collider2D = GetComponent<Collider2D>();
		collider2D.enabled = false;
	}

	// ====================================================
	protected override void LateUpdate() {

		if( !shouldReturnToGuardPos ) {
			return;
		}

		Vector2 currentpos = GetPosition();
		if( currentpos == guardPosition ) {
			return;
		}
		Vector2 diff = guardPosition - currentpos;
		Vector2 delta = Vector2.zero;
		if( diff.magnitude > MinigameTimeManager.instance.deltaTime * movingSpeed ) {
			delta = diff.normalized * MinigameTimeManager.instance.deltaTime * movingSpeed;
		}
		else {
			delta = diff.normalized * diff.magnitude;
		}

		myTranform.position += VectorUtils.GetPosition3D( delta );
	}

	//=====================================
	public override void OnSwap(){
		StartCoroutine( WaitToReturnCoroutine() );
	}

	//=====================================
	public IEnumerator WaitToReturnCoroutine(){
		shouldReturnToGuardPos = false;
		yield return new WaitForSeconds( timeBeforeReturning );
		shouldReturnToGuardPos = true;
	}
}
