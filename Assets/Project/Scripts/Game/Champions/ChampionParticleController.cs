using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChampionParticleController : MonoBehaviour
{

	public GameObject orangeGlow, redGlow, cyanGlow;
	public ParticleSystem bloodSplatter, bloodDrip;

	public void PlayEffect(ParticleSystem particleSystem)
	{
		particleSystem.Stop();
		particleSystem.Play();
	}
}
