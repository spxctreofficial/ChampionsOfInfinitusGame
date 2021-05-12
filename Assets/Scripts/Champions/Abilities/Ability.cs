using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "New Ability")]
public class Ability : ScriptableObject
{
	public string abilityName;
	public string abilityID;
	public bool isActiveAbility;
	public List<Champion> isExclusiveTo;
	public List<AudioClip> customAudioClips;
}
