using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum DamageType { Normal, Fire, Lightning, Water }

public class Player : MonoBehaviour
{
    public int currentHP;
    public int maxHP;
    public int attackDamage;
    public string attackName;
    public DamageType damageType;
    
    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    public bool isAttacked = false;

    public void Damage(int amount)
    {
        currentHP -= amount;
    }
    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }
}
