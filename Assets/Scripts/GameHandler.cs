using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase { GAMESTART, PLAYERBEGINNINGPHASE, PLAYERACTIONPHASE, PLAYERENDPHASE, OPPONENTBEGINNINGPHASE, OPPONENTACTIONPHASE, OPPONENTENDPHASE, GAMEWIN, GAMELOSE }

public class GameHandler : MonoBehaviour
{
    public GamePhase phase;
    public CardIndex cardIndex;
    public CardLogicHandler cardLogicHandler;

    public GameObject startCanvas;
    public GameObject gameCanvas;
    public GameObject firstTurnCanvas;

    public GameObject playerPrefab;

    public GameObject PlayerArea;
    public GameObject OpponentArea;

    void Start()
    {
        cardIndex.PopulatePlayingCardsList();
        GameStart();
    }
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
        PlayerTurn();
    }
    public void PlayerTurn()
    {
        phase = GamePhase.PLAYERBEGINNINGPHASE;
    }

    // PRIVATE VOIDS

    private void DealCards(int cards)
    {
        for (int x = 0; x < cards; x++)
        {
            GameObject playerCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
            playerCard.transform.SetParent(PlayerArea.transform, false);
        }
    }
}
