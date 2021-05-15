using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Ability", menuName = "New Ability")]
public class Ability : ScriptableObject
{
	public enum AbilityType { Passive, Active, AttackB, DefenseB, Ultimate }
	public enum AbilityEffect { Positive, Neutral, Negative }

	public string abilityName;
	public string abilityID;
	public Sprite sprite;
	[TextArea(3, 15)]
	public string abilityDescription;
	public AbilityType abilityType;
	public List<Champion> isExclusiveTo;
	public List<AudioClip> customAudioClips;
}
