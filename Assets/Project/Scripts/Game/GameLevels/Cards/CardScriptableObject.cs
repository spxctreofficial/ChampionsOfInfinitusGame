using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CardSuit { SPADE, HEART, CLUB, DIAMOND };

[CreateAssetMenu(fileName = "Card", menuName = "New Card")]
public class CardScriptableObject : ScriptableObject {
	[Header("Card Values")]
	public int cardValue;
	public CardSuit cardSuit;
	public Sprite cardFront, cardBack;

	[Header("Card Identification")]
	public string cardName;
	[TextArea(5, 20)]
	public string cardDescription;
}
