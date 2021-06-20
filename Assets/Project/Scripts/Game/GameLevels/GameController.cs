using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using TMPro;

public enum GamePhase { GameStart, BeginningPhase, ActionPhase, EndPhase, GameEnd }

public class GameController : MonoBehaviour {
	// Singleton
	public static GameController instance;

	// GameController Enums
	public enum Difficulty { Noob, Novice, Warrior, Champion }
	public enum Gamemodes { Duel, Competitive2v2, FFA }

	// Current Game Phase
	public GamePhase gamePhase;

	// Card Index Reference
	public CardIndex cardIndex;

	// Panels References
	[Header("Panels")]
	public GameObject gameArea;
	public GameObject gameEndArea, gameEndAreaTeam;
	public GameObject discardArea;
	public GameObject gamemodeSelectionConfig;
	public GameObject mapSelectionConfig;
	public GameObject championSelectionConfig;
	public GameObject difficultySelectionConfig;
	public Hand playerHand;
	public List<ChampionSlot> slots = new List<ChampionSlot>();

	// Prefab References
	[Header("Prefab References")]
	public GameObject championTemplate;
	public GameObject abilityTemplate;
	public GameObject cardTemplate;
	public GameObject handPrefab;
	public GameObject abilityPanelPrefab;
	public GameObject confirmDialogPrefab;
	public GameObject miniConfirmDialogPrefab;
	public GameObject gamemodeSelectionButtonPrefab;
	public GameObject mapSelectionButtonPrefab;
	public GameObject championSelectionButtonPrefab;
	public GameObject difficultySelectionButtonPrefab;
	public GameObject championSlotPrefab;

	// UI-Specific References
	[Header("UI References")]
	public TMP_Text phaseIndicator;
	[Obsolete("PlayerActionTooltip is no longer used.")]
	public TMP_Text playerActionTooltip;
	public ConfirmButton confirmButton;
	public GambleButton gambleButton;
	public Button endTurnButton;

	// Champion Configuration Variables
	[Header("Game Settings")]
	[Range(2, 6)]
	public int players;
	[HideInInspector]
	public List<ChampionController> champions = new List<ChampionController>();
	[HideInInspector]
	public Champion playerChampion;

	// Map Configuration Variables
	[HideInInspector]
	public Map currentMap;

	// Difficulty Configuration Variables
	public Difficulty difficulty;
	[HideInInspector]
	public bool hasChosenDifficulty;

	// Gamemode Configuration Variables
	public Gamemodes gamemodes;
	[HideInInspector]
	public bool hasChosenGamemode;

	// In-game Variables
	public int roundsElapsed = 0;

	private IEnumerator currentPhaseRoutine;


	private void Awake() {
		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
	}
	private void Start() {
		StartCoroutine(GameStart());
	}

