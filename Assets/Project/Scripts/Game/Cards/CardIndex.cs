using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardIndex : MonoBehaviour {
	private static List<CardScriptableObject> playingCards = new List<CardScriptableObject>();
	[SerializeField]
	private List<CardScriptableObject> spades, hearts, clubs, diamonds;

	public List<CardScriptableObject> PlayingCards {
		get => playingCards;
	}
	public List<CardScriptableObject> Spades {
		get => spades;
	}
	public List<CardScriptableObject> Hearts {
		get => hearts;
	}
	public List<CardScriptableObject> Clubs {
		get => clubs;
	}
	public List<CardScriptableObject> Diamonds {
		get => diamonds;
	}

	public static CardScriptableObject FindCardInfo(CardSuit cardSuit, int value) {
		foreach (CardScriptableObject cardScriptableObject in playingCards) {
			if (cardScriptableObject.cardSuit != cardSuit || cardScriptableObject.cardValue != value) continue;

			return cardScriptableObject;
		}
		
		Debug.LogError("No card was found within the given criteria!");
		return null;
	}

	private void Awake() {
		foreach (CardScriptableObject card in spades) {
			playingCards.Add(card);
		}
		foreach (CardScriptableObject card in hearts) {
			playingCards.Add(card);
		}
		foreach (CardScriptableObject card in clubs) {
			playingCards.Add(card);
		}
		foreach (CardScriptableObject card in diamonds) {
			playingCards.Add(card);
		}
	}
}
