using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public enum GamePhase { GameStart, BeginningPhase, ActionPhase, EndPhase, GameEnd }


public class GameController : MonoBehaviour
{
	public static GameController instance;
	public enum Difficulty { Novice, Warrior, Champion }
	public enum Gamemodes { Duel, Competitive2v2, FFA }

	public GamePhase gamePhase;
	public CardIndex cardIndex;

	public GameObject gameArea;
	public GameObject mapSelectionConfig;
	public GameObject championSelectionConfig;
	public Hand playerHand;
	public GameObject discardArea;

	public GameObject championTemplate;
	public GameObject abilityPrefab;
	public GameObject handPrefab;
	public GameObject abilityPanelPrefab;
	public GameObject mapSelectionButtonPrefab;
	public GameObject championSelectionButtonPrefab;
	
	public TMP_Text playerActionTooltip;
	public Button confirmButton;
	public Button endTurnButton;

	[HideInInspector]
	public List<ChampionController> champions = new List<ChampionController>();
	public List<Champion> championIndex = new List<Champion>();
	public Champion playerChampion;
	[Range(2, 4)]
	public int players;
	public List<Map> mapIndex = new List<Map>();
	public Map currentMap;

	public Difficulty difficulty;
	public Gamemodes gamemodes;
	public int roundsElapsed = 0;

