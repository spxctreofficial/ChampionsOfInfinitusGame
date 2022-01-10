using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;
using EZCameraShake;
using TMPro;

public enum DamageType { Melee, Ranged, Fire, Lightning, Shadow, Unblockable }

public class ChampionController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler {
	[Header("Champion")]
	public Champion champion;
	[HideInInspector]
	public Hand hand;
	[HideInInspector]
	public MatchStatistic matchStatistic;
	public ChampionAbilityFeed abilityFeed;
	[SerializeField]
	private Button championButton;
	[SerializeField]
	private Image championImage;
	[SerializeField]
	private TMP_Text nameText, healthText, cardsText;
	public ChampionParticleController championParticleController;
	
	[Header("Variables")]
	[HideInInspector]
	public int maxHP;
	public int currentHP;

	[HideInInspector]
	public int attackDamage;
	[HideInInspector]
	public DamageType attackDamageType;
	[HideInInspector]
	public string attackName;

	public List<Ability> abilities;

	[HideInInspector]
	public int discardAmount, spadesBeforeExhaustion, heartsBeforeExhaustion, diamondsBeforeExhaustion;
	[HideInInspector]
	public bool isPlayer, isMyTurn, isAttacking, currentlyTargeted, hasAttacked, hasDefended, isDead;
	public string team;
	public ChampionSlot slot;
	[HideInInspector]
	public ChampionController currentTarget, currentNemesis, currentOwner;
	public List<ChampionController> teamMembers = new List<ChampionController>();
	[HideInInspector]
	public Card attackingCard, defendingCard;
	[HideInInspector]
	public bool isUltReady;
	
	// Click & Hold
	private float secondsHeld;
	private bool isHolding;

	private static int delayID;

	private void Start() {
		ChampionSetup();
	}
	private void Update() {
		AppearanceUpdater();

		if (isHolding) {
			secondsHeld += Time.deltaTime;
			
			if (secondsHeld >= 1f) {
				secondsHeld = 0f;
				isHolding = false;
				ChampionInfoPanel.Create(champion).transform.SetParent(GameController.instance.gameArea.transform, false);
			}
		}
	}

	/// <summary>
	/// Sets up the champion's statistics.
	/// This is called automatically on Start().
	/// </summary>
	public void ChampionSetup() {
		name = champion.championName;

		// References
		championImage.sprite = champion.avatar;

		// Statistics
		maxHP = champion.maxHP;
		currentHP = champion.currentHP;

		attackDamage = champion.attackDamage;
		attackDamageType = champion.attackDamageType;
		attackName = champion.attackName;

		foreach (AbilityScriptableObject abilityScriptableObject in champion.abilities) {
			Ability ability = gameObject.AddComponent<Ability>();
			ability.Setup(this, abilityScriptableObject);
			abilities.Add(ability);
		}

		discardAmount = 0;
		ResetExhaustion();
		isMyTurn = false;
		isAttacking = false;
		currentlyTargeted = false;
		hasAttacked = false;
		hasDefended = false;
		isDead = false;
		currentTarget = null;
		currentNemesis = null;
		attackingCard = null;
		defendingCard = null;
		isUltReady = false;
	}

