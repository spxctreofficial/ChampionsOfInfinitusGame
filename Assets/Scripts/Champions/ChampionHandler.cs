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

    public void ChampionSetup()
    {
        championName = champion.championName;
        championImage = champion.championImage;
        currentHP = champion.currentHP;
        maxHP = champion.maxHP;
        attackDamage = champion.attackDamage;
        attackName = champion.attackName;
        damageType = champion.damageType;
        isAttackUnblockable = champion.isAttackUnblockable;
        cards = 0;
        discardAmount = 0;
        spadesBeforeExhaustion = champion.spadesBeforeExhaustion;
        heartsBeforeExhaustion = champion.heartsBeforeExhaustion;
        diamondsBeforeExhaustion = champion.diamondsBeforeExhaustion;
        isDead = champion.isDead;
        isAttacking = champion.isAttacking;
        isAttacked = champion.isAttacked;
        mustDiscard = champion.mustDiscard;

        Image thisImage = GetComponent<Image>();
        thisImage.sprite = this.championImage;
    }

    public void EnlargeChampionDashboard()
    {
        GameHandler gameHandler = GameObject.FindWithTag("GameHandler").GetComponent<GameHandler>();
        gameHandler.EnlargeChampionDashboard();
    }
}
