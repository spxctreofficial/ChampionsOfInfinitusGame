using System;
using System.Collections;
using UnityEngine;
using TMPro;

public class AbilityFeedEntry : MonoBehaviour {
	[HideInInspector]
	public AbilityScriptableObject abilityScriptableObject;
	[HideInInspector]
	public int count = 1;

	private int delayID;

	public static AbilityFeedEntry New(AbilityScriptableObject abilityScriptableObject, ChampionController champion, float duration = 5f) {
		// Instantiate
		AbilityFeedEntry abilityFeedEntry = Instantiate(champion.abilityFeed.abilityFeedEntryPrefab, Vector2.zero, Quaternion.identity).GetComponent<AbilityFeedEntry>();

		// Set Display
		abilityFeedEntry.GetComponent<TMP_Text>().text = abilityScriptableObject.abilityName;
		foreach (Transform child in champion.abilityFeed.transform) {
			AbilityFeedEntry anotherAbilityFeedEntry = child.GetComponent<AbilityFeedEntry>();
			if (abilityFeedEntry == anotherAbilityFeedEntry || abilityFeedEntry.abilityScriptableObject != anotherAbilityFeedEntry.abilityScriptableObject) continue;

			abilityFeedEntry.count += anotherAbilityFeedEntry.count;

			Destroy(anotherAbilityFeedEntry.gameObject);
			champion.abilityFeed.feedEntries.Remove(anotherAbilityFeedEntry.gameObject);
			champion.abilityFeed.abilityFeedEntries.Remove(anotherAbilityFeedEntry);
		}
		abilityFeedEntry.GetComponent<TMP_Text>().text += abilityFeedEntry.count > 1 ? " x" + abilityFeedEntry.count : string.Empty;

		// Set Affiliation
		abilityFeedEntry.transform.SetParent(champion.abilityFeed.transform, false);
		champion.abilityFeed.feedEntries.Add(abilityFeedEntry.gameObject);
		champion.abilityFeed.abilityFeedEntries.Add(abilityFeedEntry);

		// Animation
		abilityFeedEntry.GetComponent<RectTransform>().localScale = Vector2.zero;
		LeanTween.scale(abilityFeedEntry.GetComponent<RectTransform>(), Vector2.one, 0.1f).setEaseInOutQuad().setOnComplete(() => {
			abilityFeedEntry.delayID = LeanTween.delayedCall(duration, () => {
				if (abilityFeedEntry == null) return;
				LeanTween.scale(abilityFeedEntry.GetComponent<RectTransform>(), Vector2.zero, 0.15f).setEaseInOutQuad().setDestroyOnComplete(true).setOnComplete(() => {
					champion.abilityFeed.feedEntries.Remove(abilityFeedEntry.gameObject);
					champion.abilityFeed.abilityFeedEntries.Remove(abilityFeedEntry);
				});
			}).uniqueId;
		});

		return abilityFeedEntry;
	}
}
