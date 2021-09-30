using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Ability : MonoBehaviour {
	public ChampionController champion;
	public AbilityScriptableObject abilityScriptableObject;

	public Dictionary<string, int> abilityInts = new Dictionary<string, int>();
	public Dictionary<string, float> abilityFloats = new Dictionary<string, float>();
	public Dictionary<string, bool> abilityBools = new Dictionary<string, bool>();
	public Dictionary<string, StateFeedEntry> affiliatedStateFeedEntries = new Dictionary<string, StateFeedEntry>();
	[HideInInspector]
	public List<string> tags = new List<string>();

	public void Setup(ChampionController champion, AbilityScriptableObject abilityScriptableObject) {
		this.champion = champion;
		this.abilityScriptableObject = abilityScriptableObject;

		StartCoroutine(OnSetup());
	}

	public bool CheckForAbility(string searchCriteria) {
		if (champion.champion.abilities.Count == 0) return false;
		foreach (AbilityScriptableObject ability in champion.champion.abilities) {
			if (!IsExclusive()) return false;
			if (searchCriteria == ability.abilityID) return true;
		}
		return false;
	}
	public bool IsExclusive() {
		if (abilityScriptableObject.isExclusiveTo.Count == 0) return true;
		foreach (Champion champion in abilityScriptableObject.isExclusiveTo) {
			if (champion == this.champion.champion) return true;
		}
		return false;
	}

	/// <summary>
	/// Checks for abilities on setup of the ability.
	/// </summary>
	/// <returns></returns>
	private IEnumerator OnSetup() {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_Persistence":
				abilityInts.Add("persistence", 0);
				break;
		}
		yield break;
	}
	/// <summary>
	/// Checks for abilities on start of Beginning Phase.
	/// </summary>
	/// <returns></returns>
	public IEnumerator OnBeginningPhase() {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_QuickAssist":
				yield return StartCoroutine(QuickAssist());
				break;
		}
	}
	/// <summary>
	/// Checks for abilities on start of Action Phase.
	/// </summary>
	/// <returns></returns>
	public IEnumerator OnActionPhase() {
		yield break;
	}
	/// <summary>
	/// Checks for abilities on start of End Phase.
	/// </summary>
	/// <returns></returns>
	public IEnumerator OnEndPhase() {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_Rejuvenation":
				yield return StartCoroutine(Rejuvenation());
				break;
		}
	}
	/// <summary>
	/// Checks for abilities when calculating the next turn.
	/// </summary>
	/// <returns></returns>
	public IEnumerator OnNextTurnCalculate() {
		yield break;
	}

	/// <summary>
	/// Checks for abilities when `dealtTo` is dealt a card.
	/// </summary>
	/// <param name="card"></param>
	/// <param name="dealtTo"></param>
	/// <returns></returns>
	public IEnumerator OnDeal(Card card, ChampionController dealtTo) {
		yield break;
	}
	/// <summary>
	/// Checks for abilities when this ability's champion is damaged.
	/// </summary>
	/// <param name="amount"></param>
	/// <returns></returns>
	public IEnumerator OnDamage(int amount) {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_Persistence":
				yield return StartCoroutine(PersistenceIncrement());
				break;
		}
	}
	/// <summary>
	/// Checks for ability when `damagedChampion` is damaged.
	/// </summary>
	/// <param name="damagedChampion"></param>
	/// <param name="amount"></param>
	/// <returns></returns>
	public IEnumerator OnDamage(ChampionController damagedChampion, int amount) {
		yield break;
	}
	/// <summary>
	/// Checks for abilities when this ability's champion is healed.
	/// </summary>
	/// <param name="amount"></param>
	/// <returns></returns>
	public IEnumerator OnHeal(int amount) {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_Smite":
				yield return StartCoroutine(Smite(amount));
				break;
			case "Ability_QuickHeal":
				yield return StartCoroutine(QuickHeal(amount));
				break;
		}
	}
	/// <summary>
	/// Checks for abilities when `healedChampion` is healed.
	/// </summary>
	/// <param name="healedChampion"></param>
	/// <param name="amount"></param>
	/// <returns></returns>
	public IEnumerator OnHeal(ChampionController healedChampion, int amount) {
		yield break;
	}
	/// <summary>
	/// Checks for abilities when this ability right before this ability's champion dies.
	/// </summary>
	/// <returns></returns>
	public IEnumerator OnDeath() {
		yield break;
	}
	/// <summary>
	/// Checks for abilities when this ability's champion calculates the result of a combat as the attacker.
	/// </summary>
	/// <param name="attackingCard"></param>
	/// <param name="defendingCard"></param>
	/// <returns></returns>
	public IEnumerator OnCombatCalculationAttacker(Card attackingCard, Card defendingCard) {
		yield break;
	}
	/// <summary>
	/// Checks for abilities when this ability's champion calculates the result of a combat as the defender.
	/// </summary>
	/// <param name="attackingCard"></param>
	/// <param name="defendingCard"></param>
	/// <returns></returns>
	public IEnumerator OnCombatCalculationDefender(Card attackingCard, Card defendingCard) {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_Bojutsu":
				yield return StartCoroutine(Bojutsu(attackingCard));
				break;
			case "Ability_StrategicManeuver":
				yield return StartCoroutine(StrategicManeuver(defendingCard));
				break;
		}
	}
	public IEnumerator OnAttackSuccess(Card attackingCard, Card defendingCard) {
		yield break;
	}
	public IEnumerator OnAttackFailure(Card attackingCard, Card defendingCard) {
		yield break;
	}
	public IEnumerator OnDefenseSuccess(Card attackingCard, Card defendingCard) {
		yield break;
	}
	public IEnumerator OnDefenseFailure(Card attackingCard, Card defendingCard) {
		yield break;
	}
	public int DamageCalculationBonusSource(int amount, DamageType damageType) {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_Persistence":
				return Persistence(damageType);
			default:
				return 0;
		}
	}
	/// <summary>
	/// Checks for abilities on the damaged champion, then returns a bonus that is added on to incoming damage.
	/// </summary>
	/// <param name="amount"></param>
	/// <param name="damageType"></param>
	/// <returns></returns>
	public int DamageCalculationBonus(int amount, DamageType damageType) {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_HopliteShield":
				return HopliteShield(amount, damageType);
			default:
				return 0;
		}

	}
	public bool CanBeTargetedByAttack() {
		switch (abilityScriptableObject.abilityID) {
			case "Ability_Stealth":
				return Stealth();
		}
		return true;
	}

	// Ability Methods
	private IEnumerator QuickAssist() {
		if (!champion.isMyTurn) yield break;

		yield return new WaitForSeconds(1f);

		foreach (ChampionController selectedChampion in GameController.instance.champions) {
			if (selectedChampion == champion || selectedChampion.isDead || selectedChampion.faction != champion.faction || selectedChampion.faction == Champion.Faction.Undefined) continue;

			yield return StartCoroutine(champion.hand.Deal(1));
			Debug.Log(abilityScriptableObject.abilityName + " was activated for " + champion.championName + ". Dealing that champion a card!");
			AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
		}
	}
	private IEnumerator QuickHeal(int amount) {
		Debug.Log(abilityScriptableObject.abilityName + " activated for " + champion.championName + ", 50% chance to heal for double the amount!");
		if (Random.Range(0f, 1f) < 0.5f || champion.currentHP == champion.maxHP) yield break;

		yield return new WaitForSeconds(0.5f);
		yield return StartCoroutine(champion.Heal(amount, false));
		AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
	}
	private bool Stealth() {
		if (!IsExclusive()) return true;
		if (champion.spadesBeforeExhaustion == 1 && champion.heartsBeforeExhaustion == 3 && champion.diamondsBeforeExhaustion == 1) {
			AudioController.instance.Play(abilityScriptableObject.customAudioClips[0], false, 0.5f);
			AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
			return false;
		}
		return true;
	}
	private int HopliteShield(int amount, DamageType damageType) {
		if (!IsExclusive()) return 0;
		switch (damageType) {
			case DamageType.Melee:
			case DamageType.Ranged:
				break;
			default:
				return 0;
		}

		Debug.Log(abilityScriptableObject.abilityName + " was activated for " + champion.championName + ". A 33% chance to negate the damage by half!" +
		          "\n This chance is increased to 50% if the damage is fatal.");

		float chance = champion.currentHP - amount <= 0 ? 0.5f : (float)1 / 3;
		if (Random.Range(0f, 1f) < chance) {
			AudioController.instance.Play(abilityScriptableObject.customAudioClips[0]);
			AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
			return -amount / 2;
		}
		return 0;
	}
	private IEnumerator Bojutsu(Card attackingCard) {
		if (!IsExclusive()) yield break;
		if (attackingCard.cardScriptableObject.cardValue < 10) yield break;

		attackingCard.CombatValue--;
		Debug.Log(abilityScriptableObject.abilityName + " was activated for " + champion.championName + " because another champion attacked with a J or higher. " + " That card's value is reduced by 1. It is now " + attackingCard.CombatValue);
		AbilityFeedEntry.New(abilityScriptableObject, champion);
	}
	private IEnumerator Rejuvenation() {
		if (!IsExclusive()) yield break;
		if (Random.Range(0f, 1f) < 0.5f || champion.currentHP == champion.maxHP) yield break;

		Debug.Log(abilityScriptableObject.abilityName + " was activated for " + champion.championName + ". Healing for 5.");
		yield return StartCoroutine(champion.Heal(5, false));
		AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
	}
	private IEnumerator Smite(int amount) {
		if (!IsExclusive()) yield break;
		
		Debug.Log(abilityScriptableObject.abilityName + " was activated for " + champion.championName + ".");
		
		ChampionController markedForSmite = GameController.instance.champions[Random.Range(0, GameController.instance.champions.Count)];
		int tries = 0;
		while ((markedForSmite.isDead || markedForSmite == champion || markedForSmite.teamMembers.Contains(champion)) && tries <= 15) {
			markedForSmite = GameController.instance.champions[Random.Range(0, GameController.instance.champions.Count)];
			tries++;
		}
		yield return StartCoroutine(markedForSmite.Damage(amount, DamageType.Lightning, champion, true));
		AudioController.instance.Play(abilityScriptableObject.customAudioClips[0], false, 0.5f);
		AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
	}
	private IEnumerator StrategicManeuver(Card defendingCard) {
		if (!IsExclusive()) yield break;
		if (defendingCard.cardScriptableObject.cardValue < 10) yield break;

		Debug.Log(abilityScriptableObject.abilityName + " was activated for " + champion.championName + ".");

		yield return StartCoroutine(champion.hand.Deal(1));
		AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
	}
	private IEnumerator PersistenceIncrement() {
		if (!IsExclusive()) yield break;

		abilityInts["persistence"] = Mathf.Min(25, abilityInts["persistence"] + 5);
		Debug.Log(abilityInts["persistence"]);
		AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
	}
	private int Persistence(DamageType damageType) {
		if (!IsExclusive() || damageType != DamageType.Melee) return 0;

		int amount = abilityInts["persistence"];
		abilityInts["persistence"] = 0;
		AbilityFeedEntry.New(abilityScriptableObject, champion, 2f);
		return amount;
	}
}