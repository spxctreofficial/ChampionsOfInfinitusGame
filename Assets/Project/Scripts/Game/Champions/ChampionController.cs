using System.Collections;
using System.Collections.Generic;
using EZCameraShake;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChampionController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	[Header("Champion")] public Champion champion;

	[HideInInspector] public Hand hand;

	[HideInInspector] public MatchStatistic matchStatistic;

	public ChampionAbilityFeed abilityFeed;

	[SerializeField] private Image championImage;

	[SerializeField] private TMP_Text nameText, heartsText, weaponDamageText, weaponDurabilityText, cardsText;

	[SerializeField] private CanvasGroup heartsStats, weaponStats;
	public ChampionParticleController championParticleController;

	[Header("Variables")] public int currentHP;

	public Weapon equippedWeapon;

	public List<Ability> abilities;
	public int currentStamina;

	[HideInInspector] public int discardAmount;

	[HideInInspector] public bool isPlayer, isMyTurn, isDead;

	public string team;
	public ChampionSlot slot;

	[HideInInspector] public ChampionController currentNemesis, currentOwner;

	public List<ChampionController> teamMembers = new();

	private int delayID;
	private readonly List<int> delayIDs2 = new();
	private bool isHolding;

	// Click & Hold
	private float secondsHeld;

	private void Start()
	{
		name = champion.championName;

		// References
		championImage.sprite = champion.avatar;

		// Statistics
		currentHP = champion.maxHP;
		equippedWeapon = new Weapon(champion.signatureWeapon, this);
		currentStamina = 0;

		foreach (AbilityScriptableObject abilityScriptableObject in champion.abilities)
		{
			Ability ability = gameObject.AddComponent<Ability>();
			ability.Setup(this, abilityScriptableObject);
			abilities.Add(ability);
		}

		isDead = false;
		currentNemesis = null;
	}

	private void Update()
	{
		AppearanceUpdater();

		if (isHolding)
		{
			secondsHeld += Time.deltaTime;

			if (secondsHeld >= 1f)
			{
				secondsHeld = 0f;
				isHolding = false;
				ChampionInfoPanel.Create(champion).transform.SetParent(GameManager.instance.gameArea.transform, false);
			}
		}
	}

	/// <summary>
	///     Damage this ChampionController.
	///     Set AbilityCheck to false to disable ability checking.
	/// </summary>
	/// <param name="amount"></param>
	/// <param name="damageType"></param>
	/// <param name="source"></param>
	/// <param name="silent"></param>
	/// <param name="abilityCheck"></param>
	/// <returns></returns>
	public IEnumerator Damage(int amount, DamageType damageType, ChampionController source = null, bool silent = false, bool abilityCheck = true)
	{
		if (isDead)
		{
			Debug.Log(champion.championName + " is dead!");
			yield break;
		}

		// Damage Calculation
		if (source is { })
			foreach (Ability ability in source.abilities)
				amount += ability.DamageCalculationBonusSource(amount, damageType);
		foreach (Ability ability in abilities) amount += ability.DamageCalculationBonus(amount, damageType);
		currentHP = Mathf.Max(currentHP - amount, 0);

		// MatchStatistic Update
		matchStatistic.totalDamageReceived += amount;
		if (source != null)
		{
			source.matchStatistic.totalDamageDealt += amount;

			foreach (DamageHistory damageHistory in source.matchStatistic.damageHistories)
			{
				if (damageHistory.dealtAgainst != this) continue;
				damageHistory.amount += amount;
			}

			// Nemesis System
			foreach (DamageHistory damageHistory in source.matchStatistic.damageHistories)
			{
				if (damageHistory.dealtAgainst != this) continue;

				DamageHistory myDamageHistory = damageHistory;
				if (myDamageHistory.amount > source.matchStatistic.totalDamageDealt / 2 && myDamageHistory.attacksAgainst >= 2)
					if (currentNemesis == null)
						currentNemesis = source;
				break;
			}

			if (currentNemesis != null && currentNemesis == source)
				foreach (ChampionController teammate in teamMembers)
				foreach (DamageHistory damageHistory in currentNemesis.matchStatistic.damageHistories)
				{
					if (damageHistory.dealtAgainst != this) continue;
					float chance = damageHistory.attacksAgainst / (GameManager.instance.roundsElapsed + 1f) >= 0.65f ? 0.7f : 0.4f;
					chance -= teammate.currentNemesis == null ? 0f : 0.2f;
					chance += teammate.currentOwner == this ? 2f : 0f;
					if (Random.Range(0f, 1f) < chance) teammate.currentNemesis = currentNemesis;
					break;
				}

			// Slight chance for nemesis role to be removed.
			if (source.currentNemesis == this)
				if (Random.Range(0f, 1f) < 0.15f)
					source.currentNemesis = null;
		}

		// Effects
		float magnitude;
		switch (damageType)
		{
			case DamageType.Melee:
				magnitude = 20f;
				if (!silent) AudioManager.instance.Play("swordimpact0" + Random.Range(1, 3));
				break;
			case DamageType.Ranged:
				magnitude = 12f;
				break;
			case DamageType.Fire:
				magnitude = 8f;
				if (!silent) AudioManager.instance.Play("fireimpact");
				break;
			case DamageType.Lightning:
				magnitude = 15f;
				if (!silent) AudioManager.instance.Play("lightningimpact");
				break;
			case DamageType.Shadow:
				magnitude = 10f;
				if (!silent) AudioManager.instance.Play("unblockabledamage");
				break;
			default:
				magnitude = 5f;
				if (!silent) AudioManager.instance.Play("unblockabledamage");
				break;
		}

		StartCoroutine(slot.ShakeOccupant(0.5f, magnitude));
		CameraShaker.Instance.ShakeOnce(Mathf.Max(magnitude / 2, 6.5f) + Mathf.Max(amount - 25, 0) * 0.1f, Mathf.Max(magnitude / 5, 4f), 0f, 0.25f);
		championParticleController.PlayEffect(championParticleController.bloodSplatter);

		// Death Check
		isDead = DeathCheck();
		if (isDead && source != null) source.matchStatistic.killCount++;
		yield return StartCoroutine(GameManager.instance.GameEndCheck());

		// Ability Check
		if (abilityCheck == false) yield break;
		foreach (Ability ability in abilities) yield return StartCoroutine(ability.OnDamage(amount));
		foreach (ChampionController championController in GameManager.instance.champions)
		{
			if (championController == this || championController.isDead) continue;
			foreach (Ability ability in abilities) yield return StartCoroutine(ability.OnDamage(this, amount));
		}
	}

	/// <summary>
	///     Heals this ChampionController.
	///     Set AbilityCheck to false to disable ability checking.
	/// </summary>
	/// <param name="amount"></param>
	/// <param name="abilityCheck"></param>
	/// <returns></returns>
	public IEnumerator Heal(int amount, bool abilityCheck = true)
	{
		if (isDead)
		{
			Debug.Log(champion.championName + " is dead!");
			yield break;
		}

		int currentHPCache = currentHP;
		currentHP = Mathf.Min(currentHP + amount, champion.maxHP);
		matchStatistic.totalAmountHealed += currentHP - currentHPCache;
		AudioManager.instance.Play("heal");

		if (abilityCheck == false) yield break;
		foreach (Ability ability in abilities) yield return StartCoroutine(ability.OnHeal(amount));
		foreach (ChampionController championController in GameManager.instance.champions)
		{
			if (championController == this || championController.isDead) continue;
			foreach (Ability ability in abilities) yield return StartCoroutine(ability.OnHeal(this, amount));
		}
	}

	public void SetHand(Hand hand)
	{
		this.hand = hand;
		hand.owner = this;
	}

	private void AppearanceUpdater()
	{
		nameText.text = champion.championName;
		heartsText.text = currentHP.ToString();
		weaponDamageText.text = equippedWeapon is null ? "N/A" : equippedWeapon.EffectiveDamage.ToString();
		weaponDurabilityText.text = equippedWeapon is null ? "N/A" : equippedWeapon.currentDurability.ToString();
		if (hand != null) cardsText.text = hand.GetCardCount().ToString();
		if (currentHP <= 0)
		{
			heartsText.color = new Color32(100, 100, 100, 255);
			championImage.color = Color.gray;
			ParticleSystem.EmissionModule bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = 0f;
			return;
		}

		if (currentHP <= 0.6f * champion.maxHP)
		{
			heartsText.color = currentHP <= 0.3f * champion.maxHP ? new Color32(255, 0, 0, 255) : new Color32(255, 255, 0, 255);
			ParticleSystem.EmissionModule bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = currentHP <= 0.3f * champion.maxHP ? 20f : 8f;
		}
		else
		{
			heartsText.color = new Color32(0, 255, 0, 255);
			ParticleSystem.EmissionModule bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = 0f;
		}
	}

	#region Death Functions

	/// <summary>
	///     Checks if this ChampionController is dead.
	/// </summary>
	/// <returns></returns>
	private bool DeathCheck()
	{
		if (currentHP != 0) return false;

		StartCoroutine(DeathDiscard());
		foreach (ChampionController championController in GameManager.instance.champions)
		{
			if (championController.currentNemesis != this) continue;
			championController.currentNemesis = null;
		}

		return true;
	}

	private IEnumerator DeathDiscard()
	{
		Card[] cards = hand.cards.ToArray();
		foreach (Card card in cards)
		{
			StartCoroutine(hand.Discard(card, false, true, false));
			card.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.black);
			card.discardFeed.text = "DEAD";
			yield return new WaitForSeconds(0.1f);
		}
	}

	#endregion

	#region AI Card Logic

	public IEnumerator CardLogic()
	{
		if (isDead)
		{
			Debug.LogWarning("Attempted to apply logic to dead champion!");
			GameManager.instance.NextTurnCalculator(this);
			yield break;
		}

		yield return new WaitForSeconds(0.5f);

		if (equippedWeapon is null)
		{
			Card weaponCard = hand.GetPlayableWeaponCard();

			if (weaponCard is { })
			{
			}
		}


		foreach (Transform child in hand.transform)
		{
			Card card = child.GetComponent<Card>();

			if (currentStamina < card.EffectiveStaminaRequirement)
			{
				Debug.Log("Cannot play this card due to stamina requirement!");
				continue;
			}

			if (card.cardData is WeaponCardData)
			{
				Debug.Log("Not looking for a weapon currently.");
				continue;
			}

			switch (card.cardData.cardFunctions.primaryFunction)
			{
				case "attack":
					if (equippedWeapon is { })
					{
						ChampionController target = SelectTarget();

						if (target is { }) yield return card.AttackFunction(this, target);
					}

					break;
				case "block":
				case "parry":
					continue;
				case "draw":
					yield return StartCoroutine(card.DrawFunction(this));
					break;
				case "heal":
					yield return StartCoroutine(card.HealFunction(this));
					break;
			}

			yield return new WaitForSeconds(0.25f);
		}

		GameManager.instance.StartEndPhase(this);
	}

	private ChampionController SelectTarget()
	{
		foreach (ChampionController selectedChampionController in GameManager.instance.champions)
		{
			if (selectedChampionController.isDead || selectedChampionController.teamMembers.Contains(this) || selectedChampionController == this) continue;
			if (selectedChampionController.currentHP - equippedWeapon.EffectiveDamage > 0) continue;

			if (Random.Range(0f, 1f) < (selectedChampionController.hand.GetCardCount() <= 2 ? 1f : 0.85f)) return selectedChampionController;
		}

		bool didntAttackPlayer = false;
		foreach (ChampionController selectedChampionController in GameManager.instance.champions)
		{
			if (selectedChampionController.isDead || selectedChampionController.teamMembers.Contains(this) || selectedChampionController == this) continue;

			// Standard Targeting
			float chance = 0.7f;
			chance = didntAttackPlayer ? 0.8f : chance;
			chance += currentHP >= 0.8f * champion.maxHP ? 0.1f : 0f;
			if (Random.Range(0f, 1f) < chance) return selectedChampionController;

			if (selectedChampionController.isPlayer) didntAttackPlayer = true;
		}

		Debug.Log("Couldn't find suitable attacker! Returning null");
		return null;
	}

	#endregion

	#region Pointer Events

	public void OnClick()
	{
		if (FightManager.fightInstance is null || FightManager.fightInstance.Attacker is null) return;
		if (!FightManager.fightInstance.Attacker.isPlayer) return;
		if (FightManager.fightInstance.Attacker == this)
		{
			TooltipSystem.instance.ShowError("You cannot select yourself as a target!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			return;
		}

		if (FightManager.fightInstance.Attacker.team.Contains(team))
		{
			TooltipSystem.instance.ShowError("This champion is on your team!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
		}

		FightManager.fightInstance.Defender = this;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		delayID = LeanTween.delayedCall(0.5f, () =>
		{
			string body = "Health: " + currentHP + "/" + champion.maxHP;
			body += equippedWeapon is null ? " None" : "\nWeapon: " + equippedWeapon.weaponScriptableObject.weaponName + " (" + equippedWeapon.currentDurability + "/" + equippedWeapon.weaponScriptableObject.maxDurability + " Durability, " + equippedWeapon.EffectiveDamage + " Damage)";
			body += "\nCards: " + hand.GetCardCount();
			body += "\nStamina: (" + currentStamina + "/" + GameManager.instance.CurrentRoundStamina + ")";

			body += currentNemesis is null ? "\nNemesis: None" : "\nNemesis: " + currentNemesis.champion.championName;
			body += "\n\nCLICK & HOLD FOR MORE INFO";

			TooltipSystem.instance.Show(body, champion.championName);
		}).uniqueId;

		foreach (int delayID in delayIDs2) LeanTween.cancel(delayID);
		delayIDs2.Clear();
		delayIDs2.Add(LeanTween.alphaCanvas(heartsStats, 0f, 0.2f).setEaseInOutQuart().uniqueId);
		delayIDs2.Add(LeanTween.alphaCanvas(weaponStats, 1f, 0.2f).setEaseInOutQuart().uniqueId);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);

		foreach (int delayID in delayIDs2) LeanTween.cancel(delayID);
		delayIDs2.Clear();
		delayIDs2.Add(LeanTween.alphaCanvas(heartsStats, 1f, 0.2f).setEaseInOutQuart().uniqueId);
		delayIDs2.Add(LeanTween.alphaCanvas(weaponStats, 0f, 0.2f).setEaseInOutQuart().uniqueId);
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (eventData.button == PointerEventData.InputButton.Left)
		{
			TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
			LeanTween.cancel(delayID);
			isHolding = true;
		}
	}

	public void OnPointerUp(PointerEventData eventData) { isHolding = false; }

	#endregion
}