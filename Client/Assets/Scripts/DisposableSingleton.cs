using UnityEngine;
using System.Collections;

public class DisposableSingleton<T> : MonoBehaviour where T: class {
	
	protected static T _instance = null;
	
	//===================================================
	public static T instance
	{
		get {
			if(_instance == null)
				_instance = FindObjectOfType(typeof(T)) as T;
			return _instance;
		}
	}
	
	//===================================================
	public static void Instantiate()
	{
		_instance = instance;
	}
	
	//===================================================
	private void Awake()
	{
		if( _instance == this as T ) {
			// somebody has already used the instance getter, so we skip this...
			return;
		}
		if( _instance != null ) {
			Log.Error( "{0}: There are two instances of {0}", typeof(T) );
		}
		Instantiate();
	}
	
	//===================================================
	protected virtual void OnDestroy ()
	{
		_instance = null;
	}
}