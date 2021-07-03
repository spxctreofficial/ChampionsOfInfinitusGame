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
		if (!DataManager.instance.OwnedChampions.Contains(champion) && !hasBeenPurchased) return;
		UpdateInformation();
		hasBeenPurchased = true;
	}

	public void OnClick() {
		if (!DataManager.instance.FirstRunShop) return;
		currentInfoPanel = ChampionInfoPanel.Create(champion);
		currentInfoPanel.transform.SetParent(MainMenuController.instance.shopPanel.transform, false);
	}
	
	public void UpdateInformation() {
		// Updates information
		avatar.sprite = champion.avatar;
		goldCostText.text = champion.shopCost.ToString();

		foreach (var champion in DataManager.instance.OwnedChampions) {
			if (champion != this.champion) continue;
			goldCostText.text = "PURCHASED";
			goldCostText.color = new Color32(128, 128, 128, 255);
			break;
		}
	}

	// Pointer Events
	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(0.5f, () => {
			string attackType() {
				return champion.attackDamageType switch {
					DamageType.Melee => "Melee",
					DamageType.Ranged => "Ranged",
					DamageType.Fire => "Fire",
					DamageType.Lightning => "Lightning",
					DamageType.Shadow => "Shadow",
					DamageType.Unblockable => "Unblockable",
					_ => throw new ArgumentOutOfRangeException()
				};
			}
			string abilityType(Ability ability) {
				return ability.abilityType switch {
					Ability.AbilityType.Passive => "Passive",
					Ability.AbilityType.Active => "Active",
					Ability.AbilityType.AttackB => "Attack Bonus",
					Ability.AbilityType.DefenseB => "Defense Bonus",
					Ability.AbilityType.Ultimate => "Ultimate",
					_ => throw new ArgumentOutOfRangeException()
				};
			}

			var body = "Health: " + champion.maxHP; // max health
			body += "\n" + champion.attackName + " (Attack): " + champion.attackDamage + " " + attackType() + " Damage"; // attack & damage

			body += "\nAbilities:"; // abilities

			if (champion.abilities.Count != 0) {
				foreach (var ability in champion.abilities) body += "\n" + ability.abilityName + " (" + abilityType(ability) + ")"; // print all abilities
			}
			else {
				body += " None";
			}

			body += "\n\nCost: " + champion.shopCost; // cost to buy (assumes that the champion is a shop item)

			TooltipSystem.instance.Show(body, champion.championName); // show the tooltip
		}).id;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
}
