using UnityEngine;
using System.Collections;

public class ChasingEnemy : PatrollingEnemy {

	protected override float horizontalSpeed{ get{ return isRunning ? maxRunningHorizontalSpeed : maxHorizontalSpeed; } }
	public float maxRunningHorizontalSpeed = 2f;
	private bool isRunning{ get{ return ( MinigameTimeManager.instance.time < (runningDurationSecs + lastTimePlayerVisible ) ); } }

	public float visionRange = 3f;
	public Transform eyesPos;
	private float lastTimePlayerVisibleCheck = float.MinValue;
	private float lastTimePlayerVisible = float.MinValue;
	public float runningDurationSecs = 3f;
	LayerMask layerMask= LayerMask.GetMask(new string[] { "LevelLayer", "OneWayPlatformLayer", "PlayerLayer" } );

	// ====================================================
	protected override void UpdateWalking() {
		CheckPlayerVisible();
		base.UpdateWalking();
	}

	// ====================================================
	protected void CheckPlayerVisible() {
		if( MinigameTimeManager.instance.time < lastTimePlayerVisibleCheck + 0.1f ) {
			return;
		}

		
		lastTimePlayerVisibleCheck = MinigameTimeManager.instance.time;
		PlayerController player = PlayerFactory.instance.currentPlayer;
		if( player == null || player.isDead ) {
			return;
		}
		Vector2 direction = Vector2.right * myTransform.localScale.x;
		RaycastHit2D raycastHit = Physics2D.Raycast( eyesPos.position, direction, visionRange, layerMask.value );
		if( raycastHit.collider != null && player.gameObject == raycastHit.collider.gameObject ) {
			Log.Debug("is visible");
			lastTimePlayerVisible = MinigameTimeManager.instance.time;
		}
	}

}
