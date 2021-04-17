using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChooseATurn : MonoBehaviour
{
    public GameHandler gameHandler;
    public void OnClick()
    {
        gameHandler.GameSetup();
    }
}
