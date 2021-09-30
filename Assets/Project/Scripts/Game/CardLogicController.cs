using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;
using EZCameraShake;

public abstract class CardLogicController : MonoBehaviour {
	public static CardLogicController instance;

	public bool currentlyHandlingCard;

	[SerializeField]
	protected CardScriptableObject summonCard; // debugging
	[SerializeField]
	protected int dealToIndex; // debugging

	protected virtual void Awake() {
		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
	}
	protected virtual void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha5)) {
			StartCoroutine(GameController.instance.champions[dealToIndex].hand.Deal(summonCard));
		}
	}

	/// <summary>
	/// Called when the player clicks on a card.
	/// </summary>
	/// <param name="card"></param>
	/// <returns></returns>
	public virtual IEnumerator CardSelect(Card card) {
		foreach (ChampionController player in GameController.instance.champions) {
			if (!player.isPlayer || player.isDead) continue;

			// Exits if the card is not the player's.
			if (card.owner is null) {
				TooltipSystem.instance.ShowError("This is not your card!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
			}
			if (!card.owner.isPlayer) {
				TooltipSystem.instance.ShowError("This is not your card!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
			}

			switch (GameController.instance.gamePhase) {
				case GamePhase.BeginningPhase:
					TooltipSystem.instance.ShowError(player.isMyTurn ? "You cannot play a card during the Beginning Phase!" : "It is not your turn!");
					LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					break;
				case GamePhase.ActionPhase:
					if (currentlyHandlingCard) {
						Debug.Log("Cannot play! Currently handling another card.");
						yield break;
					}

					// If it's the player's turn
					switch (player.isMyTurn) {
						case true:
							// When Attacking
							if (player.isAttacking && !player.hasAttacked) {
								if (GameController.instance.gambleButton.isBlocking) {
									TooltipSystem.instance.ShowError("You cannot select another combat card after gambling!");
									LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
									yield break;
								}
								if (card == player.hand.queued.Peek()) {
									TooltipSystem.instance.ShowError("You cannot select the same card used for starting the attack as a combat card!");
									LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
									yield break;
								}
								if (player.attackingCard is {}) {
									player.attackingCard.redGlow.Stop();
									player.attackingCard.redGlow.Clear();
								}
								player.attackingCard = card;
								card.redGlow.Stop();
								card.redGlow.Play();
								GameController.instance.gambleButton.Hide();

								if (player.currentTarget is {}) {
									GameController.instance.confirmButton.Show();
									GameController.instance.confirmButton.textBox.text = "Confirm";
								}
								yield break;
							}
							
							// When Used Normally
							switch (card.cardScriptableObject.cardSuit) {
								case CardSuit.SPADE:
									StartCoroutine(SpadeLogic(card, player));
									yield break;
								case CardSuit.HEART:
									StartCoroutine(HeartLogic(card, player));
									yield break;
								case CardSuit.CLUB:
									StartCoroutine(ClubLogic(card, player));
									yield break;
								case CardSuit.DIAMOND:
									StartCoroutine(DiamondLogic(card, player));
									yield break;
							}
							break;
						case false:
							// When Defense
							foreach (ChampionController champion in GameController.instance.champions) {
								if (champion.currentTarget != player || !champion.isAttacking) continue;

								if (player.defendingCard is {}) player.defendingCard.Flip(true);
								player.defendingCard = card;
								card.Flip(true);

								GameController.instance.playerActionTooltip.text = "Confirm the defense, or change selected card.";
								GameController.instance.confirmButton.Show();
								GameController.instance.confirmButton.textBox.text = "Confirm";
								yield break;
							}
							break;
					}

					// When Forced to Discard
					if (player.discardAmount > 0) {
						yield return StartCoroutine(player.hand.Discard(card));
						
						player.discardAmount--;
						card.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
						card.discardFeed.text = "DISCARDED";
						GameController.instance.confirmButton.Hide();

						if (player.discardAmount != 0) {
							GameController.instance.playerActionTooltip.text = "Please discard " + player.discardAmount + ".";
						}
						else {

							GameController.instance.playerActionTooltip.text = string.Empty;
						}
					}
					// If all else fails, stops the player from using the card.
					else {
						TooltipSystem.instance.ShowError("It is not your turn!");
						LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					}
					break;
				case GamePhase.EndPhase:
					switch (player.isMyTurn) {
						case true:
							// When Discarding Naturally
							if (player.discardAmount > 0) {
								yield return StartCoroutine(player.hand.Discard(card));
								
								player.discardAmount--;
								card.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
								card.discardFeed.text = "DISCARDED";
								GameController.instance.confirmButton.Hide();

								if (player.discardAmount != 0) {
									GameController.instance.playerActionTooltip.text = "Please discard " + player.discardAmount + ".";
								}
								else {

									GameController.instance.playerActionTooltip.text = string.Empty;
								}
							}
							yield break;
						case false:
							TooltipSystem.instance.ShowError("It is not your turn!");
							LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
							yield break;
					}
			}
		}
	}
	/// <summary>
	/// Called to handle the bot's card logic.
	/// The perceived coherence of the bot will be determined by the difficulty.
	/// </summary>
	/// <param name="champion"></param>
	/// <returns></returns>
	/// <exception cref="ArgumentOutOfRangeException"></exception>
	public virtual IEnumerator BotCardLogic(ChampionController champion) {
		float PauseDuration() {
			// Sets the bot's pause duration to simulate logical breakdown.
			switch (GameController.instance.difficulty) {
				case GameController.Difficulty.Noob:
				case GameController.Difficulty.Novice:
					return Random.Range(2f, 4f);
				case GameController.Difficulty.Warrior:
					return Random.Range(1f, 3f);
				case GameController.Difficulty.Champion:
					return Random.Range(0.4f, 1.25f);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}
		float CardScanDuration() {
			// Sets the bot's scan duration to simulate the logical scanning of the next card to play.
			switch (GameController.instance.difficulty) {
				case GameController.Difficulty.Noob:
				case GameController.Difficulty.Novice:
					return 2.75f;
				case GameController.Difficulty.Warrior:
					return Random.Range(0.75f, 2f);
				case GameController.Difficulty.Champion:
					return Random.Range(0.25f, 1f);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		if (champion.isDead) {
			// Just in case, to handle if logic is called on dead champion.
			Debug.LogWarning("Attempted to apply logic to dead champion!");
			GameController.instance.NextTurnCalculator(champion);
			yield break;
		}

		yield return new WaitForSeconds(PauseDuration());

		// Clubs
		foreach (Transform child in champion.hand.transform) {
			Card card = child.GetComponent<Card>();
			if (card.cardScriptableObject.cardSuit != CardSuit.CLUB) continue;

			switch (GameController.instance.difficulty) {
				case GameController.Difficulty.Noob:
				case GameController.Difficulty.Novice:
					break;
				default:
					if (card.cardScriptableObject.cardValue > 10 && Random.Range(0f, 1f) < 0.9f) {
						Debug.Log(champion.championName + " refuses to trade in a CLUB worth: " + card.cardScriptableObject.cardValue);
						continue;
					}
					break;
			}

			yield return StartCoroutine(ClubLogic(card, champion));
			yield return new WaitForSeconds(CardScanDuration());
		}
		yield return new WaitForSeconds(PauseDuration());

		// Diamonds
		foreach (Transform child in champion.hand.transform) {
			Card card = child.GetComponent<Card>();
			if (card.cardScriptableObject.cardSuit != CardSuit.DIAMOND) continue;
			if (champion.diamondsBeforeExhaustion == 0 && (card.cardScriptableObject.cardValue < 5 || card.cardScriptableObject.cardValue > 8)) {
				Debug.Log(champion.championName + " can't play this DIAMOND.");
				break;
			}

			yield return StartCoroutine(DiamondLogic(card, champion));
			yield return new WaitForSeconds(CardScanDuration());
		}
		yield return new WaitForSeconds(PauseDuration());

		// Spades
		foreach (Transform child in champion.hand.transform) {
			Card card = child.GetComponent<Card>();
			if (card.cardScriptableObject.cardSuit != CardSuit.SPADE) continue;
			if (champion.spadesBeforeExhaustion == 0) {
				Debug.Log(champion.championName + " is exhausted. Cannot attack.");
				break;
			}

			bool wontAttack = false;
			switch (GameController.instance.difficulty) {
				case GameController.Difficulty.Noob:
					break;
				case GameController.Difficulty.Novice:
					if (champion.currentHP <= 0.3f * champion.maxHP && Random.Range(0f, 1f) < 0.25f) {
						Debug.Log(champion.championName + " realizes that they might fuck up and die!");
						champion.spadesBeforeExhaustion--;
						wontAttack = true;
					}
					break;
				case GameController.Difficulty.Warrior:
					if (champion.currentHP <= 0.2f * champion.maxHP && Random.Range(0f, 1f) < 0.45f) {
						Debug.Log(champion.championName + " doesn't want to attack!");
						champion.spadesBeforeExhaustion--;
						wontAttack = true;
					}
					break;
				case GameController.Difficulty.Champion:
					if ((champion.currentHP <= 0.2f * champion.maxHP && Random.Range(0f, 1f) < 0.65f) || Random.Range(0f, 1f) < 0.15f) {
						Debug.Log(champion.championName + " doesn't want to attack!");
						champion.spadesBeforeExhaustion--;
						wontAttack = true;
					}
					break;
			}
			if (wontAttack) break;

			yield return StartCoroutine(SpadeLogic(card, champion));
			yield return new WaitForSeconds(CardScanDuration());
		}
		yield return new WaitForSeconds(PauseDuration());

		// Hearts
		foreach (Transform child in champion.hand.transform) {
			Card card = child.GetComponent<Card>();
			if (champion.currentHP == champion.maxHP) break;
			if (champion.heartsBeforeExhaustion == 0) break;
			if (card.cardScriptableObject.cardSuit != CardSuit.HEART) continue;

			switch (GameController.instance.difficulty) {
				case GameController.Difficulty.Champion:
					if (champion.currentHP + 20 >= 0.9f * champion.maxHP && card.cardScriptableObject.cardValue == 13) {
						Debug.Log("Health would be clamped! The " + champion.championName + " decides not to use an ACE of HEARTS to heal!");
						continue;
					}
					break;
			}

			yield return StartCoroutine(HeartLogic(card, champion));
			yield return new WaitForSeconds(CardScanDuration());
		}
		yield return new WaitForSeconds(PauseDuration());

		GameController.instance.StartEndPhase(champion);
	}

	/// <summary>
	/// Calculates the result of combat, as well as who should take damage and checking for activated abilities.
	/// Do not call this randomly, as this could very well break code if run at the wrong time.
	/// </summary>
	/// <param name="attacker"></param>
	/// <param name="defender"></param>
	/// <param name="abilityCheck"></param>
	/// <returns></returns>
	public virtual IEnumerator CombatCalculation(ChampionController attacker, ChampionController defender, bool abilityCheck = true) {
		if (attacker.attackingCard == null) {
			// Fail safe in case no attackingCard was defined prior to combat calculation.
			Debug.LogError("No attacking card was specified on an initiated attack!");
			yield break;
		}

		// Sets the values to listen for.
		attacker.hasAttacked = true;
		defender.currentlyTargeted = true;
		defender.hasDefended = false;

		// Discarding the attacking card.
		switch (attacker.isPlayer) {
			case true:
				currentlyHandlingCard = true;
				yield return StartCoroutine(attacker.hand.Discard(attacker.attackingCard, true));
				if (GameController.instance.gambleButton.isBlocking) {
					attacker.attackingCard.Flip(true);
					attacker.attackingCard.caption.text = "Gambled by " + attacker.championName;
				}
				break;
			case false:
				yield return StartCoroutine(attacker.hand.Discard(attacker.attackingCard, true));
				attacker.attackingCard.halo.Stop();
				attacker.attackingCard.halo.Play();
				break;
		}
		// Wait for or get the defending card.
		switch (defender.isPlayer) {
			case true:
				GameController.instance.playerActionTooltip.text = attacker.championName + " is attacking the " + defender.championName + ". Defend with a card.";
				GameController.instance.gambleButton.Show();
				yield return new WaitUntil(() => defender.defendingCard != null);
				GameController.instance.gambleButton.Hide();
				yield return new WaitUntil(() => defender.hasDefended);
				break;
			case false:
				defender.defendingCard = defender.hand.GetCard("Defense");
				if (defender.defendingCard == null || Random.Range(0f, 1f) < 0.15f && defender.currentHP - attacker.attackDamage > 0) {
					defender.defendingCard = Instantiate(GameController.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
					defender.defendingCard.cardScriptableObject = GameController.instance.cardIndex.PlayingCards[Random.Range(0, GameController.instance.cardIndex.PlayingCards.Count)];
					defender.defendingCard.caption.text = "Gambled by " + defender.championName;
				}
				break;
		}

		// Discards defending card.
		if (!defender.isPlayer) {
			yield return new WaitForSeconds(Random.Range(0.5f, 3f));
			yield return StartCoroutine(defender.hand.Discard(defender.defendingCard));
		}
		else {
			yield return StartCoroutine(defender.hand.Discard(defender.defendingCard));
			defender.defendingCard.Flip();
		}
		defender.matchStatistic.totalDefends++;
		attacker.attackingCard.Flip(true);

		// CombatCalculation Ability heck
		if (abilityCheck) {
			foreach (Ability ability in attacker.abilities) {
				yield return StartCoroutine(ability.OnCombatCalculationAttacker(attacker.attackingCard, defender.defendingCard));
			}

			foreach (Ability ability in defender.abilities) {
				yield return StartCoroutine(ability.OnCombatCalculationDefender(attacker.attackingCard, defender.defendingCard));
			}
		}

		DamageHistory attackerDamageHistory = null;
		foreach (DamageHistory damageHistory in attacker.matchStatistic.damageHistories) {
			if (damageHistory.dealtAgainst != defender) continue;
			attackerDamageHistory = damageHistory;
		}
		attackerDamageHistory ??= new DamageHistory(defender);
		attacker.matchStatistic.damageHistories.Add(attackerDamageHistory);
		attackerDamageHistory.attacksAgainst++;

		// Calculating Combat Result
		if (attacker.attackingCard.CombatValue > defender.defendingCard.CombatValue) {
			foreach (Ability ability in attacker.abilities) {
				yield return StartCoroutine(ability.OnAttackSuccess(attacker.attackingCard, defender.defendingCard));
			}
			foreach (Ability ability in defender.abilities) {
				yield return StartCoroutine(ability.OnDefenseFailure(attacker.attackingCard, defender.defendingCard));
			}
			
			yield return StartCoroutine(attacker.Attack(defender));

			attacker.matchStatistic.successfulAttacks++;
			defender.matchStatistic.failedDefends++;
		}
		else if (attacker.attackingCard.CombatValue < defender.defendingCard.CombatValue) {
			foreach (Ability ability in attacker.abilities) {
				yield return StartCoroutine(ability.OnAttackFailure(attacker.attackingCard, defender.defendingCard));
			}
			foreach (Ability ability in defender.abilities) {
				yield return StartCoroutine(ability.OnDefenseSuccess(attacker.attackingCard, defender.defendingCard));
			}
			
			yield return StartCoroutine(attacker.Damage(defender.attackDamage, defender.attackDamageType, defender));

			attacker.matchStatistic.failedAttacks++;
			defender.matchStatistic.successfulDefends++;
		}
		else {
			Debug.Log("lol it tie");
			AudioController.instance.Play("swordimpact_fail");
			CameraShaker.Instance.ShakeOnce(1f, 4f, 0.1f, 0.2f);
		}
		Debug.Log(attacker.championName + attacker.currentHP);
		Debug.Log(defender.championName + defender.currentHP);

		yield return new WaitForSeconds(2.5f);

		// Resetting Values to continue game flow.
		currentlyHandlingCard = false;
		GameController.instance.confirmButton.Hide();
		if (attacker.isPlayer) GameController.instance.endTurnButton.gameObject.SetActive(true);

		attacker.isAttacking = false;
		attacker.hasAttacked = false;
		attacker.attackingCard = null;
		attacker.currentTarget = null;
		defender.currentlyTargeted = false;
		defender.hasDefended = false;
		defender.defendingCard = null;
		defender.championParticleController.RedGlow.Stop();
		defender.championParticleController.RedGlow.Clear();
	}
	/// <summary>
	/// Handles card logic for playing cards of type SPADE.
	/// </summary>
	/// <param name="card"></param>
	/// <param name="champion"></param>
	/// <param name="tries"></param>
	/// <returns></returns>
	protected virtual IEnumerator SpadeLogic(Card card, ChampionController champion, int tries = 0) {
		if (champion.spadesBeforeExhaustion <= 0) {
			// Fail-safe to disallow the champion from using the SPADE.
			if (champion.isPlayer) {
				TooltipSystem.instance.ShowError("You cannot play any more SPADES!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			}
			yield break;
		}

		switch (champion.isPlayer) {
			case true:
				// Player Spade Logic

				GameController.instance.endTurnButton.gameObject.SetActive(false);
				GameController.instance.attackCancelButton.Show();
				GameController.instance.gambleButton.Show();
				
				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion.isDead || selectedChampion.team.Contains(champion.team)) continue;
					selectedChampion.championParticleController.PlayEffect(selectedChampion.championParticleController.GreenGlow);
				}
				card.halo.Stop();
				card.halo.Play();
				champion.isAttacking = true;
				champion.hand.queued.Enqueue(card);
				break;
			case false:
				// Bot Spade Logic
				bool gambled = false;

				if (card.cardScriptableObject.cardValue > 10 && Random.Range(0f, 1f) < 0.75f) {
					// Coherence for using a high spade.
					Debug.Log(champion.championName + " refuses to use a SPADE worth: " + card.cardScriptableObject.cardValue);
					yield break;
				}
				if (tries > 3) {
					Debug.Log(champion.championName + "already tried more than 3 times!");
					yield break;
				}

				// Targeting Champion
				if (Random.Range(0f, 1f) < 0.75f && champion.currentNemesis is { isDead: false }) {
					Debug.Log(champion.championName + " is furious! Targeting their nemesis immediately.");
					champion.currentTarget = champion.currentNemesis;
				}
				else {
					foreach (ChampionController targetChampion in GameController.instance.champions) {
						if (targetChampion.isDead) continue;
						
						bool skipThisChampion = false;
						foreach (Ability ability in targetChampion.abilities) {
							if (targetChampion == champion) {
								skipThisChampion = true;
								break;
							}
							skipThisChampion = ability.CanBeTargetedByAttack();
							skipThisChampion = !skipThisChampion;
							if (skipThisChampion) break;
						}
						if (targetChampion.teamMembers.Contains(champion) || targetChampion == champion || skipThisChampion) continue;

						// Low-HP Targeting
						float chance = targetChampion.hand.GetCardCount() <= 3 ? 1f : 0.85f;
						if (targetChampion.currentHP - champion.attackDamage <= 0 && Random.Range(0f, 1f) < chance) {
							champion.currentTarget = targetChampion;
							break;
						}

						// Standard Targeting
						chance = targetChampion.isPlayer ? 0.8f : 0.7f;
						chance += champion.currentHP >= 0.75f * champion.maxHP ? 0.5f : 0f;
						chance += !targetChampion.isPlayer && targetChampion.currentHP - champion.attackDamage <= 0 ? 0.1f : 0f;
						if (Random.Range(0f, 1f) < chance) {
							champion.currentTarget = targetChampion;
							break;
						}
					}
				}
				if (champion.currentTarget is null) {
					Debug.Log(champion.championName + " decides not to attack!");
					champion.spadesBeforeExhaustion--;
					break;
				}

				// Attacking Card
				yield return new WaitForSeconds(Random.Range(0.5f, 1.25f));
				if (champion.hand.GetCardCount() - 1 != 0 || Random.Range(0f, 1f) < 0.85f) {
					champion.attackingCard = champion.hand.GetAttackingCard(card);
				}
				if (champion.attackingCard is null) {
					switch (GameController.instance.difficulty) {
						case GameController.Difficulty.Warrior:
						case GameController.Difficulty.Champion:
							champion.attackingCard = Instantiate(GameController.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
							champion.attackingCard.cardScriptableObject = GameController.instance.cardIndex.PlayingCards[Random.Range(0, GameController.instance.cardIndex.PlayingCards.Count)];
							champion.attackingCard.caption.text = "Gambled by " + champion.championName;
							gambled = true;
							break;
					}
				}

				// Reviewing Choices
				switch (GameController.instance.difficulty) {
					case GameController.Difficulty.Warrior:
						if ((champion.attackingCard.CombatValue <= card.CombatValue ||
						     champion.attackingCard.CombatValue <= 9)
						    && !gambled) {
							Debug.Log(champion.championName + " does not want to attack with the current configuration!");
							champion.attackingCard = null;
							champion.currentTarget = null;
							yield break;
						}
						break;
					case GameController.Difficulty.Champion:
						float f = champion.currentTarget.currentHP <= 0.25f * champion.currentTarget.maxHP ? 0.4f : 0.75f;
						if ((champion.attackingCard.CombatValue <= card.CombatValue ||
						     champion.attackingCard.CombatValue <= 9 ||
						     champion.hand.GetCardCount() <= 2 && Random.Range(0f, 1f) < f)
						    && !gambled || (champion.currentTarget.isDead)) {
							Debug.Log(champion.championName + " does not want to attack with the current configuration!");
							champion.attackingCard = null;
							champion.currentTarget = null;
							yield break;
						}
						break;
				}

				// Confirming Attack
				yield return StartCoroutine(champion.hand.Discard(card));
				card.redGlow.Stop();
				card.redGlow.Play();
				champion.currentTarget.championParticleController.PlayEffect(champion.currentTarget.championParticleController.RedGlow);
				champion.isAttacking = true;
				champion.spadesBeforeExhaustion--;
				champion.matchStatistic.totalAttacks++;

				yield return new WaitForSeconds(Random.Range(0.25f, 1.5f));
				Debug.Log(champion.championName + " is attacking " + champion.currentTarget.championName + " with a card with a value of " + champion.attackingCard.CombatValue);

				yield return StartCoroutine(CombatCalculation(champion, champion.currentTarget));
				break;
		}
	}
	protected virtual IEnumerator HeartLogic(Card card, ChampionController champion) {
		if (champion.heartsBeforeExhaustion <= 0) {
			// Fail-safe if champion can't play more HEARTS.
			if (champion.isPlayer) {
				TooltipSystem.instance.ShowError("You cannot play any more HEARTS!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			}
			yield break;
		}
		if (champion.currentHP >= champion.maxHP) {
			// Fail-safe if champion is full health.
			if (champion.isPlayer) {
				TooltipSystem.instance.ShowError("Your health is full!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			}
			yield break;
		}

		if (champion.isPlayer) currentlyHandlingCard = true;

		switch (card.cardScriptableObject.cardValue) {
			default:
				StartCoroutine(champion.Heal(5));
				champion.heartsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));
				break;
			case 7:
			case 8:
			case 9:
				if (champion.heartsBeforeExhaustion - 2 < 0) {
					if (champion.isPlayer) {
						TooltipSystem.instance.ShowError("You cannot play this HEART right now!");
						LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					}
					break;
				}

				StartCoroutine(champion.Heal(10));
				champion.heartsBeforeExhaustion -= 2;
				yield return StartCoroutine(champion.hand.Discard(card));
				break;
			case 10:
			case 11:
			case 12:
				if (champion.heartsBeforeExhaustion - 3 < 0) {
					if (champion.isPlayer) {
						TooltipSystem.instance.ShowError("You cannot play this HEART right now!");
						LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					}
					break;
				}
				StartCoroutine(champion.Heal(20));
				champion.heartsBeforeExhaustion -= 3;
				yield return StartCoroutine(champion.hand.Discard(card));
				break;
			case 13:
				if (champion.heartsBeforeExhaustion - 3 < 0) {
					if (champion.isPlayer) {
						TooltipSystem.instance.ShowError("You cannot play this HEART right now!");
						LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					}
					break;
				}
				StartCoroutine(champion.Heal(40));
				champion.heartsBeforeExhaustion -= 3;
				yield return StartCoroutine(champion.hand.Discard(card));
				break;
		}

		currentlyHandlingCard = false;
	}
	protected virtual IEnumerator ClubLogic(Card card, ChampionController champion) {
		yield return StartCoroutine(champion.hand.Discard(card));
		yield return StartCoroutine(champion.hand.Deal(1, false, true, false));
	}
	protected virtual IEnumerator DiamondLogic(Card card, ChampionController champion) {
		if (champion.diamondsBeforeExhaustion <= 0 && (card.cardScriptableObject.cardValue < 5 || card.cardScriptableObject.cardValue > 8)) {
			// Fail-safe for if the champion can't play more DIAMONDS.
			if (champion.isPlayer) {
				TooltipSystem.instance.ShowError("You cannot play any more DIAMONDS!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			}
			yield break;
		}

		if (champion.isPlayer) currentlyHandlingCard = true;

		switch (card.cardScriptableObject.cardValue) {
			case 1:
				if (!champion.isPlayer) {
					bool wontUse = false;
					switch (GameController.instance.difficulty) {
						case GameController.Difficulty.Noob:
						case GameController.Difficulty.Novice:
							break;
						case GameController.Difficulty.Warrior:
							if (champion.hand.GetCardCount() >= 4) wontUse = true;
							break;
						case GameController.Difficulty.Champion:
							foreach (ChampionController selectedChampion in GameController.instance.champions) {
								if (selectedChampion == champion || selectedChampion.isDead
								                                 || selectedChampion.hand.GetCardCount() < 5 && selectedChampion.currentHP - champion.attackDamage > 0) continue;
								wontUse = true;
								break;
							}
							break;
					}
					if (wontUse) break;
				}

				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion.isDead) continue;
					yield return StartCoroutine(selectedChampion.hand.Deal(2));
				}
				break;
			case 2:
				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion.hand.GetCardCount() == 0 || selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.isPlayer) {
						selectedChampion.discardAmount = 1;
						GameController.instance.playerActionTooltip.text = "Please discard " + selectedChampion.discardAmount + ".";

						yield return new WaitUntil(() => selectedChampion.discardAmount == 0);

						continue;
					}

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.championName + ".";

					selectedChampion.discardAmount = 1;

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					List<Card> discardList = new List<Card>();
					for (int discarded = 0; discarded < selectedChampion.discardAmount; discarded++) {
						Card discard = selectedChampion.hand.GetCard("Lowest");
						selectedChampion.hand.cards.Remove(discard);
						discard.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
						discard.discardFeed.text = "DISCARDED";
						discardList.Add(discard);
					}
					yield return StartCoroutine(selectedChampion.hand.Discard(discardList.ToArray()));

					selectedChampion.discardAmount = 0;
				}
				GameController.instance.playerActionTooltip.text = string.Empty;
				break;
			case 3:
				if (!champion.isPlayer) {
					bool wontUse = false;
					switch (GameController.instance.difficulty) {
						case GameController.Difficulty.Noob:
						case GameController.Difficulty.Novice:
							break;
						case GameController.Difficulty.Warrior:
							if (champion.hand.GetCardCount() >= 3) wontUse = true;
							break;
						case GameController.Difficulty.Champion:
							foreach (ChampionController selectedChampion in GameController.instance.champions) {
								if (selectedChampion == champion || selectedChampion.isDead
								                                 || selectedChampion.hand.GetCardCount() < 5 && selectedChampion.currentHP - champion.attackDamage > 0) continue;
								wontUse = true;
								break;
							}
							break;
					}
					if (wontUse) break;
				}

				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion.isDead) continue;
					yield return StartCoroutine(selectedChampion.hand.Deal());
				}
				break;
			case 4:
				if (!champion.isPlayer) {
					bool jeopardized = false;
					foreach (ChampionController teammate in GameController.instance.champions) {
						if (!teammate.teamMembers.Contains(champion) || teammate == champion || teammate.isDead) continue;

						switch (GameController.instance.difficulty) {
							case GameController.Difficulty.Noob:
							case GameController.Difficulty.Novice:
								break;
							case GameController.Difficulty.Warrior:
								if (teammate.currentHP - 20 <= 0) {
									Debug.Log(champion.championName + " does not want to jeopardize his teammate, " + teammate.championName + "!");
									jeopardized = true;
								}
								break;
							case GameController.Difficulty.Champion:
								if (teammate.currentHP - 20 <= 0) {
									int enemiesJeopardized = 0, enemiesLeft = 0;
									foreach (ChampionController enemyChampion in GameController.instance.champions) {
										if (enemyChampion.teamMembers.Contains(champion) || enemyChampion == champion || enemyChampion == teammate || enemyChampion.isDead) continue;
										if (enemyChampion.currentHP - 20 > 0) enemiesJeopardized++;
										else {
											enemiesLeft++;
										}
									}
									if (enemiesLeft <= enemiesJeopardized && Random.Range(0f, 1f) < 0.8f || enemiesJeopardized != 0 && Random.Range(0f, 1f) < 0.25f) {
										Debug.Log(champion.championName + " does not want to jeopardize his teammate, " + teammate.championName + "!");
										jeopardized = true;
									}
								}
								break;
						}
					}
					if (jeopardized) break;
				}

				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.hand.GetCardCount() == 0) {
						Debug.Log(selectedChampion.championName + " has no cards! Dealing damage automatically...");
						yield return StartCoroutine(selectedChampion.Damage(20, DamageType.Unblockable, champion));
						yield return new WaitForSeconds(2f);
						continue;
					}

					if (selectedChampion.isPlayer) {
						selectedChampion.discardAmount = Mathf.Min(selectedChampion.hand.GetCardCount(), 2);
						GameController.instance.playerActionTooltip.text = "Please discard " + selectedChampion.discardAmount + ".";
						GameController.instance.confirmButton.Show();
						GameController.instance.confirmButton.textBox.text = "Skip";

						yield return new WaitUntil(() => selectedChampion.discardAmount <= 0);

						if (selectedChampion.discardAmount == -1) {
							yield return StartCoroutine(selectedChampion.Damage(20, DamageType.Unblockable, champion));
							selectedChampion.discardAmount = 0;
							GameController.instance.confirmButton.textBox.text = "Confirm";
						}

						continue;
					}

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.championName + ".";

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					float chance = selectedChampion.currentHP >= 0.75f * selectedChampion.maxHP ? 0.75f : 0.5f;
					if (Random.Range(0f, 1f) < chance && selectedChampion.currentHP - 20 > 0 || selectedChampion.hand.GetCardCount() == 0) {
						yield return StartCoroutine(selectedChampion.Damage(20, DamageType.Unblockable, champion));
						continue;
					}

					selectedChampion.discardAmount = Mathf.Min(selectedChampion.hand.GetCardCount(), 2);

					List<Card> discardList = new List<Card>();
					for (int discarded = 0; discarded < selectedChampion.discardAmount; discarded++) {
						Card discard = selectedChampion.hand.GetCard("Lowest");
						selectedChampion.hand.cards.Remove(discard);
						discard.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
						discard.discardFeed.text = "DISCARDED";
						discardList.Add(discard);
					}
					yield return StartCoroutine(selectedChampion.hand.Discard(discardList.ToArray()));

					selectedChampion.discardAmount = 0;
				}

				GameController.instance.playerActionTooltip.text = string.Empty;

				break;
			case 5:
			case 6:
			case 7:
			case 8:
				yield return StartCoroutine(champion.hand.Discard(card));
				yield return StartCoroutine(champion.hand.Deal(1, false, true, false));
				break;
			case 9:
				switch (champion.isPlayer) {
					case false:
						if (champion.currentHP >= 0.8f * champion.maxHP && (champion.currentHP == champion.maxHP || Random.Range(0f, 1f) < 0.75f)) break;
						break;
				}
				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));
				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					yield return StartCoroutine(selectedChampion.Heal(10));
				}
				break;
			case 10:
				switch (champion.isPlayer) {
					case false:
						if (champion.currentHP >= 0.8f * champion.maxHP && (champion.currentHP == champion.maxHP || Random.Range(0f, 1f) < 0.9f)) break;
						break;
				}
				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));
				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					yield return StartCoroutine(selectedChampion.Heal(20));
				}
				break;
			case 11:
				if (!champion.isPlayer) {
					bool jeopardized = false;
					foreach (ChampionController quickSelectChampion in GameController.instance.champions) {
						if (!quickSelectChampion.teamMembers.Contains(champion) || quickSelectChampion == champion || quickSelectChampion.isDead) continue;

						switch (GameController.instance.difficulty) {
							case GameController.Difficulty.Noob:
							case GameController.Difficulty.Novice:
								break;
							case GameController.Difficulty.Warrior:
								if (quickSelectChampion.currentHP - 20 <= 0) {
									Debug.Log(champion.championName + " does not want to jeopardize his teammate, " + quickSelectChampion.championName + "!");
									jeopardized = true;
								}
								break;
							case GameController.Difficulty.Champion:
								if (quickSelectChampion.currentHP - 20 <= 0) {
									int enemiesJeopardized = 0, enemiesLeft = 0;
									foreach (ChampionController enemyChampion in GameController.instance.champions) {
										if (enemyChampion.teamMembers.Contains(champion) || enemyChampion == champion || enemyChampion == quickSelectChampion || enemyChampion.isDead) continue;
										if (enemyChampion.currentHP - 20 > 0) enemiesJeopardized++;
										else {
											enemiesLeft++;
										}
									}
									if (enemiesLeft <= enemiesJeopardized) {
										Debug.Log(champion.championName + " does not want to jeopardize his teammate, " + quickSelectChampion.championName + "!");
										jeopardized = true;
									}
								}
								break;
						}
					}
					if (jeopardized) break;
				}

				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion == champion || selectedChampion.isDead) continue;

					yield return StartCoroutine(selectedChampion.Damage(20, DamageType.Fire, champion));

					yield return new WaitForSeconds(1f);
				}
				break;
			case 12:
				if (!champion.isPlayer) {
					bool jeopardized = false;
					foreach (ChampionController quickSelectChampion in GameController.instance.champions) {
						if (!quickSelectChampion.teamMembers.Contains(champion) || quickSelectChampion == champion || quickSelectChampion.isDead) continue;

						switch (GameController.instance.difficulty) {
							case GameController.Difficulty.Noob:
							case GameController.Difficulty.Novice:
								break;
							case GameController.Difficulty.Warrior:
								if (quickSelectChampion.currentHP - 40 <= 0) {
									Debug.Log(champion.championName + " does not want to jeopardize his teammate, " + quickSelectChampion.championName + "!");
									jeopardized = true;
								}
								break;
							case GameController.Difficulty.Champion:
								if (quickSelectChampion.currentHP - 40 <= 0) {
									int enemiesJeopardized = 0, enemiesLeft = 0;
									foreach (ChampionController enemyChampion in GameController.instance.champions) {
										if (enemyChampion.teamMembers.Contains(champion) || enemyChampion == champion || enemyChampion == quickSelectChampion || enemyChampion.isDead) continue;
										if (enemyChampion.currentHP - 40 > 0) enemiesJeopardized++;
										else {
											enemiesLeft++;
										}
									}
									if (enemiesLeft <= enemiesJeopardized && Random.Range(0f, 1f) < 0.65f || enemiesJeopardized != 0 && Random.Range(0f, 1f) < 0.65f) {
										Debug.Log(champion.championName + " does not want to jeopardize his teammate, " + quickSelectChampion.championName + "!");
										jeopardized = true;
									}
								}
								break;
						}
					}
					if (jeopardized) break;
				}

				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(card));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.hand.GetCardCount() == 0) {
						Debug.Log(selectedChampion.championName + " has no cards! Dealing damage automatically...");
						yield return StartCoroutine(selectedChampion.Damage(40, DamageType.Fire, champion));
						yield return new WaitForSeconds(2f);
						continue;
					}

					if (selectedChampion.isPlayer) {
						selectedChampion.discardAmount = Mathf.Min(selectedChampion.hand.GetCardCount(), 4);
						GameController.instance.playerActionTooltip.text = "Please discard " + selectedChampion.discardAmount + ".";
						GameController.instance.confirmButton.Show();
						GameController.instance.confirmButton.textBox.text = "Skip";

						yield return new WaitUntil(() => selectedChampion.discardAmount <= 0);

						if (selectedChampion.discardAmount == -1) {
							yield return StartCoroutine(selectedChampion.Damage(40, DamageType.Fire, champion));
							selectedChampion.discardAmount = 0;
							GameController.instance.confirmButton.textBox.text = "Confirm";
						}

						continue;
					}

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.championName + ".";

					yield return new WaitForSeconds(Random.Range(0.2f, 2f));

					float chance = selectedChampion.currentHP >= 0.75f * selectedChampion.maxHP ? 0.5f : 0.15f;
					if (Random.Range(0f, 1f) < chance && selectedChampion.currentHP - 40 > 0 || selectedChampion.hand.GetCardCount() == 0) {
						yield return StartCoroutine(selectedChampion.Damage(40, DamageType.Fire, champion));
						continue;
					}

					selectedChampion.discardAmount = Mathf.Min(selectedChampion.hand.GetCardCount(), 4);

					List<Card> discardList = new List<Card>();
					for (int discarded = 0; discarded < selectedChampion.discardAmount; discarded++) {
						Card discard = selectedChampion.hand.GetCard("Lowest");
						selectedChampion.hand.cards.Remove(discard);
						discard.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
						discard.discardFeed.text = "DISCARDED";
						discardList.Add(discard);
					}
					yield return StartCoroutine(selectedChampion.hand.Discard(discardList.ToArray()));

					selectedChampion.discardAmount = 0;
				}

				GameController.instance.playerActionTooltip.text = string.Empty;

				break;
			default:
				Debug.Log("Not implemented yet. Skipping...");
				break;
		}

		currentlyHandlingCard = false;
	}
}
