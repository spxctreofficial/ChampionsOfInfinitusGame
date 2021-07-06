using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using TMPro;

public enum GamePhase { GameStart, BeginningPhase, ActionPhase, EndPhase, GameEnd }

public abstract class GameController : MonoBehaviour {
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
	public Hand playerHand;
	public List<ChampionSlot> slots = new List<ChampionSlot>();

	// Prefab References
	[Header("Prefab References")]
	public GameObject championTemplate;
	public GameObject abilityTemplate;
	public GameObject cardTemplate;
	public GameObject handPrefab;
	public GameObject dialogueSystemPrefab;
	public GameObject confirmDialogPrefab;
	public GameObject miniConfirmDialogPrefab;
	public GameObject notificationDialogPrefab;
	public GameObject championSlotPrefab;
	public GameObject championInfoPanelPrefab;

	// UI-Specific References
	[Header("UI References")]
	public TMP_Text phaseIndicator;
	// [Obsolete("PlayerActionTooltip is no longer used.")]
	public TMP_Text playerActionTooltip;
	public ConfirmButton confirmButton;
	public GambleButton gambleButton;
	public Button endTurnButton;

	// Champion Configuration Variables
	[Header("Game Settings")]
	public List<Champion> players;
	public List<ChampionController> champions = new List<ChampionController>();

	// Map Configuration Variables
	public Map currentMap;

	// Difficulty Configuration Variables
	public Difficulty difficulty;
	[HideInInspector]
	public bool hasChosenDifficulty;

	// In-game Variables
	public int roundsElapsed = 0;

	protected virtual void Awake() {
		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
		Debug.Log(instance);
	}
	private void Start() {
		StartCoroutine(GameStart(GamePrep()));
	}

