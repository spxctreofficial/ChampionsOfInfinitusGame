using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class Card : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {

	[Header("Card Information")]
	public CardScriptableObject cardScriptableObject;
	[Header("References")]
	public Image cardImage;
	public TMP_Text caption;
	public TMP_Text discardFeed, advantageFeed;
	public ParticleSystem halo, redGlow;

	[HideInInspector]
	public ChampionController owner;
	[HideInInspector]
	public bool isHidden;
	public List<string> tags = new List<string>();

	private int combatValue;
	private static int delayID;

	public int CombatValue {
		get => combatValue;
		set {
			combatValue = value;
			if (combatValue == cardScriptableObject.cardValue) {
				advantageFeed.text = string.Empty;
				return;
			}

			int difference = combatValue - cardScriptableObject.cardValue;

			if (combatValue > cardScriptableObject.cardValue) {
				Color greenGlow = (Color)new Color32(46, 191, 0, 128) * Mathf.Pow(2, 2);
				greenGlow.a = 0.5f;
				advantageFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, greenGlow);
				advantageFeed.text = "+" + difference;
			}
			else {
				Color redGlow = Color.red * Mathf.Pow(2, 2);
				redGlow.a = 0.65f;
				advantageFeed.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, redGlow);
				advantageFeed.text = difference.ToString();
			}
			advantageFeed.text += " Advantage";
		}
	}
	
	private void Start() {
		gameObject.name = cardScriptableObject.cardName;
		combatValue = cardScriptableObject.cardValue;
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
			
			advantageFeed.gameObject.SetActive(false);
		}
		else {
			isHidden = false;
			cardImage.sprite = cardScriptableObject.cardFront;

			advantageFeed.gameObject.SetActive(true);
			if (doFlipAnimation) {
				transform.localScale = new Vector3(1.1f, 1.1f, 1.1f);
				GetComponent<SmartHover>().ScaleDown();
			}
		}
	}
	
	// Static Functions
	public static void Spawn(CardScriptableObject cardScriptableObject, Hand hand) {
		Card card = Instantiate(PrefabManager.instance.cardTemplate, Vector2.zero, Quaternion.identity).GetComponent<Card>();
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

			string description = cardScriptableObject.cardDescription;
			description += "\n\nWhen played in combat, this has a base combat value of " + combatValue +" .";
			TooltipSystem.instance.Show(description, cardScriptableObject.cardName);
		}).uniqueId;
	}
	public void OnPointerExit(PointerEventData eventData) {
		LeanTween.cancel(delayID);
		TooltipSystem.instance.Hide(TooltipSystem.TooltipType.Tooltip);
	}
}
