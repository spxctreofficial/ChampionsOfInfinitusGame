using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class ChampionAbilityFeed : MonoBehaviour {

	public List<GameObject> feedEntries = new List<GameObject>();
	
	[SerializeField]
	private GameObject abilityFeedEntryPrefab;

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

	public void NewAbilityFeedEntry(string text, float duration = 5f) {
		var abilityFeedEntry = Instantiate(abilityFeedEntryPrefab, Vector2.zero, Quaternion.identity).GetComponent<TMP_Text>();
		abilityFeedEntry.transform.SetParent(transform, false);
		feedEntries.Add(abilityFeedEntry.gameObject);
		
		abilityFeedEntry.text = text;
		var entriesWithIdenticalName = 0;
		foreach (Transform child in transform) {
			var anotherFeedEntry = child.GetComponent<TMP_Text>();
			if (anotherFeedEntry.text != abilityFeedEntry.text) continue;

			entriesWithIdenticalName++;
			if (anotherFeedEntry != abilityFeedEntry) Destroy(anotherFeedEntry.gameObject);
		}
		abilityFeedEntry.text += entriesWithIdenticalName <= 1 ? "" : " x" + entriesWithIdenticalName;

		abilityFeedEntry.GetComponent<RectTransform>().localScale = Vector2.zero;
		LeanTween.scale(abilityFeedEntry.GetComponent<RectTransform>(), Vector2.one, 0.1f).setEaseInOutQuad().setOnComplete(() => LeanTween.delayedCall(duration, () => {
			if (!abilityFeedEntry.IsDestroyed()) LeanTween.scale(abilityFeedEntry.GetComponent<RectTransform>(), Vector3.zero, 0.15f).setEaseInOutQuad().setDestroyOnComplete(true);
		}));
	}
}
