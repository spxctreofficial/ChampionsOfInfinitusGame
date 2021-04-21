using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WinLoseMenu : MonoBehaviour
{
    public void PlayAgainButtonClick()
    {
        SceneManager.LoadScene("GameScene");
    }
    public void MainMenuButtonClick()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
