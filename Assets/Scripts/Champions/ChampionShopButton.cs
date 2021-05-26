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

		var confirmDialog = Instantiate(MainMenuController.instance.confirmDialogPrefab, Vector2.zero, Quaternion.identity).GetComponent<ConfirmDialog>();
		confirmDialog.transform.SetParent(MainMenuController.instance.shopPanel.transform, false);
		confirmDialog.Setup("Purchase", "Are you sure you want to purchase this item for " + goldCostText.text + " gold?", CancelPurchase, ConfirmPurchase, true);


	}
	private void CancelPurchase() {
		foreach (Transform child in MainMenuController.instance.shopPanel.transform) {
			if (child == transform || child.GetComponent<ConfirmDialog>() == null) continue;
			child.GetComponent<ConfirmDialog>().Hide();
		}
	}
	private void ConfirmPurchase() {
		DataManager.instance.GoldAmount -= champion.shopCost;
		DataManager.instance.OwnedChampions.Add(champion);
		DataManager.instance.Save();
		AudioController.instance.Play("CoinToss0" + Random.Range(1, 3));
		UpdateInformation();
		
		CancelPurchase();
	}
	
	
	private void UpdateInformation() {
		// Updates information
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
