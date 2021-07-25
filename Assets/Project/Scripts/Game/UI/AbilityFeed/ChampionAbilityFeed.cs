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
			GridLayoutGroup gridlayoutGroup = gameObject.AddComponent<GridLayoutGroup>();
			gridlayoutGroup.cellSize = new Vector2(192, 32);
			gridlayoutGroup.startAxis = GridLayoutGroup.Axis.Horizontal;
			gridlayoutGroup.childAlignment = TextAnchor.LowerCenter;
			gridlayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
			gridlayoutGroup.constraintCount = 1;
		}
	}
}
