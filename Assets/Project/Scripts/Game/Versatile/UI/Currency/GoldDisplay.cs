using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class GoldDisplay : MonoBehaviour {
    [SerializeField]
    private TMP_Text goldAmountText;

    private Coroutine currentTextRoutine;

    private void Start() {
        goldAmountText.text = DataManager.instance.goldAmount.ToString();
    }
    private void Update() {
        if (DataManager.instance.goldAmount.ToString() == goldAmountText.text || currentTextRoutine != null) return;
        StartCoroutine(UpdateText());
    }

    private IEnumerator UpdateText() {
        int goldAmountShown = int.Parse(goldAmountText.text);
        while (goldAmountShown < DataManager.instance.goldAmount) {
            goldAmountShown++;
            if (Mathf.Abs(goldAmountShown - DataManager.instance.goldAmount) > 250) goldAmountShown++;
            if (Mathf.Abs(goldAmountShown - DataManager.instance.goldAmount) > 1000) goldAmountShown++;
            goldAmountText.text = goldAmountShown.ToString();
            yield return null;
        }
        while (goldAmountShown > DataManager.instance.goldAmount) {
            goldAmountShown--;
            if (Mathf.Abs(goldAmountShown - DataManager.instance.goldAmount) > 250) goldAmountShown--;
            if (Mathf.Abs(goldAmountShown - DataManager.instance.goldAmount) > 1000) goldAmountShown--;
            goldAmountText.text = goldAmountShown.ToString();
            yield return null;
        }
    }
}
