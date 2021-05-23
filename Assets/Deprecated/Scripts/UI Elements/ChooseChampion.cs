﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChooseChampion : MonoBehaviour
{
	public void ChooseTheWraithKing()
	{
        var gameHandler = FindObjectOfType<GameHandler>();
        gameHandler.GamePlayerChooseTurn();
	}
    public void ChoosePlayer()
    {
        FindObjectOfType<GameHandler>().GamePlayerChooseTurn();
    }

    #region Obsolete Functions
    [Obsolete]
    public void Choose1(GameHandler gameHandler)
    {
        gameHandler.GamePlayerChooseTurn();
    }
    [Obsolete]
    public void Choose2(GameHandler gameHandler)
    {
        gameHandler.GameSetup();
    }
    #endregion
}
