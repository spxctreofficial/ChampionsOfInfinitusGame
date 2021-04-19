using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseChampion : MonoBehaviour
{
    public void Choose1(GameHandler gameHandler)
    {
        gameHandler.GamePlayerChooseTurn();
    }
    public void Choose2(GameHandler gameHandler)
    {
        gameHandler.GameSetup();
    }
}
