using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GambleButton : MonoBehaviour {
	public TMP_Text textBox;
	public bool isBlocking;

	public void OnClick() {
		switch (GameController.instance.gamePhase) {
			case GamePhase.ActionPhase:
				ChampionController player = null;
				foreach (var selectedChampion in GameController.instance.champions) {
					if (!selectedChampion.isPlayer || selectedChampion.isDead) continue;
					player = selectedChampion;
				}

				switch (player.isMyTurn) {
					case true:
						if (player.isAttacking && player.isMyTurn && player.attackingCard == null) {
							player.attackingCard = Instantiate(GameController.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
							player.attackingCard.cardScriptableObject = GameController.instance.cardIndex.PlayingCards[Random.Range(0, GameController.instance.cardIndex.PlayingCards.Count)];
							player.attackingCard.Flip(true);
							player.attackingCard.transform.SetParent(GameController.instance.discardArea.transform, false);
							player.attackingCard.caption.text = "Gambled by " + player.championName;
							isBlocking = true;
						}
						break;
					case false:
						foreach (var selectedChampion in GameController.instance.champions) {
							if (selectedChampion == player || !selectedChampion.isAttacking || selectedChampion.currentTarget != player || selectedChampion.isDead) continue;

							player.defendingCard = Instantiate(GameController.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
							player.defendingCard.cardScriptableObject = GameController.instance.cardIndex.PlayingCards[Random.Range(0, GameController.instance.cardIndex.PlayingCards.Count)];
							player.defendingCard.Flip(true);
							player.defendingCard.transform.SetParent(GameController.instance.discardArea.transform, false);
							player.defendingCard.caption.text = "Gambled by " + player.championName;
							player.hasDefended = true;
							isBlocking = true;
						}
						break;
				}
				
				if (GameController.instance.discardArea.transform.childCount > 8) {
					for (int i = GameController.instance.discardArea.transform.childCount; i > 8; i--) {
						Destroy(GameController.instance.discardArea.transform.GetChild(0).gameObject);
					}
				}
				break;
		}
		Hide();
	}

	public void Show() {
		isBlocking = false;
		gameObject.SetActive(true);
	}
	public void Hide() {
		gameObject.SetActive(false);
	}
}
