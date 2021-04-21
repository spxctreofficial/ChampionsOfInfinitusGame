using System.Collections;
using System.Collections.Generic;
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

    public GameObject startCanvas;
    public GameObject gameCanvas;
    public GameObject firstTurnCanvas;
    public GameObject championDashboard;

    public GameObject playerPrefab;
    public GameObject opponentPrefab;
    public GameObject healthDisplayTextPrefab;
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
                opponent.cards = opponentArea.transform.childCount;
                int value = 999;
                int siblingIndex = 0;
                for (int x = 0; x < opponent.cards; x++)
                {
                    if (value > opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                    {
                        value = opponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                        siblingIndex = opponentArea.transform.GetChild(x).GetSiblingIndex();
                    }
                }
                opponentArea.transform.GetChild(siblingIndex).gameObject.GetComponent<Card>().ToggleCardVisibility();
                opponentArea.transform.GetChild(siblingIndex).gameObject.transform.SetParent(playArea.transform, false);
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
                SceneManager.LoadScene("StartMenu");
                break;
            case GamePhase.GAMELOSE:
                playerActionTooltip.text = "You lost!";

                yield return new WaitForSeconds(3f);
                SceneManager.LoadScene("StartMenu");
                break;
		}
	}
    #endregion


    #region Other Callable Functions
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
                attackingCard.transform.SetParent(playerArea.transform, false);
                StartCoroutine(cardLogicHandler.AttackCalc(attackingCard));
                break;
            case GamePhase.OPPONENTACTIONPHASE:
                if (player.isAttacked != true || opponent.isAttacking != true) return;

                attackingCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
                attackingCard.transform.SetParent(playerArea.transform, false);
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
            case GamePhase.OPPONENTACTIONPHASE:
                if (player.discardAmount >= 2)
				{
                    if (player.discardAmount >= 4)
					{
                        player.Damage(40, DamageType.Fire);
					}
                    else
					{
                        player.Damage(20, DamageType.Unblockable);
                    }
                    player.discardAmount = 0;
                    playerActionTooltip.text = "It is the opponent's Action Phase.";
                    StartCoroutine(cardLogicHandler.OpponentCardLogic());
                }
                break;
		}
	}
    [HideInInspector]
    public void EnlargeChampionDashboard()
    {
        Image image = player.GetComponent<Image>();
        championDashboard.SetActive(true);
        Image championImage = championDashboard.transform.GetChild(1).gameObject.GetComponent<Image>();
        championImage.sprite = image.sprite;
    }
    [HideInInspector]
    public void CloseChampionDashboard()
    {
        championDashboard.SetActive(false);
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