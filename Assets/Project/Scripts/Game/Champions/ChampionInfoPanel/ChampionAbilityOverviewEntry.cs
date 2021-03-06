using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ChampionAbilityOverviewEntry : MonoBehaviour
{
	[SerializeField]
	private TMP_Text abilityNameText, abilityDescriptionText;
	[SerializeField]
	private Image abilityImage;

	public void Setup(AbilityScriptableObject abilityScriptableObject)
	{
		abilityNameText.text = abilityScriptableObject.abilityName;
		abilityDescriptionText.text = abilityScriptableObject.abilityDescription;
		abilityImage.sprite = abilityScriptableObject.sprite;
	}
}
