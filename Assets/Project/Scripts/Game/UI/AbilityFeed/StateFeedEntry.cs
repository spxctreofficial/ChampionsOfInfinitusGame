using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StateFeedEntry : MonoBehaviour {
	[HideInInspector]
	public int count = 1;
	[HideInInspector]
	public ChampionController champion;

	private int tweenInEvent;

	public static StateFeedEntry New(string text, ChampionController champion, Color color) {
		StateFeedEntry stateFeedEntry = Instantiate(champion.abilityFeed.stateFeedEntryPrefab, Vector2.zero, Quaternion.identity).GetComponent<StateFeedEntry>();

		stateFeedEntry.GetComponent<TMP_Text>().text = text;
		stateFeedEntry.GetComponent<TMP_Text>().fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, color);
		
		stateFeedEntry.transform.SetParent(champion.abilityFeed.transform, false);
		stateFeedEntry.champion = champion;
		champion.abilityFeed.feedEntries.Add(stateFeedEntry.gameObject);
		champion.abilityFeed.stateFeedEntries.Add(stateFeedEntry);
		
		stateFeedEntry.GetComponent<RectTransform>().localScale = Vector2.zero;
		stateFeedEntry.tweenInEvent = LeanTween.scale(stateFeedEntry.GetComponent<RectTransform>(), Vector2.one, 0.1f).setEaseInOutQuad().uniqueId;

		return stateFeedEntry;
	}
	public static StateFeedEntry New(string text, ChampionController champion) {
		Color color = (Color)new Color32(0, 90, 191, 128) * Mathf.Pow(2, 2);
		color.a = 0.5f;
		return New(text, champion, color);
	}
	public static void ClearAll(ChampionController champion) {
		foreach (StateFeedEntry stateFeedEntry in champion.abilityFeed.stateFeedEntries) {
			stateFeedEntry.Remove();
		}
	}
	
	public void Remove() {
		LeanTween.cancel(tweenInEvent);
		
		LeanTween.scale(GetComponent<RectTransform>(), Vector2.zero, 0.15f).setEaseInOutQuad().setDestroyOnComplete(true).setOnComplete(() => {
			champion.abilityFeed.feedEntries.Remove(gameObject);
			champion.abilityFeed.stateFeedEntries.Remove(this);
		});
	}
}
