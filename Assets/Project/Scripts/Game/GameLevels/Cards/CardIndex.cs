using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardIndex : MonoBehaviour {
	private List<CardScriptableObject> playingCards = new List<CardScriptableObject>();
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

	private void Awake() {
		foreach (var card in spades) {
			playingCards.Add(card);
		}
		foreach (var card in hearts) {
			playingCards.Add(card);
		}
		foreach (var card in clubs) {
			playingCards.Add(card);
		}
		foreach (var card in diamonds) {
			playingCards.Add(card);
		}
	}
}
