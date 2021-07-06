using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	public CardScriptableObject cardScriptableObject;
	[Space]
	public Image cardImage;
	public TMP_Text caption;
	public TMP_Text advantageFeed;
	public ParticleSystem halo;

	[HideInInspector]
	public ChampionController owner;
	[HideInInspector]
	public bool isHidden;
	public List<string> tags = new List<string>();

	private static int delayID;
	
	private void Start() {
		gameObject.name = cardScriptableObject.cardName;
		if (cardImage.sprite != cardScriptableObject.cardFront && cardImage.sprite != cardScriptableObject.cardBack) cardImage.sprite = cardScriptableObject.cardFront;
	}

	/// <summary>
	/// Flips a card, hiding it from the player's view.
	/// </summary>
	/// <param name="doFlipAnimation"></param>
	public void Flip(bool doFlipAnimation = false) {
		Debug.Log("calleed");
		if (!isHidden) {
			isHidden = true;
			cardImage.sprite = cardScriptableObject.cardBack;
		}
		else {
			isHidden = false;
			cardImage.sprite = cardScriptableObject.cardFront;

			if (doFlipAnimation) {
				transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				StartCoroutine(GetComponent<SmartHover>().ScaleDown(new Vector3(1f, 1f, 1f)));
			}
		}
	}
	
	// Static Functions
	public static void Spawn(CardScriptableObject cardScriptableObject, Hand hand) {
		Card card = Instantiate(GameController.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
		card.transform.SetParent(hand.transform, false);
		card.cardScriptableObject = cardScriptableObject;
		card.owner = hand.owner;
		hand.cards.Add(card);
	}
	
	// Pointer Events
	public void CardSelect() {
		StartCoroutine(CardLogicController.instance.CardSelect(this));
	}
	public void OnPointerEnter(PointerEventData eventData) {
		delayID = LeanTween.delayedCall(1f, () => {
			if (isHidden) {
				TooltipSystem.instance.Show(null, "Flipped Card");
				return;
			}
			TooltipSystem.instance.Show(cardScriptableObject.cardDescription, cardScriptableObject.cardName);
		}).uniqueId;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
}
