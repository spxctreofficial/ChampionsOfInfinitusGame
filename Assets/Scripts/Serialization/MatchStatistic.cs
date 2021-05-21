using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MatchStatistic {
	public Champion champion;

	public int totalAttacks, successfulAttacks, failedAttacks, totalDefends, successfulDefends, failedDefends;
	public int totalDamageDealt, totalDamageReceived, totalAmountHealed;
	public int remainingHP;
	public int killCount;

	public MatchStatistic(Champion champion) {
		this.champion = champion;
	}
}
