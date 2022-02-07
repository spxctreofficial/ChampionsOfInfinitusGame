using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public enum GamePhase { GameStart, BeginningPhase, ActionPhase, EndPhase, GameEnd }

public abstract class GameManager : MonoBehaviour {
	public static GameManager instance;

	public enum Difficulty { Noob, Novice, Warrior, Champion }
	public enum Gamemodes { Duel, Competitive2v2, FFA }

	public GamePhase gamePhase;

	[Header("Panels")]
	public GameObject gameArea;

	public GameEndPanel gameEndPanel;
	public GameObject discardArea;
	public Hand playerHand;
	public List<ChampionSlot> slots = new List<ChampionSlot>();

	[Header("UI References")]
	public TMP_Text phaseIndicator;
	public TMP_Text playerActionTooltip;
	public Button endTurnButton;

	[Header("Game Settings")]
	public List<Champion> players;
	public List<ChampionController> champions = new List<ChampionController>();

	public Map currentMap;

	public Difficulty difficulty;
	[HideInInspector]
	public bool hasChosenDifficulty;

	public int roundsElapsed;

	public int CurrentRoundStamina => Mathf.Min(8, roundsElapsed + 1);

	[HideInInspector]
	public bool currentlyHandlingCard;

	protected virtual void Awake() {
		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
	}
	protected virtual void Start() {
		StartCoroutine(GameStart(GamePrep()));
	}

	private IEnumerator GameStart(IEnumerator enumerator) {
		yield return StartCoroutine(enumerator);
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
		foreach (ChampionController championController in champions) {
			championController.currentStamina = CurrentRoundStamina;
			foreach (ChampionController selectedChampionController in champions) {
				if (selectedChampionController == championController || selectedChampionController.team != championController.team) continue;
				championController.teamMembers.Add(selectedChampionController);
			}
		}

		endTurnButton.transform.SetAsLastSibling();

		yield return new WaitForSeconds(2f);

		NextTurnCalculator();
	}

	/// <summary>
	/// The Beginning Phase coroutine.
	/// </summary>
	/// <param name="championController"></param>
	/// <returns></returns>
	protected virtual IEnumerator BeginningPhase(ChampionController championController) {
		gamePhase = GamePhase.BeginningPhase;

		phaseIndicator.text = "Beginning Phase - " + championController.champion.championName;
		championController.championParticleController.cyanGlow.SetActive(true);
		championController.currentStamina = CurrentRoundStamina;
		yield return StartCoroutine(championController.hand.Deal(2)); // Deals to the player

		foreach (ChampionController selectedChampionController in champions) {
			foreach (Ability ability in selectedChampionController.abilities) {
				yield return StartCoroutine(ability.OnBeginningPhase());
			}
		}
		
		if (discardArea.transform.childCount > 8) {
			for (int i = discardArea.transform.childCount; i > 8; i--) {
				Destroy(discardArea.transform.GetChild(0).gameObject);
			}
		}

		yield return new WaitForSeconds(2f);

		StartCoroutine(ActionPhase(championController));
	}

