using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : DisposableSingleton<SoundManager> {

	public enum SoundType {
		// ADD SOUNDS AT THE END!!!
        Walk,
		Jump,
		Swap,
		Stomp,
		Death,
		NoSwap,
		EnemyDeath,
		Spikes,
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
		public AudioInfo clip;
	}
	public AudioSource oneShotAudioSource;
	public AudioSource loopAudioSource;
	public List<AudioList> sounds;
	private Dictionary<SoundType,AudioList> soundsDictionary;

	//======================================================
	private void Start() {
		loopAudioSource.loop = true;
		loopAudioSource.enabled = false;
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
		if( !soundsDictionary.ContainsKey( sound ) ) {
			Log.Error( "SoundManager.PlaySound: Missing audio list: " + sound.ToString() );
			return;
		}

		AudioList audioList = soundsDictionary[sound];
		if( audioList.clip == null ) {
			Log.Error( "SoundManager.PlaySound: empty audio clip list: " + sound.ToString() );
			return;
		}

		AudioInfo audioInfo = audioList.clip;
		AudioClip audioToPlay = audioInfo.clip;
		float volume = audioInfo.volume;

		if( audioToPlay == null ) {
			Log.Error( "SoundManager.PlaySound: Missing sound: " + sound.ToString() );
			return;
		}
		oneShotAudioSource.PlayOneShot( audioToPlay, volume );
	}

	//======================================================
	public void LoopSound( SoundType sound, bool enabled ) {
		if( !soundsDictionary.ContainsKey( sound ) ) {
			Log.Error( "SoundManager.PlaySound: Missing audio list: " + sound.ToString() );
			return;
		}
		
		AudioList audioList = soundsDictionary[sound];
		if( audioList.clip == null ) {
			Log.Error( "SoundManager.PlaySound: empty audio clip list: " + sound.ToString() );
			return;
		}
		
		AudioInfo audioInfo = audioList.clip;
		AudioClip audioToPlay = audioInfo.clip;
		
		if( audioToPlay == null ) {
			Log.Error( "SoundManager.PlaySound: Missing sound: " + sound.ToString() );
			return;
		}
		if( loopAudioSource.enabled == enabled ) {
			return;
		}
		loopAudioSource.volume = audioInfo.volume;
		loopAudioSource.clip = audioInfo.clip;
		loopAudioSource.enabled = enabled;
	}
}
