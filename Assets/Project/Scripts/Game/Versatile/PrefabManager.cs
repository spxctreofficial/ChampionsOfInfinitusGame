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
		public CardRegPool genericBasicCards, genericSpecialCards, genericWeapons;

		public CardData GenerateRandomCardData(ChampionController championController) {
			if (championController.equippedWeapon is null && championController.hand.GetWeaponCards().Length == 0 && Random.Range(0f, 1f) < 0.2f) {
				return genericWeapons.Retrieve().cardData;
			}
			if (Random.Range(0f, 1f) < 2f / 3f) {
				return genericBasicCards.Retrieve().cardData;
			}
			else {
				return genericSpecialCards.Combine(genericWeapons).Retrieve().cardData;
			}
		}

		[System.Serializable]
		public class CardRegPool {
			public CardRegEntry[] entries;

			public CardRegEntry Retrieve() {
				List<CardRegEntry> pool = new List<CardRegEntry>();

				if (entries is null || entries.Length <= 0) {
					Debug.LogError("The entry was null!");
					return null;
                }
				foreach (CardRegEntry cardRegEntry in entries) {
					for (int i = 0; i < cardRegEntry.weight; i++) {
						pool.Add(cardRegEntry);
					}
                }

				return pool[Random.Range(0, pool.Count)];
            }
			public CardRegPool Combine(CardRegPool pool) {
				CardRegPool newPool = new CardRegPool();
				newPool.entries = entries;
				newPool.entries.Concat(pool.entries);
				return newPool;
            }

        }

		[System.Serializable]
		public class CardRegEntry {
			public CardData cardData;
			[Range(1, short.MaxValue)]
			public int weight;
        }
	}
}