using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionParticleController : MonoBehaviour {
	[SerializeField]
	private ParticleSystem orangeGlow, cyanGlow, redGlow, greenGlow, bloodSplatter;
	public ParticleSystem bloodDrip;

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
	public ParticleSystem RedGlow {
		get {
			return redGlow;
		}
	}
	public ParticleSystem GreenGlow {
		get {
			return greenGlow;
		}
	}
	public ParticleSystem BloodSplatter {
		get {
			return bloodSplatter;
		}
	}

	public void PlayEffect(ParticleSystem particleSystem) {
		particleSystem.Stop();
		particleSystem.Play();
	}
}