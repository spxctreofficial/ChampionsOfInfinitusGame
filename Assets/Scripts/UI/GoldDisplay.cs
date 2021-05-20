using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GoldDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text goldAmountText;

    private Coroutine currentTextRoutine;

    private void Start()
    {
        goldAmountText.text = PlayerPrefs.GetInt("goldAmount").ToString();
    }
    private void Update()
    {
        if (PlayerPrefs.GetInt("goldAmount").ToString() == goldAmountText.text || currentTextRoutine != null) return;
        currentTextRoutine = StartCoroutine(UpdateText());
    }

    private IEnumerator UpdateText()
    {
        var goldAmountShown = int.Parse(goldAmountText.text);
        var goldAmount = PlayerPrefs.GetInt("goldAmount");
        while (goldAmountShown < goldAmount)
        {
            goldAmountShown++;
            goldAmountText.text = goldAmountShown.ToString();
            yield return null;
        }
        while (goldAmountShown > goldAmount)
        {
            goldAmountShown--;
            goldAmountText.text = goldAmountShown.ToString();
            yield return null;
        }
        currentTextRoutine = null;
    }
}
