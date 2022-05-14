using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardRenderer : MonoBehaviour {
    public Card card;
    public Image image, icon;
    public TMP_Text cornerText, staminaRequirementText;

    private void Start() {
        Refresh();
    }

    public void Refresh() {
        if (card is null) {
            Debug.Log("Didn't refresh CardRenderer. No card was loaded.");
            return;
        }
        if (image.sprite != card.cardData.cardFront && image.sprite != card.cardData.cardBack) image.sprite = card.cardData.cardFront;
        icon.sprite = card.cardData.cardIcon;
        cornerText.text = card.cardData.cornerText;
        cornerText.color = card.cardData.cardColor == CardColor.Dark ? Color.white : Color.black;
        staminaRequirementText.text = card.EffectiveStaminaRequirement.ToString();
        staminaRequirementText.color = card.cardData.cardColor == CardColor.Dark ? Color.white : Color.black;
    }

    public void Flip() {
        if (!card.isHidden) {
            image.sprite = card.cardData.cardBack;

            icon.gameObject.SetActive(false);
            cornerText.gameObject.SetActive(false);
            staminaRequirementText.gameObject.SetActive(false);
            return;
        }

        image.sprite = card.cardData.cardFront;

        icon.gameObject.SetActive(true);
        cornerText.gameObject.SetActive(true);
        staminaRequirementText.gameObject.SetActive(true);
    }
}