	/// <summary>
	/// The GameStart function is called at the start of the game.
	/// This function sets up all of the game assets and functionality,
	/// as well as allowing the player to choose their champion and map.
	/// </summary>
	/// <returns></returns>
	private IEnumerator GameStart() {
		phaseIndicator.text = "Start of Game";

		// Game Preparation
		yield return StartCoroutine(GamePrep());
		SetMap();

		// Setting Up the Game
		switch (gamemodes) {
			case Gamemodes.Competitive2v2:
				players = 4;
				Debug.LogWarning("Configured number of players will be replaced with: " + players);
				break;
			case Gamemodes.Duel:
				players = 2;
				Debug.LogWarning("Configured number of players will be replaced with: " + players);
				break;
			case Gamemodes.FFA:
				players = 3;
				if (difficulty == Difficulty.Champion) {
					players++;
					Debug.Log("Since the difficulty is " + Difficulty.Champion + ", there will be four bots.");
				}
				break;
		}
		for (var i = 0; i < players; i++) {
			Champion champion = null;
			ChampionSlot slot = null;

			// Champion Slot Setup
			switch (gamemodes) {
				case Gamemodes.FFA:
					switch (i) {
						case 0:
							slot = slots[i];
							break;
						default:
							slot = slots[i + 1];
							break;
					}
					break;
				case Gamemodes.Competitive2v2:
					slot = slots[i];
					break;
			}

			// Champion Setup
			if (i != 0) {
				List<Champion> otherChampions = new List<Champion>();
				foreach (var anotherChampion in champions) {
					otherChampions.Add(anotherChampion.champion);
				}

				switch (difficulty) {
					case Difficulty.Noob:
						foreach (var anotherChampion in DataManager.instance.championIndex.champions) {
							if (anotherChampion.championID != "Champion_RegimeSoldier" || anotherChampion.championID != "Champion_RegimeCaptain" || Random.Range(0f, 1f) < 0.5f && DataManager.instance.championIndex.champions.IndexOf(anotherChampion) == DataManager.instance.championIndex.champions.Count - 1) continue;
							champion = anotherChampion;
							break;
						}
						break;
					case Difficulty.Novice:
						int repeats = 0;
						while ((champion == playerChampion || otherChampions.Contains(champion) || champion == null || champion.shopCost > 2000) && repeats <= 6) {
							Debug.Log("novice boop");
							champion = DataManager.instance.championIndex.champions[Random.Range(0, DataManager.instance.championIndex.champions.Count)];
							repeats++;
						}

						if (repeats > 6) champion = DataManager.instance.championIndex.champions[Random.Range(0, 2)];
						break;
					case Difficulty.Warrior:
					case Difficulty.Champion:
						repeats = 0;
						while ((champion == playerChampion || otherChampions.Contains(champion) || champion == null) && repeats <= 8) {
							Debug.Log("high difficulty boop");
							champion = DataManager.instance.championIndex.champions[Random.Range(0, DataManager.instance.championIndex.champions.Count)];
							repeats++;
						}

						if (repeats > 8) champion = DataManager.instance.championIndex.champions[Random.Range(0, 2)];
						break;
				}
			}
			else {
				champion = playerChampion;
			}

			var championController = Spawn(champion, slot, i == 0);

			// Configuring & Setting Teams
			switch (gamemodes) {
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
				case Gamemodes.FFA:
					championController.team = championController.championID + i;
					break;
			}
		}
		AudioController.instance.Play(gameArea.GetComponent<AudioSource>());
		StatisticManager.instance.StartTrackingStatistics();

		// Adds the team members to each champion's list.
		foreach (var champion in champions) {
			foreach (var selectedChampion in champions) {
				if (selectedChampion == champion || selectedChampion.team != champion.team) continue;
				champion.teamMembers.Add(selectedChampion);
			}
		}

		confirmButton.transform.SetAsLastSibling();
		gambleButton.transform.SetAsLastSibling();
		endTurnButton.transform.SetAsLastSibling();

		yield return new WaitForSeconds(2f);

		NextTurnCalculator();
	}

	/// <summary>
	/// The Beginning Phase coroutine.
	/// </summary>
	/// <param name="champion"></param>
	/// <returns></returns>
	private IEnumerator BeginningPhase(ChampionController champion) {
		gamePhase = GamePhase.BeginningPhase;

		// Updates Text
		phaseIndicator.text = "Beginning Phase - " + champion.championName;
		champion.championParticleController.PlayEffect(champion.championParticleController.CyanGlow);
		yield return StartCoroutine(champion.hand.Deal(2)); // Deals to the player

		// Beginning Phase Ability Check
		foreach (var selectedChampion in champions) {
			foreach (Transform child in selectedChampion.abilityPanel.panel.transform) {
				var ability = child.GetComponent<AbilityController>();
				yield return StartCoroutine(ability.OnBeginningPhase());
			}
		}

		yield return new WaitForSeconds(2f);

		SetPhase(ActionPhase(champion));
	}

