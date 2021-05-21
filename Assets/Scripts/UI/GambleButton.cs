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
							player.attackingCard = Instantiate(GameController.instance.cardIndex.playingCards[Random.Range(0, GameController.instance.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();
							player.attackingCard.ToggleCardVisibility(true);
							player.attackingCard.transform.SetParent(GameController.instance.discardArea.transform, false);
							isBlocking = true;
						}
						break;
					case false:
						foreach (var selectedChampion in GameController.instance.champions) {
							if (selectedChampion == player || !selectedChampion.isAttacking || selectedChampion.currentTarget != player || selectedChampion.isDead) continue;

							player.defendingCard = Instantiate(GameController.instance.cardIndex.playingCards[Random.Range(0, GameController.instance.cardIndex.playingCards.Count)], new Vector2(0, 0), Quaternion.identity).GetComponent<Card>();
							player.defendingCard.ToggleCardVisibility(true);
							player.defendingCard.transform.SetParent(GameController.instance.discardArea.transform, false);
							isBlocking = true;
						}
						break;
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
