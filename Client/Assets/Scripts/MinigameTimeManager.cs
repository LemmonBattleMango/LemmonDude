using UnityEngine;
using System.Collections;

public class MinigameTimeManager : DisposableSingleton<MinigameTimeManager> {

	// Variables
	private float _timeScale = 1f;
	private float _time = 0f;

	// Getters/Setters
	public float timeScale{
		get{
			return _timeScale;
		}
		set{
			_timeScale = value;
			if( onTimeScaleChanged != null ) {
				onTimeScaleChanged();
			}
		}
	}

	public float time{ get{ return _time; } }
	public float deltaTime{ get{ return Time.deltaTime * _timeScale; } }

	// Events
	public event System.Action onTimeScaleChanged;

	//===================================================
	private void Start()
	{
		_time = Time.time;
	}

	//===================================================
	private void Update()
	{
		_time += deltaTime;
	}

	//===================================================
	public IEnumerator WaitForSecs( float seconds )
	{
		float endTime = time + seconds;
		while( time < endTime ) {
			yield return 0;
		}
	}
}
