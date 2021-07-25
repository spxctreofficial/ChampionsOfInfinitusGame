using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapSelectionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	[HideInInspector]
	public Map mapComponent;

	private static int delayID;

	private void Start() {
		StartCoroutine(Setup());
	}

	public void OnClick() {
		SandboxGameController.instance.currentMap = mapComponent;
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}

	private IEnumerator Setup() {
		yield return null;
		if (mapComponent != null) gameObject.GetComponent<Image>().sprite = mapComponent.mapBackground;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(0.5f, () => TooltipSystem.instance.Show(null, mapComponent.mapName)).uniqueId;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
}
