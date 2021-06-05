using UnityEngine.Audio;
using System;
using UnityEngine;

public class AudioController : MonoBehaviour {

	public static AudioController instance;

	public AudioMixerGroup mixerGroup;
	public AudioLowPassFilter lowPassFilter;

	public Sound[] sounds;

	private void Awake() {
		if (instance != null) {
			Destroy(gameObject);
		}
		else {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}

		foreach (var s in sounds) {
			s.source = gameObject.AddComponent<AudioSource>();
			s.source.clip = s.clip;
			s.source.loop = s.loop;

			s.source.outputAudioMixerGroup = mixerGroup;
		}
	}

	/// <summary>
	/// Plays a sound with the matching name.
	/// </summary>
	/// <param name="sound"></param>
	public void Play(string sound) {
		var s = Array.Find(sounds, item => item.name == sound);
		if (s == null) {
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
		s.source.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

		s.source.Play();
	}
	/// <summary>
	/// Plays a sound with the given `clip`.
	/// 
	/// If `loop` is false, the AudioSource will clean itself up.
	/// If `loop` is true, the AudioSource must be manually stopped by calling `Stop()`.
	/// 
	/// SpatialBlend will be ignored if `loop` is set to false.
	/// </summary>
	/// <param name="clip"></param>
	/// <param name="volume"></param>
	/// <param name="loop"></param>
	public void Play(AudioClip clip, bool loop = false, float volume = 0.3f, float spatialBlend = 0.5f) {
		switch (loop) {
			case true:
				var audioSource = gameObject.AddComponent<AudioSource>();
				audioSource.clip = clip;
				audioSource.loop = loop;
				audioSource.volume = volume;
				audioSource.spatialBlend = spatialBlend;

				audioSource.outputAudioMixerGroup = mixerGroup;

				audioSource.Play();
				break;
			case false:
				AudioSource.PlayClipAtPoint(clip, transform.localPosition, volume);
				break;
		}
	}
	/// <summary>
	/// Plays a sound cloned from another AudioSource.
	/// </summary>
	/// <param name="source"></param>
	public void Play(AudioSource source) {
		var audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = source.clip;
		audioSource.loop = source.loop;
		audioSource.volume = source.volume;
		audioSource.pitch = source.pitch;
		audioSource.spatialBlend = source.spatialBlend;

		audioSource.outputAudioMixerGroup = mixerGroup;

		audioSource.Play();
	}
	/// <summary>
	/// Stops a sound with the matching name.
	/// </summary>
	/// <param name="sound"></param>
	public void Stop(string sound) {
		var s = Array.Find(sounds, item => item.name == sound);
		if (s == null) {
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		s.source.Stop();
	}
	/// <summary>
	/// Stops a sound with the given `clip`.
	/// </summary>
	/// <param name="clip"></param>
	public void Stop(AudioClip clip, bool destroySource = false) {
		AudioSource[] audioSources = GetComponents<AudioSource>();
		foreach (var audioSource in audioSources) {
			if (clip != audioSource.clip) continue;

			audioSource.Stop();
			if (destroySource) Destroy(audioSource);
			Debug.Log("It worked");
		}
	}

	public AudioSource GetAudioSource(AudioClip clip) {
		AudioSource[] audioSources = GetComponents<AudioSource>();
		foreach (var audioSource in audioSources) {
			if (audioSource.clip != clip) continue;
			return audioSource;
		}
		return null;
	}
}
