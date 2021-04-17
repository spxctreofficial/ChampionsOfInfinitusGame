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
    
    public bool isDead = false;
    public bool isAttacking = false;
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
