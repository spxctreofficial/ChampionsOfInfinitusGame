using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PrefabManager : MonoBehaviour {
	public static PrefabManager instance;

	[Header("GameController")]
	public GameObject championTemplate;
	public GameObject abilityTemplate;
	public GameObject cardTemplate;
	public GameObject handPrefab;
	public GameObject championSlotPrefab;
	public CardReg cardReg;

	[Header("Versatile")]
	public GameObject dialogueSystemPrefab;
	public GameObject confirmDialogPrefab;
	public GameObject miniConfirmDialogPrefab;
	public GameObject notificationDialogPrefab;
	public GameObject championInfoPanelPrefab;

	private void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
        }
    }

	[System.Serializable]
	public class CardReg {
		public CardData[] genericBasicCards, genericSpecialCards, genericWeapons;

		public CardData GenerateRandomCardData(ChampionController championController) {
			CardData cardData;
			if (championController.equippedWeapon is null && Random.Range(0f, 1f) < 0.2f) {
				return genericWeapons[Random.Range(0, genericWeapons.Length)];
			}
			if (Random.Range(0f, 1f) < 2f / 3f) {
				cardData = genericBasicCards[Random.Range(0, genericBasicCards.Length)];
			}
			else {
				CardData[] genericSpecialCards = this.genericSpecialCards;
				List<CardData> availableCards = genericSpecialCards.Concat(genericWeapons).ToList();
				cardData = availableCards[Random.Range(0, availableCards.Count)];
			}

			return cardData;
		}
	}
}