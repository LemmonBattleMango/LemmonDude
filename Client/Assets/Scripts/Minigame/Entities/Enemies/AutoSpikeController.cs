using UnityEngine;
using System.Collections;

public class AutoSpikeController : MonoBehaviour {

	public GameObject spike;
	public float travelingDistance = 1f;
	public float activationDelay = 0.3f;
	public float deactivationDelay = 1f;
	public float movementDuration = 0.3f;
	public float vibrationWidth = 0.1f;
	public float offDuration = 1f;

	private PlayerDetector playerDetector;

	// ====================================================
	void Start () {
		playerDetector = GetComponentInChildren<PlayerDetector>();
		playerDetector.onPlayerDetected = OnPlayerDetected;

	}

	// ====================================================
	private void OnPlayerDetected( PlayerDetector playerDetector ) {
		StartCoroutine( ActivateCorrutine() );
	}

	// ====================================================
	private IEnumerator ActivateCorrutine () {
		playerDetector.gameObject.SetActive( false );
		yield return StartCoroutine( VibrateCorrutine( activationDelay ) );
		Vector3 initialPos = spike.transform.localPosition;
		Vector3 finalPos = initialPos + travelingDistance * Vector3.up;

		float startTime = MinigameTimeManager.instance.time;
		float fraction = 0;
		while( fraction < 1f ) {
			fraction = ( MinigameTimeManager.instance.time - startTime ) / movementDuration;
			spike.transform.localPosition = Vector3.Lerp( initialPos, finalPos, fraction );
			yield return 0;
		}

		yield return StartCoroutine( MinigameTimeManager.instance.WaitForSecs( deactivationDelay ) );
		startTime = MinigameTimeManager.instance.time;
		fraction = 0;
		while( fraction < 1f ) {
			fraction = ( MinigameTimeManager.instance.time - startTime ) / movementDuration;
			spike.transform.localPosition = Vector3.Lerp( finalPos, initialPos, fraction );
			yield return 0;
		}

		yield return StartCoroutine( MinigameTimeManager.instance.WaitForSecs( offDuration ) );
		playerDetector.gameObject.SetActive( true );
	}

	// ====================================================
	private IEnumerator VibrateCorrutine ( float durationSecs ) {
		Vector3 initialPos = spike.transform.localPosition;
		float initialTime = MinigameTimeManager.instance.time;
		float factor = 1f;
		while( MinigameTimeManager.instance.time < initialTime + durationSecs ) {
			spike.transform.localPosition = initialPos + Vector3.right * vibrationWidth * factor;
			factor *= -1f;
			yield return 0;
			yield return 0;
		}
		spike.transform.localPosition = initialPos;
	}
}
