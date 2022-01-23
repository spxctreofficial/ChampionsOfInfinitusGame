using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GamemodeSelectionButton : MonoBehaviour {
	[HideInInspector]
	public GameManager.Gamemodes gamemode;

	private TMP_Text text;

	private void Start() {
		text = transform.GetChild(0).GetComponent<TMP_Text>();
		switch (gamemode) {
			case GameManager.Gamemodes.Competitive2v2:
				text.text = "COMPETITIVE 2V2";
				break;
			case GameManager.Gamemodes.FFA:
				text.text = "FFA";
				break;
			case GameManager.Gamemodes.Duel:
				text.text = "DUEL";
				break;
		}
	}

	public void OnClick() {
		SandboxGameManager.instance.hasChosenGamemode = true;
		SandboxGameManager.instance.gamemodes = gamemode;
	}
}
