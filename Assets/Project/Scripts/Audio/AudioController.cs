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
	}

	/// <summary>
	/// Plays a sound with the matching name.
	/// </summary>
	/// <param name="sound"></param>
	public void Play(string sound, bool playAsNewSound = true) {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s is null) {
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}
		if (!playAsNewSound) {
			foreach (AudioSource source in GetComponents<AudioSource>()) {
				if (source.clip == s.clip) {
					source.Stop();
					if (source.loop) Destroy(source);
				}
			}
		}
		
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = s.clip;
		audioSource.loop = s.loop;
		audioSource.outputAudioMixerGroup = mixerGroup;
		
		audioSource.volume = s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
		audioSource.pitch = s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

		audioSource.Play();
		if (!audioSource.loop) Destroy(audioSource, audioSource.clip.length);
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
	/// <param name="spatialBlend"></param>
	public void Play(AudioClip clip, bool loop = false, float volume = 0.3f, float pitch = 1f,float spatialBlend = 0.5f) {
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = clip;
		audioSource.loop = loop;
		audioSource.volume = volume;
		audioSource.pitch = pitch;
		audioSource.spatialBlend = spatialBlend;

		audioSource.outputAudioMixerGroup = mixerGroup;
		
		audioSource.Play();

		if (!loop) Destroy(audioSource, clip.length);
	}
	/// <summary>
	/// Plays a sound cloned from another AudioSource.
	/// </summary>
	/// <param name="source"></param>
	public void Play(AudioSource source) {
		AudioSource audioSource = gameObject.AddComponent<AudioSource>();
		audioSource.clip = source.clip;
		audioSource.loop = source.loop;
		audioSource.volume = source.volume;
		audioSource.pitch = source.pitch;
		audioSource.spatialBlend = source.spatialBlend;

		audioSource.outputAudioMixerGroup = mixerGroup;

		audioSource.Play();
		if (!audioSource.loop) Destroy(audioSource, audioSource.clip.length);
	}
	/// <summary>
	/// Stops a sound with the matching name.
	/// </summary>
	/// <param name="sound"></param>
	public void Stop(string sound) {
		Sound s = Array.Find(sounds, item => item.name == sound);
		if (s == null) {
			Debug.LogWarning("Sound: " + name + " not found!");
			return;
		}

		foreach (AudioSource source in GetComponents<AudioSource>()) {
			if (source.clip == s.clip) {
				source.Stop();
				if (source.loop) Destroy(source);
			}
		}
	}
	/// <summary>
	/// Stops all sounds with the given `clip`.
	/// </summary>
	/// <param name="clip"></param>
	/// <param name="destroySource"></param>
	public void Stop(AudioClip clip, bool destroySource = true) {
		foreach (AudioSource audioSource in GetComponents<AudioSource>()) {
			if (clip != audioSource.clip) continue;

			audioSource.Stop();
			if (audioSource.loop) Destroy(audioSource);
		}
	}

	public AudioSource GetAudioSource(AudioClip clip) {
		foreach (AudioSource audioSource in GetComponents<AudioSource>()) {
			if (audioSource.clip != clip) continue;
			return audioSource;
		}
		return null;
	}
}
