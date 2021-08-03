using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipSystem : MonoBehaviour {
	public static TooltipSystem instance;

	public enum TooltipType { Tooltip, ErrorTooltip }
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

	/// <summary>
	/// Displays a normal tooltip.
	/// </summary>
	/// <param name="body"></param>
	/// <param name="header"></param>
	public void Show(string body, string header = "") {
		tooltip.UpdateTransform();
		tooltip.UpdatePivot();
		tooltip.gameObject.SetActive(true);

		tooltip.SetText(body, header);
		tooltip.GetComponent<CanvasGroup>().alpha = 0f;
		LeanTween.alphaCanvas(tooltip.GetComponent<CanvasGroup>(), 1f, 0.25f).setEaseInOutQuad();
	}
	/// <summary>
	/// Displays an error tooltip at the given vector2.
	/// </summary>
	/// <param name="vector2"></param>
	/// <param name="body"></param>
	/// <param name="header"></param>
	public void ShowError(string body, string header = "") {
		fixedTooltip.UpdateTransform();
		fixedTooltip.UpdatePivot();
		fixedTooltip.gameObject.SetActive(true);

		fixedTooltip.SetText(body, header);
		fixedTooltip.GetComponent<CanvasGroup>().alpha = 0f;
		LeanTween.alphaCanvas(fixedTooltip.GetComponent<CanvasGroup>(), 1f, 0.25f).setEaseInOutQuad();
	}
	/// <summary>
	/// Hide all active tooltips.
	/// </summary>
	public void Hide() {
		tooltip.GetComponent<CanvasGroup>().alpha = 0f;
		tooltip.gameObject.SetActive(false);

		fixedTooltip.GetComponent<CanvasGroup>().alpha = 0f;
		fixedTooltip.gameObject.SetActive(false);
	}
	/// <summary>
	/// Hide a specific type of tooltip.
	/// </summary>
	/// <param name="tooltipType"></param>
	public void Hide(TooltipType tooltipType) {
		float fadeOutTime = 0.1f;

		switch (tooltipType) {
			case TooltipType.Tooltip:
				LeanTween.alphaCanvas(tooltip.GetComponent<CanvasGroup>(), 0f, fadeOutTime).setOnComplete(() => tooltip.gameObject.SetActive(false)).setEaseInOutQuad();
				break;
			case TooltipType.ErrorTooltip:
				LeanTween.alphaCanvas(fixedTooltip.GetComponent<CanvasGroup>(), 0f, fadeOutTime).setOnComplete(() => fixedTooltip.gameObject.SetActive(false)).setEaseInOutQuad();
				break;
		}
	}
}
