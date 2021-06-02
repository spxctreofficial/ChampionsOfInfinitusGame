using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class ChampionSlot : MonoBehaviour {
	private ChampionController occupiedChampion;

	// Occupant Methods
	/// <summary>
	/// Get the current occupant of this slot.
	/// </summary>
	/// <returns></returns>
	public ChampionController CurrentOccupant() {
		return occupiedChampion;
	}
	/// <summary>
	/// Sets `champion` to the occupant of this slot.
	/// </summary>
	/// <param name="champion"></param>
	public void SetOccupant(ChampionController champion) {
		if (IsOccupied()) {
			switch (CurrentOccupant().team == champion.team) {
				case true:
					FindNextVacantSlot("Ally").SetOccupant(CurrentOccupant());
					break;
				case false:
					FindNextVacantSlot("Opponent").SetOccupant(CurrentOccupant());
					break;
			}
		}

		champion.transform.localPosition = GetComponent<RectTransform>().localPosition;
		occupiedChampion = champion;
	}
	/// <summary>
	/// Clear the occupant of this slot.
	/// </summary>
	public void ClearOccupant() {
		occupiedChampion = null;
	}
	/// <summary>
	/// Checks if this slot is currently occupied.
	/// </summary>
	/// <returns></returns>
	public bool IsOccupied() {
		return CurrentOccupant() switch {
			null => false,
			_ => true
		};
	}
	/// <summary>
	/// Finds and returns the next vacant slot, given that a GameController exists within the scene.
	///
	/// Possible filters: "Normal" (default), "Ally", "Opponent"
	/// </summary>
	/// <param name="filter"></param>
	/// <returns></returns>
	public static ChampionSlot FindNextVacantSlot(string filter = "Normal") {
		foreach (var championSlot in GameController.instance.slots) {
			if (championSlot.occupiedChampion != null) continue;

			Vector2 position = championSlot.GetComponent<RectTransform>().localPosition;
			switch (filter) {
				case "Ally":
					if (position != defaultLocations[1]
					    || position == defaultLocations[2]
					    || position == defaultLocations[3]
					    || position == defaultLocations[4]
					    || position == defaultLocations[7]
					    || position == defaultLocations[8]) continue;
					break;
				case "Opponent":
					if (position == defaultLocations[1]
					    || position != defaultLocations[2]
					    || position != defaultLocations[3]
					    || position != defaultLocations[4]
					    || position != defaultLocations[7]
					    || position != defaultLocations[8]) continue;
					break;
			}

			return championSlot;
		}

		Debug.LogError("No vacant slot was found!");
		return null;
	}

	// Static Methods

	/// <summary>
	/// Creates all the default slots based on the default locations, given that a GameController exists within the scene.
	/// </summary>
	public static void CreateDefaultSlots() {
		foreach (Vector3 vector3 in defaultLocations) {
			ChampionSlot slot = Instantiate(GameController.instance.championSlotPrefab, vector3, Quaternion.identity).GetComponent<ChampionSlot>();
			GameController.instance.slots.Add(slot);
			slot.transform.SetParent(GameController.instance.gameArea.transform, false);
		}
	}

	/// <summary>
	/// Default locations for common & verified slots.
	/// </summary>
	public static readonly List<Vector2> defaultLocations = new List<Vector2> {
		new Vector2(-864, (float)-213.25), // Player Slot
		new Vector2(864, (float)-213.25),  // *Usually* Ally Slot
		new Vector2(864, (float)372.75),   // Enemy Slot 1
		new Vector2(-864, (float)372.75),  // Enemy Slot 2
		new Vector2(0, (float)372.75),     // Enemy Slot 3
		new Vector2(-864, 96),             // Miscellaneous Slot 1 (Usually Minion)
		new Vector2(864, 96),              // Miscellaneous Slot 2 (Usually Minion)
		new Vector2(-432, (float)372.75),  // Enemy Slot 3 (Usually Enemy Minion)
		new Vector2(432, (float)372.75)    // Enemy Slot 4 (Usually Enemy Minion)
	};
}
