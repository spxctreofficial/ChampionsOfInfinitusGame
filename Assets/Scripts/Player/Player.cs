using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum DamageType { Normal, Fire, Lightning, Water }

public class Player : MonoBehaviour
{
    public string championName;
    public int currentHP;
    public int maxHP;
    public int attackDamage;
    public string attackName;
    public DamageType damageType;

    [HideInInspector]
    public int cards;
    [HideInInspector]
    public int discardAmount;
    [HideInInspector]
    public int spadesBeforeExhaustion = 1;
    [HideInInspector]
    public int heartsBeforeExhaustion = 1;
    [HideInInspector]
    public int diamondsBeforeExhaustion = 1;

    [HideInInspector]
    public bool isDead = false;
    [HideInInspector]
    public bool isAttacking = false;
    [HideInInspector]
    public bool isAttacked = false;
    [HideInInspector]
    public bool mustDiscard = false;

    void Update()
    {

    }


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
    [HideInInspector]
    public void EnlargeChampionDashboard()
    {
        GameHandler gameHandler = GameObject.FindWithTag("GameHandler").GetComponent<GameHandler>();
        gameHandler.EnlargeChampionDashboard();
    }
}
