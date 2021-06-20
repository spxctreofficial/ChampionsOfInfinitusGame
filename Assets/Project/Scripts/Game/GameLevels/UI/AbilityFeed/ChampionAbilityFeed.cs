using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ChampionAbilityFeed : MonoBehaviour {
	
	public List<GameObject> feedEntries = new List<GameObject>();
	public List<AbilityFeedEntry> abilityFeedEntries = new List<AbilityFeedEntry>();
	public List<StateFeedEntry> stateFeedEntries = new List<StateFeedEntry>();
	
	public GameObject abilityFeedEntryPrefab, stateFeedEntryPrefab;

	private void Awake() {
		if (GetComponent<GridLayoutGroup>() == null) {
			var gridlayoutGroup = gameObject.AddComponent<GridLayoutGroup>();
			gridlayoutGroup.cellSize = new Vector2(192, 32);
			gridlayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
			gridlayoutGroup.childAlignment = TextAnchor.LowerCenter;
			gridlayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			gridlayoutGroup.constraintCount = 1;
		}
	}

	/// <summary>
	/// Creates a new Ability Feed Entry.
	/// </summary>
	/// <param name="ability"></param>
	/// <param name="champion"></param>
	/// <param name="duration"></param>
	public void NewAbilityFeedEntry(Ability ability, ChampionController champion, float duration = 5f) {
		var abilityFeedEntry = AbilityFeedEntry.New(ability, champion, duration);
	}
	/// <summary>
	/// Creates a new State Feed Entry (persistent, must be cleared by using `ClearStateFeedEntries`).
	/// </summary>
	/// <param name="text"></param>
	public void NewStateFeedEntry(string text, Color color) {
		var stateFeedEntry = Instantiate(stateFeedEntryPrefab, Vector2.zero, Quaternion.identity).GetComponent<StateFeedEntry>();
		stateFeedEntry.transform.SetParent(transform, false);
		feedEntries.Add(stateFeedEntry.gameObject);
		stateFeedEntries.Add(stateFeedEntry);

		stateFeedEntry.GetComponent<TMP_Text>().text = text;
		stateFeedEntry.GetComponent<TMP_Text>().fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, color);

		stateFeedEntry.GetComponent<RectTransform>().localScale = Vector2.zero;
		LeanTween.scale(stateFeedEntry.GetComponent<RectTransform>(), Vector2.one, 0.1f).setEaseInOutQuad();
	}
	public void NewStateFeedEntry(string text) {
		Color color = ((Color)new Color32(0, 90, 191, 128)) * Mathf.Pow(2, 2);
		color.a = 0.5f;
		NewStateFeedEntry(text, color);
	}
	public void ClearStateFeedEntry(string text) {
		foreach (var stateFeedEntry in stateFeedEntries) {
			if (stateFeedEntry.GetComponent<StateFeedEntry>() == null || !stateFeedEntry.GetComponent<TMP_Text>().text.Contains(text)) continue;
			LeanTween.scale(stateFeedEntry.GetComponent<RectTransform>(), Vector2.zero, 0.15f).setEaseInOutQuad().setDestroyOnComplete(true).setOnComplete(() => {
				feedEntries.Remove(stateFeedEntry.gameObject);
				stateFeedEntries.Remove(stateFeedEntry);
			});
			break;
		}
	}
	public void ClearAllStateFeedEntries() {
		foreach (var stateFeedEntry in stateFeedEntries) {
			LeanTween.scale(stateFeedEntry.GetComponent<RectTransform>(), Vector2.zero, 0.15f).setEaseInOutQuad().setDestroyOnComplete(true).setOnComplete(() => {
				feedEntries.Remove(stateFeedEntry.gameObject);
				stateFeedEntries.Remove(stateFeedEntry);
			});
		}
	}
}
