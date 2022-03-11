using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.Serialization;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{

	[FormerlySerializedAs("cardScriptableObject")] [Header("Card Information")]
	public CardData cardData;
	[Header("References")]
	public CardRenderer cardRenderer;
	public TMP_Text caption;
	public TMP_Text discardFeed, advantageFeed;
	public GameObject[] redGlow, greenGlow;

	private int staminaRequirementModifier;
	public int StaminaRequirementModifier
	{
		get
		{
			return staminaRequirementModifier;
		}
		set
		{
			staminaRequirementModifier = value;
			cardRenderer.Refresh();
		}
	}
	public int EffectiveStaminaRequirement
	{
		get
		{
			return cardData.staminaRequirement + staminaRequirementModifier;
		}
	}


	[HideInInspector]
	public ChampionController owner;
	[HideInInspector]
	public bool isHidden;
	public List<string> tags = new();

	private List<int> delayIDs = new List<int>();

	protected virtual void Start()
	{
		gameObject.name = cardData.cardName;
		StaminaRequirementModifier = 0;
	}

	public void Flip(bool doFlipAnimation = false)
	{
		if (!isHidden)
		{
			cardRenderer.Flip();
			isHidden = true;

			advantageFeed.gameObject.SetActive(false);
		}
		else
		{
			cardRenderer.Flip();
			isHidden = false;

			advantageFeed.gameObject.SetActive(true);

			if (doFlipAnimation)
			{
				transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				GetComponent<SmartHover>().ScaleDown();
			}
		}
	}

	protected virtual IEnumerator CardSelect()
	{
		ChampionController cachedOwner = owner;

		if (cachedOwner is null) yield break;
		if (!cachedOwner.isPlayer || cachedOwner.isDead) yield break;

		// Exits if the card is not the player's.
		if (!cachedOwner.isPlayer)
		{
			TooltipSystem.instance.ShowError("This is not your card!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}

		switch (GameManager.instance.gamePhase)
		{
			case GamePhase.GameStart:
				yield return StartCoroutine(SelectDuringGameStart(cachedOwner));
				yield break;
			case GamePhase.BeginningPhase:
				yield return StartCoroutine(SelectDuringBeginningPhase(cachedOwner));
				yield break;
			case GamePhase.ActionPhase:
				yield return StartCoroutine(SelectDuringActionPhase(cachedOwner));
				yield break;
			case GamePhase.EndPhase:
				yield return StartCoroutine(SelectDuringEndPhase(cachedOwner));
				yield break;
		}
	}

	protected virtual IEnumerator SelectDuringGameStart(ChampionController championController)
	{
		TooltipSystem.instance.ShowError("It is not your turn!");
		LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
		yield break;
	}
	protected virtual IEnumerator SelectDuringBeginningPhase(ChampionController championController)
	{
		TooltipSystem.instance.ShowError(championController.isMyTurn ? "You cannot play a card during the Beginning Phase!" : "It is not your turn!");
		LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
		yield break;
	}
	protected virtual IEnumerator SelectDuringActionPhase(ChampionController championController)
	{
		if (GameManager.instance.currentlyHandlingCard)
		{
			Debug.Log("Cannot play! Currently handling another card.");
			yield break;
		}

		switch (championController.isMyTurn)
		{
			case true:
				if (GameManager.instance.CurrentRoundStamina < EffectiveStaminaRequirement)
				{
					TooltipSystem.instance.ShowError("You don't hae enough stamina to play this card yet! Wait until Round " + EffectiveStaminaRequirement + ".");
					LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					yield break;
				}
				if (championController.currentStamina < EffectiveStaminaRequirement)
				{
					TooltipSystem.instance.ShowError("You are too exhausted to play this card!");
					LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					yield break;
				}

				if (DiscardManager.instance is { } && DiscardManager.instance.discarder == championController && championController.discardAmount > 0)
				{
					yield return StartCoroutine(DiscardManager.instance.PlayerDiscardOperator(this));
					yield break;
				}

				// Parry
				if (FightManager.fightInstance is { } && FightManager.fightInstance is { })
				{
					yield return StartCoroutine(OnParryActionPhase(championController));

					yield break;
				}

				// Change Attack Card
				if (FightManager.fightInstance is {})
				{
					yield return StartCoroutine(OnAttackChangeActionPhase(championController));
					yield break;
				}
				else
				{
					yield return StartCoroutine(OnNormalFunctionActionPhase(championController));
					yield break;
				}
			case false:
				if (FightManager.fightInstance is {} && FightManager.fightInstance.Defender == championController)
				{
					yield return StartCoroutine(OnDefenseActionPhase(championController));
				}
				yield break;
		}
	}
	protected virtual IEnumerator OnNormalFunctionActionPhase(ChampionController championController)
	{
		if (cardData is WeaponCardData)
		{
			yield return StartCoroutine(EquipWeaponFunction(championController));
			yield break;
		}

		switch (cardData.cardFunctions.primaryFunction)
		{
			case "attack":
				yield return StartCoroutine(AttackFunction(owner));
				yield break;
			case "draw":
				yield return StartCoroutine(DrawFunction(owner));
				yield break;
			case "heal":
				yield return StartCoroutine(HealFunction(owner));
				yield break;
			default:
				TooltipSystem.instance.ShowError("You can't play this card right now!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
		}
	}
	protected virtual IEnumerator OnAttackChangeActionPhase(ChampionController championController)
	{
		if (FightManager.fightInstance.Attacker.equippedWeapon is null)
		{
			TooltipSystem.instance.ShowError("You can't attack without a weapon!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}
		switch (cardData.cardFunctions.primaryFunction)
		{
			case "attack":
				FightManager.fightInstance.AttackingCard.Flip(true);
				FightManager.fightInstance.AttackingCard = this;
				FightManager.fightInstance.AttackingCard.Flip(true);
				yield break;
			default:
				TooltipSystem.instance.ShowError("You can't play this card during a fight!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
		}
	}
	protected virtual IEnumerator OnParryActionPhase(ChampionController championController)
	{
		switch (cardData.cardFunctions.primaryFunction)
		{
			case "block":
			case "parry":
				if (cardData.cardFunctions.primaryFunction == "parry" && championController.equippedWeapon is null)
				{
					TooltipSystem.instance.ShowError("You cannot parry without a weapon!");
					LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					yield break;
				}
				FightManager.parryInstance.ParryingCard = this;
				yield break;
			default:
				TooltipSystem.instance.ShowError("You cannot play this card to parry the attack!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
		}
	}
	protected virtual IEnumerator OnDefenseActionPhase(ChampionController championController)
	{
		switch (cardData.cardFunctions.primaryFunction)
		{
			case "block":
			case "parry":
				if (cardData.cardFunctions.primaryFunction == "parry" && championController.equippedWeapon is null)
				{
					TooltipSystem.instance.ShowError("You cannot parry without a weapon!");
					LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
					yield break;
				}

				if (FightManager.parryInstance is { }) FightManager.parryInstance.DefendingCard = this;
				else FightManager.fightInstance.DefendingCard = this;
				yield break;
			default:
				TooltipSystem.instance.ShowError("You cannot defend with this card!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
		}
	}
	protected virtual IEnumerator SelectDuringEndPhase(ChampionController championController)
	{
		switch (championController.isMyTurn)
		{
			case true:
				if (DiscardManager.instance is {} && DiscardManager.instance.discarder == championController && championController.discardAmount > 0) yield return StartCoroutine(DiscardManager.instance.PlayerDiscardOperator(this));
				yield break;
			case false:
				TooltipSystem.instance.ShowError("It is not your turn!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
				yield break;
		}
	}

	public IEnumerator AttackFunction(ChampionController championController = null, ChampionController defender = null, Card attackingCard = null)
	{
		if (championController is null) championController = owner;
		if (attackingCard is null) attackingCard = this;
		if (championController.equippedWeapon is null)
		{
			if (!championController.isPlayer) yield break;
			TooltipSystem.instance.ShowError("You can't attack without a weapon!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}

		FightManager fightManager = FightManager.Create();
		yield return StartCoroutine(fightManager.InitiateFight(championController, defender, attackingCard));
	}
	public IEnumerator HealFunction(ChampionController championController = null)
	{
		if (championController is null) championController = owner;

		if (championController.currentHP == championController.champion.maxHP)
		{
			if (!championController.isPlayer) yield break;
			TooltipSystem.instance.ShowError("Your health is full!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}
		yield return StartCoroutine(championController.hand.UseCard(this));
		switch (cardData.cardFunctions.secondaryFunction)
		{
			case "":
				yield return StartCoroutine(championController.Heal(1));
				break;
			default:
				yield return StartCoroutine(championController.Heal(int.Parse(cardData.cardFunctions.secondaryFunction)));
				break;
		}
	}
	public IEnumerator DrawFunction(ChampionController championController = null)
	{
		if (championController is null) championController = owner;

		yield return StartCoroutine(championController.hand.UseCard(this));
		switch (cardData.cardFunctions.secondaryFunction)
		{
			case "":
				yield return StartCoroutine(championController.hand.Deal(1, championController.hand.deck.Filter(cardData.cardColor), true));
				break;
			default:
				yield return StartCoroutine(championController.hand.Deal(int.Parse(cardData.cardFunctions.secondaryFunction), championController.hand.deck.Filter(cardData.cardColor), true));
				break;
		}
	}
	public IEnumerator EquipWeaponFunction(ChampionController championController = null)
	{
		if (championController is null) championController = owner;

		if (championController.equippedWeapon is {})
		{
			if (!championController.isPlayer) yield break;
			TooltipSystem.instance.ShowError("You already have a weapon equipped!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			yield break;
		}

		yield return StartCoroutine(championController.hand.UseCard(this));
		Weapon weapon = new(((WeaponCardData) cardData).weaponScriptableObject, championController);
	}

	#region Pointer Events

	public void OnClick()
	{
		StartCoroutine(CardSelect());
	}
	public virtual void OnPointerEnter(PointerEventData eventData)
	{
		delayIDs.Add(LeanTween.delayedCall(0.75f, () =>
		{
			if (this is { })
            {
				TooltipSystem.instance.cardTooltip.Setup(this);
				delayIDs.Add(LeanTween.delayedCall(0.25f, () =>
				{
					TooltipSystem.instance.ShowCard(this);
				}).uniqueId);
			}
		}).uniqueId);
	}
	public virtual void OnPointerExit(PointerEventData eventData)
	{
		foreach (int delayID in delayIDs)
        {
			LeanTween.cancel(delayID);
		}
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.CardTooltip);
	}

	#endregion
}
