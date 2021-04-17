using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogicHandler : MonoBehaviour
{
    public GameHandler gameHandler;
    public CardIndex cardIndex;

    public GameObject PlayerArea;
    public GameObject OpponentArea;

    Card card;
    bool cardOfPlayer;

    public void CardSelect(GameObject card)
    {
        this.card = card.GetComponent<Card>();
        GameObject parentObject = card.transform.parent.gameObject;
        cardOfPlayer = parentObject == PlayerArea;

        Debug.Log(this.card.cardType + ", " + this.card.cardValue + ". Owned by player? " + cardOfPlayer);

        /*switch (this.card.cardType)
        {
            case CardType.SPADE:
                break;
            case CardType.HEART:
                break;
            case CardType.CLUB:
                break;
            case CardType.DIAMOND:
                break;
        }*/
    }
}
