using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class AbilityController : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	public ChampionController champion;
	public Ability ability;

	private static LTDescr delay;

	// Constructors
	public void Setup(ChampionController champion, Ability ability) {
		// Setting up variables
		this.champion = champion;
		this.ability = ability;

		// Setting up appearance
		gameObject.GetComponent<Image>().sprite = ability.sprite;
		switch (ability.abilityType) {
			case Ability.AbilityType.Passive:
			case Ability.AbilityType.AttackB:
			case Ability.AbilityType.DefenseB:
			case Ability.AbilityType.Ultimate:
				gameObject.GetComponent<Button>().interactable = false;
				break;
			default:
				gameObject.AddComponent<SmartHover>();
				break;
		}
	}

	// Click Event
	public void OnClick() {}

	// Checks
	public bool CheckForAbility(string searchCriteria) {
		if (champion.abilities.Count == 0) return false;
		foreach (var ability in champion.abilities) {
			if (!IsExclusive()) return false;
			if (searchCriteria == ability.abilityID) return true;
		}
		return false;
	}
	public bool IsExclusive() {
		foreach (var champion in ability.isExclusiveTo) {
			if (champion == this.champion.champion) return true;
		}
		return false;
	}

	// Triggers
	public IEnumerator OnBeginningPhase() {
		switch (ability.abilityID) {
			case "QuickAssist":
				StartCoroutine(QuickAssist());
				break;
		}
		yield break;
	}
	public IEnumerator OnActionPhase() {
		yield break;
	}
	public IEnumerator OnEndPhase() {
		switch (ability.abilityID) {
			case "Rejuvenation":
				StartCoroutine(Rejuvenation());
				break;
		}
		yield break;
	}
	public IEnumerator OnNextTurnCalculate() {
		yield break;
	}

	public IEnumerator OnDeal(Card card, ChampionController dealtTo) {
		switch (ability.abilityID) {
			case "HopliteTradition":
				StartCoroutine(HopliteTradition(card, dealtTo));
				break;
		}
		yield break;
	}
	public IEnumerator OnDamage(int amount) {
		yield break;
	}
	public int DamageCalculationBonus(int amount, DamageType damageType) {
		switch (ability.abilityID) {
			case "HopliteShield":
				return HopliteShield(amount, damageType);
			default:
				return 0;
		}

	}
	public IEnumerator OnHeal(int amount) {
		switch (ability.abilityID) {
			case "QuickHeal":
				StartCoroutine(QuickHeal(amount));
				break;
		}
		yield break;
	}
	public IEnumerator OnDeath() {
		yield break;
	}

	public IEnumerator OnCombatCalculationAttacker(Card attackingCard, Card defendingCard) {
		yield break;
	}
	public IEnumerator OnCombatCalculationDefender(Card attackingCard, Card defendingCard) {
		switch (ability.abilityID) {
			case "Bojutsu":
				StartCoroutine(Bojutsu(attackingCard));
				break;
		}
		yield break;
	}

	// Ability Methods
	private IEnumerator QuickAssist() {
		if (!champion.isMyTurn) yield break;

		yield return new WaitForSeconds(1f);

		foreach (var selectedChampion in GameController.instance.champions) {
			if (selectedChampion == champion || selectedChampion.isDead || selectedChampion.faction != champion.faction || selectedChampion.faction == Champion.Faction.Undefined) continue;

			champion.hand.Deal(1);
			Debug.Log(ability.abilityName + " was activated for " + champion.name + ". Dealing that champion a card!");
			champion.ShowAbilityFeed(ability.abilityName, 2f);
		}
	}
	private IEnumerator QuickHeal(int amount) {
		Debug.Log(ability.abilityName + " activated for " + champion.name + ", 50% chance to heal for double the amount!");
		if (Random.Range(0f, 1f) < 0.5f || champion.currentHP == champion.maxHP) yield break;

		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(champion.Heal(amount, false));
		champion.ShowAbilityFeed(ability.abilityName, 2f);
	}
	private IEnumerator HopliteTradition(Card card, ChampionController dealtTo) {
		if (!IsExclusive()) yield break;
		if (dealtTo == champion || card.cardValue <= 10) yield break;

		Debug.Log(ability.abilityName + " was activated for " + champion.name + " because " + dealtTo.name + " was dealt a card with a value higher than 10. A 50% chance to heal for 20!");

		if (Random.Range(0f, 1f) < 0.5f && champion.currentHP != champion.maxHP) {
			Debug.Log("Check succeeded! Healing " + champion.name + " for 20.");
			yield return StartCoroutine(champion.Heal(20, false));
			champion.ShowAbilityFeed(ability.abilityName, 2f);
			yield break;
		}
		Debug.Log("Check failed! Nothing happens.");
	}
	private int HopliteShield(int amount, DamageType damageType) {
		if (!IsExclusive()) return 0;

		Debug.Log(ability.abilityName + " was activated for " + champion.name + ". A 20% chance to negate the damage by half!" +
		          "\n This chance is increased to 50% if the damage is fatal.");

		var chance = champion.currentHP - amount <= 0 ? 0.5f : 0.2f;
		if (Random.Range(0f, 1f) < chance) {
			AudioController.instance.Play("ShieldBlock");
			champion.ShowAbilityFeed(ability.abilityName, 2f);
			return -amount / 2;
		}
		return 0;
	}
	private IEnumerator Bojutsu(Card attackingCard) {
		if (!IsExclusive()) yield break;
		if (attackingCard.cardValue < 10) yield break;

		attackingCard.cardValue--;
		Debug.Log(ability.abilityName + " was activated for " + champion.name + " because another champion attacked with a J or higher. " + " That card's value is reduced by 1. It is now " + attackingCard.cardValue);
		champion.ShowAbilityFeed(ability.abilityName);
	}
	private IEnumerator Rejuvenation() {
		if (!IsExclusive()) yield break;
		if (Random.Range(0f, 1f) < 0.5f || champion.currentHP == champion.maxHP) yield break;

		Debug.Log(ability.abilityName + " was activated for " + champion.name + ". Healing for 5.");
		yield return StartCoroutine(champion.Heal(5, false));
		champion.ShowAbilityFeed(ability.abilityName, 2f);
	}






	// Image Shake Borrowed From ChampionController
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
	public void OnPointerEnter(PointerEventData eventData) {
		delay = LeanTween.delayedCall(0.5f, () => {
			var body = ability.abilityDescription;

			string abilityType() {
				return ability.abilityType switch {
					Ability.AbilityType.Passive => "Passive",
					Ability.AbilityType.Active => "Active",
					Ability.AbilityType.AttackB => "Attack Bonus",
					Ability.AbilityType.DefenseB => "Defense Bonus",
					Ability.AbilityType.Ultimate => "Ultimate",
					_ => throw new ArgumentOutOfRangeException()
				};
			}

			body += "\n Ability Type: " + abilityType();

			TooltipSystem.instance.Show(body, ability.abilityName);
		});
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delay.uniqueId);
		TooltipSystem.instance.Hide();
	}
}
