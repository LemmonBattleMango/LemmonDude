using UnityEngine;
using System.Collections;

public class EntitySpawner : MonoBehaviour {

	public Entity entityPrefab;
	public string gizmoImageFile;

	// ====================================================
	public Entity Spawn() {
		Entity entity = Instantiate<Entity>( entityPrefab );
		entity.transform.position = transform.position;
		entity.transform.rotation = transform.rotation;
		entity.transform.parent = transform.parent;
		return entity;
	}

	// ====================================================
	private void OnDrawGizmos () {
		entityPrefab._OnDrawGizmos( transform.position );
		if( string.IsNullOrEmpty( gizmoImageFile ) ) {
			return;
		}
		Gizmos.DrawIcon( transform.position, gizmoImageFile, true );
	}

}
