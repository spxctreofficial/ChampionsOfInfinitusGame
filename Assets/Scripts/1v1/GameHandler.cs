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

    public GameObject StartCanvas;
    public GameObject GameCanvas;
    public GameObject FirstTurnCanvas;
    public GameObject ChampionDashboard;

    public GameObject PlayerPrefab;
    public GameObject OpponentPrefab;
    public GameObject HealthDisplayTextPrefab;
    public GameObject PlayerArea;
    public GameObject OpponentArea;
    public GameObject PlayArea;

    public Text PlayerActionTooltip;
    public GameObject EndTurnButton;

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
            player.cards = PlayerArea.transform.childCount;
            opponent.cards = OpponentArea.transform.childCount;

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
        GameCanvas.SetActive(false);
        StartCanvas.SetActive(true);
    }
    public void GamePlayerChooseTurn()
    {
        StartCanvas.SetActive(false);
        FirstTurnCanvas.SetActive(true);
    }
    public void GameSetup()
    {
        FirstTurnCanvas.SetActive(false);
        GameCanvas.SetActive(true);
        DealCards(4);

        GameObject playerGO = Instantiate(PlayerPrefab, new Vector2(-866, -139), Quaternion.identity);
        GameObject playerHealthTextGO = Instantiate(HealthDisplayTextPrefab, new Vector2(-866, 29), Quaternion.identity);
        player = playerGO.GetComponent<ChampionHandler>();
        playerGO.transform.SetParent(GameCanvas.transform, false);
        playerHealthTextGO.transform.SetParent(GameCanvas.transform, false);
        player.ChampionSetup();
        playerHealthText = playerHealthTextGO.GetComponent<Text>();
        Debug.Log("Player: " + player.currentHP);
        Debug.Log("Cards: " + PlayerArea.transform.childCount);

        GameObject opponentGO = Instantiate(OpponentPrefab, new Vector2(866, 139), Quaternion.identity);
        GameObject opponentHealthTextGO = Instantiate(HealthDisplayTextPrefab, new Vector2(866, -29), Quaternion.identity);
        opponent = opponentGO.GetComponent<ChampionHandler>();
        opponentGO.transform.SetParent(GameCanvas.transform, false);
        opponentHealthTextGO.transform.SetParent(GameCanvas.transform, false);
        opponent.ChampionSetup();
        opponentHealthText = opponentHealthTextGO.GetComponent<Text>();
        Debug.Log("Opponent: " + opponent.currentHP);
        Debug.Log("Cards: " + OpponentArea.transform.childCount);

        StartCoroutine(PlayerTurn());
        PlayerActionTooltip.text = "You are " + player.championName + ".";
        playerHealthText.text = player.currentHP.ToString();
        opponentHealthText.text = opponent.currentHP.ToString();
    }
    IEnumerator PlayerTurn()
    {
        yield return new WaitForSeconds(2f);

        phase = GamePhase.PLAYERBEGINNINGPHASE;
        PlayerActionTooltip.text = "It is your Beginning Phase.";
        DealCardsPlayer(2);

        yield return new WaitForSeconds(1f);

        phase = GamePhase.PLAYERACTIONPHASE;
        player.spadesBeforeExhaustion = 1;
        player.heartsBeforeExhaustion = 3;
        player.diamondsBeforeExhaustion = 1;
        EndTurnButton.SetActive(true);
        PlayerActionTooltip.text = "It is your Action Phase.";
    }
    void EndPlayerTurn()
    {
        phase = GamePhase.PLAYERENDPHASE;
        PlayerActionTooltip.text = "It is your End Phase.";
        if (player.cards > 6)
        {
            player.discardAmount = player.cards - 6;
            PlayerActionTooltip.text = "Please discard " + player.discardAmount + ".";
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

        PlayerActionTooltip.text = "It is the opponent's Beginning Phase.";

        yield return new WaitForSeconds(1f);
        DealCardsOpponent(2);
        yield return new WaitForSeconds(2f);

        phase = GamePhase.OPPONENTACTIONPHASE;
        opponent.spadesBeforeExhaustion = 1;
        opponent.heartsBeforeExhaustion = 3;
        opponent.diamondsBeforeExhaustion = 1;
        PlayerActionTooltip.text = "It is the opponent's Action Phase.";


        StartCoroutine(cardLogicHandler.OpponentCardLogic());
    }
    public void EndOpponentTurn()
    {
        phase = GamePhase.OPPONENTENDPHASE;
        PlayerActionTooltip.text = "It is the opponent's End Phase.";
        if (opponent.cards > 6)
        {
            opponent.discardAmount = opponent.cards - 6;

            for (int i = 0; i < opponent.discardAmount; i++)
            {
                int value = 999;
                int siblingIndex = 0;
                for (int x = 0; x < opponent.cards; x++)
                {
                    if (value > OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue)
                    {
                        value = OpponentArea.transform.GetChild(x).gameObject.GetComponent<Card>().cardValue;
                        siblingIndex = OpponentArea.transform.GetChild(x).GetSiblingIndex();
                    }
                }
                OpponentArea.transform.GetChild(siblingIndex).gameObject.transform.SetParent(PlayArea.transform, false);
                OpponentArea.transform.GetChild(siblingIndex).gameObject.GetComponent<Card>().ToggleCardVisibility();
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
				PlayerActionTooltip.text = "Congratulations! You win.";

                yield return new WaitForSeconds(3f);
                SceneManager.LoadScene("StartMenu");
                break;
            case GamePhase.GAMELOSE:
                PlayerActionTooltip.text = "You lost!";

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
        EndTurnButton.SetActive(false);
        EndPlayerTurn();
    }
    [HideInInspector]
    public void EnlargeChampionDashboard()
    {
        Image image = player.GetComponent<Image>();
        ChampionDashboard.SetActive(true);
        Image championImage = ChampionDashboard.transform.GetChild(1).gameObject.GetComponent<Image>();
        championImage.sprite = image.sprite;
    }
    [HideInInspector]
    public void CloseChampionDashboard()
    {
        ChampionDashboard.SetActive(false);
    }
    public void DealCardsPlayer(int cards)
    {
        for (int x = 0; x < cards; x++)
        {
            GameObject playerCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
            playerCard.transform.SetParent(PlayerArea.transform, false);
        }
    }
    public void DealCardsOpponent(int cards)
    {
        for (int x = 0; x < cards; x++)
        {
            GameObject opponentCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
            opponentCard.transform.SetParent(OpponentArea.transform, false);
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