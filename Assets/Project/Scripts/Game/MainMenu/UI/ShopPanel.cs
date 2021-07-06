using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopPanel : MonoBehaviour {

	[SerializeField]
	private GameObject shopContent;

	[SerializeField]
	private ChampionShopButton championShopButtonPrefab;
	
	public void Show() {
		if (!DataManager.instance.firstRunGame) return;
		LeanTween.move(MainMenuController.instance.mainPanel.GetComponent<RectTransform>(), new Vector2(-1920, 0), 1f).setEaseOutQuad();
		LeanTween.move(GetComponent<RectTransform>(), Vector2.zero, 1f).setEaseOutQuad().setOnComplete(() => {
			if (!DataManager.instance.firstRunShop) DialogueSystem.Create(MainMenuController.instance.firstRunShopSession, new Vector2(0, -270), () => {
				DataManager.instance.firstRunShop = true;
				DataManager.instance.Save();
			}).transform.SetParent(MainMenuController.instance.shopPanel.transform, false);
		});
	}
	public void Hide() {
		if (!DataManager.instance.firstRunShop) return;
		LeanTween.move(MainMenuController.instance.mainPanel.GetComponent<RectTransform>(), Vector2.zero, 1f).setEaseOutQuad();
		LeanTween.move(GetComponent<RectTransform>(), new Vector2(1920, 0), 1f).setEaseOutQuad();
	}

	public void Setup() {
		foreach (Champion champion in DataManager.instance.championIndex.champions) {
			if (champion.unlockability != Champion.Unlockability.ShopItem) continue;
			ChampionShopButton championShopButton = Instantiate(championShopButtonPrefab, Vector2.zero, Quaternion.identity).GetComponent<ChampionShopButton>();
			championShopButton.transform.SetParent(shopContent.transform, false);
			championShopButton.champion = champion;
		}
	}
}
