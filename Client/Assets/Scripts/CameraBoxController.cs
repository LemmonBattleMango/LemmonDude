using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CameraBoxController : MonoBehaviour {

	//Config
	public float movementSpeedFactor = 0.3f;
	public float zoomSpeedFactor = 0.4f;
	public float cameraEdgeFactor = 1.2f;
	public float maxEdgeOffsetX = 3f;
	public float maxEdgeOffsetY = 3f;
	public float minCameraOthographicSize = 3f;

	// References
	private Camera mainCamera;
	private List<PlayerController> alivePlayers = new List<PlayerController>();

	// Status
	private Vector2 targetPosition;
	private float targetOthographicSize;
	
	//=====================================
	void Awake () {
		mainCamera = GetComponentInChildren<Camera>();
		targetPosition = transform.position;
		targetOthographicSize = mainCamera.orthographicSize;
	}

	//=====================================
	public void OnPlayerSpawn( PlayerController player ) {
		if( alivePlayers.Contains( player ) ) {
			return;
		}
		alivePlayers.Add( player );
	}
	
	//=====================================
	public void OnPlayerDead( PlayerController player ) {
		alivePlayers.Remove( player );
	}

	// ====================================================
	private void LateUpdate() {

		MinigameLevelReferences levelRef = MinigameLevelReferences.instance;
		Vector3 bottomLeftPos = levelRef.bottomLeftPos;
		Vector3 upperRightPos = levelRef.upperRightPos;
		UpdateTargetValues( bottomLeftPos, upperRightPos );

		float deltaOthographicSize = targetOthographicSize - mainCamera.orthographicSize;
		mainCamera.orthographicSize = mainCamera.orthographicSize + deltaOthographicSize * zoomSpeedFactor;

		if( mainCamera.orthographicSize * 2f > ( upperRightPos.y - bottomLeftPos.y ) ) {
			targetPosition.y = ( upperRightPos.y + bottomLeftPos.y ) * 0.5f;
		}
		else {
			if( ( targetPosition.y + mainCamera.orthographicSize ) > upperRightPos.y ) {
				targetPosition.y = upperRightPos.y - mainCamera.orthographicSize;
			}
			else if( ( targetPosition.y - mainCamera.orthographicSize ) < bottomLeftPos.y ) {
				targetPosition.y = bottomLeftPos.y + mainCamera.orthographicSize;
			}
		}

		float orthographicWidth = mainCamera.orthographicSize * mainCamera.aspect;
		if( orthographicWidth * 2f > ( upperRightPos.x - bottomLeftPos.x ) ) {
			targetPosition.x = ( upperRightPos.x + bottomLeftPos.x ) * 0.5f;
		}
		else {
			if( ( targetPosition.x + orthographicWidth ) > upperRightPos.x ) {
				targetPosition.x = upperRightPos.x - orthographicWidth;
			}
			else if( ( targetPosition.x - orthographicWidth ) < bottomLeftPos.x ) {
				targetPosition.x = bottomLeftPos.x + orthographicWidth;
			}
		}

		// TODO: include delta time?
		Vector3 deltaPos = VectorUtils.GetPosition3D( targetPosition ) - transform.position;
		transform.position = transform.position + movementSpeedFactor * deltaPos;
	}

	//=====================================
	private void UpdateTargetValues( Vector3 bottomLeftPos, Vector3 upperRightPos ) {
		int count = alivePlayers.Count;
		if( count == 0 ) {
			return;
		}
		float minX = float.MaxValue;
		float maxX = float.MinValue;
		float minY = float.MaxValue;
		float maxY = float.MinValue;

		List<Vector3> allPositions = new List<Vector3>();
		allPositions.AddRange( alivePlayers.ConvertAll( i => i.transform.position ) );

		foreach( Vector3 position in allPositions ) {
			if( position.x < minX ) {
				minX = position.x;
			}
			if( position.x > maxX ) {
				maxX = position.x;
			}
			if( position.y < minY ) {
				minY = position.y;
			}
			if( position.y > maxY ) {
				maxY = position.y;
			}
		}

		// force to show the ground
		//minY = bottomLeftPos.y*0.5f+minY*0.5f;

		float edgeOffsetX = Mathf.Min( maxEdgeOffsetX, 0.5f * ( maxX - minX ) * cameraEdgeFactor );
		float edgeOffsetY = Mathf.Min( maxEdgeOffsetY, 0.5f * ( maxY - minY ) * cameraEdgeFactor );

		float modifiedMaxX = maxX + edgeOffsetX;
		float modifiedMinX = minX - edgeOffsetX;
		float modifiedMaxY = maxY + edgeOffsetY;
		float modifiedMinY = minY - edgeOffsetY;

		maxX = Mathf.Clamp( modifiedMaxX, bottomLeftPos.x, upperRightPos.x );
		minX = Mathf.Clamp( modifiedMinX, bottomLeftPos.x, upperRightPos.x );
		maxY = Mathf.Clamp( modifiedMaxY, bottomLeftPos.y, upperRightPos.y );
		minY = Mathf.Clamp( modifiedMinY, bottomLeftPos.y, upperRightPos.y );

		float newAspect = ( maxX - minX ) / ( maxY - minY );

		if( mainCamera.aspect > newAspect ) {
			targetOthographicSize = ( maxY - minY ) * 0.5f;
		}
		else {
			targetOthographicSize = ( maxX - minX ) * 0.5f / mainCamera.aspect;
		}

		if( targetOthographicSize < minCameraOthographicSize ) {
			targetOthographicSize = minCameraOthographicSize;
		}
		targetPosition = new Vector2( 0.5f * ( minX + maxX ), 0.5f * ( minY + maxY ) );
	}
}
