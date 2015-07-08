using UnityEngine;
using System.Collections;

public class RoomController : MonoBehaviour {
	public Transform spawnPoint;
	
	public Transform levelCorner1;
	public Transform levelCorner2;
	public RoomEntrance entrance;
	public RoomExit exit;
	
	public Vector2 bottomLeftPos{
		get{
			return new Vector3( Mathf.Min( levelCorner1.position.x, levelCorner2.position.x ), Mathf.Min( levelCorner1.position.y, levelCorner2.position.y ), 0f );
		}
	}
	
	public Vector2 upperRightPos{
		get{
			return new Vector3( Mathf.Max( levelCorner1.position.x, levelCorner2.position.x ), Mathf.Max( levelCorner1.position.y, levelCorner2.position.y ), 0f );
		}
	}

	//=====================================
	private void Awake() {
		if( entrance != null ) {
			entrance.Configure( this );
		}
		if( exit != null ) {
			exit.Configure( this );
		}

		EntitySpawner[] entitySpawners = GetComponentsInChildren<EntitySpawner>();
		foreach( EntitySpawner entitySpawner in entitySpawners ) {
			entitySpawner.Spawn();
		}

		PatrollingEnemy[] enemies = GetComponentsInChildren<PatrollingEnemy>();
		foreach( PatrollingEnemy enemy in enemies ) {
			enemy.Configure( this );
		}
	}
	
	//=====================================
	private void OnDrawGizmos() {
		if( levelCorner1 == null || levelCorner2 == null ) {
			return;
		}
		Gizmos.color = Color.yellow;
		
		Vector3 upperLeftPos = new Vector3( bottomLeftPos.x, upperRightPos.y );
		Vector3 bottomRightPos = new Vector3( upperRightPos.x, bottomLeftPos.y );
		
		Gizmos.DrawLine( bottomLeftPos, bottomRightPos );
		Gizmos.DrawLine( bottomRightPos, upperRightPos );
		Gizmos.DrawLine( upperRightPos, upperLeftPos );
		Gizmos.DrawLine( upperLeftPos, bottomLeftPos );
	}
}
