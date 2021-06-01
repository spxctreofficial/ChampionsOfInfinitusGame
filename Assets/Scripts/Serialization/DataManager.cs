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
		if (OwnedChampions.Count != 0) OwnedChampions.Sort((x, y) => String.Compare(x.championName, y.championName, StringComparison.Ordinal));
		SaveObject saveObject = new SaveObject {
			goldAmount = goldAmount,
			ownedChampions = ownedChampions
		};
		
		string json = JsonUtility.ToJson(saveObject);
		if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
		File.WriteAllText(saveFolder + "/save.lohsave", json);
	}
	public void Load() {
		// Loads SaveObject
		if (!File.Exists(saveFolder + "/save.lohsave")) return;
		string savedJson = File.ReadAllText(saveFolder + "/save.lohsave");
		Debug.Log(savedJson);

		SaveObject loadedSaveObject = JsonUtility.FromJson<SaveObject>(savedJson);

		// Sets Values
		goldAmount = loadedSaveObject.goldAmount;
		ownedChampions = loadedSaveObject.ownedChampions;
		
		// Special Behavior
		if (ownedChampions.Count == 0) ownedChampions.Add(championIndex.champions[0]);
	}

	// Class that stores serialized variables to save and load progress
	private class SaveObject {
		public int goldAmount;
		public List<Champion> ownedChampions;
	}
}
