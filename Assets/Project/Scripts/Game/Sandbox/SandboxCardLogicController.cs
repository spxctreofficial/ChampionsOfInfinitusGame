using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandboxCardLogicController : CardLogicController {
	public static new SandboxCardLogicController instance;

	protected override void Awake() {
		base.Awake();

		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
	}
	protected override void Update() {
		if (Input.GetKeyDown(KeyCode.Alpha5)) {
			StartCoroutine(GameController.instance.champions[dealToIndex].hand.Deal(summonCard));
		}
		if (Input.GetKeyDown(KeyCode.Equals)) {
			foreach (ChampionController champion in GameController.instance.champions) {
				if (!champion.isPlayer) continue;
				champion.hand.cards[0].CombatValue++;
			}
		}
		if (Input.GetKeyDown(KeyCode.Minus)) {
			foreach (ChampionController champion in GameController.instance.champions) {
				if (!champion.isPlayer) continue;
				champion.hand.cards[0].CombatValue--;
			}
		}
	}
}
