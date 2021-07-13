using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SmartHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	private Vector3 defaultScale = new Vector3(1f, 1f, 1f);

	private int scaleUpID, scaleDownID;
	
	public void OnPointerEnter(PointerEventData eventData) {
		AudioController.instance.Play("uiselect");
		scaleUpID = LeanTween.scale(GetComponent<RectTransform>(), new Vector3(1.1f, 1.1f, 1.1f), 0.2f).setEaseInOutCubic().uniqueId;
		LeanTween.cancel(scaleDownID);
	}
	public void OnPointerExit(PointerEventData eventData) {
		ScaleDown();
	}

	public void ScaleDown() {
		scaleDownID = LeanTween.scale(GetComponent<RectTransform>(), defaultScale, 0.2f).setEaseInOutCubic().uniqueId;
		LeanTween.cancel(scaleUpID);
	}
}
