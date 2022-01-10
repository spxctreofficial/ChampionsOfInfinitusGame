using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	[Header("Card Information")]
	public CardScriptableObject cardScriptableObject;
	[Header("References")]
	public Image cardImage;
	public TMP_Text caption;
	public TMP_Text discardFeed, advantageFeed;
	public ParticleSystem halo, redGlow;

	[HideInInspector]
	public ChampionController owner;
	[HideInInspector]
	public bool isHidden;
	public List<string> tags = new List<string>();

	private int combatValue;
	private static int delayID;

	public int CombatValue {
		get => combatValue;
		set {
			combatValue = value;
			if (combatValue == cardScriptableObject.cardValue) {
				advantageFeed.text = string.Empty;
				return;
			}

			int difference = combatValue - cardScriptableObject.cardValue;

			if (combatValue > cardScriptableObject.cardValue) {
				Color greenGlow = (Color)new Color32(46, 191, 0, 128) * Mathf.Pow(2, 2);
				greenGlow.a = 0.5f;
				advantageFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, greenGlow);
				advantageFeed.text = "+" + difference;
			}
			else {
				Color redGlow = Color.red * Mathf.Pow(2, 2);
				redGlow.a = 0.65f;
				advantageFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, redGlow);
				advantageFeed.text = difference.ToString();
			}
			advantageFeed.text += " Advantage";
		}
	}
	
	private void Start() {
		gameObject.name = cardScriptableObject.cardName;
		combatValue = cardScriptableObject.cardValue;
		if (cardImage.sprite != cardScriptableObject.cardFront && cardImage.sprite != cardScriptableObject.cardBack) cardImage.sprite = cardScriptableObject.cardFront;
	}

	/// <summary>
	/// Flips a card, hiding it from the player's view.
	/// </summary>
	/// <param name="doFlipAnimation"></param>
	public void Flip(bool doFlipAnimation = false) {
		Debug.Log("calleed");
		if (!isHidden) {
			isHidden = true;
			cardImage.sprite = cardScriptableObject.cardBack;
			
			advantageFeed.gameObject.SetActive(false);
		}
		else {
			isHidden = false;
			cardImage.sprite = cardScriptableObject.cardFront;

			advantageFeed.gameObject.SetActive(true);
			if (doFlipAnimation) {
				transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				GetComponent<SmartHover>().ScaleDown();
			}
		}
	}

	private IEnumerator CardSelect() {
		if (!owner.isPlayer || owner.isDead) yield break;

		// Exits if the card is not the player's.
		if (owner is null) {
			TooltipSystem.instance.ShowError("This is not your card!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}
		if (!owner.isPlayer) {
			TooltipSystem.instance.ShowError("This is not your card!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}

		switch (GameController.instance.gamePhase) {
			case GamePhase.BeginningPhase:
				TooltipSystem.instance.ShowError(owner.isMyTurn ? "You cannot play a card during the Beginning Phase!" : "It is not your turn!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				break;
			case GamePhase.ActionPhase:
				if (GameController.instance.currentlyHandlingCard) {
					Debug.Log("Cannot play! Currently handling another card.");
					yield break;
				}

				// If it's the player's turn
				switch (owner.isMyTurn) {
					case true:
						// When Attacking
						if (owner.isAttacking && !owner.hasAttacked) {
							if (GameController.instance.gambleButton.isBlocking) {
								TooltipSystem.instance.ShowError("You cannot select another combat card after gambling!");
								LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
								yield break;
							}
							if (this == owner.hand.queued.Peek()) {
								TooltipSystem.instance.ShowError("You cannot select the same card used for starting the attack as a combat card!");
								LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
								yield break;
							}
							if (owner.attackingCard is { }) {
								owner.attackingCard.redGlow.Stop();
								owner.attackingCard.redGlow.Clear();
							}
							owner.attackingCard = this;
							redGlow.Stop();
							redGlow.Play();
							GameController.instance.gambleButton.Hide();

							if (owner.currentTarget is { }) {
								GameController.instance.confirmButton.Show();
								GameController.instance.confirmButton.textBox.text = "Confirm";
							}
							yield break;
						}

						// When Used Normally
						switch (cardScriptableObject.cardSuit) {
							case CardSuit.SPADE:
								StartCoroutine(SpadeLogic(owner));
								yield break;
							case CardSuit.HEART:
								StartCoroutine(HeartLogic(owner));
								yield break;
							case CardSuit.CLUB:
								StartCoroutine(ClubLogic(owner));
								yield break;
							case CardSuit.DIAMOND:
								StartCoroutine(DiamondLogic(owner));
								yield break;
						}
						break;
					case false:
						// When Defense
						foreach (ChampionController champion in GameController.instance.champions) {
							if (champion.currentTarget != owner || !champion.isAttacking) continue;

							if (owner.defendingCard is { }) owner.defendingCard.Flip(true);
							owner.defendingCard = this;
							Flip(true);

							GameController.instance.playerActionTooltip.text = "Confirm the defense, or change selected card.";
							GameController.instance.confirmButton.Show();
							GameController.instance.confirmButton.textBox.text = "Confirm";
							yield break;
						}
						break;
				}

				// When Forced to Discard
				if (owner.discardAmount > 0) {
					yield return StartCoroutine(owner.hand.Discard(this));

					owner.discardAmount--;
					discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
					discardFeed.text = "DISCARDED";
					GameController.instance.confirmButton.Hide();

					if (owner.discardAmount != 0) {
						GameController.instance.playerActionTooltip.text = "Please discard " + owner.discardAmount + ".";
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
				switch (owner.isMyTurn) {
					case true:
						// When Discarding Naturally
						if (owner.discardAmount > 0) {
							yield return StartCoroutine(owner.hand.Discard(this));

							owner.discardAmount--;
							discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
							discardFeed.text = "DISCARDED";
							GameController.instance.confirmButton.Hide();

							if (owner.discardAmount != 0) {
								GameController.instance.playerActionTooltip.text = "Please discard " + owner.discardAmount + ".";
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

	public virtual IEnumerator SpadeLogic(ChampionController champion, int tries = 0) {
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
				halo.Stop();
				halo.Play();
				champion.isAttacking = true;
				champion.hand.queued.Enqueue(this);
				break;
			case false:
				// Bot Spade Logic
				bool gambled = false;

				if (cardScriptableObject.cardValue > 10 && Random.Range(0f, 1f) < 0.75f) {
					// Coherence for using a high spade.
					Debug.Log(champion.champion.championName + " refuses to use a SPADE worth: " + cardScriptableObject.cardValue);
					yield break;
				}
				if (tries > 3) {
					Debug.Log(champion.champion.championName + "already tried more than 3 times!");
					yield break;
				}

				// Targeting Champion
				if (Random.Range(0f, 1f) < 0.75f && champion.currentNemesis is { isDead: false }) {
					Debug.Log(champion.champion.championName + " is furious! Targeting their nemesis immediately.");
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
					Debug.Log(champion.champion.championName + " decides not to attack!");
					champion.spadesBeforeExhaustion--;
					break;
				}

				// Attacking Card
				yield return new WaitForSeconds(Random.Range(0.5f, 1.25f));
				if (champion.hand.GetCardCount() - 1 != 0 || Random.Range(0f, 1f) < 0.85f) {
					champion.attackingCard = champion.hand.GetAttackingCard(this);
				}
				if (champion.attackingCard is null) {
					switch (GameController.instance.difficulty) {
						case GameController.Difficulty.Warrior:
						case GameController.Difficulty.Champion:
							champion.attackingCard = Instantiate(PrefabManager.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
							champion.attackingCard.cardScriptableObject = GameController.instance.cardIndex.PlayingCards[Random.Range(0, GameController.instance.cardIndex.PlayingCards.Count)];
							champion.attackingCard.caption.text = "Gambled by " + champion.champion.championName;
							gambled = true;
							break;
					}
				}

				// Reviewing Choices
				switch (GameController.instance.difficulty) {
					case GameController.Difficulty.Warrior:
						if ((champion.attackingCard.CombatValue <= CombatValue ||
							 champion.attackingCard.CombatValue <= 9)
							&& !gambled) {
							Debug.Log(champion.champion.championName + " does not want to attack with the current configuration!");
							champion.attackingCard = null;
							champion.currentTarget = null;
							yield break;
						}
						break;
					case GameController.Difficulty.Champion:
						float f = champion.currentTarget.currentHP <= 0.25f * champion.currentTarget.maxHP ? 0.4f : 0.75f;
						if ((champion.attackingCard.CombatValue <= CombatValue ||
							 champion.attackingCard.CombatValue <= 9 ||
							 champion.hand.GetCardCount() <= 2 && Random.Range(0f, 1f) < f)
							&& !gambled || (champion.currentTarget.isDead)) {
							Debug.Log(champion.champion.championName + " does not want to attack with the current configuration!");
							champion.attackingCard = null;
							champion.currentTarget = null;
							yield break;
						}
						break;
				}

				// Confirming Attack
				yield return StartCoroutine(champion.hand.Discard(this));
				redGlow.Stop();
				redGlow.Play();
				champion.currentTarget.championParticleController.PlayEffect(champion.currentTarget.championParticleController.RedGlow);
				champion.isAttacking = true;
				champion.spadesBeforeExhaustion--;
				champion.matchStatistic.totalAttacks++;

				yield return new WaitForSeconds(Random.Range(0.25f, 1.5f));
				Debug.Log(champion.champion.championName + " is attacking " + champion.currentTarget.champion.championName + " with a card with a value of " + champion.attackingCard.CombatValue);

				yield return StartCoroutine(champion.CombatCalculation(champion, champion.currentTarget));
				break;
		}
	}
	public virtual IEnumerator HeartLogic(ChampionController champion) {
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

		if (champion.isPlayer) GameController.instance.currentlyHandlingCard = true;

		switch (cardScriptableObject.cardValue) {
			default:
				StartCoroutine(champion.Heal(5));
				champion.heartsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(this));
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
				yield return StartCoroutine(champion.hand.Discard(this));
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
				yield return StartCoroutine(champion.hand.Discard(this));
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
				yield return StartCoroutine(champion.hand.Discard(this));
				break;
		}

		GameController.instance.currentlyHandlingCard = false;
	}
	public virtual IEnumerator ClubLogic(ChampionController champion) {
		yield return StartCoroutine(champion.hand.Discard(this));
		yield return StartCoroutine(champion.hand.Deal(1, false, true, false));
	}
	public virtual IEnumerator DiamondLogic(ChampionController champion) {
		if (champion.diamondsBeforeExhaustion <= 0 && (cardScriptableObject.cardValue < 5 || cardScriptableObject.cardValue > 8)) {
			// Fail-safe for if the champion can't play more DIAMONDS.
			if (champion.isPlayer) {
				TooltipSystem.instance.ShowError("You cannot play any more DIAMONDS!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			}
			yield break;
		}

		if (champion.isPlayer) GameController.instance.currentlyHandlingCard = true;

		switch (cardScriptableObject.cardValue) {
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
				yield return StartCoroutine(champion.hand.Discard(this));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion.isDead) continue;
					yield return StartCoroutine(selectedChampion.hand.Deal(2));
				}
				break;
			case 2:
				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(this));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion.hand.GetCardCount() == 0 || selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.isPlayer) {
						selectedChampion.discardAmount = 1;
						GameController.instance.playerActionTooltip.text = "Please discard " + selectedChampion.discardAmount + ".";

						yield return new WaitUntil(() => selectedChampion.discardAmount == 0);

						continue;
					}

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.champion.championName + ".";

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
				yield return StartCoroutine(champion.hand.Discard(this));

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
									Debug.Log(champion.champion.championName + " does not want to jeopardize his teammate, " + teammate.champion.championName + "!");
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
										Debug.Log(champion.champion.championName + " does not want to jeopardize his teammate, " + teammate.champion.championName + "!");
										jeopardized = true;
									}
								}
								break;
						}
					}
					if (jeopardized) break;
				}

				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(this));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.hand.GetCardCount() == 0) {
						Debug.Log(selectedChampion.champion.championName + " has no cards! Dealing damage automatically...");
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

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.champion.championName + ".";

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
				yield return StartCoroutine(champion.hand.Discard(this));
				yield return StartCoroutine(champion.hand.Deal(1, false, true, false));
				break;
			case 9:
				// switch (champion.isPlayer) {
				// 	case false:
				// 		if (champion.currentHP >= 0.8f * champion.maxHP && (champion.currentHP == champion.maxHP || Random.Range(0f, 1f) < 0.75f)) yield break;
				// 		break;
				// }
				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(this));
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
				yield return StartCoroutine(champion.hand.Discard(this));
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
									Debug.Log(champion.champion.championName + " does not want to jeopardize his teammate, " + quickSelectChampion.champion.championName + "!");
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
										Debug.Log(champion.champion.championName + " does not want to jeopardize his teammate, " + quickSelectChampion.champion.championName + "!");
										jeopardized = true;
									}
								}
								break;
						}
					}
					if (jeopardized) break;
				}

				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(this));

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
									Debug.Log(champion.champion.championName + " does not want to jeopardize his teammate, " + quickSelectChampion.champion.championName + "!");
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
										Debug.Log(champion.champion.championName + " does not want to jeopardize his teammate, " + quickSelectChampion.champion.championName + "!");
										jeopardized = true;
									}
								}
								break;
						}
					}
					if (jeopardized) break;
				}

				champion.diamondsBeforeExhaustion--;
				yield return StartCoroutine(champion.hand.Discard(this));

				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					if (selectedChampion == champion || selectedChampion.isDead) continue;
					if (selectedChampion.hand.GetCardCount() == 0) {
						Debug.Log(selectedChampion.champion.championName + " has no cards! Dealing damage automatically...");
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

					GameController.instance.playerActionTooltip.text = "Waiting for " + selectedChampion.champion.championName + ".";

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

		GameController.instance.currentlyHandlingCard = false;
	}

	// Pointer Events
	public void OnClick() {
		StartCoroutine(CardSelect());
	}
	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(1f, () => {
			if (isHidden) {
				TooltipSystem.instance.Show(null, "Flipped Card");
				return;
			}

			string description = cardScriptableObject.cardDescription;
			description += "\n\nWhen played in combat, this has a base combat value of " + combatValue +" .";
			TooltipSystem.instance.Show(description, cardScriptableObject.cardName);
		}).uniqueId;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
}
