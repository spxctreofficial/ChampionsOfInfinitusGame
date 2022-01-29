using System;
using System.Collections;
using EZCameraShake;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightManager : MonoBehaviour {
	public static FightManager instance;
	public static Fight fightInstance;

	public bool parrying;
	private ChampionController initialAttacker;

	private void Awake() {
		if (instance is null) {
			instance = this;
		}
		else {
			Destroy(gameObject);
		}
	}

	public static FightManager Create() {
		FightManager fightManager = new GameObject("FightManager").AddComponent<FightManager>();
		return fightManager;
	}

	public IEnumerator HandleFight(ChampionController attacker, ChampionController defender, Card attackingCard) {
		GameManager.instance.endTurnButton.gameObject.SetActive(false);
		Fight fight = new Fight(attacker, defender, attackingCard);
		if (!parrying) fight.AttackingCard.Flip();

		yield return new WaitUntil(() => fight.AttackCanStart);

		StartCoroutine(fightInstance.Attacker.hand.UseCard(fight.AttackingCard));
		if (!parrying) fight.AttackingCard.Flip();
		fightInstance.Defender.championParticleController.redGlow.SetActive(true);

		if (!fightInstance.Defender.isPlayer) {
			foreach (Card card in fightInstance.Defender.hand.cards) {
				switch (card.cardData.cardFunctions.primaryFunction) {
					case "block":
					case "parry":
						if (card.cardData.cardFunctions.primaryFunction == "parry" && fightInstance.Defender.equippedWeapon is null) {
							continue;
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

						yield return new WaitForSeconds(0.2f);
						break;
				}
			}
		}
		else {
			bool willWait = false;
			foreach (Card card in fightInstance.Defender.hand.cards) {
				if (card.cardData.cardColor != fightInstance.AttackingCard.cardData.cardColor) continue;
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

			if (willWait) {
				yield return new WaitUntil(() => fightInstance.DefendingCard is { });
			}

			foreach (Card card in fightInstance.Defender.hand.cards) {
				foreach (GameObject gameObject in card.greenGlow) {
					gameObject.SetActive(false);
				}
			}
			
		}

		if (fightInstance.DefendingCard is { }) {
			yield return StartCoroutine(fightInstance.Defender.hand.UseCard(fightInstance.DefendingCard));
		}
		
		yield return StartCoroutine(CombatCalculation());
		
		fightInstance = null;
		instance = null;
		Destroy(gameObject);
	}

	private IEnumerator CombatCalculation() {
		if (!parrying) {
			initialAttacker = fightInstance.Attacker;
		}

		fightInstance.Defender.championParticleController.redGlow.SetActive(false);
		
		switch (fightInstance.AttackingCard.cardData.cardFunctions.primaryFunction) {
			case "attack":
			case "parry":
				if (!parrying) fightInstance.Attacker.equippedWeapon.Damage(1);
				
				if (fightInstance.DefendingCard is null) {
					yield return StartCoroutine(fightInstance.Defender.Damage(fightInstance.Attacker.equippedWeapon.EffectiveDamage, fightInstance.Attacker.equippedWeapon.weaponScriptableObject.damageType, fightInstance.Attacker));
					fightInstance.Attacker.matchStatistic.successfulAttacks++;
					fightInstance.Defender.matchStatistic.failedDefends++;
					break;
				}
				
				switch (fightInstance.DefendingCard.cardData.cardFunctions.primaryFunction) {
					case "block":
						if (fightInstance.AttackingCard.cardData.cardColor == fightInstance.DefendingCard.cardData.cardColor) {
							AudioManager.instance.Play("swordimpact_fail");
							CameraShaker.Instance.ShakeOnce(1f, 4f, 0.1f, 0.2f);
							fightInstance.Attacker.matchStatistic.failedAttacks++;
							fightInstance.Defender.matchStatistic.successfulDefends++;
							break;
						}

						yield return StartCoroutine(fightInstance.Defender.Damage(fightInstance.Attacker.equippedWeapon.EffectiveDamage, fightInstance.Attacker.equippedWeapon.weaponScriptableObject.damageType, fightInstance.Attacker));
						fightInstance.Attacker.matchStatistic.successfulAttacks++;
						fightInstance.Defender.matchStatistic.failedDefends++;

						break;
					case "parry":
						if (fightInstance.AttackingCard.cardData.cardColor == fightInstance.DefendingCard.cardData.cardColor) {
							AudioManager.instance.Play("swordimpact_fail");
							CameraShaker.Instance.ShakeOnce(1f, 4f, 0.1f, 0.2f);
							parrying = true;
							fightInstance.Defender.equippedWeapon.Damage(1);
							yield return StartCoroutine(instance.HandleFight(fightInstance.Defender, fightInstance.Attacker, fightInstance.DefendingCard));
							break;
						}
						
						yield return StartCoroutine(fightInstance.Defender.Damage(fightInstance.Attacker.equippedWeapon.EffectiveDamage, fightInstance.Attacker.equippedWeapon.weaponScriptableObject.damageType, fightInstance.Attacker));
						fightInstance.Attacker.matchStatistic.successfulAttacks++;
						fightInstance.Defender.matchStatistic.failedDefends++;
						break;
				}
				break;
		}
		
		if (initialAttacker.isPlayer) GameManager.instance.endTurnButton.gameObject.SetActive(true);
	}
}