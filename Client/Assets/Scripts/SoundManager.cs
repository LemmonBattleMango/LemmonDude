using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : DisposableSingleton<SoundManager> {

	public enum SoundType {
		// ADD SOUNDS AT THE END!!!
		Hit,
        Death,
		meleeClash,
		Parry,
		Swing,
		DominionCoin,
		LinkDown,
		Jump,
		DoubleJump,
		Throw

		// ADD SOUNDS AT HERE ↑↑↑↑
	}

	[System.Serializable]
	public class AudioInfo {
		public AudioClip clip;
		public float volume = 1f;
	}

	[System.Serializable]
	public class AudioList {
		public SoundType soundType;
		public List<AudioInfo> clips;
	}

	public List<AudioList> sounds;
	private Dictionary<SoundType,AudioList> soundsDictionary;

	//======================================================
	private void Start() {
		soundsDictionary = new Dictionary<SoundType, AudioList>();
		foreach( AudioList audioList in sounds ) {
			if ( audioList == null ) {
				continue;
			}

			if( soundsDictionary.ContainsKey( audioList.soundType ) ) {
				Log.Error( "SoundManager.Start... repeated sound type: " + audioList.soundType.ToString() );
				continue;
			}
			soundsDictionary[audioList.soundType] = audioList;
		}
	}

	//======================================================
	public void PlaySound( SoundType sound ) {

		return;
		if( !soundsDictionary.ContainsKey( sound ) ) {
			Log.Error( "SoundManager.PlaySound: Missing audio list: " + sound.ToString() );
			return;
		}

		AudioList audioList = soundsDictionary[sound];
		if( audioList.clips == null || audioList.clips.Count == 0 ) {
			Log.Error( "SoundManager.PlaySound: empty audio clip list: " + sound.ToString() );
			return;
		}

		int randomSound = UnityEngine.Random.Range(0, audioList.clips.Count);

		AudioInfo audioInfo = audioList.clips[randomSound];
		if( audioInfo == null ) {
			Log.Error( "SoundManager.PlaySound: Missing audio Info: " + sound.ToString() + " on index " + randomSound );
			return;
		}

		AudioClip audioToPlay = audioInfo.clip;
		float volume = audioInfo.volume;

		if( audioToPlay == null ) {
			Log.Error( "SoundManager.PlaySound: Missing sound: " + sound.ToString() );
			return;
		}
		//audio.PlayOneShot( audioToPlay, volume );
	}

}
