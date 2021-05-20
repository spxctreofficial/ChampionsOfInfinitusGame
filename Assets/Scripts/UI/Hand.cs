using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Hand : MonoBehaviour
{
	public ChampionController owner;

	public void Deal(int amount = 4, bool flip = false, bool animate = true, bool abilityCheck = true)
	{
		for (var x = 0; x < amount; x++) StartCoroutine(Deal(flip, animate, abilityCheck));
	}
	public void DealSpecificCard(Card specificCard)
	{
		StartCoroutine(Deal(specificCard, false, true));
	}
	public void SetOwner(ChampionController championController)
	{
		this.owner = championController;
		championController.hand = this;
	}
	public Card GetCard(string type)
	{
		Card selectedCard = null;
		int value;
		switch (type)
		{
			case "Lowest":
				value = 999;
				foreach (Transform child in transform)
				{
					if (child.GetComponent<Card>().cardValue >= value) continue;
					selectedCard = child.GetComponent<Card>();
					value = selectedCard.cardValue;
				}
				break;
			case "Highest":
				value = -999;
				foreach (Transform child in transform)
				{
					if (child.GetComponent<Card>().cardValue <= value) continue;
					selectedCard = child.GetComponent<Card>();
					value = selectedCard.cardValue;
				}
				break;
			case "Defense":
				value = -999;
				foreach (Transform child in transform)
				{
					switch (GameController.instance.difficulty)
					{
						case GameController.Difficulty.Noob:
							if (child.GetSiblingIndex() == child.parent.childCount - 1) selectedCard = child.GetComponent<Card>();
							if (Random.Range(0f, 1f) < 0.5f) continue;
							break;
						case GameController.Difficulty.Novice:
							if (child.GetSiblingIndex() == child.parent.childCount - 1) selectedCard = child.GetComponent<Card>();
							if (child.GetComponent<Card>().cardValue <= 9) continue;
							break;
						case GameController.Difficulty.Warrior:
							if (owner.currentHP >= 0.3f * owner.maxHP && child.GetComponent<Card>().cardValue >= 12 && Random.Range(0f, 1f) < 0.75f)
							{
								Debug.Log("The " + owner.name + " is confident! They refuse to use a value of " + child.GetComponent<Card>().cardValue + " to defend!");
								continue;
							}
							if (value < child.GetComponent<Card>().cardValue)
							{
								selectedCard = child.GetComponent<Card>();
								value = selectedCard.cardValue;
							}
							break;
						case GameController.Difficulty.Champion:
							if (owner.currentHP >= 0.5f * owner.maxHP && child.GetComponent<Card>().cardValue >= 12 && Random.Range(0f, 1f) < 0.75f)
							{
								Debug.Log("The " + owner.name + " is confident! They refuse to use a value of " + child.GetComponent<Card>().cardValue + " to defend!");
								continue;
							}
							if (value < child.GetComponent<Card>().cardValue)
							{
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

		if (selectedCard == null)
		{
			Debug.LogWarning("No card within criteria was found! Returtning a null.");
			return null;
		}
		return selectedCard;
	}
	public Card GetCard(string type, CardSuit suitCriteria)
	{
		Card selectedCard = null;
		int value;
		switch (type)
		{
			case "Lowest":
				value = 999;
				foreach (Transform child in transform)
				{
					if (child.GetComponent<Card>().cardValue >= value || child.GetComponent<Card>().cardSuit != suitCriteria) continue;
					selectedCard = child.GetComponent<Card>();
					value = selectedCard.cardValue;
				}
				break;
			case "Highest":
				value = -999;
				foreach (Transform child in transform)
				{
					if (child.GetComponent<Card>().cardValue <= value || child.GetComponent<Card>().cardSuit != suitCriteria) continue;
					selectedCard = child.GetComponent<Card>();
					value = selectedCard.cardValue;
				}
				break;
			default:
				Debug.LogError("No GetCard type was specified!");
				return null;
		}

		if (selectedCard == null)
		{
			Debug.LogWarning("No card within criteria was found! Returning a null.");
			return null;
		}
		return selectedCard;
	}
	public Card GetAttackingCard(Card card)
	{
		Card selectedCard = null;
		var value = -999;
		foreach (Transform child in transform)
		{
			if (child.GetComponent<Card>() == card) continue;

			switch (GameController.instance.difficulty)
			{
				case GameController.Difficulty.Noob:
					if (child.GetSiblingIndex() == child.parent.childCount - 1) selectedCard = child.GetComponent<Card>();
					if (Random.Range(0f, 1f) < 0.5f) continue;
					break;
				case GameController.Difficulty.Novice:
					if (child.GetSiblingIndex() == child.parent.childCount - 1) selectedCard = child.GetComponent<Card>();
					if (child.GetComponent<Card>().cardValue <= 9) continue;
					break;
				case GameController.Difficulty.Warrior:
					if (owner.currentHP >= 0.25f * owner.maxHP && child.GetComponent<Card>().cardValue >= 10 && Random.Range(0f, 1f) < 0.75f)
					{
						Debug.Log("The opponent is confident! They refuse to use a value of " + child.GetComponent<Card>().cardValue + " to attack!");
						continue;
					}
					if (value < child.GetComponent<Card>().cardValue)
					{
						selectedCard = child.GetComponent<Card>();
						value = selectedCard.cardValue;
					}
					break;
				case GameController.Difficulty.Champion:
					if (child.GetComponent<Card>().cardSuit == CardSuit.HEART && owner.currentHP <= 0.75f * owner.maxHP && Random.Range(0f, 1f) < 0.75f)
					{
						Debug.Log("The " + owner.name + " refuses to use a HEART to attack!");
						continue;
					}
					if (owner.currentHP >= 0.5f * owner.maxHP && child.GetComponent<Card>().cardValue >= 12 && Random.Range(0f, 1f) < 0.75f)
					{
						Debug.Log("The opponent is confident! They refuse to use a value of " + child.GetComponent<Card>().cardValue + " to attack!");
						continue;
					}
					if (value < child.GetComponent<Card>().cardValue)
					{
						selectedCard = child.GetComponent<Card>();
						value = selectedCard.cardValue;
					}
					break;
			}
		}

		if (selectedCard == null)
		{
			Debug.LogWarning("No card within criteria was found! Returning a null.");
			return null;
		}
		return selectedCard;
	}
	
	private IEnumerator Deal(bool flip, bool animate, bool abilityCheck = true)
	{
		var card = Instantiate(GameController.instance.cardIndex.playingCards[Random.Range(0, GameController.instance.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();

		// Noob mode crutch
		if (owner.isPlayer && Random.Range(0f, 1f) < 0.25f)
		{
			switch (GameController.instance.difficulty)
			{
				case GameController.Difficulty.Noob:
					int rerollValue = Mathf.Min(card.cardValue + 3, 13);
					foreach (var newCardGO in GameController.instance.cardIndex.playingCards)
					{
						var newCard = newCardGO.GetComponent<Card>();
						if (rerollValue != newCard.cardValue || card.cardSuit != newCard.cardSuit || card == newCard) continue;

						Destroy(card.gameObject);
						card = Instantiate(newCard, Vector2.zero, Quaternion.identity).GetComponent<Card>();
					}
					Debug.Log("oh yes crutch");
					break;
			}
		}
		
		card.transform.SetParent(transform, false);
		if (flip) card.ToggleCardVisibility();
		if (animate)
		{
			card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
			StartCoroutine(card.GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
		}

		if (abilityCheck)
		{
			foreach (var selectedChampion in GameController.instance.champions)
			{
				foreach (Transform child in selectedChampion.abilityPanel.panel.transform)
				{
					var ability = child.GetComponent<AbilityController>();
					yield return StartCoroutine(ability.OnDeal(card, owner));
				}
			}
		}

		yield return new WaitForSeconds(0.25f);
	}
	private IEnumerator Deal(Card specificCard, bool flip, bool animate, bool abilityCheck = true)
	{
		var card = Instantiate(specificCard, new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();
		card.transform.SetParent(transform, false);
		if (flip) card.ToggleCardVisibility();
		if (animate)
		{
			card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
			StartCoroutine(card.GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
		}

		if (abilityCheck)
		{
			foreach (var selectedChampion in GameController.instance.champions)
			{
				foreach (Transform child in selectedChampion.abilityPanel.panel.transform)
				{
					var ability = child.GetComponent<AbilityController>();
					yield return StartCoroutine(ability.OnDeal(card, owner));
				}
			}
		}

		yield return new WaitForSeconds(0.25f);
	}
}
