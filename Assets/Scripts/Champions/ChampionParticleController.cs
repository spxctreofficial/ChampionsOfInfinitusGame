using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
public class ChampionParticleController : MonoBehaviour {
	[SerializeField]
	private ParticleSystem orangeGlow, cyanGlow, bloodSplatter, bloodDrip;

	public ParticleSystem OrangeGlow {
		get {
			return orangeGlow;
		}
	}
	public ParticleSystem CyanGlow {
		get {
			return cyanGlow;
		}
	}
	public ParticleSystem BloodSplatter {
		get {
			return bloodSplatter;
		}
	}
	public ParticleSystem BloodDrip {
		get {
			return bloodDrip;
		}
	}

	public void PlayEffect(ParticleSystem particleSystem) {
		particleSystem.Stop();
		particleSystem.Play();
	}
	public void StopEffect(ParticleSystem particleSystem) {
		particleSystem.Stop();
	}
}