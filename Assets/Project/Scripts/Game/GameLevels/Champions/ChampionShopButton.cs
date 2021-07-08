using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class ChampionShopButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	[HideInInspector]
	public Champion champion;

	[SerializeField]
	private Image avatar;
	[SerializeField]
	private TMP_Text goldCostText;

	private ChampionInfoPanel currentInfoPanel;
	private bool hasBeenPurchased = false;

	private static int delayID;

	private void Start() {
		UpdateInformation();
	}
	private void Update() {
		if (!DataManager.instance.ownedChampions.Contains(champion) && !hasBeenPurchased) return;
		UpdateInformation();
		hasBeenPurchased = true;
	}

	public void OnClick() {
		if (!DataManager.instance.firstRunShop) return;
		currentInfoPanel = ChampionInfoPanel.Create(champion);
		currentInfoPanel.transform.SetParent(MainMenuController.instance.shopPanel.transform, false);
	}
	
	public void UpdateInformation() {
		// Updates information
		avatar.sprite = champion.avatar;
		goldCostText.text = champion.value.ToString();

		foreach (Champion champion in DataManager.instance.ownedChampions) {
			if (champion != this.champion) continue;
			goldCostText.text = "PURCHASED";
			goldCostText.color = new Color32(128, 128, 128, 255);
			break;
		}
	}

	// Pointer Events
	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(0.5f, () => {
			string body = "Health: " + champion.maxHP; // max health
			body += "\n" + champion.attackName + " (Attack): " + champion.attackDamage + " " + champion.attackDamageType + " Damage"; // attack & damage
			body += "\nCLICK FOR MORE INFO";

			body += "\n\nCost: " + champion.value; // cost to buy (assumes that the champion is a shop item)

			TooltipSystem.instance.Show(body, champion.championName); // show the tooltip
		}).id;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
}
