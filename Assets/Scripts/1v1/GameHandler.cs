using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum GamePhase { GAMESTART, PLAYERBEGINNINGPHASE, PLAYERACTIONPHASE, PLAYERENDPHASE, OPPONENTBEGINNINGPHASE, OPPONENTACTIONPHASE, OPPONENTENDPHASE, GAMEWIN, GAMELOSE }

public class GameHandler : MonoBehaviour
{
    #region Variables
    public GamePhase phase;
    public CardIndex cardIndex;
    public CardLogicHandler cardLogicHandler;
    public ChampionIndex championIndex;

    public GameObject startCanvas;
    public GameObject gameCanvas;
    public GameObject firstTurnCanvas;
    public GameObject championDashboard;

    public GameObject playerPrefab;
    public GameObject opponentPrefab;
    public GameObject healthDisplayTextPrefab;
    public GameObject playerAbilityStatusPrefab;
    public GameObject playerArea;
    public GameObject opponentArea;
    public GameObject playArea;

    public Text playerActionTooltip;
    public GameObject endTurnButton;
    public GameObject gambleButton;
    public GameObject skipButton;

    [HideInInspector]
    public ChampionHandler player;
    [HideInInspector]
    public ChampionHandler opponent;
    Text playerHealthText;
    Text opponentHealthText;
    [HideInInspector]
    public Text playerAbilityStatus1;
    [HideInInspector]
    public Text playerAbilityStatus2;
    [HideInInspector]
    public Text playerAbilityStatus3;
    [HideInInspector]
    public Text playerAbilityStatus4;
    #endregion

    #region Default Functions
    private void Start()
    {
        cardIndex.PopulatePlayingCardsList();
        GameStart();
    }
    private void Update()
    {
        if (phase != GamePhase.GAMESTART)
        {
            player.cards = playerArea.transform.childCount;
            opponent.cards = opponentArea.transform.childCount;

            PlayerAbilityCheck();

            playerHealthText.text = player.currentHP.ToString();
            opponentHealthText.text = opponent.currentHP.ToString();
        }
        if (phase == GamePhase.PLAYERENDPHASE && player.discardAmount == 0)
        {
            StartOpponentTurn();
        }
    }
    #endregion

