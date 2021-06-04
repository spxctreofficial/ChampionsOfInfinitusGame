using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Random = UnityEngine.Random;

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
	public bool isPlayer, isMyTurn, isAttacking, currentlyTargeted, hasDefended;
	public bool isDead;
	// [HideInInspector]
	public string team;
	[HideInInspector]
	public ChampionController currentTarget, currentNemesis, currentOwner;
	[HideInInspector]
	public Card attackingCard, defendingCard;
	[HideInInspector]
	public bool isUltReady;

	private static LTDescr delay;

	private void Start() {
		ChampionSetup();
	}
	private void Update() {
		TextUpdater();
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
		hasDefended = false;
		isDead = false;
		currentTarget = null;
		currentNemesis = null;
		attackingCard = null;
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
	/// <param name="abilityCheck"></param>
	/// <returns></returns>
	public IEnumerator Damage(int amount, DamageType damageType, ChampionController source = null, bool silent = false, bool abilityCheck = true) {
		var currentHPCache = currentHP;
		foreach (Transform child in abilityPanel.panel.transform) {
			var ability = child.GetComponent<AbilityController>();
			amount += ability.DamageCalculationBonus(amount, damageType);
		}
		currentHP = Mathf.Max(currentHP - amount, 0);
		GetMatchStatistic().totalDamageReceived += currentHPCache - currentHP;
		if (source != null) source.GetMatchStatistic().totalDamageDealt += currentHPCache - currentHP;

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
				if (!silent) AudioController.instance.Play("FireDamage1");
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
		StartCoroutine(ShakeImage(0.2f, magnitude));

		isDead = DeathCheck();
		if (isDead && source != null) source.GetMatchStatistic().killCount++;
		yield return StartCoroutine(GameController.instance.GameEndCheck());

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
	/// <summary>
	/// Updates visual elements of this ChampionController, such as nameText and healthText.
	/// This is automatically called in Update(), so there is no need to manually call this.
	/// </summary>
	private void TextUpdater() {
		nameText.text = championName;
		healthText.text = isDead ? "DEAD" : currentHP.ToString();
		if (hand != null) cardsText.text = hand.GetCardCount().ToString();
		if (currentHP == 0) {
			healthText.color = new Color32(100, 100, 100, 255);
			return;
		}
		if (currentHP <= 0.6f * maxHP) {
			healthText.color = currentHP <= 0.3f * maxHP ? new Color32(255, 0, 0, 255) : new Color32(255, 255, 0, 255);
		}
		else {
			healthText.color = new Color32(0, 255, 0, 255);
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
		foreach (var champion in GameController.instance.champions) {

			if (!champion.isAttacking || !champion.isPlayer || isPlayer || isDead) continue;

			champion.currentTarget = this;
			GameController.instance.playerActionTooltip.text = (champion.team != this.team) ? "Confirm the attack, or change selected card and/or target." : "This champion is on your team! Confirm the attack, or consider changing the selected target and/or selected card.";
			GameController.instance.confirmButton.Show();

			if (champion.attackingCard != null) break;
			GameController.instance.playerActionTooltip.text = (champion.team != this.team) ? "Choose another card to represent your attack, or change selected target." : "This champion is on your team! Consider changing the selected target, or continue by selecting a card to represent your attack.";
			GameController.instance.confirmButton.Hide();
		}
	}
	public void OnPointerClick(PointerEventData eventData) {
		if (eventData.button == PointerEventData.InputButton.Right) {
			abilityPanel.OpenPanel();
		}
	}
	public void OnPointerEnter(PointerEventData eventData) {
		delay = LeanTween.delayedCall(0.5f, () => {
			var body = "Health: " + currentHP + "/" + maxHP;

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

			body += "\n" + attackName + " (Attack): " + attackDamage + " " + attackType() + " Damage" +
			        "\nAbilities:";

			foreach (var ability in abilities) body += "\n" + ability.abilityName + " (" + abilityType(ability) + ")";

			body += "\n Cards: " + hand.GetCardCount();
			TooltipSystem.instance.Show(body, championName);
		});
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delay.uniqueId);
		TooltipSystem.instance.Hide();
	}
	private IEnumerator ShakeImage(float duration, float magnitude) {
		var originalPos = transform.localPosition;

		for (float t = 0; t < 1; t += Time.deltaTime / duration) {
			var x = Random.Range(originalPos.x - 1f * magnitude, originalPos.x + 1f * magnitude);
			var y = Random.Range(originalPos.y - 1f * magnitude, originalPos.y + 1f * magnitude);
			var shake = new Vector3(x, y, originalPos.z);
			transform.localPosition = shake;
			yield return null;
		}
		transform.localPosition = originalPos;
	}
	private IEnumerator ShakeImage(float duration, float magnitude, Transform transform) {
		var originalPos = transform.localPosition;

		for (float t = 0; t < 1; t += Time.deltaTime / duration) {
			var x = Random.Range(originalPos.x - 1f * magnitude, originalPos.x + 1f * magnitude);
			var y = Random.Range(originalPos.y - 1f * magnitude, originalPos.y + 1f * magnitude);
			var shake = new Vector3(x, y, originalPos.z);
			transform.localPosition = shake;
			yield return null;
		}
		transform.localPosition = originalPos;
	}
}
