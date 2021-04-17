using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardType { SPADE, HEART, CLUB, DIAMOND };
public class Card : MonoBehaviour
{
    public CardType cardType;
    public int cardValue; // 1 = Two of X, 13 = Ace of X

    public void CardSelect()
    {
        CardLogicHandler cardLogicHandler = GameObject.FindWithTag("CardLogicHandler").GetComponent<CardLogicHandler>();
        cardLogicHandler.CardSelect(this.gameObject);
    }
}
