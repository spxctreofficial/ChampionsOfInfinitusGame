using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DifficultySelectionButton : MonoBehaviour
{
    [HideInInspector]
    public GameController.Difficulty difficulty;

    private TMP_Text text;

    private void Start()
    {
        text = transform.GetChild(0).GetComponent<TMP_Text>();
        switch (difficulty)
        {
            case GameController.Difficulty.Noob:
                text.text = "NOOB";
                break;
            case GameController.Difficulty.Novice:
                text.text = "NOVICE";
                break;
            case GameController.Difficulty.Warrior:
                text.text = "WARRIOR";
                break;
            case GameController.Difficulty.Champion:
                text.text = "CHAMPION";
                break;
        }
    }

    public void OnClick()
    {
        GameController.instance.hasChosenDifficulty = true;
        GameController.instance.difficulty = difficulty;
    }
}
