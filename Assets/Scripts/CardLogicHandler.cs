using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CardLogicHandler : MonoBehaviour
{
    public GameHandler gameHandler;
    public CardIndex cardIndex;

    public GameObject PlayerArea;
    public GameObject OpponentArea;
    public GameObject PlayArea;

    Card card;
    bool cardOfPlayer;

    void Update()
    {
        PurgePlayArea();
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            card.ToggleCardVisibility();
        }
    }

    public void CardSelect(GameObject card)
    {
        this.card = card.GetComponent<Card>();
        GameObject parentObject = card.transform.parent.gameObject;
        cardOfPlayer = parentObject == PlayerArea;
        // Debug.Log(this.card.cardType + ", " + this.card.cardValue + ". Owned by player? " + cardOfPlayer);

        if (cardOfPlayer)
        {
            if (gameHandler.phase == GamePhase.PLAYERACTIONPHASE)
            {
                switch (this.card.cardType)
                {
                    case CardType.SPADE:
                        Debug.Log("Player is now attacking the opponent.");
                        gameHandler.player.isAttacking = true;
                        gameHandler.opponent.isAttacked = true;
                        card.transform.SetParent(PlayArea.transform, false);
                        break;
                    case CardType.HEART:
                        Debug.Log("Player is attempting to heal.");
                        break;
                    case CardType.CLUB:
                        Debug.Log("Player is attempting to trade a CLUB.");
                        break;
                    case CardType.DIAMOND:
                        Debug.Log("Player is attempting to use a DIAMOND.");
                        break;
                }
            }
            else
            {
                Debug.Log("It is not the player's turn!");
            }
        }
        else
        {
            Debug.Log("This is not the player's card!");
        }
    }

    private void PurgePlayArea()
    {
        if (PlayArea.transform.childCount > 1)
        {
            Transform transform = PlayArea.transform.GetChild(0);
            GameObject gameObject = transform.gameObject;
            Object.Destroy(gameObject);
        }
    }
}
