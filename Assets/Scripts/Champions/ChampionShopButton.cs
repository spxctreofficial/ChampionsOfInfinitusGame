using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class ChampionShopButton : MonoBehaviour {
    [HideInInspector]
    public Champion champion;
    
    [SerializeField]
    private Image avatar;
    [SerializeField]
    private TMP_Text goldCostText;

    private void Start() {
        UpdateInformation();
    }

    public void OnClick() {
        if (DataManager.instance.GoldAmount - int.Parse(goldCostText.text) < 0) return;
        
        DataManager.instance.GoldAmount -= int.Parse(goldCostText.text);
        DataManager.instance.OwnedChampions.Add(champion);
        DataManager.instance.Save();
        UpdateInformation();
    }

    private void UpdateInformation() {
        avatar.sprite = champion.avatar;
        goldCostText.text = champion.shopCost.ToString();
        
        Debug.Log("PURCHASE SUCCESSFUL!" + DataManager.instance.OwnedChampions);

        foreach (var champion in DataManager.instance.OwnedChampions) {
            if (champion != this.champion) continue;
            GetComponent<Button>().enabled = false;
            goldCostText.text = "PURCHASED";
            goldCostText.color = new Color32(128, 128, 128, 255);
            break;
        }
    }
}
