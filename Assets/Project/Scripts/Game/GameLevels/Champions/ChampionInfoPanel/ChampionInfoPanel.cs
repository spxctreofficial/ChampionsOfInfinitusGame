using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using TMPro;

public class ChampionInfoPanel : MonoBehaviour {
	public Champion champion;
	
	[SerializeField]
	private TMP_Text championNameText, championDescriptionText, championHealthText, championWeaponNameText, championWeaponStatsText;
	[SerializeField]
	private Image championAvatar;
	[SerializeField]
	private Button backButton, purchaseButton;
	[SerializeField]
	private GameObject abilityOverview;

	[SerializeField]
	private GameObject championAbilityOverviewEntryPrefab;

	public static ChampionInfoPanel Create(Champion champion) {
		GameObject championInfoPanelPrefab = MainMenuController.instance == null ? GameController.instance.championInfoPanelPrefab : MainMenuController.instance.championInfoPanelPrefab;
		ChampionInfoPanel championInfoPanel = Instantiate(championInfoPanelPrefab, Vector2.zero, Quaternion.identity).GetComponent<ChampionInfoPanel>();
		championInfoPanel.Setup(champion);

		championInfoPanel.GetComponent<RectTransform>().localScale = Vector3.zero;
		LeanTween.scale(championInfoPanel.GetComponent<RectTransform>(), Vector3.one, 0.25f).setEaseOutQuad();
        
		return championInfoPanel;
	}

	public void Destroy() {
		LeanTween.scale(GetComponent<RectTransform>(), Vector3.zero, 0.25f).setEaseOutQuad().setDestroyOnComplete(true);
	}
	public void OnPurchaseButtonClick() {
		StartPurchase();
	}

	private void Setup(Champion champion) {
		this.champion = champion;
		championNameText.text = champion.championName;
		championDescriptionText.text = champion.description;
		championHealthText.text = champion.maxHP + " HP";
		championWeaponNameText.text = champion.attackName;
		championWeaponStatsText.text = champion.attackDamage + " " + champion.attackDamageType + " Damage";
		championAvatar.sprite = champion.avatar;

		foreach (Ability ability in champion.abilities) {
			ChampionAbilityOverviewEntry abilityOverviewEntry = Instantiate(championAbilityOverviewEntryPrefab, Vector2.zero, Quaternion.identity).GetComponent<ChampionAbilityOverviewEntry>();
			abilityOverviewEntry.transform.SetParent(abilityOverview.transform, false);
			
			abilityOverviewEntry.Setup(ability);
			LayoutRebuilder.ForceRebuildLayoutImmediate(abilityOverviewEntry.GetComponent<RectTransform>());
		}

		if (DataManager.instance.ownedChampions.Contains(champion)) {
			purchaseButton.gameObject.SetActive(false);
		}
		else if (champion.unlockability == Champion.Unlockability.ShopItem) {
			purchaseButton.transform.GetChild(1).GetComponent<TMP_Text>().text = champion.shopCost.ToString();
			foreach (Transform child in purchaseButton.transform) LayoutRebuilder.ForceRebuildLayoutImmediate(child.gameObject.GetComponent<RectTransform>());
		}
	}
	private void StartPurchase() {
		if (DataManager.instance.goldAmount - champion.shopCost < 0) {
			TooltipSystem.instance.ShowError("Insufficient funds!");
			LeanTween.delayedCall(1f, () => TooltipSystem.instance.Hide(TooltipSystem.TooltipType.ErrorTooltip));
			return;
		}

		string description = "Are you sure you want to purchase " + champion.championName + " for " + champion.shopCost + " gold? This purchase is irreversible, and is therefore a permanent purchase.";
		ConfirmDialog confirmDialog = ConfirmDialog.CreateNew("Purchase", description, () => {
			ConfirmDialog.instance.Hide();
		}, () => {
			ConfirmDialog.instance.Hide();
			Destroy();

			Debug.Log("PURCHASE SUCCESSFUL!");
			DataManager.instance.goldAmount -= champion.shopCost;
			DataManager.instance.ownedChampions.Add(champion);
			DataManager.instance.Save();
			AudioController.instance.Play("cointoss0" + Random.Range(1, 3));
		});
		confirmDialog.transform.SetParent(MainMenuController.instance.shopPanel.transform, false);
	}
}
