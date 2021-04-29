using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Champion", menuName = "New Champion")]
public class Champion : ScriptableObject
{
	public new string name;
	public Sprite avatar;
	public enum Gender { Male, Female, Nonbinary, Undefined }
	public Gender gender;

	public int maxHP;
	public int currentHP;

	public int attackDamage;
	public DamageType attackDamageType;
	public string attackName;


}