	private IEnumerator GameStart(IEnumerator enumerator) {
		Debug.Log("lol one");
		yield return StartCoroutine(enumerator);
		Debug.Log("lol two");
		StartCoroutine(GameStart());
	}
	/// <summary>
	/// The GameStart function is called at the start of the game.
	/// This function sets up all of the game assets and functionality,
	/// as well as allowing the player to choose their champion and map.
	/// </summary>
	/// <returns></returns>
	protected virtual IEnumerator GameStart() {
		phaseIndicator.text = "Start of Game";

		// Game Preparation
		yield return StartCoroutine(GameSetup());

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
	protected virtual IEnumerator BeginningPhase(ChampionController champion) {
		gamePhase = GamePhase.BeginningPhase;

		// Updates Text
		phaseIndicator.text = "Beginning Phase - " + champion.championName;
		champion.championParticleController.PlayEffect(champion.championParticleController.CyanGlow);
		yield return StartCoroutine(champion.hand.Deal(2)); // Deals to the player

		// Beginning Phase Ability Check
		foreach (var selectedChampion in champions) {
			foreach (var ability in selectedChampion.abilities) {
				yield return StartCoroutine(ability.OnBeginningPhase());
			}
		}
		
		// Discard Area Prune
		if (discardArea.transform.childCount > 8) {
			for (int i = discardArea.transform.childCount; i > 8; i--) {
				Destroy(discardArea.transform.GetChild(0).gameObject);
			}
		}

		yield return new WaitForSeconds(2f);

		StartCoroutine(ActionPhase(champion));
	}

	/// <summary>
	/// The Action Phase coroutine.
	/// </summary>
	/// <param name="champion"></param>
	/// <returns></returns>
	protected virtual IEnumerator ActionPhase(ChampionController champion) {
		gamePhase = GamePhase.ActionPhase;

		// Updates Text
		phaseIndicator.text = "Action Phase - " + champion.championName;
		champion.ResetExhaustion();

		// Action Phase Ability Check
		foreach (var selectedChampion in champions) {
			foreach (var ability in selectedChampion.abilities) {
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
	public virtual void StartEndPhase(ChampionController champion) {
		if (champion != null) {
			StartCoroutine(EndPhase(champion));
			return;
		}

		Debug.LogWarning("No overload was specified! Searching manually for current turn's champion.");
		StartEndPhase();
	}
	/// <summary>
	/// Searches for the current turn champion, then handles the End Phase for that champion.
	/// </summary>
	public virtual void StartEndPhase() {
		ChampionController champion = null;
		foreach (var selectedChampion in champions) {
			if (!selectedChampion.isMyTurn) continue;

			champion = selectedChampion;
		}
		if (champion == null) {
			Debug.LogWarning("What the fuck?");
			return;
		}

		StartCoroutine(EndPhase(champion));
	}
	/// <summary>
	/// The End Phase coroutine.
	/// </summary>
	/// <param name="champion"></param>
	/// <returns></returns>
	protected virtual IEnumerator EndPhase(ChampionController champion) {
		gamePhase = GamePhase.EndPhase;
		endTurnButton.gameObject.SetActive(false);

		// Updates Text
		phaseIndicator.text = "End Phase - " + champion.championName;
		int childCount = champion.hand.GetCardCount();
		champion.discardAmount = childCount > 6 ? childCount - 6 : 0;

		// End Phase Ability Check
		foreach (var selectedChampion in champions) {
			foreach (var ability in selectedChampion.abilities) {
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
					for (var discarded = 0; discarded < champion.discardAmount; discarded++) {
						var discard = champion.hand.GetCard("Lowest");
						discard.advantageFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
						discard.advantageFeed.text = "DISCARDED";
						yield return StartCoroutine(champion.hand.Discard(discard));
					}
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
	protected virtual void GameEnd(ChampionController victoriousChampion) {
		gamePhase = GamePhase.GameEnd;



		Destroy(playerActionTooltip);
		foreach (var champion in champions) StatisticManager.instance.TrackRemainingStatistics(champion);
		CardLogicController.instance.StopAllCoroutines();
		TooltipSystem.instance.StopAllCoroutines();
		StatisticManager.instance.winState = victoriousChampion.isPlayer;

		StartCoroutine(GameEndAction(victoriousChampion));
	}
	protected virtual IEnumerator GameEndAction(ChampionController victoriousChampion) {
		yield break;
	}
	/// <summary>
	/// Prepares the game to the player's configuration.
	/// Listens and saves the player's configuration of the map, champion, difficulty at runtime, and then reports them back to GameStart.
	/// </summary>
	/// <returns></returns>
	protected virtual IEnumerator GamePrep() {
		ChampionSlot.CreateDefaultSlots();
		yield break;
	}
	protected virtual IEnumerator GameSetup() {
		gameArea.GetComponent<Image>().sprite = currentMap.mapBackground;
		gameArea.GetComponent<AudioSource>().clip = currentMap.themeSong;

		AudioController.instance.Play(gameArea.GetComponent<AudioSource>());

		foreach (var champion in players) {
			ChampionSlot slot = slots[players.IndexOf(champion)];

			var championController = Spawn(champion, slot, players.IndexOf(champion) == 0);
			championController.team = championController.championID + players.IndexOf(champion);
		}
		yield break;
	}
	/// <summary>
	/// Checks for if the game should end, based on what gamemode the game is currently playing on.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator GameEndCheck() {
		var aliveChampions = new List<ChampionController>();
		foreach (var champion in champions) {
			if (champion.isDead || champion.currentOwner != null) continue;
			aliveChampions.Add(champion);
		}

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
			GameEnd(aliveChampions[0]);
		}
		yield break;
	}
	/// <summary>
	/// Sets the champion of the next turn to the next champion in the list.
	/// If the index is out of range, it will catch an ArgumentOutOfRangeException and elapse a round, then reset back to the first in the index.
	/// </summary>
	/// <param name="currentTurnChampion"></param>
	public virtual void NextTurnCalculator(ChampionController currentTurnChampion) {
		ChampionController nextTurnChampion;
		try {
			nextTurnChampion = champions[champions.IndexOf(currentTurnChampion) + 1];
		}
		catch (ArgumentOutOfRangeException) {
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
		StartCoroutine(BeginningPhase(nextTurnChampion));
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
		StartCoroutine(BeginningPhase(nextTurnChampion));
	}
	/// <summary>
	/// Fades away scene and returns the game to the main menu.
	/// </summary>
	public void ReturnToMainMenu() {
		var gameAreaCanvasGroup = gameArea.AddComponent<CanvasGroup>();
		var gameEndAreaCanvasGroup = gameEndArea.AddComponent<CanvasGroup>();
		var gameEndAreaTeamCanvasGroup = gameEndAreaTeam.AddComponent<CanvasGroup>();

		LeanTween.alphaCanvas(gameAreaCanvasGroup, 0f, 1f);
		LeanTween.alphaCanvas(gameEndAreaCanvasGroup, 0f, 1f).setOnComplete(() => {
			Destroy(StatisticManager.instance);
			AudioController.instance.Stop(gameArea.GetComponent<AudioSource>().clip);
			SceneManager.LoadScene("MainMenu");
		});
		LeanTween.alphaCanvas(gameEndAreaTeamCanvasGroup, 0f, 1f);
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
		
		// Dependency Setup
		IEnumerator Setup() {
			yield return null;
			hand.SetOwner(championController);

			yield return StartCoroutine(championController.hand.Deal(4, false, true, false));
		}
		StatisticManager.instance.StartTrackingStatistics(championController);
		StartCoroutine(Setup());

		// Returning the Spawned Champion
		return championController;
	}
}