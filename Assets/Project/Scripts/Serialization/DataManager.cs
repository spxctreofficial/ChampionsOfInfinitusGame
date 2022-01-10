using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataManager : MonoBehaviour {
	public static DataManager instance;
	
	public ChampionIndex championIndex = new ChampionIndex();
	public MapIndex mapIndex = new MapIndex();

	private string saveFolder;

	public int goldAmount;
	public List<Champion> ownedChampions = new List<Champion>();

	public bool firstRunGame, firstRunShop, firstRunTutorial;
	
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

		LoadDefaultSave();
		LoadFirstRunSave();
		Save();
	}
	private void Start() {
		championIndex.champions.Sort((x, y) => x.value.CompareTo(y.value));
	}

	// Serialization Methods
	public void Save() {
		// Sort & save owned champions by their ID.
		if (this.ownedChampions.Count != 0) this.ownedChampions.Sort((x, y) => String.Compare(x.championName, y.championName, StringComparison.Ordinal));
		List<string> ownedChampions = new List<string>();
		foreach (Champion champion in this.ownedChampions) {
			ownedChampions.Add(champion.championID);
		}
		
		DefaultSaveObject defaultSaveObject = new DefaultSaveObject {
			goldAmount = goldAmount,
			ownedChampions = ownedChampions
		};
		FirstRunSaveObject firstRunSaveObject = new FirstRunSaveObject {
			firstRunGame = firstRunGame,
			firstRunShop = firstRunShop,
			firstRunTutorial = firstRunTutorial
		};
		
		string defaultSaveJson = JsonUtility.ToJson(defaultSaveObject, true);
		string firstRunSaveJson = JsonUtility.ToJson(firstRunSaveObject, true);
		if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
		
		File.WriteAllText(saveFolder + "/save.coisave", defaultSaveJson);
		File.WriteAllText(saveFolder + "/firstrun.coisave", firstRunSaveJson);
	}

	private void LoadDefaultSave() {
		// Loads SaveObject
		if (!File.Exists(saveFolder + "/save.coisave")) {
			// Fail-safe that auto-adds the Regime Soldier if the player does not have any champions.
			ownedChampions.Add(championIndex.champions[0]);
			return;
		}

		string defaultSavedJson = File.ReadAllText(saveFolder + "/save.coisave");
		
		Debug.Log(defaultSavedJson);
		DefaultSaveObject loadedDefaultSaveObject = JsonUtility.FromJson<DefaultSaveObject>(defaultSavedJson);
		
		// Sets Values
		goldAmount = loadedDefaultSaveObject.goldAmount;
		foreach (string id in loadedDefaultSaveObject.ownedChampions) {
			foreach (Champion champion in championIndex.champions) {
				if (champion.championID != id) continue;
				ownedChampions.Add(champion);
			}
		}
		if (!ownedChampions.Contains(championIndex.champions[0])) {
			ownedChampions.Add(championIndex.champions[0]);
		}
	}

	private void LoadFirstRunSave() {
		// Loads SaveObject
		if (!File.Exists(saveFolder + "/firstrun.coisave")) return;

		string firstRunSavedJson = File.ReadAllText(saveFolder + "/firstrun.coisave");

		Debug.Log(firstRunSavedJson);
		FirstRunSaveObject loadedFirstRunSaveObject = JsonUtility.FromJson<FirstRunSaveObject>(firstRunSavedJson);

		// Sets Values
		firstRunGame = loadedFirstRunSaveObject.firstRunGame;
		firstRunShop = loadedFirstRunSaveObject.firstRunShop;
		firstRunTutorial = loadedFirstRunSaveObject.firstRunTutorial;
	}

	// Classes that stores serialized variables to save and load progress
	private class DefaultSaveObject {
		public int goldAmount;
		public List<string> ownedChampions;
	}
	private class FirstRunSaveObject {
		public bool firstRunGame, firstRunShop, firstRunTutorial;
	}
}
