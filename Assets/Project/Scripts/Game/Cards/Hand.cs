using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Hand : MonoBehaviour {
	public ChampionController owner;

	public List<Card> cards = new List<Card>();
	public Queue<Card> queued = new Queue<Card>();

	/// <summary>
	/// Sets `championController` as the owner of this hand.
	/// </summary>
	/// <param name="championController"></param>
	public void SetOwner(ChampionController championController) {
		owner = championController;
		championController.hand = this;

		name = owner.champion.championName + "'s Hand";
	}

    #region Get Cards Functions
    /// <summary>
    /// Returns the amount of cards that this hand owns.
    /// </summary>
    /// <returns></returns>
    public int GetCardCount() {
		int cardCount = 0;

		foreach (Card card in cards) {
			if (card.owner is null) continue;
			cardCount++;
		}

		return cardCount;
	}

	public Card GetDiscard() {
		Card card = null;
		foreach (Card selectedCard in cards) {
			if (card is null) {
				card = selectedCard;
				continue;
			}
			
			if (selectedCard.cardData.cardImportanceFactor > card.cardData.cardImportanceFactor ||
			    selectedCard.cardData.cardImportanceFactor == card.cardData.cardImportanceFactor && Random.Range(0f, 1f) < 0.5f ||
			    GetDefenseCards().Contains(selectedCard) && GetDefenseCards().Length > 2) {
				card = selectedCard;
			}
		}

		return card;
	}
	public Card[] GetDiscardArray(int discardAmount) {
		List<Card> discardList = new List<Card>();
		for (int discarded = 0; discarded < discardAmount; discarded++) {
			Card discard = GetDiscard();
			cards.Remove(discard);
			discard.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
			discard.discardFeed.text = "DISCARDED";
			discardList.Add(discard);
		}

		return discardList.ToArray();
	}
	public Card[] GetDefenseCards() {
		List<Card> cards = new List<Card>();

		foreach (Card card in this.cards) {
			switch (card.cardData.cardFunctions.primaryFunction) {
				case "block":
				case "parry":
					cards.Add(card);
					break;
			}
		}

		return cards.ToArray();
	}

	public Card[] GetWeaponCards() {
		List<Card> cards = new List<Card>();

		foreach (Card card in this.cards) {
			if (card.cardData is WeaponCardData) {
				cards.Add(card);
            }
        }

		return cards.ToArray();
    }
	public Card GetPlayableWeaponCard() {
		Card selectedCard = null;

		foreach (Card card in this.cards) {
			if (card.cardData is not WeaponCardData) continue;
			if (card.cardData.staminaRequirement > owner.currentStamina) continue;
			if (card is null) {
				selectedCard = card;
				continue;
            }

			if (((WeaponCardData) card.cardData).weaponScriptableObject.attackDamage > ((WeaponCardData) card.cardData).weaponScriptableObject.attackDamage) {
				selectedCard = card;
				continue;
            }
        }

		if (selectedCard is null) Debug.Log("No playable weapon was found! Returning a null.");
		return selectedCard;
    }

    #endregion

    #region Draw Functions

    /// <summary>
    /// Deals an amount of randomly generated cards to this hand, with additional parameters for animation and fine control.
    /// </summary>
    /// <param name="amount"></param>
    /// <param name="cardColor"></param>
    /// <param name="excludeDraw"></param>
    /// <param name="flip"></param>
    /// <param name="animate"></param>
    /// <param name="abilityCheck"></param>
    /// <returns></returns>
    public IEnumerator Deal(int amount = 4, CardColor cardColor = CardColor.NoPref, bool excludeDraw = false, bool flip = false, bool animate = true, bool abilityCheck = true) {
		for (int i = 0; i < amount; i++) {

			CardData cardData = PrefabManager.instance.cardReg.GenerateRandomCardData(owner);
			switch (cardColor) {
				case CardColor.NoPref:
					break;
				default:
					List<CardData> usedIndexes = new List<CardData>();
					while (cardData.cardColor != cardColor || (excludeDraw && cardData.cardFunctions.primaryFunction == "draw")) {
						CardData index = PrefabManager.instance.cardReg.GenerateRandomCardData(owner);
						if (usedIndexes.Contains(index)) continue;
						usedIndexes.Add(index);
						cardData = index;
					}
					break;
			}

			 yield return StartCoroutine(Deal(cardData, flip, animate, abilityCheck));
			 
			 yield return new WaitForSeconds(0.25f);
		}
	}
	public IEnumerator Deal(CardData cardData, bool flip = false, bool animate = true, bool abilityCheck = true) {
		AudioManager.instance.Play("flip");
		Card card = Instantiate(PrefabManager.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
		card.cardData = cardData;

		// Sets card to the hand and adds it to the list of cards of this hand for easy reference.
		card.transform.SetParent(transform, false);
		card.owner = owner;
		card.owner.matchStatistic.totalCardsDealt++;
		cards.Add(card);
		
		// Extra parameters.
		if (flip) card.Flip();
		if (animate) {
			card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
			card.GetComponent<SmartHover>().ScaleDown();
		}
		if (abilityCheck) {
			foreach (ChampionController selectedChampion in GameManager.instance.champions) {
				foreach (Ability ability in selectedChampion.abilities) {
					yield return StartCoroutine(ability.OnDeal(card, owner));
				}
			}
		}
	}
    #endregion

    #region Discard / Use Card Functions
    /// <summary>
    /// Discards a specified card from this hand.
    /// </summary>
    /// <param name="card"></param>
    /// <param name="flip"></param>
    /// <param name="animate"></param>
    /// <param name="abilityCheck"></param>
    /// <returns></returns>
    public IEnumerator Discard(Card card, bool flip = false, bool animate = true, bool abilityCheck = true) {
		// Authority Check
		/*if (card != owner.attackingCard || card != owner.defendingCard) {
			if (!cards.Contains(card) || card.owner != owner) {
				Debug.LogError("This hand does not have authority to discard another hand's card!");
				yield break;
			}
		}*/
		
		
		// Sets card to the discard area, removing the card's specified owner, and removing the card from the list of cards from this hand to avoid memory leaks.
		AudioManager.instance.Play("flip");
		card.transform.SetParent(GameManager.instance.discardArea.transform, false);
		
		if (GameManager.instance.discardArea.transform.childCount > 8) {
			for (int i = GameManager.instance.discardArea.transform.childCount; i > 8; i--) {
				Destroy(GameManager.instance.discardArea.transform.GetChild(0).gameObject);
			}
		}
		if (card.owner != null) {
			card.owner.matchStatistic.totalCardsDiscarded++;
			card.caption.text = "Played by " + card.owner.champion.championName;
		}
		card.owner = null;
		cards.Remove(card);

		// Extra parameters.
		if (flip) card.Flip();
		if (animate) {
			card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
			card.GetComponent<SmartHover>().ScaleDown();
		}
		if (abilityCheck) {
			foreach (ChampionController selectedChampion in GameManager.instance.champions) {
				foreach (Ability ability in selectedChampion.abilities) {
					// ABILITY CHECK HERE
				}
			}
		}
		yield break;
	}
	public IEnumerator Discard(Card[] cards, bool flip = false, bool animate = true, bool abilityCheck = false) {
		if (cards.Length == 0) yield break;

		foreach (Card card in cards) {
			yield return StartCoroutine(Discard(card, flip, animate, abilityCheck));
			yield return new WaitForSeconds(0.5f);
		}
	}

	public IEnumerator UseCard(Card card, bool flip = false, bool animate = true, bool abilityCheck = false) {
		owner.currentStamina = Mathf.Max(owner.currentStamina - card.EffectiveStaminaRequirement, 0);
		yield return StartCoroutine(Discard(card, flip, animate, abilityCheck));
	}

    #endregion
}
