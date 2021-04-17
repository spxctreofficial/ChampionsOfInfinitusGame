using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum GamePhase { GAMESTART, PLAYERBEGINNINGPHASE, PLAYERACTIONPHASE, PLAYERENDPHASE, OPPONENTBEGINNINGPHASE, OPPONENTACTIONPHASE, OPPONENTENDPHASE, GAMEWIN, GAMELOSE }

public class GameHandler : MonoBehaviour
{
    public GamePhase phase;
    public CardIndex cardIndex;
    public CardLogicHandler cardLogicHandler;

    public GameObject StartCanvas;
    public GameObject GameCanvas;
    public GameObject FirstTurnCanvas;

    public Text PlayerActionTooltip;

    public GameObject PlayerPrefab;
    public GameObject OpponentPrefab;
    public GameObject PlayerArea;
    public GameObject OpponentArea;
    public GameObject PlayArea;

    [HideInInspector]
    public Player player;
    [HideInInspector]
    public Player opponent;

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
        Debug.Log("Cards: " + PlayerArea.transform.childCount);

        GameObject opponentGO = Instantiate(OpponentPrefab, new Vector2(866, 139), Quaternion.identity);
        opponent = opponentGO.GetComponent<Player>();
        opponentGO.transform.SetParent(GameCanvas.transform, false);
        Debug.Log("Opponent: " + opponent.currentHP);
        Debug.Log("Cards: " + OpponentArea.transform.childCount);

        StartCoroutine(PlayerTurn());
        PlayerActionTooltip.text = "You are " + player.championName + ".";
    }
    IEnumerator PlayerTurn()
    {
        yield return new WaitForSeconds(2f);

        phase = GamePhase.PLAYERBEGINNINGPHASE;
        PlayerActionTooltip.text = "It is your Beginning Phase.";

        yield return new WaitForSeconds(1f);
        DealCardsPlayer(2);
        yield return new WaitForSeconds(2f);

        phase = GamePhase.PLAYERACTIONPHASE;
        PlayerActionTooltip.text = "It is your Action Phase.";
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
