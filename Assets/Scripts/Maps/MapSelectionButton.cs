using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class MapSelectionButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	[HideInInspector]
	public Map mapComponent;

	private static LTDescr delay;

	private void Start() {
		StartCoroutine(Setup());
	}

	public void OnClick() {
		GameController.instance.currentMap = mapComponent;
		LeanTween.cancel(delay.uniqueId);
		TooltipSystem.instance.Hide();
	}

	private IEnumerator Setup() {
		yield return null;
		if (mapComponent != null) gameObject.GetComponent<Image>().sprite = mapComponent.mapBackground;
	}

	public void OnPointerEnter(PointerEventData eventData) {
		delay = LeanTween.delayedCall(0.5f, () => {
			TooltipSystem.instance.Show(null, mapComponent.mapName);
		});
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delay.uniqueId);
		TooltipSystem.instance.Hide();
	}
}
