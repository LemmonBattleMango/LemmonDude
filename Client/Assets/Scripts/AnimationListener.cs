using UnityEngine;
using System.Collections;
using System;

public class AnimationListener : MonoBehaviour {

	public Action cb;

	// ====================================================
	void Execute () { 
		if( cb != null ) {
			cb();
			cb = null;
		}
	}
}
