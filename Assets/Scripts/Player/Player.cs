using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType { Normal, Fire, Lightning, Water }

public class Player : MonoBehaviour
{
    public int currentHP;
    public int maxHP;

    public int damage;
    public DamageType damageType;
}
