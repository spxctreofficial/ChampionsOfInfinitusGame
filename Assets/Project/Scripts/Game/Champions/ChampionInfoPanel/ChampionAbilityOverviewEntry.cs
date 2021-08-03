using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChampionAbilityOverviewEntry : MonoBehaviour {
	[SerializeField]
	private TMP_Text abilityNameText, abilityDescriptionText;
	[SerializeField]
	private Image abilityImage;

	public void Setup(Ability ability) {
		abilityNameText.text = ability.abilityName;
		abilityDescriptionText.text = ability.abilityDescription;
		abilityImage.sprite = ability.sprite;
	}
}
