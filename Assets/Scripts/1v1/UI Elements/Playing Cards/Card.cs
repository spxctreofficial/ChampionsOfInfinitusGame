using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum CardType { SPADE, HEART, CLUB, DIAMOND };
public class Card : MonoBehaviour
{
    public CardType cardType;
    public int cardValue; // 1 = Two of X, 13 = Ace of X
    [HideInInspector]
    public bool isHidden = false;
    [HideInInspector]
    public Sprite cardFront;
    public Sprite cardBack;

    Image image;

    public void CardSelect()
    {
        CardLogicHandler cardLogicHandler = FindObjectOfType<CardLogicHandler>();
        StartCoroutine(cardLogicHandler.CardSelect(this.gameObject));
    }
    [HideInInspector]
    public void ToggleCardVisibility(bool doFlipAnimation = false)
    {
        image = GetComponent<Image>();
        if (!this.isHidden)
        {
            this.isHidden = true;
            cardFront = image.sprite;
            image.sprite = cardBack;
        }
        else
        {
            this.isHidden = false;
            image.sprite = cardFront;

            if (doFlipAnimation)
			{
                transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
                StartCoroutine(GetComponent<HoverScale>().ScaleDown(new Vector3(1f, 1f, 1f)));
			}
        }
    }
}
