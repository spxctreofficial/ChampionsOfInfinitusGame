using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour {
	public static TooltipSystem instance;

	public Tooltip tooltip;
	public FixedTooltip fixedTooltip;

	private void Awake() {
		if (instance == null)
			instance = this;
		else {
			Destroy(gameObject);
		}
		if (tooltip.GetComponent<CanvasGroup>() == null) tooltip.gameObject.AddComponent<CanvasGroup>();
	}

	public void Show(string body, string header = "", bool error = false) {
		switch (error) {
			case true:
				fixedTooltip.gameObject.SetActive(true);
				
				fixedTooltip.SetText(body, header);
				fixedTooltip.GetComponent<CanvasGroup>().alpha = 0f;
				LeanTween.alphaCanvas(fixedTooltip.GetComponent<CanvasGroup>(), 1f, 0.25f);
				break;
			case false:
				tooltip.UpdatePosition();
				tooltip.gameObject.SetActive(true);

				tooltip.SetText(body, header);
				tooltip.GetComponent<CanvasGroup>().alpha = 0f;
				LeanTween.alphaCanvas(tooltip.GetComponent<CanvasGroup>(), 1f, 0.25f);
				break;
		}
	}
	public void Hide() {
		tooltip.GetComponent<CanvasGroup>().alpha = 0f;
		tooltip.gameObject.SetActive(false);

		fixedTooltip.GetComponent<CanvasGroup>().alpha = 0f;
		fixedTooltip.gameObject.SetActive(false);
	}
}
