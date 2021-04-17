using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum GamePhase { GAMESTART, PLAYERBEGINNINGPHASE, PLAYERACTIONPHASE, PLAYERENDPHASE, OPPONENTBEGINNINGPHASE, OPPONENTACTIONPHASE, OPPONENTENDPHASE, GAMEWIN, GAMELOSE }

public class GameHandler : MonoBehaviour
{
    public GamePhase phase;
    public CardIndex cardIndex;
    public CardLogicHandler cardLogicHandler;

    public GameObject StartCanvas;
    public GameObject GameCanvas;
    public GameObject FirstTurnCanvas;

    public GameObject PlayerPrefab;
    public GameObject OpponentPrefab;
    public GameObject PlayerArea;
    public GameObject OpponentArea;

    Player player;
    Player opponent;

    void Start()
    {
        cardIndex.PopulatePlayingCardsList();
        GameStart();
    }
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
        playerGO.transform.SetParent(GameCanvas.transform, false);
        player = playerGO.GetComponent<Player>();
        Debug.Log("Player: " + player.currentHP);

        GameObject opponentGO = Instantiate(OpponentPrefab, new Vector2(866, 139), Quaternion.identity);
        opponent = opponentGO.GetComponent<Player>();
        opponentGO.transform.SetParent(GameCanvas.transform, false);
        Debug.Log("Opponent: " + opponent.currentHP);

        PlayerTurn();
    }
    public void PlayerTurn()
    {
        phase = GamePhase.PLAYERBEGINNINGPHASE;
    }

    // PRIVATE VOIDS

    private void DealCardsPlayer(int cards)
    {
        for (int x = 0; x < cards; x++)
        {
            GameObject playerCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
            playerCard.transform.SetParent(PlayerArea.transform, false);
        }
    }
    private void DealCardsOpponent(int cards)
    {
        for (int x = 0; x < cards; x++)
        {
            GameObject opponentCard = Instantiate(cardIndex.playingCards[Random.Range(0, cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity);
            opponentCard.transform.SetParent(OpponentArea.transform, false);
        }
    }
    private void DealCards(int cards)
    {
        DealCardsPlayer(cards);
        DealCardsOpponent(cards);
    }
}
