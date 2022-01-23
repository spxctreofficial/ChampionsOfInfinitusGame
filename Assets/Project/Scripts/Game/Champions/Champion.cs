using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Champion", menuName = "ChampionsOfInfinitusGame/Champion/New Champion")]
public class Champion : ScriptableObject {
	// Identification
	public string championName;
	public string championID;
	public Sprite avatar;
	[TextArea(3, 15)]
	public string description;
	
	// Basic Information
	public enum Gender { Male, Female, Nonbinary, Undefined }
	public enum Faction { Castlefel, Regime, Empire, Order, WraithLegion, Undefined }
	public enum Race { Human, Wraith, Dragon, Dragonborn, Arachnoid, Centaur }
	public enum Class { Warrior, Brute, Mage, Priest, Shaman, Rogue, AllRounder }
	public Gender gender;
	public Faction faction;
	public Race race;

	// Statistics
	[Range(1, 100)]
	public int maxHP;
	public WeaponScriptableObject signatureWeapon;
	public Class championClass;

	public List<AbilityScriptableObject> abilities;
	
	// Shop
	public enum Unlockability { ShopItem, EarnableItem, Unplayable }
	public Unlockability unlockability;
	public int value;
}
