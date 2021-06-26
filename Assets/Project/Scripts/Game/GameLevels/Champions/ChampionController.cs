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

public class ChampionController : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
	// Champion
	public Champion champion;
	[HideInInspector]
	public Hand hand;
	[HideInInspector]
	public AbilityPanel abilityPanel;
	public ChampionAbilityFeed abilityFeed;
	[SerializeField]
	private Button championButton;
	[SerializeField]
	private Image championImage;
	[SerializeField]
	private TMP_Text nameText, healthText, cardsText;
	public ChampionParticleController championParticleController;

	// Identification & Basic Information
	[HideInInspector]
	public string championName;
	[HideInInspector]
	public string championID;
	[HideInInspector]
	public Sprite avatar;
	[HideInInspector]
	public string description;
	[HideInInspector]
	public Champion.Gender gender;
	[HideInInspector]
	public Champion.Faction faction;
	[HideInInspector]
	public Champion.Race race;

	// Statistics
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
	public bool isPlayer, isMyTurn, isAttacking, currentlyTargeted, hasAttacked, hasDefended;
	public bool isDead;
	public string team;
	public ChampionSlot slot;
	[HideInInspector]
	public ChampionController currentTarget, currentNemesis, currentOwner;
	public List<ChampionController> teamMembers = new List<ChampionController>();
	[HideInInspector]
	public Card attackingCard, defendingCard;
	[HideInInspector]
	public bool isUltReady;

	private static int delayID;

	private void Start() {
		ChampionSetup();
	}
	private void Update() {
		AppearanceUpdater();
	}

	/// <summary>
	/// Sets up the champion's statistics.
	/// This is called automatically on Start().
	/// </summary>
	public void ChampionSetup() {
		// Identification & Basic Information
		championName = champion.championName;
		name = champion.championName;
		championID = champion.championID;
		avatar = champion.avatar;
		description = champion.description;
		gender = champion.gender;
		faction = champion.faction;
		race = champion.race;

		// References
		championImage.sprite = avatar;

		// Statistics
		maxHP = champion.maxHP;
		currentHP = champion.currentHP;

		attackDamage = champion.attackDamage;
		attackDamageType = champion.attackDamageType;
		attackName = champion.attackName;

		abilities = champion.abilities;

		discardAmount = 0;
		ResetExhaustion();
		isPlayer = this == GameController.instance.champions[0];
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
			Debug.Log(championName + " is dead!");
			yield break;
		}

		// Damage Calculation
		var currentHPCache = currentHP;
		foreach (Transform child in abilityPanel.panel.transform) {
			var ability = child.GetComponent<AbilityController>();
			amount += ability.DamageCalculationBonus(amount, damageType);
		}
		currentHP = Mathf.Max(currentHP - amount, 0);
		
		// MatchStatistic Update
		GetMatchStatistic().totalDamageReceived += currentHPCache - currentHP;
		if (source != null) {
			source.GetMatchStatistic().totalDamageDealt += currentHPCache - currentHP;

			foreach (var damageHistory in source.GetMatchStatistic().damageHistories) {
				if (damageHistory.dealtAgainst != this) continue;
				damageHistory.amount += amount;
			}

			// Nemesis System
			var myDamageHistory = (DamageHistory)null;
			foreach (var damageHistory in source.GetMatchStatistic().damageHistories) {
				if (damageHistory.dealtAgainst != this) continue;

				myDamageHistory = damageHistory;
				if (myDamageHistory.amount > source.GetMatchStatistic().totalDamageDealt / 2 && myDamageHistory.attacksAgainst >= 2) {
					if (currentNemesis == null) currentNemesis = source;
				}
				break;
			}

			if (currentNemesis != null && currentNemesis == source) {
				foreach (var teammate in teamMembers) {
					foreach (var damageHistory in currentNemesis.GetMatchStatistic().damageHistories) {
						if (damageHistory.dealtAgainst != this) continue;
						var chance = damageHistory.attacksAgainst / (GameController.instance.roundsElapsed + 1f) >= 0.65f ? 0.7f : 0.4f;
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
				if (!silent) AudioController.instance.Play("Sword" + Random.Range(1, 3));
				break;
			case DamageType.Ranged:
				magnitude = 12f;
				break;
			case DamageType.Fire:
				magnitude = 8f;
				if (!silent) AudioController.instance.Play("FireBall1");
				break;
			case DamageType.Lightning:
				magnitude = 15f;
				if (!silent) AudioController.instance.Play("Lightning");
				break;
			case DamageType.Shadow:
				magnitude = 10f;
				if (!silent) AudioController.instance.Play("Unblockable1");
				break;
			case DamageType.Unblockable:
				magnitude = 5f;
				if (!silent) AudioController.instance.Play("Unblockable1");
				break;
			default:
				magnitude = 5f;
				if (!silent) AudioController.instance.Play("Unblockable1");
				break;

		}
		StartCoroutine(slot.ShakeOccupant(0.5f, magnitude));
		CameraShaker.Instance.ShakeOnce(Mathf.Min(magnitude / 4, 3f) + (Mathf.Max(amount - 25, 0) * 0.1f), Mathf.Min(magnitude / 5, 4f), 0.1f, 0.2f);
		championParticleController.PlayEffect(championParticleController.BloodSplatter);

		// Death Check
		isDead = DeathCheck();
		if (isDead && source != null) source.GetMatchStatistic().killCount++;
		yield return StartCoroutine(GameController.instance.GameEndCheck());

		// Ability Check
		if (abilityCheck == false) yield break;
		foreach (Transform child in abilityPanel.panel.transform) {
			var ability = child.GetComponent<AbilityController>();
			yield return StartCoroutine(ability.OnDamage(amount));
		}
		foreach (ChampionController champion in GameController.instance.champions) {
			if (champion == this || champion.isDead) continue;
			foreach (Transform child in abilityPanel.panel.transform) {
				var ability = child.GetComponent<AbilityController>();
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
			Debug.Log(championName + " is dead!");
			yield break;
		}

		var currentHPCache = currentHP;
		currentHP = Mathf.Min(currentHP + amount, maxHP);
		GetMatchStatistic().totalAmountHealed += currentHP - currentHPCache;
		AudioController.instance.Play("Heal");

		if (abilityCheck == false) yield break;
		foreach (Transform child in abilityPanel.panel.transform) {
			var ability = child.GetComponent<AbilityController>();
			yield return StartCoroutine(ability.OnHeal(amount));
		}
		foreach (ChampionController champion in GameController.instance.champions) {
			if (champion == this || champion.isDead) continue;
			foreach (Transform child in abilityPanel.panel.transform) {
				var ability = child.GetComponent<AbilityController>();
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
		var championController = Instantiate(GameController.instance.championTemplate, Vector2.zero, Quaternion.identity).GetComponent<ChampionController>();
		championController.champion = champion;
		championController.team = team;
		championController.currentOwner = this;
		GameController.instance.champions.Add(championController);
		teamMembers.Add(championController);
		slot.SetOccupant(championController);
		championController.transform.SetParent(GameController.instance.gameArea.transform, false);

		// Champion Dependencies
		var hand = spawnAsPlayer ? GameController.instance.playerHand : Instantiate(GameController.instance.handPrefab, new Vector3(-3000, 3000), Quaternion.identity).GetComponent<Hand>();
		hand.transform.SetParent(GameController.instance.gameArea.transform, false);

		var abilityPanel = Instantiate(GameController.instance.abilityPanelPrefab, Vector2.zero, Quaternion.identity).GetComponent<AbilityPanel>();
		abilityPanel.transform.SetParent(GameController.instance.gameArea.transform, false);

		// Dependency Setup
		IEnumerator Setup() {
			yield return null;
			hand.SetOwner(championController);
			abilityPanel.Setup(championController);

			yield return StartCoroutine(championController.hand.Deal(4, false, true, false));
		}
		StartCoroutine(Setup());

		// Returning the Spawned Champion
		return championController;
	}
	/// <summary>
	/// Checks if this ChampionController is dead.
	/// </summary>
	/// <returns></returns>
	private bool DeathCheck() {
		if (currentHP != 0) return false;
		StartCoroutine(DeathDiscardAll());
		return true;
	}

	/// <summary>
	/// Discards all cards from this champion's hand.
	/// Currently not quite working.
	/// </summary>
	/// <returns></returns>
	private IEnumerator DeathDiscardAll() {
		yield return new WaitForSeconds(Random.Range(2f, 5f));

		foreach (Transform child in hand.transform) {
			var card = child.gameObject.GetComponent<Card>();
			yield return StartCoroutine(hand.Discard(card));
		}
	}
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
		nameText.text = championName;
		healthText.text = isDead ? "DEAD" : currentHP.ToString();
		if (hand != null) cardsText.text = hand.GetCardCount().ToString();
		if (currentHP <= 0) {
			healthText.color = new Color32(100, 100, 100, 255);
			championImage.color = Color.gray;
			var bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = 0f;
			return;
		}
		if (currentHP <= 0.6f * maxHP) {
			healthText.color = currentHP <= 0.3f * maxHP ? new Color32(255, 0, 0, 255) : new Color32(255, 255, 0, 255);
			var bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = currentHP <= 0.3f * maxHP ? 20f : 8f;
		}
		else {
			healthText.color = new Color32(0, 255, 0, 255);
			var bloodEmission = championParticleController.bloodDrip.emission;
			bloodEmission.rateOverTime = 0f;
		}
	}

	/// <summary>
	/// Returns this ChampionController's match statistic, based on the index of the ChampionController.
	/// </summary>
	/// <returns></returns>
	public MatchStatistic GetMatchStatistic() {
		var index = GameController.instance.champions.IndexOf(this);
		return StatisticManager.instance.matchStatistics[index];
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

		foreach (var champion in GameController.instance.champions) {
			if (!champion.isAttacking || !champion.isPlayer || isDead) continue;
			
			bool canBeTargeted = true;
			foreach (Transform child in abilityPanel.panel.transform) {
				var ability = child.GetComponent<AbilityController>();
				canBeTargeted = ability.CanBeTargetedByAttack();
			}
			if (!canBeTargeted) return;

			if (champion.currentTarget != null && champion.currentTarget != this) {
				champion.currentTarget.championParticleController.RedGlow.Stop();
				champion.currentTarget.championParticleController.RedGlow.Clear();
			}
			else {
				foreach (var selectedChampion in GameController.instance.champions) {
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
			GameController.instance.confirmButton.Show();

			if (champion.attackingCard != null) break;
			GameController.instance.confirmButton.Hide();
		}
	}
	public void OnPointerClick(PointerEventData eventData) {
		if (eventData.button == PointerEventData.InputButton.Right) {
			abilityPanel.OpenPanel();
		}
	}
	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(0.5f, () => {
			string attackType() {
				return attackDamageType switch {
					DamageType.Melee => "Melee",
					DamageType.Ranged => "Ranged",
					DamageType.Fire => "Fire",
					DamageType.Lightning => "Lightning",
					DamageType.Shadow => "Shadow",
					DamageType.Unblockable => "Unblockable",
					_ => throw new ArgumentOutOfRangeException()
				};
			}
			string abilityType(Ability ability) {
				return ability.abilityType switch {
					Ability.AbilityType.Passive => "Passive",
					Ability.AbilityType.Active => "Active",
					Ability.AbilityType.AttackB => "Attack Bonus",
					Ability.AbilityType.DefenseB => "Defense Bonus",
					Ability.AbilityType.Ultimate => "Ultimate",
					_ => throw new ArgumentOutOfRangeException()
				};
			}

			var body = "Health: " + currentHP + "/" + maxHP; // health
			body += "\n" + attackName + " (Attack): " + attackDamage + " " + attackType() + " Damage"; // attack & damage
			body += "\nCards: " + hand.GetCardCount(); // card amount

			body += "\nAbilities:"; // abilities
			if (abilities.Count != 0) {
				foreach (var ability in abilities) body += "\n" + ability.abilityName + " (" + abilityType(ability) + ")"; // print all abilities
			}
			else {
				body += " None";
			}

			body += currentNemesis == null ? "\nNemesis: None" : "\nNemesis: " + currentNemesis.championName; // nemesis
			TooltipSystem.instance.Show(body, championName); // show the tooltip
		}).uniqueId;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
}
