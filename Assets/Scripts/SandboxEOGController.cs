using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SandboxEOGController : MonoBehaviour
{
    public static SandboxEOGController instance;

    public TMP_Text titleText;
    public TMP_Text rewardsText;
    public Button collectButton;
    
    private int initialGoldReward;
    private int goldReward;
    
    private readonly int successfulAttackBonus = StatisticController.instance.playerChampionStatistic.successfulAttacks * 5;
    private readonly int successfulDefendBonus = StatisticController.instance.playerChampionStatistic.successfulDefends * 2;
    private readonly int failedAttacksBonus = StatisticController.instance.playerChampionStatistic.failedAttacks;
    private readonly int failedDefendsPenalty = StatisticController.instance.playerChampionStatistic.failedDefends;

    private readonly int killCountBonus = StatisticController.instance.playerChampionStatistic.killCount * 100;
    private readonly int totalDamageDealtBonus = StatisticController.instance.playerChampionStatistic.totalDamageDealt / 2;
    private readonly int totalDamageReceivedCompensation = StatisticController.instance.playerChampionStatistic.totalDamageReceived / 4;
    private readonly int totalHealthRemainingBonus = (StatisticController.instance.playerChampionStatistic.remainingHP / StatisticController.instance.playerChampionStatistic.champion.maxHP) * 100;

    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    private void Start()
    {
        Debug.Log("it called");
        initialGoldReward = StatisticController.instance.winState ? Random.Range(290, 311) : Random.Range(290, 311) / 10;
        switch (GameController.instance.difficulty)
        {
            case GameController.Difficulty.Noob:
                initialGoldReward /= 5;
                break;
            case GameController.Difficulty.Novice:
                initialGoldReward /= 2;
                break;
            case GameController.Difficulty.Warrior:
                break;
            case GameController.Difficulty.Champion:
                initialGoldReward *= (int)1.2f;
                break;
        }

        titleText.text = StatisticController.instance.winState ? "VICTORY" : "DEFEAT";
        RewardCalculation();
    }

    public void RewardCalculation()
    {
        Debug.Log("it called too");
        int goldAmount = PlayerPrefs.HasKey("goldAmount") ? PlayerPrefs.GetInt("goldAmount") : 0;

        switch (StatisticController.instance.gamemode)
        {
            case GameController.Gamemodes.FFA:
                goldReward = initialGoldReward;
                goldReward += successfulAttackBonus;
                goldReward += successfulDefendBonus;
                goldReward += failedAttacksBonus;
                goldReward -= failedDefendsPenalty;

                goldReward += killCountBonus;
                goldReward += totalDamageDealtBonus;
                goldReward += totalDamageReceivedCompensation;
                goldReward += totalHealthRemainingBonus;
                
                PlayerPrefs.SetInt("goldAmount", goldAmount + goldReward);
                PlayerPrefs.Save();
                
                AudioController.instance.Play(StatisticController.instance.winState ? "GameWin" : "GameLose");

                break;
            default:
                goldReward = initialGoldReward;
                break;
        }

        StartCoroutine(CaretText());
    }

    public void LoadMainMenu()
    {
        Destroy(StatisticController.instance.gameObject);
        SceneManager.LoadScene("MainMenu");
    }

    private IEnumerator CaretText()
    {
        collectButton.gameObject.SetActive(false);
        rewardsText.text = "";
        
        var text = "Rewards:\n";
        text += StatisticController.instance.winState ? "Win Reward: " : "Loss Compensation: ";
        text += initialGoldReward;
        text += "\nSuccessful Attacks: " + StatisticController.instance.playerChampionStatistic.successfulAttacks + " * 5 = " + successfulAttackBonus;
        text += "\nSuccessful Defenses: " + StatisticController.instance.playerChampionStatistic.successfulDefends + " * 2 = " + successfulDefendBonus;
        text += "\nFailed Attacks: " + StatisticController.instance.playerChampionStatistic.failedAttacks + " = " + failedAttacksBonus;
        text += "\nFailed Defenses: " + StatisticController.instance.playerChampionStatistic.failedDefends + " = " + -failedDefendsPenalty;
        text += "\nKills: " + StatisticController.instance.playerChampionStatistic.killCount + " * 100 = " + killCountBonus;
        text += "\nTotal Damage Dealt: " + StatisticController.instance.playerChampionStatistic.totalDamageDealt + " / 2 = " + totalDamageDealtBonus;
        text += "\nTotal Damage Received: " + StatisticController.instance.playerChampionStatistic.totalDamageReceived + " / 4 = " + totalDamageReceivedCompensation;
        text += "\nHealth Remaining: " + StatisticController.instance.playerChampionStatistic.remainingHP / StatisticController.instance.playerChampionStatistic.champion.maxHP + "%";
        text += "\nTotal: " + goldReward;

        foreach (var letter in text.ToCharArray())
        {
            rewardsText.text += letter;
            yield return new WaitForSeconds(0.01f);
        }
        
        collectButton.transform.localScale = new Vector3(0, 0, 0);
        LeanTween.scale(collectButton.gameObject, new Vector3(1, 1, 1), 0.2f).setEaseInOutQuad();

        LeanTween.delayedCall(StatisticController.instance.winState ? 15f : 8f, () => {
            CanvasGroup canvasGroup = titleText.transform.parent.gameObject.AddComponent<CanvasGroup>();
            LeanTween.alphaCanvas(canvasGroup, 0f, 1f).setEaseInOutQuad().setOnComplete(LoadMainMenu);
        });
    }
}
