using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Serialization;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	[FormerlySerializedAs("cardScriptableObject")] [Header("Card Information")]
	public CardData cardData;
	[Header("References")]
	public Image cardImage;
	public TMP_Text caption, cornerText, staminaRequirementText;
	public TMP_Text discardFeed, advantageFeed;
	public ParticleSystem halo, redGlow, greenGlow;

	private int staminaRequirementModifier;
	public int StaminaRequirementModifier {
		get {
			return staminaRequirementModifier;
		}
		set {
			staminaRequirementModifier = value;
			staminaRequirementText.text = EffectiveStaminaRequirement.ToString();
			staminaRequirementText.color = cardData.cardColor == CardColor.Dark ? Color.white : Color.black;
		}
	}
	public int EffectiveStaminaRequirement => cardData.staminaRequirement + staminaRequirementModifier;

	
	[HideInInspector]
	public ChampionController owner;
	[HideInInspector]
	public bool isHidden;
	public List<string> tags = new List<string>();

	private int delayID;
	
	private void Start() {
		gameObject.name = cardData.cardName;
		if (cardImage.sprite != cardData.cardFront && cardImage.sprite != cardData.cardBack) cardImage.sprite = cardData.cardFront;
		cornerText.text = cardData.cornerText;
		if (cardData.cardColor == CardColor.Dark) cornerText.color = Color.white;
		StaminaRequirementModifier = 0;
	}

	/// <summary>
	/// Flips a card, hiding it from the player's view.
	/// </summary>
	/// <param name="doFlipAnimation"></param>
	public void Flip(bool doFlipAnimation = false) {
		Debug.Log("calleed");
		if (!isHidden) {
			isHidden = true;
			cardImage.sprite = cardData.cardBack;
			
			advantageFeed.gameObject.SetActive(false);
			cornerText.gameObject.SetActive(false);
			staminaRequirementText.gameObject.SetActive(false);
		}
		else {
			isHidden = false;
			cardImage.sprite = cardData.cardFront;

			advantageFeed.gameObject.SetActive(true);
			cornerText.gameObject.SetActive(true);
			staminaRequirementText.gameObject.SetActive(true);
			
			if (doFlipAnimation) {
				transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				GetComponent<SmartHover>().ScaleDown();
			}
		}
	}

	private IEnumerator CardSelect() {
		ChampionController cachedOwner = owner;

		if (cachedOwner is null) yield break;
		if (!cachedOwner.isPlayer || cachedOwner.isDead) yield break;

		// Exits if the card is not the player's.
		if (!cachedOwner.isPlayer) {
			TooltipSystem.instance.ShowError("This is not your card!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}

		switch (GameManager.instance.gamePhase) {
			case GamePhase.GameStart:
				TooltipSystem.instance.ShowError("It is not your turn!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
			case GamePhase.BeginningPhase:
				TooltipSystem.instance.ShowError(cachedOwner.isMyTurn ? "You cannot play a card during the Beginning Phase!" : "It is not your turn!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
			case GamePhase.ActionPhase:
				if (GameManager.instance.currentlyHandlingCard) {
					Debug.Log("Cannot play! Currently handling another card.");
					yield break;
				}

				// If it's the player's turn
				switch (cachedOwner.isMyTurn) {
					case true:
						if (GameManager.instance.CurrentRoundStamina < EffectiveStaminaRequirement) {
							TooltipSystem.instance.ShowError("You don't hae enough stamina to play this card yet! Wait until Round " + EffectiveStaminaRequirement + ".");
							LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
							yield break;
						}
						if (cachedOwner.currentStamina < EffectiveStaminaRequirement) {
							TooltipSystem.instance.ShowError("You are too exhausted to play this card!");
							LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
							yield break;
						}

						// Parry
						if (FightManager.fightInstance is { } && FightManager.instance.parrying) {
							switch (cardData.cardFunctions.primaryFunction) {
								case "block":
								case "parry":
									if (cardData.cardFunctions.primaryFunction == "parry" && cachedOwner.equippedWeapon is null) {
										TooltipSystem.instance.ShowError("You cannot parry without a weapon!");
										LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
										break;
									}
									FightManager.fightInstance.DefendingCard = this;
									break;
								default:
									TooltipSystem.instance.ShowError("You cannot play this card to parry the attack!");
									LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
									break;
							}

							yield break;
						}

						// Change Attack Card
						if (FightManager.fightInstance is { }) {
							if (FightManager.fightInstance.Attacker.equippedWeapon is null) {
								TooltipSystem.instance.ShowError("You can't attack without a weapon!");
								LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
								yield break;
							}
							switch (cardData.cardFunctions.primaryFunction) {
								case "attack":
									FightManager.fightInstance.AttackingCard.Flip(true);
									FightManager.fightInstance.AttackingCard = this;
									FightManager.fightInstance.AttackingCard.Flip(true);
									break;
								default:
									TooltipSystem.instance.ShowError("You can't play this card during a fight!");
									LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
									break;
							}

							yield break;
						}
						
						// Play A Normal Card
						else {
							switch (cardData.cardFunctions.primaryFunction) {
								case "attack":
									yield return StartCoroutine(AttackFunction(owner));
									break;
								case "draw":
									yield return StartCoroutine(DrawFunction(owner));
									break;
								case "heal":
									yield return StartCoroutine(HealFunction(owner));
									break;
								default:
									TooltipSystem.instance.ShowError("You can't play this card right now!"); 
									LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
									break;
							}

							yield break;
						}
					case false:
						// Defense
						if (FightManager.fightInstance is { } && FightManager.fightInstance.Defender == cachedOwner) {
							switch (cardData.cardFunctions.primaryFunction) {
								case "block":
								case "parry":
									if (cardData.cardFunctions.primaryFunction == "parry" && cachedOwner.equippedWeapon is null) {
										TooltipSystem.instance.ShowError("You cannot parry without a weapon!");
										LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
										yield break;
									}
									FightManager.fightInstance.DefendingCard = this;
									break;
							}
						}
						yield break;
				}
			case GamePhase.EndPhase:
				switch (cachedOwner.isMyTurn) {
					case true:
						// When Discarding Naturally
						if (cachedOwner.discardAmount > 0) {
							cachedOwner.discardAmount--;
							discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.gray);
							discardFeed.text = "DISCARDED";

							if (cachedOwner.discardAmount != 0) {
								GameManager.instance.playerActionTooltip.text = "Please discard " + cachedOwner.discardAmount + ".";
							}
							else {
								GameManager.instance.playerActionTooltip.text = string.Empty;
							}
							yield return StartCoroutine(cachedOwner.hand.Discard(this));
						}
						yield break;
					case false:
						TooltipSystem.instance.ShowError("It is not your turn!");
						LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
						yield break;
				}
		}
	}

	public IEnumerator AttackFunction(ChampionController championController = null, ChampionController defender = null, Card attackingCard = null) {
		if (championController is null) championController = owner;
		if (attackingCard is null) attackingCard = this;
		if (championController.equippedWeapon is null) {
			if (!championController.isPlayer) yield break;
			TooltipSystem.instance.ShowError("You can't attack without a weapon!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}
									
		FightManager fightManager = FightManager.Create();
		yield return StartCoroutine(fightManager.HandleFight(championController, defender, attackingCard));
	}
	public IEnumerator HealFunction(ChampionController championController = null) {
		if (championController is null) championController = owner;
		
		if (championController.currentHP == championController.champion.maxHP) {
			if (!championController.isPlayer) yield break;
			TooltipSystem.instance.ShowError("Your health is full!"); 
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}
		yield return StartCoroutine(championController.hand.UseCard(this));
		switch (cardData.cardFunctions.secondaryFunction) {
			case "":
				yield return StartCoroutine(championController.Heal(1));
				break;
			default:
				yield return StartCoroutine(championController.Heal(int.Parse(cardData.cardFunctions.secondaryFunction)));
				break;
		}
	}

	public IEnumerator DrawFunction(ChampionController championController = null) {
		if (championController is null) championController = owner;

		yield return StartCoroutine(championController.hand.UseCard(this));
		switch (cardData.cardFunctions.secondaryFunction) {
			case "":
				yield return StartCoroutine(championController.hand.Deal(1, cardData.cardColor, true));
				break;
			default:
				yield return StartCoroutine(championController.hand.Deal(int.Parse(cardData.cardFunctions.secondaryFunction), cardData.cardColor, true));
				break;
		}
	}

	#region Pointer Events
	public void OnClick() {
		StartCoroutine(CardSelect());
	}
	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(1f, () => {
			if (isHidden) {
				TooltipSystem.instance.Show(null, "Flipped Card");
				return;
			}

			string description = cardData.cardDescription;
			description += "\n\nStamina Requirement: " + (EffectiveStaminaRequirement == 0 ? "FREE" : EffectiveStaminaRequirement.ToString());
			TooltipSystem.instance.Show(description, cardData.cardName);
		}).uniqueId;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
	
	#endregion
}
