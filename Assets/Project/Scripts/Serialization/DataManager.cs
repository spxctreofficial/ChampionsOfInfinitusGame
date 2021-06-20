using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour {
	// Singleton
	public static DataManager instance;
	
	// Content Lists
	public ChampionIndex championIndex = new ChampionIndex();
	public MapIndex mapIndex = new MapIndex();

	// Serialization Variables
	private string saveFolder;
	private int goldAmount = 0;
	private List<Champion> ownedChampions = new List<Champion>();
	
	public int GoldAmount {
		get => goldAmount;
		set {
			goldAmount = value;
			Save();
		}
	}
	public List<Champion> OwnedChampions {
		get => ownedChampions;
		set {
			ownedChampions = value;
			Save();
		}
	}

	private void Awake() {
		if (instance == null) {
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else {
			Destroy(gameObject);
			return;
		}

		saveFolder = Application.dataPath + "/Saves";

		Load();
		Save();
	}
	private void Start() {
		championIndex.champions.Sort((x, y) => x.shopCost.CompareTo(y.shopCost));
	}

	// Serialization Methods
	public void Save() {
		// Sort & save owned champions by their ID.
		if (OwnedChampions.Count != 0) OwnedChampions.Sort((x, y) => String.Compare(x.championName, y.championName, StringComparison.Ordinal));
		List<string> ownedChampions = new List<string>();
		foreach (var champion in OwnedChampions) {
			ownedChampions.Add(champion.championID);
		}
		
		SaveObject saveObject = new SaveObject {
			goldAmount = goldAmount,
			ownedChampions = ownedChampions
		};
		
		string json = JsonUtility.ToJson(saveObject, true);
		if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
		File.WriteAllText(saveFolder + "/save.lohsave", json);
	}
	public void Load() {
		// Loads SaveObject
		if (!File.Exists(saveFolder + "/save.lohsave")) {
			// Fail-safe that auto-adds the Regime Soldier if the player does not have any champions.
			ownedChampions.Add(championIndex.champions[0]);
			return;
		}

		string savedJson = File.ReadAllText(saveFolder + "/save.lohsave");
		Debug.Log(savedJson);

		SaveObject loadedSaveObject = JsonUtility.FromJson<SaveObject>(savedJson);

		// Sets Values
		goldAmount = loadedSaveObject.goldAmount;
		foreach (var id in loadedSaveObject.ownedChampions) {
			foreach (var champion in championIndex.champions) {
				if (champion.championID != id) continue;
				OwnedChampions.Add(champion);
			}
		}

		if (!ownedChampions.Contains(championIndex.champions[0])) {
			ownedChampions.Add(championIndex.champions[0]);
		}
	}

	// Class that stores serialized variables to save and load progress
	private class SaveObject {
		public int goldAmount;
		public List<string> ownedChampions;
	}
}
