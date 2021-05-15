using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TooltipSystem : MonoBehaviour
{
	public static TooltipSystem instance;
	
	public Tooltip tooltip;

	private void Awake()
	{
		if (instance == null)
			instance = this;
		else
		{
			Destroy(gameObject);
		}
		if (tooltip.GetComponent<CanvasGroup>() == null) tooltip.gameObject.AddComponent<CanvasGroup>();
	}

	public void Show(string body, string header = "")
	{
		var gameObject = tooltip.gameObject;
		tooltip.UpdatePosition();
		gameObject.SetActive(true);
		
		tooltip.SetText(body, header);
		tooltip.GetComponent<CanvasGroup>().alpha = 0f;
		LeanTween.alphaCanvas(tooltip.GetComponent<CanvasGroup>(), 1f, 0.25f);
	}
	public void Hide()
	{
		tooltip.GetComponent<CanvasGroup>().alpha = 0f;
		tooltip.gameObject.SetActive(false);
	}
}