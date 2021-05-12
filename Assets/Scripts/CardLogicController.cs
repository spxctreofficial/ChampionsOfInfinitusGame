using System.Collections;
using UnityEngine;
using TMPro;

public class CardLogicController : MonoBehaviour
{
	public static CardLogicController instance;
	public Card summonCard; // debugging
	public int dealToIndex; // debugging

	void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Destroy(gameObject);
			return;
		}
	}
	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Alpha5))
		{
			Card card = Instantiate(summonCard.gameObject, new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();
			card.transform.SetParent(GameController.instance.champions[dealToIndex].hand.transform, false);
		}
	}

	public IEnumerator CardSelect(Card card)
	{
		ChampionController player = GameController.instance.champions[0];
		foreach (ChampionController champion in GameController.instance.champions)
		{
			if (!champion.isPlayer) continue;
			player = champion;
			if (player.isDead)
			{
				Debug.Log("The player is dead!");
				yield break;
			}
		}
		switch (GameController.instance.gamePhase)
		{
			case GamePhase.ActionPhase:
				if (player.isMyTurn)
				{
					if (player.isAttacking)
					{
						if (player.attackingCard != null) player.attackingCard.ToggleCardVisibility(true);
						player.attackingCard = card;
						card.ToggleCardVisibility(true);
						GameController.instance.playerActionTooltip.text = "Confirm the attack, or change selected card and/or target.";
						GameController.instance.confirmButton.gameObject.SetActive(true);

						if (player.currentTarget != null) break;
						GameController.instance.playerActionTooltip.text = "Choose a target or change selected card.";
						GameController.instance.confirmButton.gameObject.SetActive(false);
						break;
					}
					switch (card.cardSuit)
					{
						case CardSuit.SPADE:
							StartCoroutine(SpadeLogic(card, player));
							break;
						case CardSuit.HEART:
							PlayerHeart(card, player);
							break;
						case CardSuit.CLUB:
							StartCoroutine(ClubLogic(card, player));
							break;
						case CardSuit.DIAMOND:
							StartCoroutine(DiamondLogic(card, player));
							break;
					}
					break;
				}
				else
				{
					foreach (ChampionController champion in GameController.instance.champions)
					{
						if (champion.currentTarget != player || !champion.isAttacking) continue;

						if (player.defendingCard != null) player.defendingCard.ToggleCardVisibility(true);
						player.defendingCard = card;
						card.ToggleCardVisibility(true);
						GameController.instance.playerActionTooltip.text = "Confirm the defense, or change selected card.";
						GameController.instance.confirmButton.gameObject.SetActive(true);
					}
				}

				if (player.discardAmount > 0)
				{
					PlayerDiscard(card, player, "Forced");
				}
				else
				{
					Debug.Log("It is not the player's Action Phase!");
				}
				break;
			case GamePhase.EndPhase:
				if (player.isMyTurn && player.discardAmount > 0)
				{
					PlayerDiscard(card, player, "Forced");
				}
				break;
		}
		yield break;
	}
	public IEnumerator BotCardLogic(ChampionController champion)
	{
		if (champion.isDead)
		{
			Debug.LogWarning("Attempted to apply logic to dead champion!");
			GameController.instance.NextTurnCalculator(champion);
			yield break;
		}

		GameController.instance.playerActionTooltip.text = "The " + champion.name + "'s Turn: Action Phase";

		yield return new WaitForSeconds(Random.Range(0.2f, 1f));

		foreach (Transform child in champion.hand.transform)
		{
			Card card = child.GetComponent<Card>();
			if (card.cardSuit != CardSuit.CLUB) continue;
			if (card.cardValue > 10 && Random.Range(0f, 1f) < 0.9f)
			{
				Debug.Log("The " + champion.name + " refuses to trade in a CLUB worth: " + card.cardValue);
				continue;
			}

			yield return StartCoroutine(ClubLogic(card, champion));
		}

		yield return new WaitForSeconds(Random.Range(0.1f, 0.75f));

		foreach (Transform child in champion.hand.transform)
		{
			Card card = child.GetComponent<Card>();
			if (card.cardSuit != CardSuit.DIAMOND) continue;
			if (champion.diamondsBeforeExhaustion == 0 && (card.cardValue < 5 || card.cardValue > 8))
			{
				Debug.Log("The " + champion.name + " can't play this DIAMOND.");
				break;
			}

			yield return StartCoroutine(DiamondLogic(card, champion));
		}

		yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

		// Spades
		foreach (Transform child in champion.hand.transform)
		{
			Card card = child.GetComponent<Card>();
			if (card.cardSuit != CardSuit.SPADE) continue;
			if (champion.spadesBeforeExhaustion == 0)
			{
				Debug.Log("The " + champion.name + " is exhausted. Cannot attack.");
				break;
			}

			if ((champion.currentHP <= 0.3f * champion.maxHP && Random.Range(0f, 1f) < 0.8f) || Random.Range(0f, 1f) < 0.25f)
			{
				Debug.Log("The " + champion.name + " doesn't want to attack!");
				champion.spadesBeforeExhaustion--;
				break;
			}

			yield return StartCoroutine(SpadeLogic(card, champion));
		}

		yield return new WaitForSeconds(Random.Range(0.5f, 1.5f));

		foreach (Transform child in champion.hand.transform)
		{
			if (champion.currentHP == champion.maxHP) break;
			if (champion.heartsBeforeExhaustion == 0) break;
			Card card = child.GetComponent<Card>();

			switch (card.cardSuit)
			{
				case CardSuit.HEART:
					if (champion.currentHP + 20 >= 0.9f * champion.maxHP && card.cardValue == 13)
					{
						Debug.Log("Health would be clamped! The " + champion.name + " decides not to use an ACE of HEARTS to heal!");
						continue;
					}

					switch (card.cardValue)
					{
						case 1:
						case 2:
						case 3:
						case 4:
						case 5:
						case 6:
						default:
							yield return StartCoroutine(champion.Heal(5));
							champion.heartsBeforeExhaustion--;
							Discard(card);
							StartCoroutine(BotCardLogic(champion));
							yield break;
						case 7:
						case 8:
						case 9:
							if (champion.heartsBeforeExhaustion - 2 < 0)
							{
								GameController.instance.playerActionTooltip.text = "Cannot play this card! The " + champion.name + " will be exhausted. Skipping...";
								continue;
							}
							yield return StartCoroutine(champion.Heal(10));
							champion.heartsBeforeExhaustion -= 2;
							Discard(card);
							StartCoroutine(BotCardLogic(champion));
							yield break;
						case 10:
						case 11:
						case 12:
							if (champion.heartsBeforeExhaustion - 3 < 0)
							{
								GameController.instance.playerActionTooltip.text = "Cannot play this card! The " + champion.name + " will be exhausted. Skipping...";
								continue;
							}
							yield return StartCoroutine(champion.Heal(20));
							champion.heartsBeforeExhaustion -= 3;
							Discard(card);
							StartCoroutine(BotCardLogic(champion));
							yield break;
						case 13:
							if (champion.heartsBeforeExhaustion - 3 < 0)
							{
								GameController.instance.playerActionTooltip.text = "Cannot play this card! The " + champion.name + " will be exhausted. Skipping...";
								continue;
							}
							yield return StartCoroutine(champion.Heal(40));
							champion.heartsBeforeExhaustion -= 3;
							Discard(card);
							StartCoroutine(BotCardLogic(champion));
							yield break;
					}
				default:
					break;
			}
		}

		yield return new WaitForSeconds(Random.Range(0.2f, 1f));

		GameController.instance.StartEndPhase(champion);
	}

	public IEnumerator Deal(Hand hand, int amount = 4, bool flip = false, bool animate = true)
	{
		for (int x = 0; x < amount; x++)
		{
			Card card = Instantiate(GameController.instance.cardIndex.playingCards[Random.Range(0, GameController.instance.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();
			card.transform.SetParent(hand.transform, false);
			if (flip) card.ToggleCardVisibility();
			if (animate)
			{
				card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				StartCoroutine(card.GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
			}
			yield return new WaitForSeconds(0.25f);
		}
	}
	public void Discard(Card card, bool flip = false, bool animate = true)
	{
		card.transform.SetParent(GameController.instance.discardArea.transform, false);
		if (flip) card.ToggleCardVisibility();
		if (!animate) return;
		card.transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
		StartCoroutine(card.GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
	}
	public IEnumerator CombatCalculation(ChampionController attacker, ChampionController defender)
	{
		if (attacker.attackingCard == null)
		{
			Debug.LogError("No attacking card was specified on an initiated attack!");
			yield break;
		}
		defender.currentlyTargeted = true;
		defender.hasDefended = false;

		switch (attacker.isPlayer)
		{
			case true:
				Discard(attacker.attackingCard);
				break;
			case false:
				Discard(attacker.attackingCard, true);
				break;
		}
		switch (defender.isPlayer)
		{
			case true:
				GameController.instance.playerActionTooltip.text = "The " + attacker.name + " is attacking the " + defender.name + ". Defend with a card.";

				yield return new WaitUntil(() => defender.hasDefended && defender.defendingCard != null);
				break;
			case false:
				defender.defendingCard = defender.hand.GetCard("Defense");
				if (defender.defendingCard == null || Random.Range(0f, 1f) < 0.15f && defender.currentHP - attacker.attackDamage > 0) defender.defendingCard = Instantiate(GameController.instance.cardIndex.playingCards[Random.Range(0, GameController.instance.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();
				break;
		}

		if (!defender.isPlayer)
		{
			yield return new WaitForSeconds(Random.Range(0.5f, 3f));
			Discard(defender.defendingCard);
		}
		else
		{
			Discard(defender.defendingCard);
			defender.defendingCard.ToggleCardVisibility();
		}
		attacker.attackingCard.ToggleCardVisibility(true);

		if (attacker.attackingCard.cardValue > defender.defendingCard.cardValue) yield return StartCoroutine(attacker.Attack(defender));
		else if (attacker.attackingCard.cardValue < defender.defendingCard.cardValue) yield return StartCoroutine(attacker.Damage(defender.attackDamage, defender.attackDamageType, defender));
		else
		{
			Debug.Log("lol it tie");
			AudioController.instance.Play("SwordClashing");
		}
		Debug.Log(attacker.name + attacker.currentHP);
		Debug.Log(defender.name + defender.currentHP);

		GameController.instance.endTurnButton.gameObject.SetActive(true);
		GameController.instance.playerActionTooltip.text = "The " + attacker.name + "'s Turn: Action Phase";
		attacker.isAttacking = false;
		attacker.attackingCard = null;
		attacker.currentTarget = null;
		defender.currentlyTargeted = false;
		defender.hasDefended = false;
		defender.defendingCard = null;

		if (!attacker.isPlayer) StartCoroutine(BotCardLogic(attacker));

		yield break;
	}
	private IEnumerator SpadeLogic(Card card, ChampionController champion)
	{
		if (champion.spadesBeforeExhaustion <= 0)
		{
			GameController.instance.playerActionTooltip.text = "The" + champion.name + " cannot play any more SPADES! Choose another card.";
			yield break;
		}

		switch (champion.isPlayer)
		{
			case true:
				GameController.instance.endTurnButton.gameObject.SetActive(false);
				GameController.instance.playerActionTooltip.text = "Choose another card to represent your attack, or choose a target.";
				champion.isAttacking = true;
				champion.spadesBeforeExhaustion--;
				Discard(card);

				yield return new WaitUntil(() => champion.attackingCard != null && champion.currentTarget != null);
				GameController.instance.confirmButton.gameObject.SetActive(true);
				break;
			case false:
				bool gambled = false;

				if (card.cardValue > 10 && Random.Range(0f, 1f) < 0.9f)
				{
					Debug.Log("The " + champion.name + " refuses to use a SPADE worth: " + card.cardValue);
					yield break;
				}

				// Targeting Champion
				foreach (ChampionController targetChampion in GameController.instance.champions)
				{
					if (targetChampion == champion || targetChampion.isDead || targetChampion.team == champion.team) continue;

					float chance = targetChampion.hand.transform.childCount <= 3 ? 1f : 0.75f;
					if ((targetChampion.currentHP - champion.attackDamage <= 0 && Random.Range(0f, 1f) < chance) || targetChampion == champion.currentNemesis)
					{
						champion.currentTarget = targetChampion;
						break;
					}

					chance = targetChampion.isPlayer ? 0.65f : 0.5f;
					if (champion.currentHP >= 0.75f * champion.maxHP && Random.Range(0f, 1f) < chance)
					{
						champion.currentTarget = targetChampion;
						break;
					}
				}
				if (champion.currentTarget == null)
				{
					Debug.Log("The " + champion.name + " decides not to attack!");
					champion.spadesBeforeExhaustion--;
					break;
				}

				// Attacking Card
				if (champion.hand.transform.childCount - 1 == 0 || (champion.currentTarget.hand.transform.childCount == 0 && Random.Range(0f, 1f) < 0.75f))
				{
					champion.attackingCard = Instantiate(GameController.instance.cardIndex.playingCards[Random.Range(0, GameController.instance.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();
					gambled = true;
				}
				else
				{
					champion.attackingCard = champion.hand.GetAttackingCard(card);
				}

				// Reviewing Choices
				float f = champion.currentTarget.currentHP <= 0.25f * champion.currentTarget.maxHP ? 0.8f : 0.5f;
				if ((champion.attackingCard.cardValue <= card.cardValue ||
					(card.cardValue >= 9 || champion.attackingCard.cardValue <= 9 || champion.hand.transform.childCount <= 2) && Random.Range(0f, 1f) < f)
					&& !gambled)
				{
					Debug.Log("The " + champion.name + " does not want to attack with the current configuration!");
					champion.attackingCard = null;
					break;
				}

				// Confirming Attack
				Discard(card);
				champion.isAttacking = true;
				champion.spadesBeforeExhaustion--;

				Debug.Log("The " + champion.name + " is attacking " + champion.currentTarget.name + " with a card with a value of " + champion.attackingCard.cardValue);

				yield return StartCoroutine(CombatCalculation(champion, champion.currentTarget));
				break;
		}
	}
	private IEnumerator ClubLogic(Card card, ChampionController champion)
	{
		champion.hand.Deal(1);
		Discard(card);

		yield break;
	}
	private IEnumerator DiamondLogic(Card card, ChampionController champion)
	{
		if (champion.diamondsBeforeExhaustion <= 0 && (card.cardValue < 5 || card.cardValue > 8))
		{
			GameController.instance.playerActionTooltip.text = "The " + champion.name + " cannot play more DIAMONDS! Choose another card.";
		}

		switch (card.cardValue)
		{
			case 1:
				foreach (ChampionController selectedChampion in GameController.instance.champions)
				{
					selectedChampion.hand.Deal(2);
				}
				champion.diamondsBeforeExhaustion--;
				Discard(card);
				break;
			case 2:
				champion.diamondsBeforeExhaustion--;
				Discard(card);

				foreach (ChampionController selectedChampion in GameController.instance.champions)
				{
					if (selectedChampion.hand.transform.childCount == 0 || selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.isPlayer)
					{
						selectedChampion.discardAmount = 1;
						GameController.instance.playerActionTooltip.text = "Please discard " + selectedChampion.discardAmount + ".";

						yield return new WaitUntil(() => selectedChampion.discardAmount == 0);

						continue;
					}

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.name + ".";

					selectedChampion.discardAmount = 1;

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					for (int discarded = 0; discarded < selectedChampion.discardAmount; discarded++) Discard(selectedChampion.hand.GetCard("Lowest"));

					selectedChampion.discardAmount = 0;
				}
				break;
			case 3:
				foreach (ChampionController selectedChampion in GameController.instance.champions)
				{
					selectedChampion.hand.Deal(4);
				}
				champion.diamondsBeforeExhaustion--;
				Discard(card);
				break;
			case 4:
				bool jeopardized = false;
				foreach (ChampionController quickSelectChampion in GameController.instance.champions)
				{
					if (quickSelectChampion.team != champion.team || quickSelectChampion == champion || quickSelectChampion.isDead) continue;

					if (quickSelectChampion.currentHP - 20 <= 0)
					{
						Debug.Log("The " + champion.name + " does not want to jeopardize his teammate, " + quickSelectChampion.name + "!");
						jeopardized = true;
					}
				}
				if (jeopardized) break;

				champion.diamondsBeforeExhaustion--;
				Discard(card);

				foreach (ChampionController selectedChampion in GameController.instance.champions)
				{
					if (selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.hand.transform.childCount == 0)
					{
						Debug.Log("The " + selectedChampion.name + " has no cards! Dealing damage automatically...");
						yield return StartCoroutine(selectedChampion.Damage(20, DamageType.Unblockable, champion));
						continue;
					}

					if (selectedChampion.isPlayer)
					{
						selectedChampion.discardAmount = Mathf.Min(champion.hand.transform.childCount, 2);
						GameController.instance.playerActionTooltip.text = "Please discard " + selectedChampion.discardAmount + ".";
						GameController.instance.confirmButton.gameObject.SetActive(true);
						GameController.instance.confirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Skip";

						yield return new WaitUntil(() => selectedChampion.discardAmount <= 0);

						if (selectedChampion.discardAmount == -1)
						{
							yield return StartCoroutine(selectedChampion.Damage(20, DamageType.Unblockable, champion));
							selectedChampion.discardAmount = 0;
							GameController.instance.confirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Confirm";
						}

						continue;
					}

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.name + ".";

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					float chance = selectedChampion.currentHP >= 0.75f * selectedChampion.maxHP ? 0.75f : 0.5f;
					if (Random.Range(0f, 1f) < chance && selectedChampion.currentHP - 20 > 0 || selectedChampion.hand.transform.childCount == 0)
					{
						yield return StartCoroutine(selectedChampion.Damage(20, DamageType.Unblockable, champion));
						continue;
					}

					selectedChampion.discardAmount = Mathf.Min(selectedChampion.hand.transform.childCount, 2);

					for (int discarded = 0; discarded < selectedChampion.discardAmount; discarded++) Discard(selectedChampion.hand.GetCard("Lowest"));

					selectedChampion.discardAmount = 0;
				}
				break;
			case 5:
			case 6:
			case 7:
			case 8:
				champion.hand.Deal(1);
				Discard(card);
				break;
			case 9:
				if (champion.currentHP >= 0.8f * champion.maxHP && (champion.currentHP == champion.maxHP || Random.Range(0f, 1f) < 0.75f)) break;
				foreach (ChampionController selectedChampion in GameController.instance.champions)
				{
					yield return StartCoroutine(selectedChampion.Heal(10));
				}
				champion.diamondsBeforeExhaustion--;
				Discard(card);
				break;
			case 10:
				if (champion.currentHP >= 0.8f * champion.maxHP && (champion.currentHP == champion.maxHP || Random.Range(0f, 1f) < 0.9f)) break;
				foreach (ChampionController selectedChampion in GameController.instance.champions)
				{
					yield return StartCoroutine(selectedChampion.Heal(20));
				}
				champion.diamondsBeforeExhaustion--;
				Discard(card);
				break;
			case 11:
				champion.diamondsBeforeExhaustion--;
				Discard(card);

				foreach (ChampionController selectedChampion in GameController.instance.champions)
				{
					if (selectedChampion == champion || selectedChampion.isDead) continue;

					yield return StartCoroutine(selectedChampion.Damage(20, DamageType.Fire, champion));

					yield return new WaitForSeconds(1f);
				}
				break;
			case 12:
				champion.diamondsBeforeExhaustion--;
				Discard(card);

				foreach (ChampionController selectedChampion in GameController.instance.champions)
				{
					if (selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.hand.transform.childCount == 0)
					{
						Debug.Log("The " + selectedChampion.name + " has no cards! Dealing damage automatically...");
						yield return StartCoroutine(selectedChampion.Damage(40, DamageType.Fire, champion));
						continue;
					}

					if (selectedChampion.isPlayer)
					{
						selectedChampion.discardAmount = Mathf.Min(champion.hand.transform.childCount, 4);
						GameController.instance.playerActionTooltip.text = "Please discard " + selectedChampion.discardAmount + ".";
						GameController.instance.confirmButton.gameObject.SetActive(true);
						GameController.instance.confirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Skip";

						yield return new WaitUntil(() => selectedChampion.discardAmount <= 0);

						if (selectedChampion.discardAmount == -1)
						{
							yield return StartCoroutine(selectedChampion.Damage(40, DamageType.Fire, champion));
							selectedChampion.discardAmount = 0;
							GameController.instance.confirmButton.transform.GetChild(0).GetComponent<TMP_Text>().text = "Confirm";
						}

						continue;
					}

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.name + ".";

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					float chance = selectedChampion.currentHP >= 0.75f * selectedChampion.maxHP ? 0.5f : 0.15f;
					if (Random.Range(0f, 1f) < chance && selectedChampion.currentHP - 40 > 0 || selectedChampion.hand.transform.childCount == 0)
					{
						yield return StartCoroutine(selectedChampion.Damage(40, DamageType.Fire, champion));
						continue;
					}

					selectedChampion.discardAmount = Mathf.Min(selectedChampion.hand.transform.childCount, 4);

					for (int discarded = 0; discarded < selectedChampion.discardAmount; discarded++) Discard(selectedChampion.hand.GetCard("Lowest"));

					selectedChampion.discardAmount = 0;
				}
				break;
			default:
				Debug.Log("Not implemented yet. Skipping...");
				break;
		}

		if (!champion.isPlayer) StartCoroutine(BotCardLogic(champion));
	}

	[System.Obsolete("No longer used to calculate combat. Use 'CombatCalculation()' instead.")]
	public IEnumerator CombatCalc(ChampionController attacker, ChampionController defender)
	{
		if (attacker.isPlayer && !defender.isPlayer)
		{
			if (attacker.attackingCard == null)
			{
				Debug.LogError("No attacking card was specified on a player-initiated attack!");
				yield break;
			}
			GameController.instance.playerActionTooltip.text = "Waiting for the opponent...";

			defender.defendingCard = defender.hand.GetCard("Defense");
			if (defender.defendingCard == null || Random.Range(0f, 1f) < 0.15f && defender.currentHP - attacker.attackDamage > 0) defender.defendingCard = Instantiate(GameController.instance.cardIndex.playingCards[Random.Range(0, GameController.instance.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();

			Discard(attacker.attackingCard);
			yield return new WaitForSeconds(Random.Range(0.5f, 3f));
			attacker.attackingCard.ToggleCardVisibility();
			Discard(defender.defendingCard);
			defender.hasDefended = true;

			if (attacker.attackingCard.cardValue > defender.defendingCard.cardValue)
			{
				attacker.Attack(defender);
			}
			else if (attacker.attackingCard.cardValue < defender.defendingCard.cardValue)
			{
				attacker.Damage(defender.attackDamage, defender.attackDamageType, defender);
			}
			else
			{
				Debug.Log("lol it tie");
				AudioController.instance.Play("SwordClashing");
			}
			Debug.Log(attacker.name + attacker.currentHP);
			Debug.Log(defender.name + defender.currentHP);

			GameController.instance.endTurnButton.gameObject.SetActive(true);
			GameController.instance.playerActionTooltip.text = "The " + attacker.name + "'s Turn: Action Phase";
			attacker.isAttacking = false;
			attacker.attackingCard = null;
			attacker.currentTarget = null;
			defender.currentlyTargeted = false;
			defender.hasDefended = false;
			defender.defendingCard = null;

			yield break;
		}

		if (!attacker.isPlayer)
		{
			Discard(attacker.attackingCard, true);
			defender.currentlyTargeted = true;
			switch (defender.isPlayer)
			{
				case true:
					GameController.instance.playerActionTooltip.text = "The " + attacker.name + " is attacking the " + defender.name + ". Defend with a card.";

					yield return new WaitUntil(() => defender.hasDefended && defender.defendingCard != null);

					Discard(defender.defendingCard, true);

					if (attacker.attackingCard.cardValue > defender.defendingCard.cardValue)
					{
						attacker.Attack(defender);
					}
					else if (attacker.attackingCard.cardValue < defender.defendingCard.cardValue)
					{
						attacker.Damage(defender.attackDamage, defender.attackDamageType, defender);
					}
					else
					{
						Debug.Log("lol it tie");
						AudioController.instance.Play("SwordClashing");
					}
					Debug.Log(attacker.name + attacker.currentHP);
					Debug.Log(defender.name + defender.currentHP);

					attacker.isAttacking = false;
					attacker.attackingCard = null;
					attacker.currentTarget = null;
					defender.currentlyTargeted = false;
					defender.hasDefended = false;
					defender.defendingCard = null;
					StartCoroutine(BotCardLogic(attacker));
					yield break;
				case false:
					break;
			}
		}
	}
	[System.Obsolete("No longer used. Use 'SpadeLogic()' instead.")]
	private IEnumerator PlayerSpade(Card card, ChampionController player)
	{
		if (player.spadesBeforeExhaustion <= 0)
		{
			GameController.instance.playerActionTooltip.text = "You cannot play any more SPADES! Choose another card.";
			yield break;
		}

		GameController.instance.endTurnButton.gameObject.SetActive(false);
		GameController.instance.playerActionTooltip.text = "Choose another card to represent your attack, or choose a target.";
		player.isAttacking = true;
		player.spadesBeforeExhaustion--;
		Discard(card);

		yield return new WaitUntil(() => player.attackingCard != null && player.currentTarget != null);
		GameController.instance.confirmButton.gameObject.SetActive(true);
	}
	private void PlayerHeart(Card card, ChampionController player)
	{
		if (player.heartsBeforeExhaustion <= 0)
		{
			GameController.instance.playerActionTooltip.text = "You cannot play any more HEARTS! Choose another card.";
			return;
		}
		if (player.currentHP >= player.maxHP)
		{
			GameController.instance.playerActionTooltip.text = "Health is full! Choose another card.";
			return;
		}

		switch (card.cardValue)
		{
			case 1:
			case 2:
			case 3:
			case 4:
			case 5:
			case 6:
			default:
				player.Heal(5);
				player.heartsBeforeExhaustion--;
				Discard(card);
				break;
			case 7:
			case 8:
			case 9:
				if (player.heartsBeforeExhaustion - 2 < 0)
				{
					GameController.instance.playerActionTooltip.text = "You will be exhausted! Choose another card.";
					break;
				}
				player.Heal(10);
				player.heartsBeforeExhaustion -= 2;
				Discard(card);
				break;
			case 10:
			case 11:
			case 12:
				if (player.heartsBeforeExhaustion - 3 < 0)
				{
					GameController.instance.playerActionTooltip.text = "You will be exhausted! Choose another card.";
					break;
				}
				player.Heal(20);
				player.heartsBeforeExhaustion -= 3;
				Discard(card);
				break;
			case 13:
				if (player.heartsBeforeExhaustion - 3 < 0)
				{
					GameController.instance.playerActionTooltip.text = "You will be exhausted! Choose another card.";
					break;
				}
				player.Heal(40);
				player.heartsBeforeExhaustion -= 3;
				Discard(card);
				break;
		}
	}
	[System.Obsolete("No longer used. Use 'ClubLogic()' instead.")]
	private void PlayerClub(Card card, ChampionController player)
	{
		player.hand.Deal(1);
		Discard(card);
	}
	[System.Obsolete("No longer used. Use 'DiamondLogic()' instead.")]
	private IEnumerator PlayerDiamond(Card card, ChampionController player)
	{
		if ((card.cardValue < 5 || card.cardValue > 8) && player.diamondsBeforeExhaustion <= 0)
		{
			GameController.instance.playerActionTooltip.text = "You cannot play more DIAMONDS! Choose another card.";
			yield break;
		}

		switch (card.cardValue)
		{
			case 1:
				foreach (ChampionController champion in GameController.instance.champions)
				{
					champion.hand.Deal(2);
				}
				player.diamondsBeforeExhaustion--;
				Discard(card);
				break;
			case 2:
				GameController.instance.endTurnButton.gameObject.SetActive(false);
				player.diamondsBeforeExhaustion--;
				Discard(card);

				foreach (ChampionController champion in GameController.instance.champions)
				{
					if (champion.hand.transform.childCount == 0 || champion == player) continue;

					GameController.instance.playerActionTooltip.text = "Waiting for " + champion.name + ".";

					champion.discardAmount = 1;

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					for (int discarded = 0; discarded < champion.discardAmount; discarded++)
					{
						Card selectedCard = null;
						int value = 999;
						foreach (Transform child in champion.hand.transform)
						{
							if (child.GetComponent<Card>().cardValue < value) 
							{
								selectedCard = child.GetComponent<Card>();
								value = selectedCard.cardValue;
							}
						}
						Discard(selectedCard);
					}

					champion.discardAmount = 0;
				}

				GameController.instance.endTurnButton.gameObject.SetActive(true);
				GameController.instance.playerActionTooltip.text = "The " + player.name + "'s Turn: Action Phase";

				break;
			case 3:
				foreach (ChampionController champion in GameController.instance.champions)
				{
					champion.hand.Deal(4);
				}
				player.diamondsBeforeExhaustion--;
				Discard(card);
				break;
			case 4:
				GameController.instance.endTurnButton.gameObject.SetActive(false);
				player.diamondsBeforeExhaustion--;
				Discard(card);

				foreach (ChampionController champion in GameController.instance.champions)
				{
					if (champion == player) continue;

					GameController.instance.playerActionTooltip.text = "Waiting for " + champion.name + ".";

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					float chance = champion.currentHP >= 0.75f * champion.maxHP ? 0.75f : 0.5f;
					if (Random.Range(0f, 1f) < chance && champion.currentHP - 20 > 0 || champion.hand.transform.childCount == 0)
					{
						champion.Damage(20, DamageType.Unblockable, player);
						continue;
					}

					champion.discardAmount = Mathf.Min(champion.hand.transform.childCount, 2);
					Debug.Log(champion.discardAmount);

					for (int discarded = 0; discarded < champion.discardAmount; discarded++)
					{
						Card selectedCard = null;
						int value = 999;
						foreach (Transform child in champion.hand.transform)
						{
							if (child.GetComponent<Card>().cardValue < value)
							{
								selectedCard = child.GetComponent<Card>();
								value = selectedCard.cardValue;
							}
						}
						Discard(selectedCard);
						Debug.Log(champion.discardAmount);
					}

					champion.discardAmount = 0;
				}

				GameController.instance.endTurnButton.gameObject.SetActive(true);
				GameController.instance.playerActionTooltip.text = "The " + player.name + "'s Turn: Action Phase";

				break;
			case 5:
			case 6:
			case 7:
			case 8:
				player.hand.Deal(1);
				Discard(card);
				break;
			case 9:
				foreach (ChampionController champion in GameController.instance.champions)
				{
					champion.Heal(10);
				}
				player.diamondsBeforeExhaustion--;
				Discard(card);
				break;
			case 10:
				foreach (ChampionController champion in GameController.instance.champions)
				{
					champion.Heal(20);
				}
				player.diamondsBeforeExhaustion--;
				Discard(card);
				break;
			case 11:
				player.diamondsBeforeExhaustion--;
				Discard(card);

				foreach (ChampionController champion in GameController.instance.champions)
				{
					if (champion == player) continue;

					champion.Damage(20, DamageType.Fire, player);

					yield return new WaitForSeconds(1f);
				}
				break;
			case 12:
				GameController.instance.endTurnButton.gameObject.SetActive(false);
				player.diamondsBeforeExhaustion--;
				Discard(card);

				foreach (ChampionController champion in GameController.instance.champions)
				{
					if (champion == player) continue;

					GameController.instance.playerActionTooltip.text = "Waiting for " + champion.name + ".";

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					float chance = champion.currentHP >= 0.75f * champion.maxHP ? 0.5f : 0.15f;
					if (Random.Range(0f, 1f) < chance && champion.currentHP - 40 > 0 || champion.hand.transform.childCount == 0)
					{
						champion.Damage(40, DamageType.Fire, player);
						continue;
					}

					champion.discardAmount = Mathf.Min(champion.hand.transform.childCount, 4);

					for (int discarded = 0; discarded < champion.discardAmount; discarded++)
					{
						Card selectedCard = null;
						int value = 999;
						foreach (Transform child in champion.hand.transform)
						{
							if (child.GetComponent<Card>().cardValue < value)
							{
								selectedCard = child.GetComponent<Card>();
								value = selectedCard.cardValue;
							}
						}
						Discard(selectedCard);
					}

					champion.discardAmount = 0;
				}

				GameController.instance.endTurnButton.gameObject.SetActive(true);
				GameController.instance.playerActionTooltip.text = "The " + player.name + "'s Turn: Action Phase";

				break;
			default:
				break;
		}
	}
	private void PlayerDiscard(Card card, ChampionController player, string type = "Normal")
	{
		switch (type)
		{
			case "Normal":
				
				Discard(card);
				player.discardAmount--;

				switch (player.discardAmount)
				{
					case 0:
						GameController.instance.NextTurnCalculator(player);
						break;
					default:
						GameController.instance.playerActionTooltip.text = "Please discard " + player.discardAmount + ".";
						break;
				}
				break;
			case "Forced":

				Discard(card);
				player.discardAmount--;

				if (player.discardAmount != 0)
				{
					GameController.instance.playerActionTooltip.text = "Please discard " + player.discardAmount + ".";
				}

				break;
		}
	}
}
