﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class SandboxGameController : GameController {
	public static new SandboxGameController instance;

	[Header("Configuration")]
	public GameObject gamemodeSelectionConfig;
	public GameObject mapSelectionConfig;
	public GameObject championSelectionConfig;
	public GameObject difficultySelectionConfig;

	public GameObject gamemodeSelectionButtonPrefab;
	public GameObject mapSelectionButtonPrefab;
	public GameObject championSelectionButtonPrefab;
	public GameObject difficultySelectionButtonPrefab;

	public Gamemodes gamemodes;
	[HideInInspector]
	public bool hasChosenGamemode;

	protected override void Awake() {
		base.Awake();

		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
	}
	private void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha6)) {
			foreach (ChampionController player in champions) {
				if (!player.isPlayer) continue;
				StartCoroutine(player.Damage(100, DamageType.Melee));

				foreach (ChampionController champion in champions) {
					if (champion.teamMembers.Contains(player) || champion == player) continue;
					champion.currentNemesis = player;
				}
				Debug.Log("applied successfully");
			}
		}
		if (Input.GetKeyDown(KeyCode.Alpha4)) {
			foreach (ChampionController champion in champions) {
				if (champion.isPlayer) continue;
				champion.currentHP = 1;
			}
		}
	}

	protected override IEnumerator GamePrep() {
		hasChosenGamemode = false;
		currentMap = null;
		hasChosenDifficulty = false;

		gameArea.SetActive(false);
		mapSelectionConfig.SetActive(true);

		foreach (Map map in DataManager.instance.mapIndex.maps) {
			MapSelectionButton mapSelectionButton = Instantiate(mapSelectionButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<MapSelectionButton>();
			mapSelectionButton.mapComponent = map;
			mapSelectionButton.transform.SetParent(mapSelectionConfig.transform.GetChild(0), false);
		}
		yield return new WaitUntil(() => currentMap != null);

		mapSelectionConfig.SetActive(false);
		championSelectionConfig.SetActive(true);

		foreach (Champion championSO in DataManager.instance.ownedChampions) {
			ChampionSelectionButton championSelectionButton = Instantiate(championSelectionButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<ChampionSelectionButton>();
			championSelectionButton.championComponent = championSO;
			championSelectionButton.transform.SetParent(championSelectionConfig.transform.GetChild(0), false);
		}
		yield return new WaitUntil(() => players.Count == 1);

		championSelectionConfig.SetActive(false);
		gamemodeSelectionConfig.SetActive(true);

		foreach (Gamemodes gamemode in (Gamemodes[])Enum.GetValues(typeof(Gamemodes))) {
			if (gamemode == Gamemodes.Duel) continue;
			GamemodeSelectionButton gamemodeSelectionButton = Instantiate(gamemodeSelectionButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<GamemodeSelectionButton>();
			gamemodeSelectionButton.gamemode = gamemode;
			gamemodeSelectionButton.transform.SetParent(gamemodeSelectionConfig.transform.GetChild(0), false);
		}
		yield return new WaitUntil(() => hasChosenGamemode);

		gamemodeSelectionConfig.SetActive(false);
		difficultySelectionConfig.SetActive(true);

		foreach (Difficulty difficulty in (Difficulty[])Enum.GetValues(typeof(Difficulty))) {
			DifficultySelectionButton difficultySelectionButton = Instantiate(difficultySelectionButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<DifficultySelectionButton>();
			difficultySelectionButton.difficulty = difficulty;
			difficultySelectionButton.transform.SetParent(difficultySelectionConfig.transform.GetChild(0), false);
		}
		difficultySelectionConfig.transform.GetChild(0).GetChild(3).SetAsFirstSibling();
		difficultySelectionConfig.transform.GetChild(0).GetChild(1).SetAsLastSibling();
		difficultySelectionConfig.transform.GetChild(0).GetChild(1).SetSiblingIndex(2);
		yield return new WaitUntil(() => hasChosenDifficulty);

		difficultySelectionConfig.SetActive(false);
		gameArea.SetActive(true);

		ChampionSlot.CreateDefaultSlots();
	}
	protected override IEnumerator GameSetup() {
		ChampionSlot slot;
		int players;

		gameArea.GetComponent<Image>().sprite = currentMap.mapBackground;
		gameArea.GetComponent<AudioSource>().clip = currentMap.themeSong;

		AudioController.instance.Play(gameArea.GetComponent<AudioSource>());

		switch (gamemodes) {
			case Gamemodes.Competitive2v2:
				players = 4;
				break;
			case Gamemodes.FFA:
				players = 3;
				break;
			default:
				players = 2;
				break;
		}

		for (int i = 1; i < players; i++) {
			Champion champion = null;

			switch (difficulty) {
				case Difficulty.Noob:
					foreach (Champion anotherChampion in DataManager.instance.championIndex.champions) {
						if (anotherChampion.championID != "Champion_RegimeSoldier" || anotherChampion.championID != "Champion_RegimeCaptain" || Random.Range(0f, 1f) < 0.5f && DataManager.instance.championIndex.champions.IndexOf(anotherChampion) == DataManager.instance.championIndex.champions.Count - 1) continue;
						champion = anotherChampion;
						break;
					}
					break;
				case Difficulty.Novice:
					int repeats = 0;
					while ((champion == this.players[0] || this.players.Contains(champion) || champion == null || champion.shopCost > 2000) && repeats <= 6) {
						Debug.Log("novice boop");
						champion = DataManager.instance.championIndex.champions[Random.Range(0, DataManager.instance.championIndex.champions.Count)];
						repeats++;
					}

					if (repeats > 6) champion = DataManager.instance.championIndex.champions[Random.Range(0, 2)];
					break;
				case Difficulty.Warrior:
				case Difficulty.Champion:
					repeats = 0;
					while ((champion == this.players[0] || this.players.Contains(champion) || champion == null) && repeats <= 8) {
						Debug.Log("high difficulty boop");
						champion = DataManager.instance.championIndex.champions[Random.Range(0, DataManager.instance.championIndex.champions.Count)];
						repeats++;
					}

					if (repeats > 8) champion = DataManager.instance.championIndex.champions[Random.Range(0, 2)];
					break;
			}

			this.players.Add(champion);
		}

		foreach (Champion champion in this.players) {
			int i = this.players.IndexOf(champion);

			slot = gamemodes switch {
				Gamemodes.FFA => i == 0 ? slots[i] : slots[i + 1],
				_ => slots[i],
			};

			ChampionController championController = Spawn(champion, slot, i == 0);

			// Configuring & Setting Teams
			switch (gamemodes) {
				case Gamemodes.FFA:
					championController.team = championController.championID + i;
					break;
				case Gamemodes.Competitive2v2:
					if (i < 2) {
						championController.team = "PlayerTeam";
					}
					else {
						championController.team = "OpponentTeam";
					}
					break;
				case Gamemodes.Duel:
					champions[0].team = "PlayerTeam";
					champions[1].team = "OpponentTeam";
					break;
			}
		}
		yield break;
	}
	protected override IEnumerator GameEndAction(ChampionController victoriousChampion) {
		TMP_Text gameEndText = gameEndArea.transform.GetChild(0).GetComponent<TMP_Text>();
		TMP_Text gameEndTextTeam = gameEndAreaTeam.transform.GetChild(0).GetComponent<TMP_Text>();
		Image winnerAvatar = gameEndArea.transform.GetChild(1).GetComponent<Image>();
		GameObject winnerAvatars = gameEndAreaTeam.transform.GetChild(1).gameObject;
		GameObject rewardPanel = gameEndArea.transform.GetChild(2).gameObject;
		GameObject rewardPanelTeam = gameEndAreaTeam.transform.GetChild(2).gameObject;
		AudioSource musicSource = AudioController.instance.GetAudioSource(gameArea.GetComponent<AudioSource>().clip);
		float cachedVolume = musicSource.volume;

		switch (gamemodes) {
			case Gamemodes.Competitive2v2:
				gameEndAreaTeam.SetActive(true);

				gameEndTextTeam.text = victoriousChampion.championName + "'s Team wins!";
				foreach (ChampionController champion in champions) {
					if (!champion.teamMembers.Contains(victoriousChampion) && champion != victoriousChampion) continue;
					Image newWinnerAvatar = Instantiate(winnerAvatar, Vector2.zero, Quaternion.identity).GetComponent<Image>();
					newWinnerAvatar.sprite = champion.avatar;
					newWinnerAvatar.transform.SetParent(winnerAvatars.transform, false);
				}
				rewardPanelTeam.transform.localPosition = new Vector2(-1920, 0);

				while (musicSource.volume > 0.5f * cachedVolume) {
					musicSource.volume -= 0.5f * cachedVolume / 180;
					yield return null;
				}
				yield return new WaitForSeconds(3f);

				LeanTween.move(winnerAvatars.GetComponent<RectTransform>(), new Vector2(1920, 0), 0.5f).setEaseInOutQuad();
				LeanTween.move(rewardPanelTeam.GetComponent<RectTransform>(), Vector2.zero, 0.5f).setEaseInOutQuad().setOnComplete(() => {
					StartCoroutine(StatisticManager.instance.RewardCalculation(rewardPanelTeam.transform.GetChild(0).GetComponent<TMP_Text>()));
				});
				break;
			case Gamemodes.FFA:
				gameEndArea.SetActive(true);

				gameEndText.text = victoriousChampion.championName + " wins!";
				winnerAvatar.sprite = victoriousChampion.avatar;
				rewardPanel.transform.localPosition = new Vector2(-1920, 0);

				while (musicSource.volume > 0.5f * cachedVolume) {
					musicSource.volume -= 0.5f * cachedVolume / 180;
					yield return null;
				}
				yield return new WaitForSeconds(3f);

				LeanTween.move(winnerAvatar.GetComponent<RectTransform>(), new Vector2(1200, 0), 0.5f).setEaseInOutQuad();
				LeanTween.move(rewardPanel.GetComponent<RectTransform>(), Vector2.zero, 0.5f).setEaseInOutQuad().setOnComplete(() => {
					StartCoroutine(StatisticManager.instance.RewardCalculation(rewardPanel.transform.GetChild(0).GetComponent<TMP_Text>()));
				});
				break;
		}
	}
	public override IEnumerator GameEndCheck() {
		List<ChampionController> aliveChampions = new List<ChampionController>();
		switch (gamemodes) {
			case Gamemodes.Competitive2v2:
				List<string> aliveTeams = new List<string>();
				foreach (ChampionController champion in champions) {
					switch (champion.isDead) {
						case false:
							if (!aliveTeams.Contains(champion.team)) aliveTeams.Add(champion.team);
							aliveChampions.Add(champion);
							break;
					}
				}

				Debug.Log(aliveChampions.Count);
				Debug.Log(aliveTeams.Count);

				if (aliveTeams.Count == 1) {
					foreach (ChampionController champion in champions) {
						champion.championParticleController.OrangeGlow.Stop();
						champion.championParticleController.CyanGlow.Stop();
						champion.championParticleController.RedGlow.Stop();

						champion.championParticleController.OrangeGlow.Clear();
						champion.championParticleController.CyanGlow.Clear();
						champion.championParticleController.RedGlow.Clear();
					}
					yield return new WaitForSeconds(3f);
					GameEnd(aliveChampions[Random.Range(0, aliveChampions.Count)]);
				}
				break;
			case Gamemodes.FFA:
				foreach (ChampionController champion in champions) {
					if (champion.isDead || champion.currentOwner != null) continue;
					aliveChampions.Add(champion);
				}

				if (aliveChampions.Count == 1) {
					foreach (ChampionController champion in champions) {
						champion.championParticleController.OrangeGlow.Stop();
						champion.championParticleController.CyanGlow.Stop();
						champion.championParticleController.RedGlow.Stop();

						champion.championParticleController.OrangeGlow.Clear();
						champion.championParticleController.CyanGlow.Clear();
						champion.championParticleController.RedGlow.Clear();
					}
					yield return new WaitForSeconds(3f);
					GameEnd(aliveChampions[0]);
				}
				break;
		}
	}
}