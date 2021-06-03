using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour {
	public ChampionController owner;

	public List<Card> cards = new List<Card>();

	/// <summary>
	/// Sets `championController` as the owner of this hand.
	/// </summary>
	/// <param name="championController"></param>
	public void SetOwner(ChampionController championController) {
		owner = championController;
		championController.hand = this;

		Debug.Log(owner.championName);
		name = owner.championName + "'s Hand";
	}
	
	// GetCard Functions
	/// <summary>
	/// Returns the amount of cards that this hand owns, identical to `hand.GetCardCount()` but supposedly filtered.
	/// </summary>
	/// <returns></returns>
	public int GetCardCount() {
		var cardCount = 0;

		foreach (Transform child in transform) {
			if (child.GetComponent<Card>() == null) continue;
			if (!cards.Contains(child.GetComponent<Card>())) continue;
			cardCount++;
		}

		return cardCount;
	}
	/// <summary>
	/// Get a specific card with a given criteria to search for.
	///
	/// Valid types are: "Lowest", "Highest", "Defense"
	/// </summary>
	/// <param name="type"></param>
	/// <returns></returns>
	public Card GetCard(string type) {
		Card selectedCard = null;
		int value;
		switch (type) {
			case "Lowest":
				value = 999;
				foreach (Transform child in transform) {
					if (child.GetComponent<Card>().cardValue >= value) continue;
					selectedCard = child.GetComponent<Card>();
					value = selectedCard.cardValue;
				}
				break;
			case "Highest":
				value = -999;
				foreach (Transform child in transform) {
					if (child.GetComponent<Card>().cardValue <= value) continue;
					selectedCard = child.GetComponent<Card>();
					value = selectedCard.cardValue;
				}
				break;
			case "Defense":
				value = -999;
				foreach (Transform child in transform) {
					switch (GameController.instance.difficulty) {
						case GameController.Difficulty.Noob:
							if (child.GetSiblingIndex() == child.parent.childCount - 1) selectedCard = child.GetComponent<Card>();
							if (Random.Range(0f, 1f) < 0.5f) continue;
							break;
						case GameController.Difficulty.Novice:
							if (child.GetSiblingIndex() == child.parent.childCount - 1) selectedCard = child.GetComponent<Card>();
							if (child.GetComponent<Card>().cardValue <= 9) continue;
							break;
						case GameController.Difficulty.Warrior:
							if (owner.currentHP >= 0.3f * owner.maxHP && child.GetComponent<Card>().cardValue >= 12 && Random.Range(0f, 1f) < 0.75f) {
								Debug.Log("The " + owner.championName + " is confident! They refuse to use a value of " + child.GetComponent<Card>().cardValue + " to defend!");
								continue;
							}
							if (value < child.GetComponent<Card>().cardValue) {
								selectedCard = child.GetComponent<Card>();
								value = selectedCard.cardValue;
							}
							break;
						case GameController.Difficulty.Champion:
							if (owner.currentHP >= 0.5f * owner.maxHP && child.GetComponent<Card>().cardValue >= 12 && Random.Range(0f, 1f) < 0.75f) {
								Debug.Log("The " + owner.championName + " is confident! They refuse to use a value of " + child.GetComponent<Card>().cardValue + " to defend!");
								continue;
							}
							if (value < child.GetComponent<Card>().cardValue) {
								selectedCard = child.GetComponent<Card>();
								value = selectedCard.cardValue;
							}
							break;
					}
				}
				break;
			default:
				Debug.LogError("No GetCard type was specified!");
				return null;
		}

		if (selectedCard == null) {
			Debug.LogWarning("No card within criteria was found! Returning a null.");
			return null;
		}
		return selectedCard;
	}
	public Card GetCard(string type, CardSuit suitCriteria) {
		Card selectedCard = null;
		int value;
		switch (type) {
			case "Lowest":
				value = 999;
				foreach (Transform child in transform) {
					if (child.GetComponent<Card>().cardValue >= value || child.GetComponent<Card>().cardSuit != suitCriteria) continue;
					selectedCard = child.GetComponent<Card>();
					value = selectedCard.cardValue;
				}
				break;
			case "Highest":
				value = -999;
				foreach (Transform child in transform) {
					if (child.GetComponent<Card>().cardValue <= value || child.GetComponent<Card>().cardSuit != suitCriteria) continue;
					selectedCard = child.GetComponent<Card>();
					value = selectedCard.cardValue;
				}
				break;
			default:
				Debug.LogError("No GetCard type was specified!");
				return null;
		}

		if (selectedCard == null) {
			Debug.LogWarning("No card within criteria was found! Returning a null.");
			return null;
		}
		return selectedCard;
	}
	/// <summary>
	/// A smarter coherence designed to return a card specifically for attacking.
	/// </summary>
	/// <param name="card"></param>
	/// <returns></returns>
	public Card GetAttackingCard(Card card) {
		Card selectedCard = null;
		var value = -999;
		foreach (Transform child in transform) {
			if (child.GetComponent<Card>() == card) continue;

			switch (GameController.instance.difficulty) {
				case GameController.Difficulty.Noob:
					if (child.GetSiblingIndex() == child.parent.childCount - 1) selectedCard = child.GetComponent<Card>();
					if (Random.Range(0f, 1f) < 0.5f) continue;
					break;
				case GameController.Difficulty.Novice:
					if (child.GetSiblingIndex() == child.parent.childCount - 1) selectedCard = child.GetComponent<Card>();
					if (child.GetComponent<Card>().cardValue <= 9) continue;
					break;
				case GameController.Difficulty.Warrior:
					if (owner.currentHP >= 0.25f * owner.maxHP && child.GetComponent<Card>().cardValue >= 10 && Random.Range(0f, 1f) < 0.75f) {
						Debug.Log("The opponent is confident! They refuse to use a value of " + child.GetComponent<Card>().cardValue + " to attack!");
						continue;
					}
					if (value < child.GetComponent<Card>().cardValue) {
						selectedCard = child.GetComponent<Card>();
						value = selectedCard.cardValue;
					}
					break;
				case GameController.Difficulty.Champion:
					if (child.GetComponent<Card>().cardSuit == CardSuit.HEART && owner.currentHP <= 0.75f * owner.maxHP && Random.Range(0f, 1f) < 0.75f) {
						Debug.Log("The " + owner.championName + " refuses to use a HEART to attack!");
						continue;
					}
					if (owner.currentHP >= 0.5f * owner.maxHP && child.GetComponent<Card>().cardValue >= 12 && Random.Range(0f, 1f) < 0.75f) {
						Debug.Log("The opponent is confident! They refuse to use a value of " + child.GetComponent<Card>().cardValue + " to attack!");
						continue;
					}
					if (value < child.GetComponent<Card>().cardValue) {
						selectedCard = child.GetComponent<Card>();
						value = selectedCard.cardValue;
					}
					break;
			}
		}

		if (selectedCard == null) {
			Debug.LogWarning("No card within criteria was found! Returning a null.");
			return null;
		}
		return selectedCard;
	}

	// Deal Functions
	/// <summary>
	/// Deal a specific card to this hand.
	/// </summary>
	/// <param name="specificCard"></param>
	public void DealSpecificCard(Card specificCard) {
		StartCoroutine(Deal(specificCard, false, true));
	}
	
	/// <summary>
	/// Deals an amount of randomly generated cards to this hand, with additional parameters for animation and fine control.
	/// </summary>
	/// <param name="amount"></param>
	/// <param name="flip"></param>
	/// <param name="animate"></param>
	/// <param name="abilityCheck"></param>
	/// <returns></returns>
	public IEnumerator Deal(int amount = 4, bool flip = false, bool animate = true, bool abilityCheck = true) {
		for (var i = 0; i < amount; i++) {
			// Creates a new card.
			var card = Instantiate(GameController.instance.cardIndex.playingCards[Random.Range(0, GameController.instance.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();

			// Noob mode crutch
			if (owner.isPlayer && Random.Range(0f, 1f) < 0.25f) {
				switch (GameController.instance.difficulty) {
					case GameController.Difficulty.Noob:
						int rerollValue = Mathf.Min(card.cardValue + 3, 13);
						foreach (var newCardGO in GameController.instance.cardIndex.playingCards) {
							var newCard = newCardGO.GetComponent<Card>();
							if (rerollValue != newCard.cardValue || card.cardSuit != newCard.cardSuit || card == newCard) continue;

							Destroy(card.gameObject);
							card = Instantiate(newCard, Vector2.zero, Quaternion.identity).GetComponent<Card>();
						}
						Debug.Log("oh yes crutch");
						break;
				}
			}

			// Sets card to the hand and adds it to the list of cards of this hand for easy reference.
			card.transform.SetParent(transform, false);
			card.owner = owner;
			cards.Add(card);
		
			// Extra parameters.
			if (flip) card.ToggleCardVisibility();
			if (animate) {
				card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				StartCoroutine(card.GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
			}
			if (abilityCheck) {
				foreach (var selectedChampion in GameController.instance.champions) {
					foreach (Transform child in selectedChampion.abilityPanel.panel.transform) {
						var ability = child.GetComponent<AbilityController>();
						yield return StartCoroutine(ability.OnDeal(card, owner));
					}
				}
			}

			yield return new WaitForSeconds(0.25f);
		}
	}
	private IEnumerator Deal(Card specificCard, bool flip, bool animate, bool abilityCheck = true) {
		var card = Instantiate(specificCard, new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();
		card.transform.SetParent(transform, false);
		card.owner = owner;
		cards.Add(card);
		
		if (flip) card.ToggleCardVisibility();
		if (animate) {
			card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
			StartCoroutine(card.GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
		}

		if (abilityCheck) {
			foreach (var selectedChampion in GameController.instance.champions) {
				foreach (Transform child in selectedChampion.abilityPanel.panel.transform) {
					var ability = child.GetComponent<AbilityController>();
					yield return StartCoroutine(ability.OnDeal(card, owner));
				}
			}
		}

		yield return new WaitForSeconds(0.25f);
	}
	/// <summary>
	/// Discards a specified card from this hand.
	/// Do note that this hand must own the card to have authority to discard the card. Otherwise, this will throw an error.
	/// </summary>
	/// <param name="card"></param>
	/// <param name="flip"></param>
	/// <param name="animate"></param>
	/// <param name="abilityCheck"></param>
	/// <returns></returns>
	public IEnumerator Discard(Card card, bool flip = false, bool animate = true, bool abilityCheck = true) {
		// Authority Check
		if (!cards.Contains(card) || card.owner != owner) {
			Debug.LogError("This hand does not have authority to discard another hand's card!");
			yield break;
		}
		
		// Sets card to the discard area, removing the card's specified owner, and removing the card from the list of cards from this hand to avoid memory leaks.
		card.transform.SetParent(GameController.instance.discardArea.transform, false);
		card.owner = null;
		cards.Remove(card);
		
		// Extra parameters.
		if (flip) card.ToggleCardVisibility();
		if (animate) {
			card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
			StartCoroutine(card.GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
		}
		if (abilityCheck) {
			foreach (var selectedChampion in GameController.instance.champions) {
				foreach (Transform child in selectedChampion.abilityPanel.panel.transform) {
					var ability = child.GetComponent<AbilityController>();
					// ABILITY CHECK HERE
				}
			}
		}
	}
}