    #region GamePhase Functions
    public void GameStart()
    {
        phase = GamePhase.GAMESTART;
        gameCanvas.SetActive(false);
        startCanvas.SetActive(true);
    }
    public void GamePlayerChooseTurn()
    {
        startCanvas.SetActive(false);
        firstTurnCanvas.SetActive(true);
    }
    public void GameSetup()
    {
        firstTurnCanvas.SetActive(false);
        gameCanvas.SetActive(true);
        DealCards(4);

        GameObject playerGO = Instantiate(playerPrefab, new Vector2(-866, -139), Quaternion.identity);
        GameObject playerHealthTextGO = Instantiate(healthDisplayTextPrefab, new Vector2(-866, 29), Quaternion.identity);
        player = playerGO.GetComponent<ChampionHandler>();
        playerGO.transform.SetParent(gameCanvas.transform, false);
        playerHealthTextGO.transform.SetParent(gameCanvas.transform, false);
        player.ChampionSetup();
        playerHealthText = playerHealthTextGO.GetComponent<Text>();
        Debug.Log("Player: " + player.currentHP);
        Debug.Log("Cards: " + playerArea.transform.childCount);

		switch (player.championName)
        {
			case "The Wraith King":
                foreach (Transform child in playerArea.transform)
				{
                    GameObject selectedCard = child.gameObject;
                    Card selectedCardComponent = selectedCard.GetComponent<Card>();
                    if (selectedCardComponent.cardType == CardType.CLUB)
                    {
                        player.clubs++;
                    }
                }
                Debug.Log(player.clubs);

                GameObject DeathCrownAbilityStatusGO = Instantiate(playerAbilityStatusPrefab, new Vector2(-547, -150), Quaternion.identity);
                GameObject DeathMistAbilityStatusGO = Instantiate(playerAbilityStatusPrefab, new Vector2(-547, -110), Quaternion.identity);
                GameObject playerAbilityStatusGO3 = Instantiate(playerAbilityStatusPrefab, new Vector2(-547, -70), Quaternion.identity);
				GameObject playerAbilityStatusGO4 = Instantiate(playerAbilityStatusPrefab, new Vector2(-547, -30), Quaternion.identity);
                DeathCrownAbilityStatusGO.transform.SetParent(gameCanvas.transform, false);
                DeathMistAbilityStatusGO.transform.SetParent(gameCanvas.transform, false);
                playerAbilityStatusGO3.transform.SetParent(gameCanvas.transform, false);
                playerAbilityStatusGO4.transform.SetParent(gameCanvas.transform, false);
                playerAbilityStatus1 = DeathCrownAbilityStatusGO.GetComponent<Text>();
                playerAbilityStatus2 = DeathMistAbilityStatusGO.GetComponent<Text>();
                playerAbilityStatus3 = playerAbilityStatusGO3.GetComponent<Text>();
                playerAbilityStatus4 = playerAbilityStatusGO4.GetComponent<Text>();
                break;
        }


        GameObject opponentGO = Instantiate(opponentPrefab, new Vector2(866, 139), Quaternion.identity);
        GameObject opponentHealthTextGO = Instantiate(healthDisplayTextPrefab, new Vector2(866, -29), Quaternion.identity);
        opponent = opponentGO.GetComponent<ChampionHandler>();
        opponentGO.transform.SetParent(gameCanvas.transform, false);
        opponentHealthTextGO.transform.SetParent(gameCanvas.transform, false);
        opponent.ChampionSetup();
        opponentHealthText = opponentHealthTextGO.GetComponent<Text>();
        Debug.Log("Opponent: " + opponent.currentHP);
        Debug.Log("Cards: " + opponentArea.transform.childCount);

        StartCoroutine(PlayerTurn());
        playerActionTooltip.text = "You are " + player.championName + ".";
        playerHealthText.text = player.currentHP.ToString();
        opponentHealthText.text = opponent.currentHP.ToString();
    }
    IEnumerator PlayerTurn()
    {
        yield return new WaitForSeconds(2f);

        phase = GamePhase.PLAYERBEGINNINGPHASE;
        playerActionTooltip.text = "It is your Beginning Phase.";
        DealCardsPlayer(2);

        yield return new WaitForSeconds(1f);

        phase = GamePhase.PLAYERACTIONPHASE;
        player.spadesBeforeExhaustion = 1;
        player.heartsBeforeExhaustion = 3;
        player.diamondsBeforeExhaustion = 1;
        endTurnButton.SetActive(true);
        playerActionTooltip.text = "It is your Action Phase.";
    }
    void EndPlayerTurn()
    {
        phase = GamePhase.PLAYERENDPHASE;
        playerActionTooltip.text = "It is your End Phase.";
        if (player.cards > 6)
        {
            player.discardAmount = player.cards - 6;
            playerActionTooltip.text = "Please discard " + player.discardAmount + ".";
        }
        else
        {
            player.discardAmount = 0;
        }
    }
    void StartOpponentTurn()
    {
        StartCoroutine(OpponentTurn());
        phase = GamePhase.OPPONENTBEGINNINGPHASE;
    }
    IEnumerator OpponentTurn()
    {
        yield return new WaitForSeconds(2f);

        playerActionTooltip.text = "It is the opponent's Beginning Phase.";

        yield return new WaitForSeconds(1f);
        DealCardsOpponent(2);
        yield return new WaitForSeconds(2f);

        phase = GamePhase.OPPONENTACTIONPHASE;
        opponent.spadesBeforeExhaustion = 1;
        opponent.heartsBeforeExhaustion = 3;
        opponent.diamondsBeforeExhaustion = 1;
        playerActionTooltip.text = "It is the opponent's Action Phase.";


        StartCoroutine(cardLogicHandler.OpponentCardLogic());
    }
    public void EndOpponentTurn()
    {
        phase = GamePhase.OPPONENTENDPHASE;
        playerActionTooltip.text = "It is the opponent's End Phase.";
        if (opponent.cards > 6)
        {
            opponent.discardAmount = opponent.cards - 6;

            for (int i = 0; i < opponent.discardAmount; i++)
            {
                int value = 999;
                GameObject chosenCard = opponentArea.transform.GetChild(0).gameObject;
                foreach (Transform child in opponentArea.transform)
                {
                    GameObject selectedCard = child.gameObject;
                    Card selectedCardComponent = selectedCard.GetComponent<Card>();
                    if (value > selectedCardComponent.cardValue)
                    {
                        value = selectedCardComponent.cardValue;
                        chosenCard = selectedCard;
                    }
                }
                cardLogicHandler.Discard(chosenCard, true);
            }
            opponent.discardAmount = 0;
        }

        StartCoroutine(PlayerTurn());
    }
    public IEnumerator GameEnd()
	{
        switch (phase)
		{
            case GamePhase.GAMEWIN:
				playerActionTooltip.text = "Congratulations! You win.";

                yield return new WaitForSeconds(3f);
                SceneManager.LoadScene("WinScene");
                break;
            case GamePhase.GAMELOSE:
                playerActionTooltip.text = "You lost!";

                yield return new WaitForSeconds(3f);
                SceneManager.LoadScene("LoseScene");
                break;
		}
	}
    #endregion


