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
		TooltipSystem.instance.Hide();
	}

	private IEnumerator Setup() {
		yield return null;
		if (championComponent != null) gameObject.GetComponent<Image>().sprite = championComponent.avatar;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		delay = LeanTween.delayedCall(0.5f, () => {
			var body = "Health: " + championComponent.maxHP;

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

			body += "\n" + championComponent.attackName + " (Attack): " + championComponent.attackDamage + " " + attackType() + " Damage" +
			        "\nAbilities:";

			foreach (var ability in championComponent.abilities) body += "\n" + ability.abilityName + " (" + abilityType(ability) + ")";

			TooltipSystem.instance.Show(body, championComponent.name);
		});
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delay.uniqueId);
		TooltipSystem.instance.Hide();
	}
}