	/// <summary>
	/// Attacks a defined ChampionController from the perspective of the ChampionController this is called on.
	/// </summary>
	/// <param name="target"></param>
	/// <returns></returns>
	public IEnumerator Attack(ChampionController target) {
		yield return StartCoroutine(target.Damage(attackDamage, attackDamageType, this));
	}
	/// <summary>
	/// Damage this ChampionController.
	/// Set AbilityCheck to false to disable ability checking.
	/// </summary>
	/// <param name="amount"></param>
	/// <param name="damageType"></param>
	/// <param name="source"></param>
	/// <param name="silent"></param>
	/// <param name="abilityCheck"></param>
	/// <returns></returns>
	public IEnumerator Damage(int amount, DamageType damageType, ChampionController source = null, bool silent = false, bool abilityCheck = true) {
		if (isDead) {
			Debug.Log(champion.championName + " is dead!");
			yield break;
		}

		// Damage Calculation
		if (source is {}) {
			foreach (Ability ability in source.abilities) {
				amount += ability.DamageCalculationBonusSource(amount, damageType);
			}
		}
		foreach (Ability ability in abilities) {
			amount += ability.DamageCalculationBonus(amount, damageType);
		}
		currentHP = Mathf.Max(currentHP - amount, 0);
		
		// MatchStatistic Update
		matchStatistic.totalDamageReceived += amount;
		if (source != null) {
			source.matchStatistic.totalDamageDealt += amount;

			foreach (DamageHistory damageHistory in source.matchStatistic.damageHistories) {
				if (damageHistory.dealtAgainst != this) continue;
				damageHistory.amount += amount;
			}

			// Nemesis System
			foreach (DamageHistory damageHistory in source.matchStatistic.damageHistories) {
				if (damageHistory.dealtAgainst != this) continue;

				DamageHistory myDamageHistory = damageHistory;
				if (myDamageHistory.amount > source.matchStatistic.totalDamageDealt / 2 && myDamageHistory.attacksAgainst >= 2) {
					if (currentNemesis == null) currentNemesis = source;
				}
				break;
			}

			if (currentNemesis != null && currentNemesis == source) {
				foreach (ChampionController teammate in teamMembers) {
					foreach (DamageHistory damageHistory in currentNemesis.matchStatistic.damageHistories) {
						if (damageHistory.dealtAgainst != this) continue;
						float chance = damageHistory.attacksAgainst / (GameController.instance.roundsElapsed + 1f) >= 0.65f ? 0.7f : 0.4f;
						chance -= teammate.currentNemesis == null ? 0f : 0.2f;
						chance += teammate.currentOwner == this ? 2f : 0f;
						if (Random.Range(0f, 1f) < chance) teammate.currentNemesis = currentNemesis;
						break;
					}
				}
			}

			// Slight chance for nemesis role to be removed.
			if (source.currentNemesis == this) {
				if (Random.Range(0f, 1f) < 0.15f) source.currentNemesis = null;
			}
		}

		// Effects
		float magnitude;
		switch (damageType) {
			case DamageType.Melee:
				magnitude = 20f;
				if (!silent) AudioController.instance.Play("swordimpact0" + Random.Range(1, 3));
				break;
			case DamageType.Ranged:
				magnitude = 12f;
				break;
			case DamageType.Fire:
				magnitude = 8f;
				if (!silent) AudioController.instance.Play("fireimpact");
				break;
			case DamageType.Lightning:
				magnitude = 15f;
				if (!silent) AudioController.instance.Play("lightningimpact");
				break;
			case DamageType.Shadow:
				magnitude = 10f;
				if (!silent) AudioController.instance.Play("unblockabledamage");
				break;
			default:
				magnitude = 5f;
				if (!silent) AudioController.instance.Play("unblockabledamage");
				break;

		}
		StartCoroutine(slot.ShakeOccupant(0.5f, magnitude));
		CameraShaker.Instance.ShakeOnce(Mathf.Max(magnitude / 2, 6.5f) + Mathf.Max(amount - 25, 0) * 0.1f, Mathf.Max(magnitude / 5, 4f), 0f, 0.25f);
		championParticleController.PlayEffect(championParticleController.BloodSplatter);

		// Death Check
		isDead = DeathCheck();
		if (isDead && source != null) source.matchStatistic.killCount++;
		yield return StartCoroutine(GameController.instance.GameEndCheck());

		// Ability Check
		if (abilityCheck == false) yield break;
		foreach (Ability ability in abilities) {
			yield return StartCoroutine(ability.OnDamage(amount));
		}
		foreach (ChampionController champion in GameController.instance.champions) {
			if (champion == this || champion.isDead) continue;
			foreach (Ability ability in abilities) {
				yield return StartCoroutine(ability.OnDamage(this, amount));
			}
		}
	}
	/// <summary>
	/// Heals this ChampionController.
	/// Set AbilityCheck to false to disable ability checking.
	/// </summary>
	/// <param name="amount"></param>
	/// <param name="abilityCheck"></param>
	/// <returns></returns>
	public IEnumerator Heal(int amount, bool abilityCheck = true) {
		if (isDead) {
			Debug.Log(champion.championName + " is dead!");
			yield break;
		}

		int currentHPCache = currentHP;
		currentHP = Mathf.Min(currentHP + amount, maxHP);
		matchStatistic.totalAmountHealed += currentHP - currentHPCache;
		AudioController.instance.Play("heal");

		if (abilityCheck == false) yield break;
		foreach (Ability ability in abilities) {
			yield return StartCoroutine(ability.OnHeal(amount));
		}
		foreach (ChampionController champion in GameController.instance.champions) {
			if (champion == this || champion.isDead) continue;
			foreach (Ability ability in abilities) {
				yield return StartCoroutine(ability.OnHeal(this, amount));
			}
		}
	}
	/// <summary>
	/// Spawns and returns a ChampionController as a minion of this ChampionController.
	/// </summary>
	/// <param name="champion"></param>
	/// <param name="slot"></param>
	/// <param name="spawnAsPlayer"></param>
	/// <returns></returns>
	public ChampionController SpawnAsMinion(Champion champion, ChampionSlot slot = null, bool spawnAsPlayer = false) {
		// Prerequisites
		if (slot == null) slot = ChampionSlot.FindNextVacantSlot();

		// Spawning
		ChampionController championController = Instantiate(PrefabManager.instance.championTemplate, Vector2.zero, Quaternion.identity).GetComponent<ChampionController>();
		championController.champion = champion;
		championController.team = team;
		championController.currentOwner = this;
		GameController.instance.champions.Add(championController);
		teamMembers.Add(championController);
		slot.SetOccupant(championController);
		championController.transform.SetParent(GameController.instance.gameArea.transform, false);

		// Champion Dependencies
		Hand hand = spawnAsPlayer ? GameController.instance.playerHand : Instantiate(PrefabManager.instance.handPrefab, new Vector3(-3000, 3000), Quaternion.identity).GetComponent<Hand>();
		hand.transform.SetParent(GameController.instance.gameArea.transform, false);


		// Dependency Setup
		IEnumerator Setup() {
			yield return null;
			hand.SetOwner(championController);

			yield return StartCoroutine(championController.hand.Deal(4, false, true, false));
		}
		StartCoroutine(Setup());

		// Returning the Spawned Champion
		return championController;
	}
	
