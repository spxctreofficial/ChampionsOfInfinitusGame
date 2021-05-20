using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;
public class StatisticManager : MonoBehaviour
{
	public static StatisticManager instance;
	public List<MatchStatistic> matchStatistics = new List<MatchStatistic>();

	public bool winState;
	public MatchStatistic playerChampionStatistic;

	public int initialGoldReward;
	public int goldReward;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public void StartTrackingStatistics()
	{
		foreach (ChampionController champion in GameController.instance.champions)
		{
			matchStatistics.Add(new MatchStatistic(champion.champion));
			Debug.Log(matchStatistics[GameController.instance.champions.IndexOf(champion)].champion);
		}
		playerChampionStatistic = matchStatistics[0];
		Debug.Log(playerChampionStatistic.champion);
	}
	public void TrackRemainingStatistics(ChampionController champion)
	{
		var index = GameController.instance.champions.IndexOf(champion);
		var matchStatistic = matchStatistics[index];

		matchStatistic.remainingHP = champion.currentHP;
	}

	public IEnumerator RewardCalculation(TMP_Text bonusRewardLog)
	{
		void Untween()
		{
			LeanTween.move(bonusRewardLog.gameObject, Vector2.zero, 0.25f).setEaseInOutQuad();
			LeanTween.scale(bonusRewardLog.gameObject, Vector3.zero, 0.25f).setEaseInOutQuad();
		}

		Debug.Log("its being called");
		initialGoldReward = winState ? Random.Range(290, 311) : Random.Range(290, 311) / 10;
		int successfulAttackBonus = instance.playerChampionStatistic.successfulAttacks * 5;
		int successfulDefendBonus = instance.playerChampionStatistic.successfulDefends * 2;
		int failedAttacksBonus = instance.playerChampionStatistic.failedAttacks;
		int failedDefendsPenalty = instance.playerChampionStatistic.failedDefends;

		int killCountBonus = instance.playerChampionStatistic.killCount * 100;
		int totalDamageDealtBonus = instance.playerChampionStatistic.totalDamageDealt / 2;
		int totalDamageReceivedCompensation = instance.playerChampionStatistic.totalDamageReceived / 4;
		int totalHealthRemainingBonus = (instance.playerChampionStatistic.remainingHP / instance.playerChampionStatistic.champion.maxHP) * 100;
		switch (GameController.instance.difficulty)
		{
			case GameController.Difficulty.Noob:
				initialGoldReward /= 5;
				break;
			case GameController.Difficulty.Novice:
				initialGoldReward /= 2;
				break;
			case GameController.Difficulty.Warrior:
				break;
			case GameController.Difficulty.Champion:
				initialGoldReward *= (int)1.2f;
				break;
		}

		// PosLoop();
		bonusRewardLog.text = winState ? "Win Reward" : "Loss Compensation";
		bonusRewardLog.text += "\n" + initialGoldReward;
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Successful Attack Bonus\n+" + successfulAttackBonus;
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Failed Attack Bonus\n+" + failedAttacksBonus;
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Successful Defense Bonus\n+" + successfulDefendBonus;
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Failed Defense Penalty\n-" + failedDefendsPenalty;
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Kill Count Bonus\n+" + killCountBonus;
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Total Damage Bonus\n+" + totalDamageDealtBonus;
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Total Damage Received Compensation\n+" + totalDamageReceivedCompensation;
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Health Remaining Bonus (" + playerChampionStatistic.remainingHP / playerChampionStatistic.champion.maxHP + "%)\n+" + totalHealthRemainingBonus;
		yield return new WaitForSeconds(0.5f);
		Untween();

		int goldAmount = PlayerPrefs.HasKey("goldAmount") ? PlayerPrefs.GetInt("goldAmount") : 0;

		switch (GameController.instance.gamemodes)
		{
			case GameController.Gamemodes.FFA:
				goldReward = initialGoldReward;
				goldReward += successfulAttackBonus;
				goldReward += successfulDefendBonus;
				goldReward += failedAttacksBonus;
				goldReward -= failedDefendsPenalty;

				goldReward += killCountBonus;
				goldReward += totalDamageDealtBonus;
				goldReward += totalDamageReceivedCompensation;
				goldReward += totalHealthRemainingBonus;

				PlayerPrefs.SetInt("goldAmount", goldAmount + goldReward);
				PlayerPrefs.Save();
				break;
			default:
				goldReward = initialGoldReward;
				break;
		}

		bonusRewardLog.transform.parent.GetChild(2).gameObject.SetActive(true);
	}

	[Obsolete("GetChampionStatistics() has been deprecated." +
	          " For more convenience and cleaner code, use method GetMatchStatistics() of the ChampionController class instead.")]
	public MatchStatistic GetChampionStatistics(ChampionController champion)
	{
		var index = GameController.instance.champions.IndexOf(champion);
		return matchStatistics[index];
	}
}
