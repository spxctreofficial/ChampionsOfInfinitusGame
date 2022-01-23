using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class TutorialStatisticManager : StatisticManager {
	public static new TutorialStatisticManager instance;

	protected override void Awake() {
		base.Awake();

		if (instance == null) {
			instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	public override IEnumerator RewardCalculation(TMP_Text bonusRewardLog, GameObject collectButton) {
		collectButton.SetActive(false);
		initialGoldReward = !DataManager.instance.firstRunTutorial ? 2000 : 50;
		bonusRewardLog.text = winState ? "Win Reward" : "Loss Compensation";
		bonusRewardLog.text += "\n" + initialGoldReward;
		AudioManager.instance.Play("cointoss0" + Random.Range(1, 3));
		yield return new WaitForSeconds(0.5f);

		LeanTween.move(bonusRewardLog.gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.25f).setEaseInOutQuad();
		LeanTween.scale(bonusRewardLog.gameObject.GetComponent<RectTransform>(), Vector3.zero, 0.25f).setEaseInOutQuad();

		goldReward = initialGoldReward;

		DataManager.instance.goldAmount += goldReward;
		DataManager.instance.firstRunTutorial = winState;
		DataManager.instance.Save();
		AudioManager.instance.Play("tutorial0" + Random.Range(1, 4));

		collectButton.SetActive(true);
	}
}
