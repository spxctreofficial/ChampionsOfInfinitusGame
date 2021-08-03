using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using TMPro;

public class TutorialCardLogicController : CardLogicController {
	public static new TutorialCardLogicController instance;

	protected override void Awake() {
		base.Awake();

		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
	}

	public override IEnumerator CardSelect(Card card) {
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
			
			switch (TutorialGameController.instance.tutorialProgress)
			{
				case 1:
					if (card.cardScriptableObject.cardValue != 4 ||
					    card.cardScriptableObject.cardSuit != CardSuit.SPADE) yield break;

					TutorialGameController.instance.tutorialProgress++;
					StartCoroutine(SpadeLogic(card, player));
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
									player.attackingCard.halo.Stop();
									player.attackingCard.halo.Clear();
								}
								player.attackingCard = card;
								card.halo.Stop();
								card.halo.Play();
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

								if (player.defendingCard != null) player.defendingCard.Flip(true);
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

	protected override IEnumerator SpadeLogic(Card card, ChampionController champion, int tries = 0) {
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

				switch (TutorialGameController.instance.tutorialProgress) {
					case 2:
						GameController.instance.endTurnButton.gameObject.SetActive(false);
						foreach (ChampionController selectedChampion in GameController.instance.champions) {
							if (selectedChampion.isDead || selectedChampion.team.Contains(champion.team)) continue;
							selectedChampion.championParticleController.PlayEffect(selectedChampion.championParticleController.GreenGlow);
						}
						card.redGlow.Stop();
						card.redGlow.Play();
						champion.isAttacking = true;
						champion.hand.queued.Enqueue(card);
						
						DialogueSystem.Create(TutorialGameController.instance.dialogueSessionsQueue.Dequeue(), new Vector2(0, -270)).transform.SetParent(GameController.instance.gameArea.transform, false);
						yield break;
				}
				GameController.instance.endTurnButton.gameObject.SetActive(false);
				GameController.instance.gambleButton.Show();
				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion.isDead || selectedChampion.team.Contains(champion.team)) continue;
					selectedChampion.championParticleController.PlayEffect(selectedChampion.championParticleController.GreenGlow);
				}
				card.redGlow.Stop();
				card.redGlow.Play();
				champion.isAttacking = true;
				champion.hand.queued.Enqueue(card);
				GameController.instance.attackCancelButton.Show();
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
						foreach (AbilityController ability in targetChampion.abilities) {
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
}
