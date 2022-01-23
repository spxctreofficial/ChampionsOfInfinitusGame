using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultySelectionButton : MonoBehaviour {
	[HideInInspector]
	public GameManager.Difficulty difficulty;

	private TMP_Text text;

	private void Start() {
		text = transform.GetChild(0).GetComponent<TMP_Text>();
		switch (difficulty) {
			case GameManager.Difficulty.Noob:
				text.text = "NOOB";
				break;
			case GameManager.Difficulty.Novice:
				text.text = "NOVICE";
				break;
			case GameManager.Difficulty.Warrior:
				text.text = "WARRIOR";
				break;
			case GameManager.Difficulty.Champion:
				text.text = "CHAMPION";
				break;
		}
	}

	public void OnClick() {
		GameManager.instance.hasChosenDifficulty = true;
		GameManager.instance.difficulty = difficulty;
	}
}
