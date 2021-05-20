using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DataManager : MonoBehaviour
{
	public static DataManager instance;

	private void Awake()
	{
		if (instance == null)
		{
			instance = this;
			DontDestroyOnLoad(gameObject);
		}
		else
		{
			Destroy(gameObject);
		}
	}

	public int GetGoldAmount()
	{
		return PlayerPrefs.HasKey("goldAmount") ? PlayerPrefs.GetInt("goldAmount") : 0;
	}
	public void SetGoldAmount(int amount)
	{
		PlayerPrefs.SetInt("goldAmount", amount);
		PlayerPrefs.Save();
	}
}
