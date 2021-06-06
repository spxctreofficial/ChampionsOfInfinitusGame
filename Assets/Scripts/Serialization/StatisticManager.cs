using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
public class StatisticManager : MonoBehaviour {
	public static StatisticManager instance;
	public List<MatchStatistic> matchStatistics = new List<MatchStatistic>();

	public bool winState;
	public MatchStatistic playerChampionStatistic;

	public int initialGoldReward;
	public int goldReward;

	private void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
		}
	}

	public void StartTrackingStatistics() {
		foreach (ChampionController champion in GameController.instance.champions) {
			matchStatistics.Add(new MatchStatistic(champion.champion));
		}
		playerChampionStatistic = matchStatistics[0];
	}
	public void TrackRemainingStatistics(ChampionController champion) {
		var index = GameController.instance.champions.IndexOf(champion);
		var matchStatistic = matchStatistics[index];

		matchStatistic.remainingHP = champion.currentHP;
	}

	public IEnumerator RewardCalculation(TMP_Text bonusRewardLog) {
		void Untween() {
			LeanTween.move(bonusRewardLog.gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.25f).setEaseInOutQuad();
			LeanTween.scale(bonusRewardLog.gameObject.GetComponent<RectTransform>(), Vector3.zero, 0.25f).setEaseInOutQuad();
		}

		Debug.Log("its being called");

		initialGoldReward = winState ? Random.Range(290, 311) : Random.Range(290, 311) / 10;
		var successfulAttackBonus = instance.playerChampionStatistic.successfulAttacks * 5;
		var successfulDefendBonus = instance.playerChampionStatistic.successfulDefends * 2;
		var failedAttacksBonus = instance.playerChampionStatistic.failedAttacks;
		var failedDefendsPenalty = instance.playerChampionStatistic.failedDefends;

		var killCountBonus = instance.playerChampionStatistic.killCount * 100;
		var totalDamageDealtBonus = instance.playerChampionStatistic.totalDamageDealt / 2;
		var totalDamageReceivedCompensation = instance.playerChampionStatistic.totalDamageReceived / 4;
		var totalHealthRemainingBonus = instance.playerChampionStatistic.remainingHP / instance.playerChampionStatistic.champion.maxHP * 100;
		switch (GameController.instance.difficulty) {
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
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Successful Attack Bonus\n+" + successfulAttackBonus;
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Failed Attack Bonus\n+" + failedAttacksBonus;
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Successful Defense Bonus\n+" + successfulDefendBonus;
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Failed Defense Penalty\n-" + failedDefendsPenalty;
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Kill Count Bonus\n+" + killCountBonus;
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Total Damage Bonus\n+" + totalDamageDealtBonus;
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Total Damage Received Compensation\n+" + totalDamageReceivedCompensation;
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		// PosLoop();
		bonusRewardLog.text = "Health Remaining Bonus (" + totalHealthRemainingBonus + "%)\n+" + totalHealthRemainingBonus;
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);
		Untween();

		switch (GameController.instance.gamemodes) {
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

				DataManager.instance.GoldAmount += goldReward;
				break;
			default:
				goldReward = initialGoldReward;
				break;
		}

		bonusRewardLog.transform.parent.GetChild(2).gameObject.SetActive(true);
	}

	[Obsolete("GetChampionStatistics() has been deprecated." +
	          " For more convenience and cleaner code, use method GetMatchStatistics() of the ChampionController class instead.")]
	public MatchStatistic GetChampionStatistics(ChampionController champion) {
		var index = GameController.instance.champions.IndexOf(champion);
		return matchStatistics[index];
	}
}