	[HideInInspector]
	public IEnumerator currentPhaseRoutine;


	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}
	}

	private void Start()
	{
		cardIndex.PopulatePlayingCardsList();
		StartCoroutine(GameStart());
	}

	private void Update()
	{
		if (discardArea.transform.childCount > 7) Destroy(discardArea.transform.GetChild(0).gameObject);


	}

	IEnumerator GameStart()
	{
		yield return StartCoroutine(GamePrep());
		SetMap();

		switch (gamemodes)
		{
			case Gamemodes.Competitive2v2:
				players = 4;
				Debug.LogWarning("Configured number of players will be replaced with: " + players);
				break;
			case Gamemodes.Duel:
				players = 2;
				Debug.LogWarning("Configured number of players will be replaced with: " + players);
				break;
			default:
				break;
		}
		for (int i = 0; i < players; i++)
		{
			ChampionController championController;
			Champion champion;
			Hand hand;
			AbilityPanel abilityPanel;
			Vector2 championControllerVector2;
			Vector2 handVector2;
			switch (i)
			{
				case 1:
					champion = championIndex[Random.Range(0, championIndex.Count)];
					championControllerVector2 = new Vector2(864, (float) 412.5);
					handVector2 = new Vector2(0, 800);
					break;
				case 2:
					champion = championIndex[Random.Range(0, championIndex.Count)];
					championControllerVector2 = new Vector2(-864, (float) 412.5);
					handVector2 = new Vector2(0, 1030);
					break;
				case 3:
					champion = championIndex[Random.Range(0, championIndex.Count)];
					championControllerVector2 = gamemodes == Gamemodes.Competitive2v2 ? new Vector2(864, (float) -213.25): new Vector2(0, 408);
					handVector2 = new Vector2(0, -800);
					break;
				default:
					champion = playerChampion;
					championControllerVector2 = new Vector2(-864, (float) -213.25);
					handVector2 = new Vector2(0, 0);
					break;
			}
			championController = Instantiate(championTemplate, championControllerVector2, Quaternion.identity).GetComponent<ChampionController>();
			championController.champion = champion;
			champions.Add(championController);
			champions[i].transform.SetParent(gameArea.transform, false);

			hand = i == 0 ? playerHand : Instantiate(handPrefab, handVector2, Quaternion.identity).GetComponent<Hand>();
			hand.transform.SetParent(gameArea.transform, false);
			hand.SetOwner(champions[i]);

			abilityPanel = Instantiate(abilityPanelPrefab, new Vector2(0, 0), Quaternion.identity).GetComponent<AbilityPanel>();
			abilityPanel.transform.SetParent(gameArea.transform, false);
			yield return null;
			abilityPanel.Setup(championController);

			switch (gamemodes)
			{
				case Gamemodes.Competitive2v2:
					if (i == 3 || champions[i].isPlayer)
					{
						champions[i].team = "PlayerTeam";
					}
					else
					{
						champions[i].team = "OpponentTeam";
					}
					break;
				case Gamemodes.Duel:
					champions[0].team = "PlayerTeam";
					champions[1].team = "OpponentTeam";
					break;
				case Gamemodes.FFA:
					champions[i].team = champions[i].name + i;
					break;
			}

			champions[i].hand.Deal(4, false, true, false);
		}
		playerActionTooltip.text = "Welcome to the Land of Heroes. Players: " + champions.Count;

		yield return new WaitForSeconds(2f);

		NextTurnCalculator();
	}
	IEnumerator BeginningPhase(ChampionController champion)
	{
		gamePhase = GamePhase.BeginningPhase;

		playerActionTooltip.text = "The " + champion.name + "'s Turn: Beginning Phase";
		champion.hand.Deal(2);

		foreach (ChampionController selectedChampion in champions)
		{
			foreach (Transform child in selectedChampion.abilityPanel.panel.transform)
			{
				AbilityController ability = child.GetComponent<AbilityController>();
				yield return StartCoroutine(ability.OnBeginningPhase());
			}
		}

		yield return new WaitForSeconds(2f);

		SetPhase(ActionPhase(champion));
	}
	IEnumerator ActionPhase(ChampionController champion)
	{
		gamePhase = GamePhase.ActionPhase;

		playerActionTooltip.text = "The " + champion.name + "'s Turn: Action Phase";
		champion.ResetExhaustion();

		foreach (ChampionController selectedChampion in champions)
		{
			foreach (Transform child in selectedChampion.abilityPanel.panel.transform)
			{
				AbilityController ability = child.GetComponent<AbilityController>();
				yield return StartCoroutine(ability.OnActionPhase());
			}
		}

		if (champion.isPlayer)
		{
			endTurnButton.gameObject.SetActive(true);
			AudioController.instance.Play("PlayerTurn");
			yield break;
		}
		else
		{
			StartCoroutine(CardLogicController.instance.BotCardLogic(champion));
		}
	}
	public void StartEndPhase(ChampionController champion)
	{
		if (champion != null)
		{
			SetPhase(EndPhase(champion));
			return;
		}

		Debug.LogWarning("No overload was specified! Searching manually for current turn's champion.");
		StartEndPhase();
	}
	public void StartEndPhase()
	{
		ChampionController champion = null;
		foreach (ChampionController selectedChampion in champions)
		{
			if (!selectedChampion.isMyTurn) continue;

			champion = selectedChampion;
		}
		if (champion == null)
		{
			Debug.LogWarning("What the fuck?");
			return;
		}

		SetPhase(EndPhase(champion));
	}
	IEnumerator EndPhase(ChampionController champion)
	{
		gamePhase = GamePhase.EndPhase;
		endTurnButton.gameObject.SetActive(false);

		playerActionTooltip.text = "The " + champion.name + "'s Turn: End Phase";
		champion.discardAmount = champion.hand.transform.childCount > 6 ? champion.hand.transform.childCount - 6 : 0;

		foreach (ChampionController selectedChampion in champions)
		{
			foreach (Transform child in selectedChampion.abilityPanel.panel.transform)
			{
				AbilityController ability = child.GetComponent<AbilityController>();
				yield return StartCoroutine(ability.OnEndPhase());
			}
		}

		yield return new WaitForSeconds(2f);

		if (champion.isPlayer)
		{
			playerActionTooltip.text = champion.discardAmount != 0 ? "Please discard " + champion.discardAmount + "." : playerActionTooltip.text;

			yield return new WaitUntil(() => champion.discardAmount == 0);

			NextTurnCalculator(champion);
			yield break;
		}
		else
		{
			if (champion.discardAmount != 0)
			{
				for (int discarded = 0; discarded < champion.discardAmount; discarded++) CardLogicController.instance.Discard(champion.hand.GetCard("Lowest"));
				champion.discardAmount = 0;
			}

			NextTurnCalculator(champion);
			yield break;
		}
	}
	public void NextTurnCalculator(ChampionController currentTurnChampion)
	{
		ChampionController nextTurnChampion;
		try
		{
			nextTurnChampion = champions[champions.IndexOf(currentTurnChampion) + 1];
		}
		catch (System.ArgumentOutOfRangeException)
		{
			Debug.LogWarning("The index was out of range. Elapsing round and resetting.");
			nextTurnChampion = champions[0];
			roundsElapsed++;
		}
		while (nextTurnChampion.isDead)
		{
			try
			{
				nextTurnChampion = champions[champions.IndexOf(nextTurnChampion) + 1];
			}
			catch (System.ArgumentOutOfRangeException)
			{
				Debug.Log("The index was out of range. Elapsing round and resetting.");
				nextTurnChampion = champions[0];
				roundsElapsed++;
			}
		}
		if (nextTurnChampion == currentTurnChampion)
		{
			Debug.LogError("Something went horribly fucking wrong. Did it miscalculate the next champion or is everyone die!?");
			return;
		}

		currentTurnChampion.isMyTurn = false;
		nextTurnChampion.isMyTurn = true;
		SetPhase(BeginningPhase(nextTurnChampion));
	}
	private void NextTurnCalculator(string version)
	{
		ChampionController champion = null;
		switch (version)
		{
			case "Smart":
				foreach (ChampionController selectedChampion in champions)
				{
					if (!selectedChampion.isMyTurn) continue;

					champion = selectedChampion;
				}
				if (champion == null)
				{
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
	private void NextTurnCalculator()
	{
		ChampionController nextTurnChampion;
		try
		{
			nextTurnChampion = champions[0];
		}
		catch (System.NullReferenceException)
		{
			Debug.LogError("The first champion was not found! An error most likely occured whilst preparing the game.");
			return;
		}
		nextTurnChampion.isMyTurn = true;
		SetPhase(BeginningPhase(nextTurnChampion));
	}

	private void SetPhase(IEnumerator enumerator)
	{
		currentPhaseRoutine = enumerator;
		StartCoroutine(enumerator);
	}
	private void SetMap()
	{
		gameArea.GetComponent<Image>().sprite = currentMap.mapBackground;
		gameArea.GetComponent<AudioSource>().clip = currentMap.themeSong;
		gameArea.GetComponent<AudioSource>().Play();
	}

	public IEnumerator GamePrep()
	{
		playerChampion = null;
		currentMap = null;

		gameArea.SetActive(false);
		mapSelectionConfig.SetActive(true);

		foreach (Map map in mapIndex)
		{
			MapSelectionButton mapSelectionButton = Instantiate(mapSelectionButtonPrefab, new Vector2(0, 0), Quaternion.identity).GetComponent<MapSelectionButton>();
			mapSelectionButton.mapComponent = map;
			mapSelectionButton.transform.SetParent(mapSelectionConfig.transform.GetChild(0), false);
		}
		yield return new WaitUntil(() => currentMap != null);

		mapSelectionConfig.SetActive(false);
		championSelectionConfig.SetActive(true);

		foreach (Champion championSO in championIndex)
		{
			ChampionSelectionButton championSelectionButton = Instantiate(championSelectionButtonPrefab, new Vector2(0, 0), Quaternion.identity).GetComponent<ChampionSelectionButton>();
			championSelectionButton.championComponent = championSO;
			championSelectionButton.transform.SetParent(championSelectionConfig.transform.GetChild(0), false);
		}
		yield return new WaitUntil(() => playerChampion != null);

		championSelectionConfig.SetActive(false);
		gameArea.SetActive(true);
	}

	public void OnConfirmButtonClick()
	{
		confirmButton.gameObject.SetActive(false);

		foreach (ChampionController champion in champions)
		{
			if (!champion.isPlayer || champion.isDead) continue;

			switch (gamePhase)
			{
				case GamePhase.ActionPhase:
					if (champion.isAttacking && champion.attackingCard != null && champion.currentTarget != null)
					{
						StartCoroutine(CardLogicController.instance.CombatCalculation(champion, champion.currentTarget));
						return;
					}
					if (champion.currentlyTargeted && champion.defendingCard != null && !champion.isMyTurn)
					{
						champion.hasDefended = true;
						return;
					}
					if (champion.discardAmount != 0 && !champion.isMyTurn)
					{
						champion.discardAmount = -1;
						return;
					}
					break;
			}
		}
	}

	private void PruneDiscardArea()
	{
		Destroy(discardArea.transform.GetChild(0));
	}
}
