using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public enum CardSuit { SPADE, HEART, CLUB, DIAMOND };
public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	// Default Card Values
	public CardSuit cardSuit;
	public int cardValue; // 1 = Two of X, 13 = Ace of X
	[HideInInspector]
	public bool isHidden;
	[HideInInspector]
	public Sprite cardFront;
	public Sprite cardBack;

	[HideInInspector]
	public ChampionController owner;
	[HideInInspector]
	public List<string> tags = new List<string>();

	private static LTDescr delay;

	private readonly string[] cardSuitNames = {
		"Spades", "Hearts", "Clubs", "Diamonds"
	};
	private readonly string[] cardValueNames = {
		"", "Two", "Three", "Four", "Five", "Six", "Seven", "Eight", "Nine", "Ten", "Jack", "Queen", "King", "Ace"
	};
	private string CardDescription() {
		string description = "When played normally: ";
		switch (cardSuit) {
			case CardSuit.SPADE:
				description += "Initializes an attack.";
				break;
			case CardSuit.HEART:
				var currentCardValue = cardValue;
				int healAmount() {
					switch (currentCardValue) {
						default:
							return 5;
						case 7:
						case 8:
						case 9:
							return 10;
						case 10:
						case 11:
						case 12:
							return 20;
						case 13:
							return 40;
					}
				}
				description += "Heal for " + healAmount() + ".";
				break;
			case CardSuit.CLUB:
				description += "Trade into deck for a random card.";
				break;
			case CardSuit.DIAMOND:
				currentCardValue = cardValue;
				string Function() {
					return currentCardValue switch {
						1 => "Deal 2 cards to all players.",
						2 => "Force all players to discard 1 card.",
						3 => "Deal 4 cards to all players.",
						4 => "Force all players (excluding user) to discard 2 cards, or be dealt 20 unblockable damage.",
						9 => "Heal all players for 10.",
						10 => "Heal all players for 20.",
						11 => "Deal 20 fire damage to all players (excluding user).",
						12 => "Force all players (excluding user) to discard 4 cards, or be dealt 40 fire damage.",
						13 => "COMING SOON ;)",
						_ => "Trade into deck for a random card."
					};
				}
				description += Function();
				break;
		}
		description += "\n" +
		               "\n If in combat, this card will be played in combat with a value of " + (cardValue + 1) + ".";

		return description;
	}
	
	private bool CardOfPlayer() {
		if (transform.parent.GetComponent<Hand>() == GameController.instance.playerHand)
			return true;
		Debug.Log("This is not the player's card!");
		return false;
	}
	
	// Pointer Events
	
	public void CardSelect() {
		if (CardOfPlayer()) StartCoroutine(CardLogicController.instance.CardSelect(this));
	}
	[HideInInspector]
	public void ToggleCardVisibility(bool doFlipAnimation = false) {
		var image = GetComponent<Image>();
		if (!isHidden) {
			isHidden = true;
			cardFront = image.sprite;
			image.sprite = cardBack;
		}
		else {
			isHidden = false;
			image.sprite = cardFront;

			if (doFlipAnimation) {
				transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				StartCoroutine(GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
			}
		}
	}
	public void OnPointerEnter(PointerEventData eventData) {
		delay = LeanTween.delayedCall(0.5f, () => {
			if (isHidden) {
				TooltipSystem.instance.Show(null, "Flipped Card");
				return;
			}
			var index = (int)cardSuit;
			TooltipSystem.instance.Show(CardDescription(), cardValueNames[cardValue] + " of " + cardSuitNames[index]);
		});
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delay.uniqueId);
		TooltipSystem.instance.Hide();
	}
}
