using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseAChampion : MonoBehaviour
{
    public GameHandler gameHandler;
    public void OnClick()
    {
        gameHandler.GamePlayerChooseTurn();
    }
}
