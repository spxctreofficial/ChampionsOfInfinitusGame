using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardLogicHandler : MonoBehaviour
{
    public GameHandler gameHandler;
    public CardIndex cardIndex;

    Card card;

    public void CardSelect(GameObject card)
    {
        this.card = card.GetComponent<Card>();
        Debug.Log(this.card.cardType + ", " + this.card.cardValue);
    }
}
