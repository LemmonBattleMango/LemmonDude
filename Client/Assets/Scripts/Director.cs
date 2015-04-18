using UnityEngine;
using System.Collections;

public class Director : DisposableSingleton<Director> {

	// References
	private CameraBoxController cameraBox;
	
	//=====================================
	void Awake () {
		cameraBox = GetComponentInChildren<CameraBoxController>();
	}

	//=====================================
	public void OnPlayerSpawn( PlayerController player ) {
		cameraBox.OnPlayerSpawn( player );
	}

	//=====================================
	public void OnPlayerDead( PlayerController player ) {
		cameraBox.OnPlayerDead( player );
	}

	// ===============================================================
	public void ScreenShake( float magnitude, float duration ) {
		StartCoroutine( ScreenShakeCoroutine( magnitude, duration ) );
	}
	
	// ===============================================================
	public IEnumerator ScreenShakeCoroutine( float magnitude, float duration ) {
		float remainingtime = duration;
		do {
			Vector3 offset = Random.insideUnitCircle * magnitude;
			transform.position += offset;
			yield return 0;
			transform.position -= offset;
			remainingtime -= Time.deltaTime;
		} while( remainingtime >= 0 );
	}
}
