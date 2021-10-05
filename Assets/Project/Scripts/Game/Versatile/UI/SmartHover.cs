using UnityEngine;
using UnityEngine.EventSystems;

public class SmartHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
	private readonly Vector3 defaultScale = new Vector3(1f, 1f, 1f);
	private readonly Vector3 animatedScale = new Vector3(1.1f, 1.1f, 1.1f);

	private int scaleUpID, scaleDownID;
	
	public void OnPointerEnter(PointerEventData eventData) {
		AudioController.instance.Play("uiselect");
		scaleUpID = LeanTween.scale(GetComponent<RectTransform>(), animatedScale, 0.3f).setEaseInOutQuad().uniqueId;
		LeanTween.cancel(scaleDownID);
	}
	public void OnPointerExit(PointerEventData eventData) {
		ScaleDown();
	}

	public void ScaleDown() {
		scaleDownID = LeanTween.scale(GetComponent<RectTransform>(), defaultScale, 0.3f).setEaseInOutQuad().uniqueId;
		LeanTween.cancel(scaleUpID);
	}
}
