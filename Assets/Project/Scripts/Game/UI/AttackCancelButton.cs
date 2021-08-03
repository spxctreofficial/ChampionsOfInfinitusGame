using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class AttackCancelButton : MonoBehaviour {
	public TMP_Text textBox;
	
	public void OnClick() {
		foreach (ChampionController champion in GameController.instance.champions) {
			if (!champion.isPlayer || champion.isDead) continue;

			champion.isAttacking = false;
			champion.currentTarget = null;
			if (GameController.instance.gambleButton.isBlocking) {
				GameController.instance.gambleButton.isBlocking = false;
				Destroy(champion.attackingCard.gameObject);
			}
			else if (champion.attackingCard is {}) {
				
				champion.attackingCard.halo.Stop();
				champion.attackingCard.halo.Clear();
				champion.attackingCard = null;
			}
			champion.hand.queued.Peek().redGlow.Stop();
			champion.hand.queued.Peek().redGlow.Clear();
			champion.hand.queued.Clear();
		}
		
		foreach (ChampionController selectedChampion in GameController.instance.champions) {
			selectedChampion.championParticleController.GreenGlow.Stop();
			selectedChampion.championParticleController.GreenGlow.Clear();
			
			selectedChampion.championParticleController.RedGlow.Stop();
			selectedChampion.championParticleController.RedGlow.Clear();
		}
		
		GameController.instance.confirmButton.Hide();
		GameController.instance.gambleButton.Hide();
		GameController.instance.endTurnButton.gameObject.SetActive(true);
		gameObject.SetActive(false);
	}
	
	public void Show() {
		GetComponent<RectTransform>().localScale = Vector3.one;
        gameObject.SetActive(true);
    }
    public void Hide() {
        gameObject.SetActive(false);
    }
}
