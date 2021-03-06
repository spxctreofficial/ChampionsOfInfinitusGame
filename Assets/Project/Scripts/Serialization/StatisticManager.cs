using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using UnityEngine.UI;

public abstract class StatisticManager : MonoBehaviour
{
	public static StatisticManager instance;
	public List<MatchStatistic> matchStatistics = new List<MatchStatistic>();

	[HideInInspector]
	public bool winState;
	public MatchStatistic playerChampionStatistic;

	protected int initialGoldReward;
	protected int goldReward;

	protected virtual void Awake()
	{
		if (instance == null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void StartTrackingStatistics(ChampionController champion)
	{
		champion.matchStatistic = new MatchStatistic(champion.champion);
		matchStatistics.Add(champion.matchStatistic);
		if (champion.isPlayer) playerChampionStatistic = champion.matchStatistic;
	}
	public void TrackRemainingStatistics(ChampionController champion)
	{
		champion.matchStatistic.remainingHP = champion.currentHP;
	}

	public virtual IEnumerator RewardCalculation(TMP_Text bonusRewardLog, GameObject collectButton)
	{

		initialGoldReward = winState ? Random.Range(290, 311) : Random.Range(290, 311) / 10;
		int successfulAttackBonus = instance.playerChampionStatistic.successfulAttacks * 5;
		int successfulDefendBonus = instance.playerChampionStatistic.successfulDefends * 2;
		int failedAttacksBonus = instance.playerChampionStatistic.failedAttacks;
		int failedDefendsPenalty = instance.playerChampionStatistic.failedDefends;

		int killCountBonus = instance.playerChampionStatistic.killCount * 100;
		int totalDamageDealtBonus = instance.playerChampionStatistic.totalDamageDealt / 2;
		int totalDamageReceivedCompensation = instance.playerChampionStatistic.totalDamageReceived / 4;
		float totalHealthRemainingBonus = (float) instance.playerChampionStatistic.remainingHP / instance.playerChampionStatistic.champion.maxHP * 100;
		switch (GameManager.instance.difficulty)
		{
			case GameManager.Difficulty.Noob:
				initialGoldReward /= 5;
				break;
			case GameManager.Difficulty.Novice:
				initialGoldReward /= 2;
				break;
			case GameManager.Difficulty.Warrior:
				break;
			case GameManager.Difficulty.Champion:
				initialGoldReward *= (int) 1.2f;
				break;
		}

		collectButton.SetActive(false);
		// PosLoop();
		bonusRewardLog.text = winState ? "Win Reward" : "Loss Compensation";
		bonusRewardLog.text += "\n" + initialGoldReward;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Successful Attack Bonus\n+" + successfulAttackBonus;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Failed Attack Bonus\n+" + failedAttacksBonus;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Successful Defense Bonus\n+" + successfulDefendBonus;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Failed Defense Penalty\n-" + failedDefendsPenalty;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Kill Count Bonus\n+" + killCountBonus;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Total Damage Bonus\n+" + totalDamageDealtBonus;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Total Damage Received Compensation\n+" + totalDamageReceivedCompensation;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Health Remaining Bonus (" + totalHealthRemainingBonus + "%)\n+" + totalHealthRemainingBonus;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		LeanTween.move(bonusRewardLog.gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.25f).setEaseInOutQuad();
		LeanTween.scale(bonusRewardLog.gameObject.GetComponent<RectTransform>(), Vector3.zero, 0.25f).setEaseInOutQuad();

		goldReward = initialGoldReward;
		goldReward += successfulAttackBonus;
		goldReward += successfulDefendBonus;
		goldReward += failedAttacksBonus;
		goldReward -= failedDefendsPenalty;

		goldReward += killCountBonus;
		goldReward += totalDamageDealtBonus;
		goldReward += totalDamageReceivedCompensation;
		goldReward += (int) totalHealthRemainingBonus;

		DataManager.instance.goldAmount += goldReward;
		DataManager.instance.Save();
		AudioManager.instance.Play("tutorial0" + Random.Range(1, 4));

		collectButton.SetActive(true);
	}
}
