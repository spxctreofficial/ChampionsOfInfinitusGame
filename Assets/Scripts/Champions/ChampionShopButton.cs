using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

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
		StartPurchase();
	}

	private void StartPurchase() {
		if (DataManager.instance.GoldAmount - int.Parse(goldCostText.text) < 0) return;

		var confirmDialog = ConfirmDialog.CreateNew("Purchase", "Are you sure you want to purchase this item for " + goldCostText.text + " gold?", () => {
			ConfirmDialog.instance.Hide();
		}, () => {
			DataManager.instance.GoldAmount -= champion.shopCost;
			DataManager.instance.OwnedChampions.Add(champion);
			DataManager.instance.Save();
			AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
			UpdateInformation();
			Debug.Log("PURCHASE SUCCESSFUL!");
			
			ConfirmDialog.instance.Hide();
		});
		confirmDialog.transform.SetParent(MainMenuController.instance.shopPanel.transform, false);


	}
	
	
	private void UpdateInformation() {
		// Updates information
		avatar.sprite = champion.avatar;
		goldCostText.text = champion.shopCost.ToString();

		foreach (var champion in DataManager.instance.OwnedChampions) {
			if (champion != this.champion) continue;
			GetComponent<Button>().enabled = false;
			goldCostText.text = "PURCHASED";
			goldCostText.color = new Color32(128, 128, 128, 255);
			break;
		}
	}
}
