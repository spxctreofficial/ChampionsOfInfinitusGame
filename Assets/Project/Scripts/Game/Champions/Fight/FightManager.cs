using System;
using System.Collections;
using EZCameraShake;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightManager : MonoBehaviour
{
	public static FightManager instance;
	public static Fight fightInstance;

	public bool parrying;

	private void Awake()
	{
		if (instance is null)
		{
			instance = this;
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public static FightManager Create()
	{
		FightManager fightManager = new GameObject("FightManager").AddComponent<FightManager>();
		return fightManager;
	}

	public IEnumerator InitiateFight(ChampionController attacker, ChampionController defender, Card attackingCard) {
		GameManager.instance.endTurnButton.gameObject.SetActive(false);
		Fight fight = new Fight(attacker, defender, attackingCard);

		fight.AttackingCard.Flip();
		yield return new WaitUntil(() => fight.AttackCanStart);
		fight.AttackingCard.Flip();

		yield return StartCoroutine(HandleFight(attacker, defender, attackingCard));
	}
	private IEnumerator HandleFight(ChampionController attacker, ChampionController defender, Card attackingCard)
	{
		StartCoroutine(fightInstance.Attacker.hand.UseCard(fightInstance.AttackingCard));

		fightInstance.Defender.championParticleController.redGlow.SetActive(true);

		if (!fightInstance.Defender.isPlayer)
		{
			foreach (Card card in fightInstance.Defender.hand.cards)
			{
				switch (card.cardData.cardFunctions.primaryFunction)
				{
					case "block":
					case "parry":
						PickBotDefenseCard(card);
						yield return new WaitForSeconds(0.2f);
						break;
				}
			}
		}
		else
		{
			bool willWait = false;
			foreach (Card card in fightInstance.Defender.hand.cards)
			{
				PickPlayerDefenseCard(card, out willWait);
			}

			if (willWait)
			{
				yield return new WaitUntil(() => fightInstance.DefendingCard is {});
			}

			foreach (Card card in fightInstance.Defender.hand.cards)
			{
				foreach (GameObject gameObject in card.greenGlow)
				{
					gameObject.SetActive(false);
				}
			}

		}

		if (fightInstance.DefendingCard is {})
		{
			yield return StartCoroutine(fightInstance.Defender.hand.UseCard(fightInstance.DefendingCard));
		}

		yield return StartCoroutine(CombatCalculation());

		fightInstance = null;
		instance = null;
		Destroy(gameObject);
	}

	private IEnumerator CombatCalculation()
	{
		fightInstance.Defender.championParticleController.redGlow.SetActive(false);

		switch (fightInstance.AttackingCard.cardData.cardFunctions.primaryFunction)
		{
			case "attack":
			case "parry":
				if (fightInstance.DefendingCard is null)
				{
					yield return StartCoroutine(DefenselessFunction(fightInstance.Attacker, fightInstance.Defender));
					yield break;
				}

				switch (fightInstance.DefendingCard.cardData.cardFunctions.primaryFunction)
				{
					case "block":
						yield return StartCoroutine(BlockFunction(fightInstance.Attacker, fightInstance.Defender, fightInstance.AttackingCard, fightInstance.DefendingCard));
						break;
					case "parry":
						yield return StartCoroutine(ParryFunction(fightInstance.Defender, fightInstance.Attacker, fightInstance.DefendingCard, fightInstance.AttackingCard));
						break;
				}

				break;
            default:
                Debug.LogError("yo hol up");
                break;
		}

		if (fightInstance.Attacker.isPlayer) GameManager.instance.endTurnButton.gameObject.SetActive(true);
	}

	private void PickBotDefenseCard(Card card) {
		if (card.cardData.cardFunctions.primaryFunction == "parry" && fightInstance.Defender.equippedWeapon is null) {
			return;
		}
		if (card.cardData.cardColor == fightInstance.AttackingCard.cardData.cardColor && fightInstance.DefendingCard == null) {
			fightInstance.DefendingCard = card;
		}

		if (fightInstance.DefendingCard != null &&
			card.cardData.cardFunctions.primaryFunction == "parry" &&
			(card.cardData.cardColor == fightInstance.AttackingCard.cardData.cardColor &&
			 card.cardData.cardFunctions.primaryFunction != fightInstance.DefendingCard.cardData.cardFunctions.primaryFunction &&
			 Random.Range(0f, 1f) < (parrying ? 0.75f : 0.5f))) {
			fightInstance.DefendingCard = card;
		}
	}
	private void PickPlayerDefenseCard(Card card, out bool willWait) {
		willWait = false;
		if (card.cardData.cardColor != fightInstance.AttackingCard.cardData.cardColor) return;

		switch (card.cardData.cardFunctions.primaryFunction) {
			case "block":
			case "parry":
				willWait = true;
				foreach (GameObject gameObject in card.greenGlow) {
					gameObject.SetActive(true);
				}
				break;
		}
	}

	private IEnumerator DefenselessFunction(ChampionController attacker, ChampionController defender) {
		yield return StartCoroutine(defender.Damage(attacker.equippedWeapon.EffectiveDamage, attacker.equippedWeapon.weaponScriptableObject.damageType, attacker));
		attacker.matchStatistic.successfulAttacks++;
		defender.matchStatistic.failedDefends++;
	}
	private IEnumerator BlockFunction(ChampionController attacker, ChampionController defender, Card attackingCard, Card defendingCard) {
		if (!parrying) attacker.equippedWeapon.Damage(1);
		if (attackingCard.cardData.cardColor == defendingCard.cardData.cardColor) {
			AudioManager.instance.Play("swordimpact_fail");
			CameraShaker.Instance.ShakeOnce(1f, 4f, 0.1f, 0.2f);
			attacker.matchStatistic.failedAttacks++;
			defender.matchStatistic.successfulDefends++;
			yield break;
		}

		yield return StartCoroutine(fightInstance.Defender.Damage(fightInstance.Attacker.equippedWeapon.EffectiveDamage, fightInstance.Attacker.equippedWeapon.weaponScriptableObject.damageType, fightInstance.Attacker));
		fightInstance.Attacker.matchStatistic.successfulAttacks++;
		fightInstance.Defender.matchStatistic.failedDefends++;
	}
	private IEnumerator ParryFunction(ChampionController parryingChampionController, ChampionController parriedChampionController, Card parryingCard, Card parriedCard) {
		if (!parrying) parriedChampionController.equippedWeapon.Damage(1);
		if (parriedCard.cardData.cardColor == parryingCard.cardData.cardColor) {
			AudioManager.instance.Play("swordimpact_fail");
			CameraShaker.Instance.ShakeOnce(1f, 4f, 0.1f, 0.2f);
			parrying = true;
			parryingChampionController.equippedWeapon.Damage(1);
			yield return StartCoroutine(instance.HandleFight(parryingChampionController, parriedChampionController, parryingCard));
			yield break;
		}

		yield return StartCoroutine(parryingChampionController.Damage(parriedChampionController.equippedWeapon.EffectiveDamage, parriedChampionController.equippedWeapon.weaponScriptableObject.damageType, parriedChampionController));
		parriedChampionController.matchStatistic.successfulAttacks++;
		parryingChampionController.matchStatistic.failedDefends++;
	}
}
