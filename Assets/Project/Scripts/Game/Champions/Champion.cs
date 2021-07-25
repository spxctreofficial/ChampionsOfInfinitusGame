using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Champion", menuName = "LandOfHeroesGame/Champion/New Champion")]
public class Champion : ScriptableObject {
	// Identification
	public string championName;
	public string championID;
	public Sprite avatar;
	[TextArea(3, 15)]
	public string description;
	
	// Basic Information
	public enum Gender { Male, Female, Nonbinary, Undefined }
	public enum Faction { Castlefel, Regime, Empire, Order, LegionOfWraiths, Undefined }
	public enum Race { Human, Wraith, Dragon, Dragonborn, Arachnoid, Centaur }
	public Gender gender;
	public Faction faction;
	public Race race;

	// Statistics
	public int maxHP;
	public int currentHP;

	public int attackDamage;
	public DamageType attackDamageType;
	public string attackName;

	public List<Ability> abilities;
	
	// Shop
	public enum Unlockability { ShopItem, EarnableItem, Unplayable }
	public Unlockability unlockability;
	public int value;
}
