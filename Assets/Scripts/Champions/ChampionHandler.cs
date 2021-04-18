using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChampionHandler : MonoBehaviour
{
    public Champion champion;

    public string championName;
    public Sprite championImage;
    public int currentHP, maxHP, attackDamage;
    public string attackName;
    public DamageType damageType;

    [HideInInspector]
    public bool isAttackUnblockable;
    [HideInInspector]
    public int cards, discardAmount;
    [HideInInspector]
    public int spadesBeforeExhaustion, heartsBeforeExhaustion, diamondsBeforeExhaustion;
    [HideInInspector]
    public bool isDead, isAttacking, isAttacked, mustDiscard;

    private void Update()
    {
    }

    public void ChampionSetup()
    {
        championName = champion.championName;
        championImage = champion.championImage;
        currentHP = champion.currentHP;
        maxHP = champion.maxHP;
        attackDamage = champion.attackDamage;
        attackName = champion.attackName;
        damageType = champion.damageType;
        isAttackUnblockable = false;
        cards = 0;
        discardAmount = 0;
        spadesBeforeExhaustion = 1;
        heartsBeforeExhaustion = 3;
        diamondsBeforeExhaustion = 1;
        isDead = false;
        isAttacking = false;
        isAttacked = false;
        mustDiscard = false;

        Image thisImage = GetComponent<Image>();
        thisImage.sprite = this.championImage;
    }
    [HideInInspector]
    public void Damage(int amount)
    {
        currentHP -= amount;
    }
    [HideInInspector]
    public void Heal(int amount)
    {
        currentHP += amount;
        if (currentHP > maxHP)
            currentHP = maxHP;
    }
    public void EnlargeChampionDashboard()
    {
        GameHandler gameHandler = GameObject.FindWithTag("GameHandler").GetComponent<GameHandler>();
        gameHandler.EnlargeChampionDashboard();
    }
}