    #region Other Callable Functions
    [HideInInspector]
    private void PlayerAbilityCheck()
	{
        switch (player.championName)
        {
            case "The Wraith King":
                if (!player.isDeathMistReady)
                {
                    int count = 0;
                    foreach (Transform child in playerArea.transform)
                    {
                        GameObject selectedCard = child.gameObject;
                        Card selectedCardComponent = selectedCard.GetComponent<Card>();
                        if (selectedCardComponent.cardType == CardType.CLUB)
                        {
                            count++;
                        }
                    }
                    player.clubs = count;
                }

                if (player.clubs >= 3)
                {
                    player.isDeathCrownReady = true;

                    if (playerAbilityStatus1.text != "Death Crown - UP")
					{
                        playerAbilityStatus1.text = "Death Crown - UP";
					}
                }
                else
                {
                    player.isDeathCrownReady = false;
                    playerAbilityStatus1.text = "";
                }
                break;
        }
    }
    [HideInInspector]
    public void OnEndTurnButtonClick()
    {
        endTurnButton.SetActive(false);
        EndPlayerTurn();
    }
    [HideInInspector]
    public void OnGambleButtonClick()
	{
        gambleButton.SetActive(false);
		switch (phase)
		{
            case GamePhase.PLAYERACTIONPHASE:
                GameObject attackingCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
                cardLogicHandler.Discard(attackingCard);
                StartCoroutine(cardLogicHandler.AttackCalc(attackingCard));
                break;
            case GamePhase.OPPONENTACTIONPHASE:
                if (player.isAttacked != true || opponent.isAttacking != true) return;

                attackingCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
                cardLogicHandler.Discard(attackingCard);
                cardLogicHandler.DefenseCalc(attackingCard);
                break;
		}

	}
    [HideInInspector]
    public void OnSkipButtonClick()
	{
        skipButton.SetActive(false);
        switch (phase)
		{
            case GamePhase.PLAYERACTIONPHASE:
                switch (player.championName)
                {
                    case "The Wraith King":
                        playerActionTooltip.text = "It is the player's Action Phase.";
                        skipButton.transform.GetChild(0).GetComponent<Text>().text = "Skip";
                        player.UndeadTurning(this, opponent);
                        break;
                }
                break;
            case GamePhase.OPPONENTACTIONPHASE:
                if (player.discardAmount >= 2)
				{
                    if (player.discardAmount >= 4)
					{
                        player.Damage(40, DamageType.Fire, opponent);
					}
                    else
					{
                        player.Damage(20, DamageType.Unblockable, opponent);
                    }
                    player.discardAmount = 0;
                    playerActionTooltip.text = "It is the opponent's Action Phase.";
                    StartCoroutine(cardLogicHandler.OpponentCardLogic());
                    break;
                }

                switch (player.championName)
				{
                    case "The Wraith King":
                        playerActionTooltip.text = "It is the opponent's Action Phase.";
                        StartCoroutine(cardLogicHandler.OpponentCardLogic());
                        break;
				}
                break;
		}
	}
    [HideInInspector]
    public void EnlargeChampionDashboard(Sprite sprite)
    {
        championDashboard.SetActive(true);
        gameCanvas.SetActive(false);
        Image championImage = championDashboard.transform.GetChild(1).gameObject.GetComponent<Image>();
        championImage.sprite = sprite;
    }
    [HideInInspector]
    public void CloseChampionDashboard()
    {
        championDashboard.SetActive(false);
        gameCanvas.SetActive(true);
    }
    public void DealCardsPlayer(int cards)
    {
        for (int x = 0; x < cards; x++)
        {
            GameObject playerCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
            playerCard.transform.SetParent(playerArea.transform, false);
        }
    }
    public void DealCardsOpponent(int cards)
    {
        for (int x = 0; x < cards; x++)
        {
            GameObject opponentCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
            opponentCard.transform.SetParent(opponentArea.transform, false);
            opponentCard.GetComponent<Card>().ToggleCardVisibility();
        }
    }
    public void DealCards(int cards)
    {
        DealCardsPlayer(cards);
        DealCardsOpponent(cards);
    }
    #endregion
}