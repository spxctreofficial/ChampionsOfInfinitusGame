using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChampionSelectionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	[HideInInspector]
	public Champion championComponent;

	private static LTDescr delay;

	private void Start() {
		StartCoroutine(Setup());
	}

	public void OnClick() {
		GameController.instance.playerChampion = championComponent;
		LeanTween.cancel(delay.uniqueId);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}

	private IEnumerator Setup() {
		yield return null;
		if (championComponent != null) gameObject.GetComponent<Image>().sprite = championComponent.avatar;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		delay = LeanTween.delayedCall(0.5f, () => {
			string attackType() {
				return championComponent.attackDamageType switch {
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

			var body = "Health: " + championComponent.maxHP; // max health
			body += "\n" + championComponent.attackName + " (Attack): " + championComponent.attackDamage + " " + attackType() + " Damage" +
			        "\nAbilities:"; // abilities

			switch (championComponent.abilities.Count) {
				case 0:
					body += " None";
					break;
				default:
					foreach (var ability in championComponent.abilities) body += "\n" + ability.abilityName + " (" + abilityType(ability) + ")";
					break;
			} // print all abilities

			TooltipSystem.instance.Show(body, championComponent.championName); // show the tooltip
		});
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delay.uniqueId);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
}