	/// <summary>
	/// The Action Phase coroutine.
	/// </summary>
	/// <param name="champion"></param>
	/// <returns></returns>
	private IEnumerator ActionPhase(ChampionController champion) {
		gamePhase = GamePhase.ActionPhase;

		// Updates Text
		phaseIndicator.text = "Action Phase - " + champion.championName;
		champion.ResetExhaustion();

		// Action Phase Ability Check
		foreach (var selectedChampion in champions) {
			foreach (Transform child in selectedChampion.abilityPanel.panel.transform) {
				var ability = child.GetComponent<AbilityController>();
				yield return StartCoroutine(ability.OnActionPhase());
			}
		}

		// Action Phase
		switch (champion.isPlayer) {
			case true:
				endTurnButton.gameObject.SetActive(true);
				AudioController.instance.Play("PlayerTurn");
				break;
			case false:
				StartCoroutine(CardLogicController.instance.BotCardLogic(champion));
				break;
		}
	}
	/// <summary>
	/// Starts the End Phase for the champion defined.
	/// </summary>
	/// <param name="champion"></param>
	public void StartEndPhase(ChampionController champion) {
		if (champion != null) {
			SetPhase(EndPhase(champion));
			return;
		}

		Debug.LogWarning("No overload was specified! Searching manually for current turn's champion.");
		StartEndPhase();
	}
	/// <summary>
	/// Searches for the current turn champion, then handles the End Phase for that champion.
	/// </summary>
	public void StartEndPhase() {
		ChampionController champion = null;
		foreach (var selectedChampion in champions) {
			if (!selectedChampion.isMyTurn) continue;

			champion = selectedChampion;
		}
		if (champion == null) {
			Debug.LogWarning("What the fuck?");
			return;
		}

		SetPhase(EndPhase(champion));
	}
	/// <summary>
	/// The End Phase coroutine.
	/// </summary>
	/// <param name="champion"></param>
	/// <returns></returns>
	private IEnumerator EndPhase(ChampionController champion) {
		gamePhase = GamePhase.EndPhase;
		endTurnButton.gameObject.SetActive(false);

		// Updates Text
		phaseIndicator.text = "End Phase - " + champion.championName;
		int childCount = champion.hand.GetCardCount();
		champion.discardAmount = childCount > 6 ? childCount - 6 : 0;

		// End Phase Ability Check
		foreach (var selectedChampion in champions) {
			foreach (Transform child in selectedChampion.abilityPanel.panel.transform) {
				var ability = child.GetComponent<AbilityController>();
				yield return StartCoroutine(ability.OnEndPhase());
			}
		}

		yield return new WaitForSeconds(2f);

		// Discard and Calculate Next Turn
		switch (champion.isPlayer) {
			case true:
				playerActionTooltip.text = champion.discardAmount != 0 ? "Please discard " + champion.discardAmount + "." : playerActionTooltip.text;

				yield return new WaitUntil(() => champion.discardAmount == 0);
				break;
			case false:
				if (champion.discardAmount != 0) {
					for (var discarded = 0; discarded < champion.discardAmount; discarded++) yield return StartCoroutine(champion.hand.Discard(champion.hand.GetCard("Lowest")));
					champion.discardAmount = 0;
				}
				break;
		}
		NextTurnCalculator(champion);
	}
	/// <summary>
	/// GameEnd coroutine.
	/// Called when GameEndCheck returns successfully, and handles the end of the game according to the gamemode.
	/// </summary>
	/// <param name="victoriousChampion"></param>
	/// <returns></returns>
	private IEnumerator GameEnd(ChampionController victoriousChampion) {
		gamePhase = GamePhase.GameEnd;

		TMP_Text gameEndText = gameEndArea.transform.GetChild(0).GetComponent<TMP_Text>();
		TMP_Text gameEndTextTeam = gameEndAreaTeam.transform.GetChild(0).GetComponent<TMP_Text>();
		Image winnerAvatar = gameEndArea.transform.GetChild(1).GetComponent<Image>();
		GameObject winnerAvatars = gameEndAreaTeam.transform.GetChild(1).gameObject;
		GameObject rewardPanel = gameEndArea.transform.GetChild(2).gameObject;
		GameObject rewardPanelTeam = gameEndAreaTeam.transform.GetChild(2).gameObject;
		AudioSource musicSource = AudioController.instance.GetAudioSource(gameArea.GetComponent<AudioSource>().clip);
		float cachedVolume = musicSource.volume;

		Destroy(playerActionTooltip);
		foreach (var champion in champions) StatisticManager.instance.TrackRemainingStatistics(champion);
		CardLogicController.instance.StopAllCoroutines();
		TooltipSystem.instance.StopAllCoroutines();
		StatisticManager.instance.winState = victoriousChampion.isPlayer;

		switch (gamemodes) {
			case Gamemodes.Competitive2v2:
				gameEndAreaTeam.SetActive(true);

				gameEndTextTeam.text = victoriousChampion.championName + "'s Team wins!";
				foreach (var champion in champions) {
					if (!champion.teamMembers.Contains(victoriousChampion) && champion != victoriousChampion) continue;
					var newWinnerAvatar = Instantiate(winnerAvatar, Vector2.zero, Quaternion.identity).GetComponent<Image>();
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
	/// <summary>
	/// Fades away scene and returns the game to the main menu.
	/// </summary>
	public void ReturnToMainMenu() {
		var gameAreaCanvasGroup = gameArea.AddComponent<CanvasGroup>();
		var gameEndAreaCanvasGroup = gameEndArea.AddComponent<CanvasGroup>();

		LeanTween.alphaCanvas(gameAreaCanvasGroup, 0f, 1f);
		LeanTween.alphaCanvas(gameEndAreaCanvasGroup, 0f, 1f).setOnComplete(() => {
			Destroy(StatisticManager.instance);
			AudioController.instance.Stop(gameArea.GetComponent<AudioSource>().clip);
			SceneManager.LoadScene("MainMenu");
		});
	}
	/// <summary>
	/// Sets the champion of the next turn to the next champion in the list.
	/// If the index is out of range, it will catch an ArgumentOutOfRangeException and elapse a round, then reset back to the first in the index.
	/// </summary>
	/// <param name="currentTurnChampion"></param>
	public void NextTurnCalculator(ChampionController currentTurnChampion) {
		ChampionController nextTurnChampion;
		try {
			nextTurnChampion = champions[champions.IndexOf(currentTurnChampion) + 1];
		}
		catch (System.ArgumentOutOfRangeException) {
			Debug.LogWarning("The index was out of range. Elapsing round and resetting.");
			nextTurnChampion = champions[0];
			roundsElapsed++;
		}
		while (nextTurnChampion.isDead) {
			try {
				nextTurnChampion = champions[champions.IndexOf(nextTurnChampion) + 1];
			}
			catch (ArgumentOutOfRangeException) {
				Debug.Log("The index was out of range. Elapsing round and resetting.");
				nextTurnChampion = champions[0];
				roundsElapsed++;
			}
		}
		if (nextTurnChampion == currentTurnChampion) {
			Debug.LogError("Something went horribly fucking wrong. Did it miscalculate the next champion or is everyone die!?");
			return;
		}

		currentTurnChampion.isMyTurn = false;
		currentTurnChampion.championParticleController.CyanGlow.Stop();
		currentTurnChampion.championParticleController.CyanGlow.Clear();
		nextTurnChampion.isMyTurn = true;
		SetPhase(BeginningPhase(nextTurnChampion));
	}
	private void NextTurnCalculator(string version) {
		ChampionController champion = null;
		switch (version) {
			case "Smart":
				foreach (var selectedChampion in champions) {
					if (!selectedChampion.isMyTurn) continue;

					champion = selectedChampion;
				}
				if (champion == null) {
					Debug.LogWarning("What the fuck?");
					break;
				}

				NextTurnCalculator(champion);

				break;
			default:
				Debug.LogWarning("No smart calculator type defined! This will soft-lock the program.");
				return;
		}
	}
	/// <summary>
	/// An overload of NextTurnCalculator.
	/// Sets the next turn to the first champion in the index.
	/// Should only be used at GameStart.
	/// </summary>
	private void NextTurnCalculator() {
		ChampionController nextTurnChampion;
		try {
			nextTurnChampion = champions[0];
		}
		catch (NullReferenceException) {
			Debug.LogError("The first champion was not found! An error most likely occured whilst preparing the game.");
			return;
		}
		nextTurnChampion.isMyTurn = true;
		SetPhase(BeginningPhase(nextTurnChampion));
	}
	/// <summary>
	/// Checks for if the game should end, based on what gamemode the game is currently playing on.
	/// </summary>
	/// <returns></returns>
	public IEnumerator GameEndCheck() {
		var aliveChampions = new List<ChampionController>();
		switch (gamemodes) {
			case Gamemodes.Competitive2v2:
				var aliveTeams = new List<string>();
				foreach (var champion in champions) {
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
					foreach (var champion in champions) {
						champion.championParticleController.OrangeGlow.Stop();
						champion.championParticleController.CyanGlow.Stop();
						champion.championParticleController.RedGlow.Stop();

						champion.championParticleController.OrangeGlow.Clear();
						champion.championParticleController.CyanGlow.Clear();
						champion.championParticleController.RedGlow.Clear();
					}
					yield return new WaitForSeconds(3f);
					StartCoroutine(GameEnd(aliveChampions[Random.Range(0, aliveChampions.Count)]));
				}
				break;
			case Gamemodes.FFA:
				foreach (var champion in champions) {
					if (champion.isDead || champion.currentOwner != null) continue;
					aliveChampions.Add(champion);
				}

				Debug.Log(aliveChampions.Count);

				if (aliveChampions.Count == 1) {
					foreach (var champion in champions) {
						champion.championParticleController.OrangeGlow.Stop();
						champion.championParticleController.CyanGlow.Stop();
						champion.championParticleController.RedGlow.Stop();

						champion.championParticleController.OrangeGlow.Clear();
						champion.championParticleController.CyanGlow.Clear();
						champion.championParticleController.RedGlow.Clear();
					}
					yield return new WaitForSeconds(3f);
					StartCoroutine(GameEnd(aliveChampions[0]));
				}
				break;
		}
		yield break;
	}

	/// <summary>
	/// Sets the current phase of the game.
	/// Used to move through different phases of the game.
	/// </summary>
	/// <param name="enumerator"></param>
	private void SetPhase(IEnumerator enumerator) {
		currentPhaseRoutine = enumerator;
		StartCoroutine(enumerator);
	}
	/// <summary>
	/// Sets the map to the map configured by the player at runtime.
	/// </summary>
	private void SetMap() {
		gameArea.GetComponent<Image>().sprite = currentMap.mapBackground;
		gameArea.GetComponent<AudioSource>().clip = currentMap.themeSong;
	}

	/// <summary>
	/// Prepares the game to the player's configuration.
	/// Listens and saves the player's configuration of the map, champion, difficulty at runtime, and then reports them back to GameStart.
	/// </summary>
	/// <returns></returns>
	private IEnumerator GamePrep() {
		// Resets variables to prevent memory leaks.
		hasChosenGamemode = false;
		playerChampion = null;
		currentMap = null;
		hasChosenDifficulty = false;

		// Map Selection Config
		gameArea.SetActive(false);
		mapSelectionConfig.SetActive(true);

		foreach (var map in DataManager.instance.mapIndex.maps) {
			var mapSelectionButton = Instantiate(mapSelectionButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<MapSelectionButton>();
			mapSelectionButton.mapComponent = map;
			mapSelectionButton.transform.SetParent(mapSelectionConfig.transform.GetChild(0), false);
		}
		yield return new WaitUntil(() => currentMap != null);

		// Champion Selection Config
		mapSelectionConfig.SetActive(false);
		championSelectionConfig.SetActive(true);

		foreach (var championSO in DataManager.instance.OwnedChampions) {
			var championSelectionButton = Instantiate(championSelectionButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<ChampionSelectionButton>();
			championSelectionButton.championComponent = championSO;
			championSelectionButton.transform.SetParent(championSelectionConfig.transform.GetChild(0), false);
		}
		yield return new WaitUntil(() => playerChampion != null);

		// Gamemode Selection Config
		championSelectionConfig.SetActive(false);
		gamemodeSelectionConfig.SetActive(true);

		foreach (var gamemode in (Gamemodes[])Enum.GetValues(typeof(Gamemodes))) {
			if (gamemode == Gamemodes.Duel) continue;
			var gamemodeSelectionButton = Instantiate(gamemodeSelectionButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<GamemodeSelectionButton>();
			gamemodeSelectionButton.gamemode = gamemode;
			gamemodeSelectionButton.transform.SetParent(gamemodeSelectionConfig.transform.GetChild(0), false);
		}
		yield return new WaitUntil(() => hasChosenGamemode);

		// Difficulty Selection Config
		gamemodeSelectionConfig.SetActive(false);
		difficultySelectionConfig.SetActive(true);

		foreach (var difficulty in (Difficulty[])Enum.GetValues(typeof(Difficulty))) {
			var difficultySelectionButton = Instantiate(difficultySelectionButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<DifficultySelectionButton>();
			difficultySelectionButton.difficulty = difficulty;
			difficultySelectionButton.transform.SetParent(difficultySelectionConfig.transform.GetChild(0), false);
		}
		difficultySelectionConfig.transform.GetChild(0).GetChild(3).SetAsFirstSibling();
		difficultySelectionConfig.transform.GetChild(0).GetChild(1).SetAsLastSibling();
		difficultySelectionConfig.transform.GetChild(0).GetChild(1).SetSiblingIndex(2);
		yield return new WaitUntil(() => hasChosenDifficulty);

		difficultySelectionConfig.SetActive(false);
		gameArea.SetActive(true);

		// Configuring Slots
		ChampionSlot.CreateDefaultSlots();
	}

	// Spawn Methods
	/// <summary>
	/// Spawns and returns a new ChampionController.
	/// </summary>
	/// <param name="champion"></param>
	/// <param name="slot"></param>
	/// <param name="spawnAsPlayer"></param>
	public ChampionController Spawn(Champion champion, ChampionSlot slot = null, bool spawnAsPlayer = false) {
		// Prerequisites
		if (slot == null) slot = ChampionSlot.FindNextVacantSlot();

		// Spawning
		var championController = Instantiate(championTemplate, Vector2.zero, Quaternion.identity).GetComponent<ChampionController>();
		championController.champion = champion;
		champions.Add(championController);
		slot.SetOccupant(championController);
		championController.transform.SetParent(gameArea.transform, false);

		// Champion Dependencies
		var hand = spawnAsPlayer ? playerHand : Instantiate(handPrefab, new Vector3(-3000, 3000), Quaternion.identity).GetComponent<Hand>();
		hand.transform.SetParent(gameArea.transform, false);

		var abilityPanel = Instantiate(abilityPanelPrefab, Vector2.zero, Quaternion.identity).GetComponent<AbilityPanel>();
		abilityPanel.transform.SetParent(gameArea.transform, false);

		// Dependency Setup
		IEnumerator Setup() {
			yield return null;
			hand.SetOwner(championController);
			abilityPanel.Setup(championController);

			yield return StartCoroutine(championController.hand.Deal(4, false, true, false));
		}
		StartCoroutine(Setup());

		// Returning the Spawned Champion
		return championController;
	}

	private void PruneDiscardArea() {
		Destroy(discardArea.transform.GetChild(0));
	}
}