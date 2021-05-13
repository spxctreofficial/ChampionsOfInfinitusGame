using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Champion", menuName = "New Champion")]
public class Champion : ScriptableObject
{
	public new string name;
	public Sprite avatar;
	public enum Gender { Male, Female, Nonbinary, Undefined }
	public enum Faction { Castlefel, Regime, Empire, Order, LegionOfWraiths, Undefined }
	public enum Race { Human, Wraith, Dragon, Dragonborn, Arachnoid, Centaur }
	public Gender gender;
	public Faction faction;
	public Race race;

	public int maxHP;
	public int currentHP;

	public int attackDamage;
	public DamageType attackDamageType;
	public string attackName;

	public List<Ability> abilities;
}
