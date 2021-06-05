using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ChampionAbilityFeed : MonoBehaviour {

	public List<GameObject> feedEntries = new List<GameObject>();
	public List<GameObject> abilityFeedEntries = new List<GameObject>();
	public List<GameObject> stateFeedEntries = new List<GameObject>();
	
	[SerializeField]
	private GameObject abilityFeedEntryPrefab, stateFeedEntryPrefab;

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
	/// <param name="text"></param>
	/// <param name="duration"></param>
	public void NewAbilityFeedEntry(string text, float duration = 5f) {
		var abilityFeedEntry = Instantiate(abilityFeedEntryPrefab, Vector2.zero, Quaternion.identity).GetComponent<TMP_Text>();
		abilityFeedEntry.transform.SetParent(transform, false);
		feedEntries.Add(abilityFeedEntry.gameObject);
		abilityFeedEntries.Add(abilityFeedEntry.gameObject);
		
		abilityFeedEntry.text = text;
		var entriesWithIdenticalName = 0;
		foreach (Transform child in transform) {
			var anotherFeedEntry = child.GetComponent<TMP_Text>();
			if (anotherFeedEntry.text != abilityFeedEntry.text) continue;

			entriesWithIdenticalName++;
			if (anotherFeedEntry != abilityFeedEntry) {
				Destroy(anotherFeedEntry.gameObject);
				feedEntries.Remove(anotherFeedEntry.gameObject);
				abilityFeedEntries.Remove(anotherFeedEntry.gameObject);
			}
		}
		abilityFeedEntry.text += entriesWithIdenticalName <= 1 ? "" : " x" + entriesWithIdenticalName;

		abilityFeedEntry.GetComponent<RectTransform>().localScale = Vector2.zero;
		LeanTween.scale(abilityFeedEntry.GetComponent<RectTransform>(), Vector2.one, 0.1f).setEaseInOutQuad().setOnComplete(() => LeanTween.delayedCall(duration, () => {
			if (!abilityFeedEntry.IsDestroyed()) LeanTween.scale(abilityFeedEntry.GetComponent<RectTransform>(), Vector3.zero, 0.15f).setEaseInOutQuad().setDestroyOnComplete(true).setOnComplete(() => {
				feedEntries.Remove(abilityFeedEntry.gameObject);
				abilityFeedEntries.Remove(abilityFeedEntry.gameObject);
			});
		}));
	}
	/// <summary>
	/// Creates a new State Feed Entry (persistent, must be cleared by using `ClearStateFeedEntries`).
	/// </summary>
	/// <param name="text"></param>
	public void NewStateFeedEntry(string text, Color color) {
		var stateFeedEntry = Instantiate(stateFeedEntryPrefab, Vector2.zero, Quaternion.identity).GetComponent<TMP_Text>();
		stateFeedEntry.transform.SetParent(transform, false);
		feedEntries.Add(stateFeedEntry.gameObject);
		stateFeedEntries.Add(stateFeedEntry.gameObject);

		stateFeedEntry.text = text;
		stateFeedEntry.fontMaterial.SetColor(ShaderUtilities.ID_GlowColor, color);

		stateFeedEntry.GetComponent<RectTransform>().localScale = Vector2.zero;
		LeanTween.scale(stateFeedEntry.GetComponent<RectTransform>(), Vector2.one, 0.1f).setEaseInOutQuad();
	}
	public void NewStateFeedEntry(string text) {
		Color color = ((Color)new Color32(0, 90, 191, 128)) * Mathf.Pow(2, 2);
		color.a = 0.5f;
		NewStateFeedEntry(text, color);
	}
	public void ClearStateFeedEntry(string text) {
		foreach (var gameObject in stateFeedEntries) {
			if (gameObject.GetComponent<TMP_Text>() == null || gameObject.GetComponent<TMP_Text>().text != text) continue;
			Destroy(gameObject);
			break;
		}
	}
	public void ClearAllStateFeedEntries() {
		foreach (var gameObject in stateFeedEntries) {
			LeanTween.scale(gameObject.GetComponent<RectTransform>(), Vector2.zero, 0.15f).setEaseInOutQuad().setDestroyOnComplete(true).setOnComplete(() => {
				feedEntries.Remove(gameObject);
				stateFeedEntries.Remove(gameObject);
			});
		}
	}
}
