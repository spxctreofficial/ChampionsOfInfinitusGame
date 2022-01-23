using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ChampionSelectionButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler {
	[HideInInspector]
	public Champion championComponent;

	private static int delayID;

	private void Start() {
		StartCoroutine(Setup());
	}

	public void OnClick() {
		try {
			GameManager.instance.players[0] = championComponent;
		}
		catch (ArgumentOutOfRangeException) {
			Debug.Log("Caught NullReferenceException.");
			GameManager.instance.players.Add(championComponent);
		}
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}

	private IEnumerator Setup() {
		yield return null;
		if (championComponent != null) gameObject.GetComponent<Image>().sprite = championComponent.avatar;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(0.5f, () => {
			string body = "Health: " + championComponent.maxHP; // max health
			body += "\nRIGHT CLICK FOR MORE INFO";
			TooltipSystem.instance.Show(body, championComponent.championName); // show the tooltip
		}).uniqueId;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
	public void OnPointerClick(PointerEventData eventData) {
		if (eventData.button == PointerEventData.InputButton.Right) {
			ChampionInfoPanel.Create(championComponent).transform.SetParent(SandboxGameManager.instance.championSelectionConfig.transform, false);
		}
	}
}
