using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DamageTypeDeprecated { Melee, Ranged, Fire, Lightning, Water, Shadow, Unblockable }

public class ChampionDeprecated : ScriptableObject
{
    public string championName;
    public Sprite championImage;
    public int currentHP;
    public int maxHP;
    public int attackDamage;
    public string attackName;
    public DamageTypeDeprecated damageType;
}
