using System;
using System.Collections;
using EZCameraShake;
using UnityEngine;
using Random = UnityEngine.Random;

public class FightManager : MonoBehaviour {
    public static FightManager instance;
    public static Fight fightInstance;
    public static Parry parryInstance;

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

    public IEnumerator InitiateFight(ChampionController attacker, ChampionController defender, Card attackingCard) {
        GameManager.instance.endTurnButton.gameObject.SetActive(false);
        Fight fight = new Fight(attacker, defender, attackingCard);

        fight.AttackingCard.Flip();
        yield return new WaitUntil(() => fight.AttackCanStart);
        fight.AttackingCard.Flip();

        yield return StartCoroutine(HandleFight(fight.Attacker, fight.Defender, fight.AttackingCard));
    }
    private IEnumerator InitiateParry(ChampionController parryingController, ChampionController defender, Card parryingCard) {
        Parry parry = new Parry(parryingController, defender, parryingCard);

        yield return StartCoroutine(HandleFight(parryingController, defender, parryingCard));
    }
    private IEnumerator HandleFight(ChampionController attacker, ChampionController defender, Card attackingCard) {
        StartCoroutine(attacker.hand.UseCard(attackingCard));

        defender.championParticleController.redGlow.SetActive(true);

        Card defendingCard = null;
        if (!defender.isPlayer) {
            foreach (Card card in defender.hand.cards) {
                if (defendingCard is { }) break;

                switch (card.cardData.cardFunctions.primaryFunction) {
                    case "block":
                    case "parry":
                        yield return new WaitForSeconds(0.2f);

                        if (PickBotDefenseCard(attacker, defender, attackingCard, card)) defendingCard = card;
                        break;
                }
            }
        }
        else {
            bool willWait = false;
            foreach (Card card in defender.hand.cards) if (IsAPlayerDefendCard(attackingCard, card)) willWait = true;

            if (willWait) {
                switch (parryInstance is { }) {
                    case true:
                        Debug.Log("parry instance checking for defending card");
                        yield return new WaitUntil(() => parryInstance.DefendingCard is { });
                        defendingCard = parryInstance.DefendingCard;
                        break;
                    case false:
                        yield return new WaitUntil(() => fightInstance.DefendingCard is { });
                        defendingCard = fightInstance.DefendingCard;
                        break;
                }

            }

            foreach (Card card in defender.hand.cards) {
                foreach (GameObject gameObject in card.greenGlow) {
                    gameObject.SetActive(false);
                }
            }

        }

        if (defendingCard is { }) {
            yield return StartCoroutine(defendingCard.owner.hand.UseCard(defendingCard));
        }

        yield return StartCoroutine(CombatCalculation(attacker, defender, attackingCard, defendingCard));

        fightInstance = null;
        parryInstance = null;
        instance = null;
        Destroy(gameObject);
    }

    private IEnumerator CombatCalculation(ChampionController attacker, ChampionController defender, Card attackingCard, Card defendingCard) {

        switch (attackingCard.cardData.cardFunctions.primaryFunction) {
            case "attack":
            case "parry":
                if (defendingCard is null) {
                    yield return new WaitForSeconds(0.75f);
                    yield return StartCoroutine(DefenselessFunction(attacker, defender));
                    break;
                }

                switch (defendingCard.cardData.cardFunctions.primaryFunction) {
                    case "block":
                        yield return StartCoroutine(BlockFunction(attacker, defender, attackingCard, defendingCard));
                        break;
                    case "parry":
                        yield return StartCoroutine(ParryFunction(defender, attacker, defendingCard, attackingCard));
                        break;
                }

                break;
            default:
                Debug.LogError("yo hol up");
                break;
        }

        defender.championParticleController.redGlow.SetActive(false);
        if (attacker.isPlayer) GameManager.instance.endTurnButton.gameObject.SetActive(true);
    }

    private bool PickBotDefenseCard(ChampionController attacker, ChampionController defender, Card attackingCard, Card defendingCard) {

        if (defendingCard.cardData.cardFunctions.primaryFunction == "parry" && defender.equippedWeapon is null) {
            return false;
        }
        if (defendingCard.cardData.cardColor == attackingCard.cardData.cardColor) {
            if (parryInstance is { }) parryInstance.DefendingCard = defendingCard;
            else fightInstance.DefendingCard = defendingCard;
            return true;
        }

        if (defendingCard.cardData.cardFunctions.primaryFunction == "parry" &&
            (defendingCard.cardData.cardColor == fightInstance.AttackingCard.cardData.cardColor &&
             defendingCard.cardData.cardFunctions.primaryFunction != fightInstance.DefendingCard.cardData.cardFunctions.primaryFunction &&
             Random.Range(0f, 1f) < (parryInstance is { } ? 0.75f : 0.5f))) {
            if (parryInstance is { } && parryInstance.DefendingCard is { }) {
                parryInstance.DefendingCard = defendingCard;
                return true;
            }
            else if (fightInstance.DefendingCard is { }) {
                fightInstance.DefendingCard = defendingCard;
                return true;
            }

            return false;
        }

        return false;
    }
    private bool IsAPlayerDefendCard(Card attackingCard, Card defendingCard) {
        if (defendingCard.cardData.cardColor != attackingCard.cardData.cardColor) return false;

        switch (defendingCard.cardData.cardFunctions.primaryFunction) {
            case "block":
            case "parry":
                foreach (GameObject gameObject in defendingCard.greenGlow) {
                    gameObject.SetActive(true);
                }
                return true;
        }

        return false;
    }

    private IEnumerator DefenselessFunction(ChampionController attacker, ChampionController defender) {
        yield return StartCoroutine(defender.Damage(attacker.equippedWeapon.EffectiveDamage, attacker.equippedWeapon.weaponScriptableObject.damageType, attacker));
        attacker.matchStatistic.successfulAttacks++;
        defender.matchStatistic.failedDefends++;
    }
    private IEnumerator BlockFunction(ChampionController attacker, ChampionController defender, Card attackingCard, Card defendingCard) {
        if (parryInstance is { }) attacker.equippedWeapon.Damage(1);
        if (attackingCard.cardData.cardColor == defendingCard.cardData.cardColor) {
            AudioManager.instance.Play("swordimpact_fail");
            CameraShaker.Instance.ShakeOnce(1f, 4f, 0.1f, 0.2f);
            attacker.matchStatistic.failedAttacks++;
            defender.matchStatistic.successfulDefends++;
            yield break;
        }

        yield return StartCoroutine(defender.Damage(attacker.equippedWeapon.EffectiveDamage, attacker.equippedWeapon.weaponScriptableObject.damageType, attacker));
        attacker.matchStatistic.successfulAttacks++;
        defender.matchStatistic.failedDefends++;
    }
    private IEnumerator ParryFunction(ChampionController parryingChampionController, ChampionController parriedChampionController, Card parryingCard, Card parriedCard) {
        if (parriedCard.cardData.cardColor == parryingCard.cardData.cardColor) {
            AudioManager.instance.Play("swordimpact_fail");
            CameraShaker.Instance.ShakeOnce(1f, 4f, 0.1f, 0.2f);
            yield return StartCoroutine(instance.InitiateParry(parryingChampionController, parriedChampionController, parryingCard));
            yield break;
        }

        yield return StartCoroutine(parryingChampionController.Damage(parriedChampionController.equippedWeapon.EffectiveDamage, parriedChampionController.equippedWeapon.weaponScriptableObject.damageType, parriedChampionController));
        parriedChampionController.matchStatistic.successfulAttacks++;
        parryingChampionController.matchStatistic.failedDefends++;
    }
}
