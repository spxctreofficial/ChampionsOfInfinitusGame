using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour {
	public static DataManager instance;

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

	public void Save() {
		SaveObject saveObject = new SaveObject {
			goldAmount = goldAmount,
			ownedChampions = ownedChampions
		};
		string json = JsonUtility.ToJson(saveObject);
		if (!Directory.Exists(saveFolder)) Directory.CreateDirectory(saveFolder);
		File.WriteAllText(saveFolder + "/save.lohsave", json);
	}
	public void Load() {
		if (!File.Exists(saveFolder + "/save.lohsave")) return;
		string savedJson = File.ReadAllText(saveFolder + "/save.lohsave");
		Debug.Log(savedJson);

		SaveObject loadedSaveObject = JsonUtility.FromJson<SaveObject>(savedJson);

		goldAmount = loadedSaveObject.goldAmount;
		ownedChampions = loadedSaveObject.ownedChampions;
	}

	public int GetGoldAmount() {
		return PlayerPrefs.HasKey("goldAmount") ? PlayerPrefs.GetInt("goldAmount") : 0;
	}
	public void SetGoldAmount(int amount) {
		PlayerPrefs.SetInt("goldAmount", amount);
		PlayerPrefs.Save();
	}

	private class SaveObject {
		public int goldAmount;
		public List<Champion> ownedChampions;
	}
}
