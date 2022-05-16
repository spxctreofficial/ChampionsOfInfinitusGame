using UnityEngine;

public class HandViewport : MonoBehaviour {
    public Hand parent;
    public RectTransform rectTransform;

    private int ltID;

    private void Awake() {
        rectTransform = GetComponent<RectTransform>();
    }

    private void Start() {
        Center(false);
    }

    public void Add(Card card) {
        if (parent.cards.Contains(card)) return;

        card.transform.SetParent(transform, false);
        Center();
    }
    public void Remove(Card card) {
        if (!parent.cards.Contains(card)) return;
        Center();
    }

    private void Center(bool animated = true) {
        LeanTween.cancel(ltID);

        if (!animated) {
            LeanTween.move(rectTransform, CalculateCenterOffset(), 0f);
            return;
        }
        ltID = LeanTween.delayedCall(0.01f, () => LeanTween.move(rectTransform, CalculateCenterOffset(), 0.2f).setEaseOutQuart()).uniqueId;
    }
    private Vector2 CalculateCenterOffset() {
        float centeredX = (parent.GetComponent<RectTransform>().rect.width - rectTransform.rect.width) / 2f;
        return new Vector2(centeredX, 0);
    }
}