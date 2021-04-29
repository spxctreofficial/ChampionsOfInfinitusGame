using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GamePhase { GameStart, BeginningPhase, ActionPhase, EndPhase, GameEnd }


public class GameController : MonoBehaviour
{
	public static GameController instance;
	public enum Difficulty { Novice, Warrior, Champion }

	public GamePhase gamePhase;
	public CardIndex cardIndex;

	public GameObject gameArea;
	public Hand playerHand;
	public GameObject discardArea;

	public GameObject temporaryPrefab;
	public GameObject handPrefab;

	public Text playerActionTooltip;
	public Button confirmButton;
	public Button endTurnButton;

	public List<ChampionController> champions = new List<ChampionController>();
	[Range(2, 4)]
	public int players;

	public Difficulty difficulty;
	public int roundsElapsed = 0;


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
		for (int i = 0; i < players; i++)
		{
			ChampionController championController;
			Hand hand;
			Vector2 championControllerVector2;
			Vector2 handVector2;
			switch (i)
			{
				case 1:
					championControllerVector2 = new Vector2(866, 408);
					handVector2 = new Vector2(0, 800);
					break;
				case 2:
					championControllerVector2 = new Vector2(-866, 408);
					handVector2 = new Vector2(0, 1030);
					break;
				case 3:
					championControllerVector2 = new Vector2(0, 408);
					handVector2 = new Vector2(0, -800);
					break;
				default:
					championControllerVector2 = new Vector2(-866, -178);
					handVector2 = new Vector2(0, 0);
					break;
			}
			championController = Instantiate(temporaryPrefab, championControllerVector2, Quaternion.identity).GetComponent<ChampionController>();
			hand = i == 0 ? playerHand : Instantiate(handPrefab, handVector2, Quaternion.identity).GetComponent<Hand>();
			champions.Add(championController);
			champions[i].transform.SetParent(gameArea.transform, false);
			champions[i].ChampionSetup();
			hand.transform.SetParent(gameArea.transform, false);
			hand.SetOwner(champions[i]);
			StartCoroutine(CardLogicController.instance.Deal(champions[i].hand));
		}
		playerActionTooltip.text = "Welcome to the Land of Heroes. Players: " + champions.Count;

		yield return new WaitForSeconds(2f);

		champions[0].isMyTurn = true; // temporary

		StartCoroutine(BeginningPhase());
	}
	IEnumerator BeginningPhase()
	{
		gamePhase = GamePhase.BeginningPhase;

		foreach (ChampionController champion in champions)
		{
			if (champion.isMyTurn == false) continue;

			playerActionTooltip.text = "The " + champion.name + "'s Turn: Beginning Phase";
			StartCoroutine(CardLogicController.instance.Deal(champion.hand, 2));
		}

		yield return new WaitForSeconds(2f);

		StartCoroutine(ActionPhase());
	}
	IEnumerator ActionPhase()
	{
		gamePhase = GamePhase.ActionPhase;

		foreach (ChampionController champion in champions)
		{
			if (champion.isMyTurn == false) continue;

			playerActionTooltip.text = "The " + champion.name + "'s Turn: Action Phase";
			champion.ResetExhaustion();

			if (champion.isPlayer)
			{
				endTurnButton.gameObject.SetActive(true);
				yield break;
			}
			else
			{
				StartCoroutine(CardLogicController.instance.BotCardLogic(champion));
			}
		}
	}
	public void StartEndPhase()
	{
		StartCoroutine(EndPhase());
	}
	IEnumerator EndPhase()
	{
		gamePhase = GamePhase.EndPhase;
		endTurnButton.gameObject.SetActive(false);

		foreach (ChampionController champion in champions)
		{
			if (!champion.isMyTurn) continue;

			playerActionTooltip.text = "The " + champion.name + "'s Turn: End Phase";
			champion.discardAmount = champion.hand.transform.childCount > 6 ? champion.hand.transform.childCount - 6 : 0;

			yield return new WaitForSeconds(2f);

			if (champion.isPlayer)
			{
				playerActionTooltip.text = champion.discardAmount != 0 ? "Please discard " + champion.discardAmount + "." : playerActionTooltip.text;

				yield return new WaitUntil(() => champion.discardAmount == 0);

				NextTurnCalculator(champion);
				break;
			}
			else
			{
				if (champion.discardAmount != 0)
				{
					for (int discarded = 0; discarded < champion.discardAmount; discarded++)
					{
						Card selectedCard = null;
						int value = 999;
						foreach (Transform child in champion.hand.transform)
						{
							if (child.GetComponent<Card>().cardValue < value)
							{
								selectedCard = child.GetComponent<Card>();
								value = selectedCard.cardValue;
							}
						}
						CardLogicController.instance.Discard(selectedCard);
						champion.discardAmount--;
					}
					champion.discardAmount = 0;
				}

				NextTurnCalculator(champion);
				break;
			}
		}
	}
	public void NextTurnCalculator(ChampionController currentTurnChampion)
	{
		int nextIndex = champions.IndexOf(currentTurnChampion) + 1;

		currentTurnChampion.isMyTurn = false;
		try
		{
			champions[nextIndex].isMyTurn = true;
		}
		catch (System.ArgumentOutOfRangeException)
		{
			Debug.Log("The index was out of range. Elapsing round.");
			champions[0].isMyTurn = true;
			roundsElapsed++;
		}
		StartCoroutine(BeginningPhase());
	}

	public void OnConfirmButtonClick()
	{
		confirmButton.gameObject.SetActive(false);

		foreach (ChampionController champion in champions)
		{
			if (!champion.isPlayer) continue;

			switch (gamePhase)
			{
				case GamePhase.ActionPhase:
					if (champion.isAttacking && champion.attackingCard != null && champion.currentTarget != null)
					{
						StartCoroutine(CardLogicController.instance.CombatCalc(champion, champion.currentTarget));
						champion.currentTarget.currentlyTargeted = true;
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
