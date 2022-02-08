using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

[System.Serializable]
public class MatchStatistic
{
	public Champion champion;

	public int totalAttacks, successfulAttacks, failedAttacks, totalDefends, successfulDefends, failedDefends;
	public int totalDamageDealt, totalDamageReceived, totalAmountHealed;
	public int totalCardsDealt, totalCardsDiscarded;
	public int remainingHP;
	public int killCount;

	public List<DamageHistory> damageHistories = new List<DamageHistory>();

	public MatchStatistic(Champion champion)
	{
		this.champion = champion;
	}
}

[System.Serializable]
public class DamageHistory
{
	public ChampionController dealtAgainst;
	public int amount, attacksAgainst;

	public DamageHistory(ChampionController dealtAgainst, int amount = 0)
	{
		this.dealtAgainst = dealtAgainst;
		this.amount = amount;
		attacksAgainst = 0;
	}
}
