using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class CardTooltip : MonoBehaviour
{
	public TMP_Text cardTitle, cardDescription, cardRequirementText, cardTypeText;
	public CardRenderer cardRenderer;

	public RectTransform rectTransform;
	public CanvasGroup canvasGroup;

	public Card card;

	public List<int> delayIDs = new();

	public void Setup(Card card)
	{
		Clear();
		cardTitle.text = card.cardData.cardName;
		cardDescription.text = card.cardData.cardDescription;
		cardRequirementText.text = "Stamina Requirement: " +
		                           (card.EffectiveStaminaRequirement == 0
			                           ? "FREE"
			                           : card.EffectiveStaminaRequirement.ToString());
		switch (card.cardData.cardColor)
		{
			case CardColor.Light:
				cardTypeText.text = "(Light)";
				break;
			case CardColor.Dark:
				cardTypeText.text = "(Dark)";
				break;
		}

		this.card = card;
		cardRenderer.card = card;
		cardRenderer.Refresh();


		TooltipSystem.instance.cardTooltip.Position(card);
	}

	public void Position(Card card)
	{
		RectTransform cardRectTransform = card.GetComponent<RectTransform>();
		if (card.owner is { })
		{
			float heightOffset = rectTransform.rect.height / 2 +
			                     card.cardRenderer.image.rectTransform.rect.height / 2;
			rectTransform.position = cardRectTransform.position;
			rectTransform.localPosition = new Vector2(rectTransform.localPosition.x,
				rectTransform.localPosition.y + heightOffset + 20);
		}
		else
		{
			rectTransform.position = cardRectTransform.position;
		}
	}

	private void Clear()
	{
		cardTitle.text = string.Empty;
		cardDescription.text = string.Empty;
		cardTypeText.text = string.Empty;
		card = null;
		cardRenderer.card = null;
	}
}