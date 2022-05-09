using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[ExecuteInEditMode]
public class CardTooltip : MonoBehaviour {
    public TMP_Text cardTitle, cardDescription, cardRequirementText, cardTypeText;
    public CardRenderer cardRenderer;

    public RectTransform rectTransform;
    public CanvasGroup canvasGroup;

    public Card card;

    public List<int> delayIDs = new();
    public static readonly Vector2 minScale = new Vector2(0.8f, 0.8f);

    public void Setup(Card card) {
        Clear();
        cardTitle.text = card.cardData.cardName;
        cardDescription.text = card.cardData.cardDescription;
        cardRequirementText.text = "Stamina Requirement: " +
                                   (card.EffectiveStaminaRequirement == 0
                                       ? "FREE"
                                       : card.EffectiveStaminaRequirement.ToString());
        switch (card.cardData.cardColor) {
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

        StartCoroutine(TooltipSystem.instance.cardTooltip.Position(card));
    }

    public IEnumerator Position(Card card) {
        yield return null;
        RectTransform cardRectTransform = card.GetComponent<RectTransform>();
        rectTransform.position = cardRectTransform.position;

        if (card.owner is { }) {
            float heightOffset = rectTransform.rect.height / 2 + card.cardRenderer.image.rectTransform.rect.height / 2;

            rectTransform.localPosition = new Vector2(rectTransform.localPosition.x, rectTransform.localPosition.y + heightOffset + 20);
        }
    }

    private void Clear() {
        cardTitle.text = string.Empty;
        cardDescription.text = string.Empty;
        cardTypeText.text = string.Empty;
        card = null;
        cardRenderer.card = null;
    }
}