	/// <summary>
	/// The Action Phase coroutine.
	/// </summary>
	/// <param name="championController"></param>
	/// <returns></returns>
	protected virtual IEnumerator ActionPhase(ChampionController championController) {
		gamePhase = GamePhase.ActionPhase;

		// Updates Text
		phaseIndicator.text = "Action Phase - " + championController.champion.championName;

		// Action Phase Ability Check
		foreach (ChampionController selectedChampionController in champions) {
			foreach (Ability ability in selectedChampionController.abilities) {
				yield return StartCoroutine(ability.OnActionPhase());
			}
		}

		// Action Phase
		switch (championController.isPlayer) {
			case true:
				endTurnButton.gameObject.SetActive(true);
				AudioManager.instance.Play("playerturn");
				break;
			case false:
				yield return StartCoroutine(championController.CardLogic());
				break;
		}
	}
	/// <summary>
	/// Starts the End Phase for the champion defined.
	/// </summary>
	/// <param name="championController"></param>
	public virtual void StartEndPhase(ChampionController championController) {
		if (championController != null) {
			StartCoroutine(EndPhase(championController));
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
		foreach (ChampionController selectedChampion in champions) {
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
	/// <param name="championController"></param>
	/// <returns></returns>
	protected virtual IEnumerator EndPhase(ChampionController championController) {
		gamePhase = GamePhase.EndPhase;
		endTurnButton.gameObject.SetActive(false);

		// Updates Text
		phaseIndicator.text = "End Phase - " + championController.champion.championName;
		championController.discardAmount = Mathf.Max(championController.hand.GetCardCount() - 6, 0);

		// End Phase Ability Check
		foreach (ChampionController selectedChampionController in champions) {
			foreach (Ability ability in selectedChampionController.abilities) {
				yield return StartCoroutine(ability.OnEndPhase());
			}
		}

		yield return new WaitForSeconds(2f);

		if (championController.discardAmount > 0) {
			yield return StartCoroutine(DiscardManager.Create(championController).Initialize());
        }
		NextTurnCalculator(championController);
	}
	/// <summary>
	/// GameEnd coroutine.
	/// Called when GameEndCheck returns successfully, and handles the end of the game according to the gamemode.
	/// </summary>
	/// <param name="victoriousChampion"></param>
	/// <returns></returns>
	protected virtual void GameEnd(ChampionController victoriousChampion) {
		gamePhase = GamePhase.GameEnd;
		ChampionController player = null;
		foreach (ChampionController champion in champions) {
			if (!champion.isPlayer) continue;
			player = champion;
		}

		Destroy(playerActionTooltip);
		foreach (ChampionController championController in champions) {
			StatisticManager.instance.TrackRemainingStatistics(championController);
			championController.StopAllCoroutines();
		}
	
		TooltipSystem.instance.StopAllCoroutines();
		StatisticManager.instance.winState = victoriousChampion.isPlayer || victoriousChampion.teamMembers.Contains(player);

		StartCoroutine(GameEndAction(victoriousChampion));
	}
	protected abstract IEnumerator GameEndAction(ChampionController victoriousChampion);
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

		AudioManager.instance.Play(gameArea.GetComponent<AudioSource>());

		foreach (Champion champion in players) {
			ChampionSlot slot = slots[players.IndexOf(champion)];

			ChampionController championController = Spawn(champion, slot, players.IndexOf(champion) == 0);
			championController.team = championController.champion.championID + players.IndexOf(champion);
		}
		yield break;
	}
	/// <summary>
	/// Checks for if the game should end, based on what gamemode the game is currently playing on.
	/// </summary>
	/// <returns></returns>
	public virtual IEnumerator GameEndCheck() {
		List<ChampionController> aliveChampions = new List<ChampionController>();
		foreach (ChampionController champion in champions) {
			if (champion.isDead || champion.currentOwner != null) continue;
			aliveChampions.Add(champion);
		}

		if (aliveChampions.Count == 1) {
			foreach (ChampionController championController in champions) {
				championController.championParticleController.redGlow.SetActive(false);
				championController.championParticleController.orangeGlow.SetActive(false);
				championController.championParticleController.cyanGlow.SetActive(false);
			}
			yield return new WaitForSeconds(3f);
			GameEnd(aliveChampions[0]);
		}
	}
	/// <summary>
	/// Sets the champion of the next turn to the next champion in the list.
	/// If the index is out of range, it will catch an ArgumentOutOfRangeException and elapse a round, then reset back to the first in the index.
	/// </summary>
	/// <param name="currentTurnChampionController"></param>
	public virtual void NextTurnCalculator(ChampionController currentTurnChampionController) {
		ChampionController nextTurnChampion;
		try {
			nextTurnChampion = champions[champions.IndexOf(currentTurnChampionController) + 1];
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
		if (nextTurnChampion == currentTurnChampionController) {
			Debug.LogError("Something went horribly fucking wrong. Did it miscalculate the next champion or is everyone die!?");
			return;
		}

		currentTurnChampionController.isMyTurn = false;
		currentTurnChampionController.championParticleController.cyanGlow.SetActive(false);
		nextTurnChampion.isMyTurn = true;
		StartCoroutine(BeginningPhase(nextTurnChampion));
	}
	/// <summary>
	/// An overload of NextTurnCalculator.
	/// Sets the next turn to the first champion in the index.
	/// Should only be used at GameStart.
	/// </summary>
	protected void NextTurnCalculator() {
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
		LeanTween.alphaCanvas(gameArea.GetComponent<CanvasGroup>(), 0f, 1f);
		LeanTween.alphaCanvas(gameEndPanel.GetComponent<CanvasGroup>(), 0f, 1f).setEaseOutQuart().setOnComplete(() => {
			Destroy(StatisticManager.instance);
			AudioManager.instance.Stop(gameArea.GetComponent<AudioSource>().clip);
			SceneManager.LoadScene("MainMenu");
		});
	}

	// Spawn Methods
	/// <summary>
	/// Spawns and returns a new ChampionController.
	/// </summary>
	/// <param name="champion"></param>
	/// <param name="slot"></param>
	/// <param name="spawnAsPlayer"></param>
	public ChampionController Spawn(Champion champion, ChampionSlot slot = null, bool spawnAsPlayer = false, bool dealHand = true) {
		if (slot is null) slot = ChampionSlot.FindNextVacantSlot();

		ChampionController championController = Instantiate(PrefabManager.instance.championTemplate, Vector2.zero, Quaternion.identity).GetComponent<ChampionController>();
		championController.champion = champion;
		champions.Add(championController);
		slot.SetOccupant(championController);
		championController.transform.SetParent(gameArea.transform, false);

		if (spawnAsPlayer) championController.isPlayer = true;

		Hand hand = spawnAsPlayer ? playerHand : Instantiate(PrefabManager.instance.handPrefab, new Vector3(-3000, 3000), Quaternion.identity).GetComponent<Hand>();
		hand.transform.SetParent(gameArea.transform, false);
		
		IEnumerator Setup() {
			yield return null;
			hand.SetOwner(championController);

			if (dealHand) yield return StartCoroutine(championController.hand.Deal(4, championController.hand.deck, false, false, true, false));
		}
		StatisticManager.instance.StartTrackingStatistics(championController);
		StartCoroutine(Setup());

		// Returning the Spawned Champion
		return championController;
	}
}