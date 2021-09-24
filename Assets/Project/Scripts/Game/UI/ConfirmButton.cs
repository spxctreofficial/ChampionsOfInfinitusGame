using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ConfirmButton : MonoBehaviour {
    public TMP_Text textBox;
    
    public virtual void OnClick() {
        foreach (ChampionController champion in GameController.instance.champions) {
            if (!champion.isPlayer || champion.isDead) continue;

            switch (GameController.instance.gamePhase) {
                case GamePhase.ActionPhase:
                    // Confirming Attack
                    if (champion.isAttacking && champion.attackingCard != null && champion.currentTarget != null) {
                        champion.spadesBeforeExhaustion--;
                        champion.matchStatistic.totalAttacks++;
                        GameController.instance.attackCancelButton.Hide();
                        StartCoroutine(champion.hand.Discard(champion.hand.queued.Dequeue()));
                        StartCoroutine(CardLogicController.instance.CombatCalculation(champion, champion.currentTarget));
                        break;
                    }
                    // Confirming Defense
                    if (champion.currentlyTargeted && champion.defendingCard != null && !champion.isMyTurn) {
                        champion.hasDefended = true;
                        break;
                    }
                    // Skipping Discard
                    if (champion.discardAmount != 0 && !champion.isMyTurn) {
                        champion.discardAmount = -1;
                    }
                    break;
            }
        }
        LeanTween.move(GetComponent<RectTransform>(), new Vector2(0, -569.23f), 0.25f).setEaseInOutQuad();
    }

    public void Show() {
        transform.localPosition = new Vector2(0, -180.03f);
        gameObject.SetActive(true);
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
}