	#region Death Functions
	/// <summary>
	/// Checks if this ChampionController is dead.
	/// </summary>
	/// <returns></returns>
	private bool DeathCheck() {
		if (currentHP != 0) return false;

		StartCoroutine(DeathDiscard());
		foreach (ChampionController champion in GameController.instance.champions) {
			if (champion.currentNemesis != this) continue;
			champion.currentNemesis = null;
		}

		return true;
	}
	private IEnumerator DeathDiscard() {
		Card[] cards = hand.cards.ToArray();
		foreach (Card card in cards) {
			StartCoroutine(hand.Discard(card, false, true, false));
			card.discardFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, Color.black);
			card.discardFeed.text = "DEAD";
			yield return new WaitForSeconds(0.1f);
		}
	}
	#endregion
	
	/// <summary>
	/// Resets this ChampionController's exhaustion.
	/// </summary>
	public void ResetExhaustion() {
		spadesBeforeExhaustion = 1;
		heartsBeforeExhaustion = 3;
		diamondsBeforeExhaustion = 1;
	}
	/// <summary>
	/// Sets this ChampionController's hand.
	/// </summary>
	/// <param name="hand"></param>
	public void SetHand(Hand hand) {
		this.hand = hand;
		hand.owner = this;
	}
	private void AppearanceUpdater() {
		nameText.text = champion.championName;
		healthText.text = isDead ? "DEAD" : currentHP.ToString();
		if (hand != null) cardsText.text = hand.GetCardCount().ToString();
		if (currentHP <= 0) {
			healthText.color = new Color32(100, 100, 100, 255);
			championImage.color = Color.gray;
			ParticleSystem.EmissionModule bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = 0f;
			return;
		}
		if (currentHP <= 0.6f * maxHP) {
			healthText.color = currentHP <= 0.3f * maxHP ? new Color32(255, 0, 0, 255) : new Color32(255, 255, 0, 255);
			ParticleSystem.EmissionModule bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = currentHP <= 0.3f * maxHP ? 20f : 8f;
		}
		else {
			healthText.color = new Color32(0, 255, 0, 255);
			ParticleSystem.EmissionModule bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = 0f;
		}
	}

	// /// <summary>
	// /// Show the AbilityFeed.
	// /// </summary>
	// /// <param name="text"></param>
	// /// <param name="duration"></param>
	// public void ShowAbilityFeed(string text, float duration = 5f) {
	// 	if (abilityFeed.text != text || !abilityFeed.IsActive()) {
	// 		abilityFeed.gameObject.SetActive(true);
	// 		abilityFeed.text = text;
	// 	}
	//
	// 	abilityFeed.transform.localScale = Vector3.zero;
	// 	LeanTween.scale(abilityFeed.GetComponent<RectTransform>(), Vector3.one, 0.1f).setEaseInOutQuad().setOnComplete(() => {
	// 		// StartCoroutine(ShakeImage(0.2f, 10f, abilityFeed.transform));
	// 		LeanTween.delayedCall(duration, HideAbilityFeed);
	// 	});
	// }
	// /// <summary>
	// /// Hide the AbilityFeed.
	// /// </summary>
	// public void HideAbilityFeed() {
	// 	LeanTween.scale(abilityFeed.GetComponent<RectTransform>(), Vector3.zero, 0.15f).setEaseInOutQuad().setOnComplete(() => {
	// 		abilityFeed.gameObject.SetActive(false);
	// 	});
	// }

	// Pointer Events
	public void OnClick() {
		if (isPlayer) {
			TooltipSystem.instance.ShowError("You cannot select yourself as a target!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			return;
		}

		foreach (ChampionController champion in GameController.instance.champions) {
			if (!champion.isAttacking || !champion.isPlayer || isDead) continue;
			
			bool canBeTargeted = true;
			foreach (Ability ability in abilities) {
				canBeTargeted = ability.CanBeTargetedByAttack();
			}
			if (!canBeTargeted) return;

			if (champion.currentTarget != null && champion.currentTarget != this) {
				champion.currentTarget.championParticleController.RedGlow.Stop();
				champion.currentTarget.championParticleController.RedGlow.Clear();
			}
			else {
				foreach (ChampionController selectedChampion in GameController.instance.champions) {
					selectedChampion.championParticleController.GreenGlow.Stop();
					selectedChampion.championParticleController.GreenGlow.Clear();
				}
			}
			
			champion.currentTarget = this;
			champion.currentTarget.championParticleController.PlayEffect(champion.currentTarget.championParticleController.RedGlow);
			if (champion.team.Contains(team)) {
				TooltipSystem.instance.ShowError("This champion is on your team!");
				LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			}
			
			if (champion.attackingCard is {}) {
				GameController.instance.confirmButton.Show();
				GameController.instance.confirmButton.textBox.text = "Confirm";
			}
		}
	}
	
	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(0.5f, () => {

			string body = "Health: " + currentHP + "/" + maxHP; // health
			body += "\n" + attackName + " (Attack): " + attackDamage + " " + champion.attackDamageType + " Damage"; // attack & damage
			body += "\nCards: " + hand.GetCardCount(); // card amount

			body += currentNemesis == null ? "\nNemesis: None" : "\nNemesis: " + currentNemesis.champion.championName; // nemesis
			body += "\n\nCLICK & HOLD FOR MORE INFO";
			TooltipSystem.instance.Show(body, champion.championName); // show the tooltip
		}).uniqueId;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
	public void OnPointerDown(PointerEventData eventData) {
		if (eventData.button == PointerEventData.InputButton.Left) {
			TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
			LeanTween.cancel(delayID);
			isHolding = true;
		}
	}
	public void OnPointerUp(PointerEventData eventData) {
		isHolding = false;
	}
}
