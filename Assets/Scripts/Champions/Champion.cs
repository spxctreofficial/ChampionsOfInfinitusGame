using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DamageType { Melee, Ranged, Fire, Lightning, Water, Shadow, Unblockable }

[CreateAssetMenu(fileName = "New Champion", menuName = "Champion")]
public class Champion : ScriptableObject
{
    public string championName;
    public Sprite championImage;
    public int currentHP;
    public int maxHP;
    public int attackDamage;
    public string attackName;
    public DamageType damageType;
}
