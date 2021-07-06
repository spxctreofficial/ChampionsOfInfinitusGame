using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamemodeSelectionButton : MonoBehaviour {
	[HideInInspector]
	public GameController.Gamemodes gamemode;

	private TMP_Text text;

	private void Start() {
		text = transform.GetChild(0).GetComponent<TMP_Text>();
		switch (gamemode) {
			case GameController.Gamemodes.Competitive2v2:
				text.text = "COMPETITIVE 2V2";
				break;
			case GameController.Gamemodes.FFA:
				text.text = "FFA";
				break;
			case GameController.Gamemodes.Duel:
				text.text = "DUEL";
				break;
		}
	}

	public void OnClick() {
		SandboxGameController.instance.hasChosenGamemode = true;
		SandboxGameController.instance.gamemodes = gamemode;
	}
}
